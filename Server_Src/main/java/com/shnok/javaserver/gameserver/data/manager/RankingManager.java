package com.shnok.javaserver.gameserver.data.manager;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

import com.shnok.javaserver.commons.pool.ConnectionPool;
import com.shnok.javaserver.gameserver.model.World;

public class RankingManager
{
	public static class RankingEntry
	{
		private final int _rank;
		private final String _name;
		private final int _classId;
		private final int _level;
		private final long _value;
		private final boolean _isOnline;

		public RankingEntry(int rank, String name, int classId, int level, long value, boolean isOnline)
		{
			_rank = rank;
			_name = name;
			_classId = classId;
			_level = level;
			_value = value;
			_isOnline = isOnline;
		}

		public int getRank() { return _rank; }
		public String getName() { return _name; }
		public int getClassId() { return _classId; }
		public int getLevel() { return _level; }
		public long getValue() { return _value; }
		public boolean isOnline() { return _isOnline; }
	}

	private static final long CACHE_DURATION_MS = 60000; // 60 seconds

	private final Map<Integer, List<RankingEntry>> _cachedRankings = new ConcurrentHashMap<>();
	private final Map<Integer, Long> _lastUpdateTimes = new ConcurrentHashMap<>();

	protected RankingManager()
	{
	}

	public List<RankingEntry> getRanking(int category)
	{
		long now = System.currentTimeMillis();
		Long lastUpdate = _lastUpdateTimes.get(category);

		if (lastUpdate != null && (now - lastUpdate) < CACHE_DURATION_MS)
		{
			List<RankingEntry> cached = _cachedRankings.get(category);
			if (cached != null)
			{
				return cached;
			}
		}

		List<RankingEntry> list = loadRankingFromDb(category);
		_cachedRankings.put(category, list);
		_lastUpdateTimes.put(category, now);
		return list;
	}

	private List<RankingEntry> loadRankingFromDb(int category)
	{
		List<RankingEntry> list = new ArrayList<>();
		String query;

		switch (category)
		{
			case 1: // PvP
				query = "SELECT char_name, classid, level, pvpkills AS val, online FROM characters WHERE accesslevel=0 ORDER BY pvpkills DESC, level DESC LIMIT 20";
				break;
			case 2: // PK
				query = "SELECT char_name, classid, level, pkkills AS val, online FROM characters WHERE accesslevel=0 ORDER BY pkkills DESC, level DESC LIMIT 20";
				break;
			case 3: // Online Time
				query = "SELECT char_name, classid, level, onlinetime AS val, online FROM characters WHERE accesslevel=0 ORDER BY onlinetime DESC, level DESC LIMIT 20";
				break;
			case 0: // Level / Exp
			default:
				category = 0;
				query = "SELECT char_name, classid, level, exp AS val, online FROM characters WHERE accesslevel=0 ORDER BY level DESC, exp DESC LIMIT 20";
				break;
		}

		try (Connection con = ConnectionPool.getConnection();
			 PreparedStatement ps = con.prepareStatement(query);
			 ResultSet rs = ps.executeQuery())
		{
			int rank = 1;
			while (rs.next())
			{
				String name = rs.getString("char_name");
				int classId = rs.getInt("classid");
				int level = rs.getInt("level");
				long val = rs.getLong("val");
				boolean isOnline = rs.getInt("online") == 1 || (World.getInstance().getPlayer(name) != null);

				list.add(new RankingEntry(rank++, name, classId, level, val, isOnline));
			}
		}
		catch (Exception e)
		{
			e.printStackTrace();
		}

		return list;
	}

	public static RankingManager getInstance()
	{
		return SingletonHolder.INSTANCE;
	}

	private static class SingletonHolder
	{
		protected static final RankingManager INSTANCE = new RankingManager();
	}
}
