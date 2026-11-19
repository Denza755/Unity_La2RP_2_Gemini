package com.shnok.javaserver.gameserver.bots;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ScheduledFuture;
import java.util.stream.Collectors;

import com.shnok.javaserver.commons.logging.CLogger;
import com.shnok.javaserver.commons.pool.ConnectionPool;
import com.shnok.javaserver.commons.pool.ThreadPool;
import com.shnok.javaserver.commons.random.Rnd;
import com.shnok.javaserver.gameserver.data.xml.RestartPointData;
import com.shnok.javaserver.gameserver.enums.LootRule;
import com.shnok.javaserver.gameserver.enums.MessageType;
import com.shnok.javaserver.gameserver.enums.RestartType;
import com.shnok.javaserver.gameserver.geoengine.GeoEngine;
import com.shnok.javaserver.gameserver.model.World;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.group.Party;
import com.shnok.javaserver.gameserver.model.location.Location;

/**
 * Manager for account companions (.team system).
 * Allows spawning up to 3 other characters from the same account controlled by CompanionAI.
 * Fully integrates with DB persistence (store), party distribution, and AI progression.
 */
public class CompanionManager
{
	private static final CLogger LOGGER = new CLogger(CompanionManager.class.getName());

	private final Map<Integer, List<Player>> _activeCompanions = new ConcurrentHashMap<>();
	private ScheduledFuture<?> _aiTask;

	public static CompanionManager getInstance()
	{
		return SingletonHolder.INSTANCE;
	}

	public void init()
	{
		_aiTask = ThreadPool.scheduleAtFixedRate(this::tickCompanions, 1000, 500);
		LOGGER.info("CompanionManager initialized. 500ms ticker active.");
	}

	public boolean hasActiveTeam(Player owner)
	{
		if (owner == null)
		{
			return false;
		}
		final List<Player> list = _activeCompanions.get(owner.getObjectId());
		return list != null && !list.isEmpty();
	}

	public List<Player> getActiveCompanions(Player owner)
	{
		if (owner == null)
		{
			return List.of();
		}
		return _activeCompanions.getOrDefault(owner.getObjectId(), List.of());
	}

	public void spawnTeam(Player owner)
	{
		if (owner == null || !owner.isOnline())
		{
			return;
		}

		if (hasActiveTeam(owner))
		{
			owner.sendMessage("You already have an active companion team. Use .team to dismiss them.");
			return;
		}

		// 1. Query other characters belonging to the same account
		final List<Integer> charIds = new ArrayList<>();
		try (Connection con = ConnectionPool.getConnection();
			PreparedStatement ps = con.prepareStatement(
				"SELECT obj_Id, char_name FROM characters WHERE account_name=? AND obj_Id<>? ORDER BY level DESC, exp DESC"))
		{
			ps.setString(1, owner.getAccountName());
			ps.setInt(2, owner.getObjectId());

			try (ResultSet rs = ps.executeQuery())
			{
				while (rs.next())
				{
					final int objId = rs.getInt("obj_Id");
					// Skip if character is already online/present in the world
					if (World.getInstance().getPlayer(objId) == null)
					{
						charIds.add(objId);
						if (charIds.size() >= 3)
						{
							break;
						}
					}
				}
			}
		}
		catch (Exception e)
		{
			LOGGER.error("Failed to query companions for account {}: {}", owner.getAccountName(), e.getMessage());
			owner.sendMessage("Database error while loading companions.");
			return;
		}

		if (charIds.isEmpty())
		{
			owner.sendMessage("No other available characters found on your account to summon as companions.");
			return;
		}

		// 2. Restore each character as a real Player from DB
		final List<Player> spawned = new ArrayList<>();
		final int count = charIds.size();

		for (int i = 0; i < count; i++)
		{
			final int objId = charIds.get(i);
			final Player companion = Player.restore(objId);
			if (companion == null)
			{
				continue;
			}

			companion.setIsCompanion(true);
			companion.setCompanionOwner(owner);
			companion.setAccessLevel(0);
			companion.setRunning(true);
			companion.setStanding(true);

			// Setup server movement engine and CompanionAI
			companion.setMove(new BotMove(companion));
			final CompanionAI ai = new CompanionAI(companion, owner);
			companion.setAI(ai);

			// Calculate offset around owner with valid ground height
			final double angle = i * (2.0 * Math.PI / count) + Rnd.nextDouble() * 0.3;
			final int dist = Rnd.get(60, 90);
			final int tx = (int) (owner.getX() + dist * Math.cos(angle));
			final int ty = (int) (owner.getY() + dist * Math.sin(angle));
			final int tz = GeoEngine.getInstance().getHeight(tx, ty, owner.getZ());

			companion.spawnMe(tx, ty, tz, owner.getHeading());
			companion.broadcastUserInfo();
			spawned.add(companion);
		}

		if (spawned.isEmpty())
		{
			owner.sendMessage("Failed to restore companions.");
			return;
		}

		// 3. Automatically unite owner and summoned companions into a single Party
		if (!owner.isInParty())
		{
			final Party party = new Party(owner, spawned.get(0), LootRule.ITEM_LOOTER);
			for (int i = 1; i < spawned.size(); i++)
			{
				party.addPartyMember(spawned.get(i));
			}
		}
		else
		{
			final Party party = owner.getParty();
			for (Player c : spawned)
			{
				if (party.getMembersCount() < 9)
				{
					party.addPartyMember(c);
				}
			}
		}

		_activeCompanions.put(owner.getObjectId(), spawned);

		final String names = spawned.stream().map(Player::getName).collect(Collectors.joining(", "));
		owner.sendMessage("Summoned " + spawned.size() + " companion(s) from your account: " + names + ".");
	}

