package com.shnok.javaserver.gameserver.data.manager;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

import com.shnok.javaserver.commons.logging.CLogger;
import com.shnok.javaserver.commons.pool.ConnectionPool;
import com.shnok.javaserver.gameserver.model.World;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.ruler.RulerBuildingData;
import com.shnok.javaserver.gameserver.network.serverpackets.ruler.RulerBuildingsPacket;

public class RulerBuildingManager
{
	private static final CLogger LOGGER = new CLogger(RulerBuildingManager.class.getName());
	
	public static boolean ALLOW_ALL_PLAYERS_BUILD = true;
	private static final int CHUNK_SIZE = 60;
	
	private final Map<Integer, RulerBuildingData> _buildings = new ConcurrentHashMap<>();
	
	private static final String CREATE_TABLE = "CREATE TABLE IF NOT EXISTS ruler_buildings ("
		+ "id INT AUTO_INCREMENT PRIMARY KEY, "
		+ "prefab_name VARCHAR(64) NOT NULL, "
		+ "pos_x DOUBLE NOT NULL, "
		+ "pos_y DOUBLE NOT NULL, "
		+ "pos_z DOUBLE NOT NULL, "
		+ "rot_y FLOAT NOT NULL, "
		+ "scale FLOAT NOT NULL, "
		+ "owner_name VARCHAR(45), "
		+ "created_at BIGINT"
		+ ") ENGINE=InnoDB DEFAULT CHARSET=utf8mb4";
		
	private static final String SELECT_BUILDINGS = "SELECT id, prefab_name, pos_x, pos_y, pos_z, rot_y, scale, owner_name, created_at FROM ruler_buildings ORDER BY id ASC";
	private static final String INSERT_BUILDING = "INSERT INTO ruler_buildings (prefab_name, pos_x, pos_y, pos_z, rot_y, scale, owner_name, created_at) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
	private static final String DELETE_BUILDING = "DELETE FROM ruler_buildings WHERE id = ?";
	
	protected RulerBuildingManager()
	{
		initTable();
		loadBuildings();
	}
	
	private void initTable()
	{
		try (Connection con = ConnectionPool.getConnection();
			Statement s = con.createStatement())
		{
			s.execute(CREATE_TABLE);
		}
		catch (Exception e)
		{
			LOGGER.error("Couldn't initialize table ruler_buildings.", e);
		}
	}
	
	private void loadBuildings()
	{
		_buildings.clear();
		try (Connection con = ConnectionPool.getConnection();
			PreparedStatement ps = con.prepareStatement(SELECT_BUILDINGS);
			ResultSet rs = ps.executeQuery())
		{
			while (rs.next())
			{
				final int id = rs.getInt("id");
				final String prefab = rs.getString("prefab_name");
				final double posX = rs.getDouble("pos_x");
				final double posY = rs.getDouble("pos_y");
				final double posZ = rs.getDouble("pos_z");
				final float rotY = rs.getFloat("rot_y");
				final float scale = rs.getFloat("scale");
				final String owner = rs.getString("owner_name");
				final long createdAt = rs.getLong("created_at");
				
				final RulerBuildingData building = new RulerBuildingData(id, prefab, posX, posY, posZ, rotY, scale, owner, createdAt);
				_buildings.put(id, building);
			}
			LOGGER.info("Loaded {} ruler buildings from database.", _buildings.size());
		}
		catch (Exception e)
		{
			LOGGER.error("Couldn't load ruler buildings.", e);
		}
	}
	
	public RulerBuildingData addBuilding(String prefabName, double posX, double posY, double posZ, float rotY, float scale, String ownerName)
	{
		final long createdAt = System.currentTimeMillis();
		int generatedId = -1;
		
		try (Connection con = ConnectionPool.getConnection();
			PreparedStatement ps = con.prepareStatement(INSERT_BUILDING, Statement.RETURN_GENERATED_KEYS))
		{
			ps.setString(1, prefabName);
			ps.setDouble(2, posX);
			ps.setDouble(3, posY);
			ps.setDouble(4, posZ);
			ps.setFloat(5, rotY);
			ps.setFloat(6, scale);
			ps.setString(7, ownerName);
			ps.setLong(8, createdAt);
			ps.executeUpdate();
			
			try (ResultSet rs = ps.getGeneratedKeys())
			{
				if (rs.next())
				{
					generatedId = rs.getInt(1);
				}
			}
		}
		catch (Exception e)
		{
			LOGGER.error("Couldn't save ruler building to database.", e);
			return null;
		}
		
		if (generatedId <= 0)
		{
			LOGGER.warn("Failed to get generated ID for ruler building: {}.", prefabName);
			return null;
		}
		
		final RulerBuildingData building = new RulerBuildingData(generatedId, prefabName, posX, posY, posZ, rotY, scale, ownerName, createdAt);
		_buildings.put(generatedId, building);
		
		LOGGER.info("Player {} built '{}' (id={}) at ({}, {}, {}). Total buildings: {}",
			ownerName, prefabName, generatedId, posX, posY, posZ, _buildings.size());
			
		// Broadcast to all online players
		final RulerBuildingsPacket pkt = new RulerBuildingsPacket(building);
		for (Player p : World.getInstance().getPlayers())
		{
			if (p != null)
			{
				p.sendPacket(pkt);
			}
		}
		
		return building;
	}
	
	public boolean removeBuilding(int buildingId)
	{
		final RulerBuildingData removed = _buildings.remove(buildingId);
		if (removed == null)
		{
			return false;
		}
		
		try (Connection con = ConnectionPool.getConnection();
			PreparedStatement ps = con.prepareStatement(DELETE_BUILDING))
		{
			ps.setInt(1, buildingId);
			ps.executeUpdate();
		}
		catch (Exception e)
		{
			LOGGER.error("Couldn't delete ruler building with id {}.", buildingId, e);
		}
		
		LOGGER.info("Demolished ruler building id={}. Remaining buildings: {}", buildingId, _buildings.size());
		
		// Broadcast removal to all online players
		final RulerBuildingsPacket pkt = new RulerBuildingsPacket(buildingId);
		for (Player p : World.getInstance().getPlayers())
		{
			if (p != null)
			{
				p.sendPacket(pkt);
			}
		}
		
		return true;
	}
	
	public void sendBuildingList(Player player)
	{
		if (player == null || _buildings.isEmpty())
		{
			return;
		}
		
		List<RulerBuildingData> chunk = new ArrayList<>(CHUNK_SIZE);
		for (RulerBuildingData b : _buildings.values())
		{
			chunk.add(b);
			if (chunk.size() >= CHUNK_SIZE)
			{
				player.sendPacket(new RulerBuildingsPacket(chunk));
				chunk = new ArrayList<>(CHUNK_SIZE);
			}
		}
		
		if (!chunk.isEmpty())
		{
			player.sendPacket(new RulerBuildingsPacket(chunk));
		}
	}
	
	public Map<Integer, RulerBuildingData> getBuildings()
	{
		return _buildings;
	}
	
	public static RulerBuildingManager getInstance()
	{
		return SingletonHolder.INSTANCE;
	}
	
	private static class SingletonHolder
	{
		protected static final RulerBuildingManager INSTANCE = new RulerBuildingManager();
	}
}