	public void despawnTeam(Player owner)
	{
		if (owner == null)
		{
			return;
		}

		final List<Player> list = _activeCompanions.remove(owner.getObjectId());
		if (list == null || list.isEmpty())
		{
			return;
		}

		for (Player companion : list)
		{
			try
			{
				if (companion.isInParty())
				{
					companion.getParty().removePartyMember(companion, MessageType.LEFT);
				}

				companion.store();
				companion.deleteMe();
			}
			catch (Exception e)
			{
				LOGGER.error("Error despawning companion {}: {}", companion.getName(), e.getMessage());
			}
		}

		owner.sendMessage("Your companion team has been saved and dismissed.");
	}

	public void onCompanionDied(Player companion, Player owner)
	{
		if (companion == null)
		{
			return;
		}

		try
		{
			// Safe respawn coordinates: nearest town
			final Location townLoc = RestartPointData.getInstance().getLocationToTeleport(companion, RestartType.TOWN);
			if (townLoc != null)
			{
				companion.getPosition().set(townLoc.getX(), townLoc.getY(), townLoc.getZ());
			}
			companion.getStatus().setHp(companion.getStatus().getMaxHp() * 0.7);
			companion.getStatus().setMp(companion.getStatus().getMaxMp() * 0.5);
			companion.setIsDead(false);

			// Persist state to DB
			companion.store();

			if (companion.isInParty())
			{
				companion.getParty().removePartyMember(companion, MessageType.LEFT);
			}

			companion.deleteMe();

			if (owner != null)
			{
				final List<Player> list = _activeCompanions.get(owner.getObjectId());
				if (list != null)
				{
					list.remove(companion);
					if (list.isEmpty())
					{
						_activeCompanions.remove(owner.getObjectId());
					}
				}
				owner.sendMessage("Companion " + companion.getName() + " died and returned to town.");
			}
		}
		catch (Exception e)
		{
			LOGGER.error("Error processing death for companion {}: {}", companion.getName(), e.getMessage());
		}
	}

	private void tickCompanions()
	{
		for (List<Player> list : _activeCompanions.values())
		{
			for (Player companion : list)
			{
				if (companion != null && companion.getAI() instanceof CompanionAI cai)
				{
					try
					{
						cai.think();
					}
					catch (Exception e)
					{
						LOGGER.error("Error ticking companion AI for {}: {}", companion.getName(), e.getMessage());
					}
				}
			}
		}
	}

	private static class SingletonHolder
	{
		protected static final CompanionManager INSTANCE = new CompanionManager();
	}
}
