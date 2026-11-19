package com.shnok.javaserver.gameserver.bots;

import com.shnok.javaserver.gameserver.model.location.Location;

public class BotSpot
{
	private final String _name;
	private final Location _center;
	private final int _radius;
	private final int _minLevel;
	private final int _maxLevel;

	public BotSpot(String name, int x, int y, int z, int radius, int minLevel, int maxLevel)
	{
		_name = name;
		_center = new Location(x, y, z);
		_radius = radius;
		_minLevel = minLevel;
		_maxLevel = maxLevel;
	}

	public String getName()
	{
		return _name;
	}

	public Location getCenter()
	{
		return _center;
	}

	public int getRadius()
	{
		return _radius;
	}

	public int getMinLevel()
	{
		return _minLevel;
	}

	public int getMaxLevel()
	{
		return _maxLevel;
	}

	// Preset spots strictly within Talking Island (16_24, 16_25, 17_24, 17_25)
	public static final BotSpot VILLAGE_TOWN = new BotSpot("TI Village Town", -84200, 243000, -3729, 600, 1, 20);
	public static final BotSpot KELTIR_PLAINS = new BotSpot("Keltir Plains (Lvl 1-4)", -84300, 240500, -3720, 1500, 1, 4);
	public static final BotSpot WOLF_FOREST = new BotSpot("Wolf Forest (Lvl 4-8)", -81800, 243500, -3720, 1500, 4, 8);
	public static final BotSpot ORC_OUTPOST = new BotSpot("Orc Outpost (Lvl 6-10)", -86500, 243000, -3720, 1500, 6, 10);
	public static final BotSpot CEDRIC_GROUNDS = new BotSpot("Cedric Grounds (Lvl 8-12)", -84500, 245500, -3720, 1600, 8, 12);
	public static final BotSpot OBELISK_HILLS = new BotSpot("Obelisk Spiders (Lvl 11-15)", -84000, 247000, -3600, 1800, 11, 15);
	public static final BotSpot ELVEN_RUINS_VALLEY = new BotSpot("Elven Ruins Valley (Lvl 14-18)", -90500, 248000, -3560, 1800, 14, 18);

	public static final BotSpot[] FARM_SPOTS =
	{
		KELTIR_PLAINS,
		WOLF_FOREST,
		ORC_OUTPOST,
		CEDRIC_GROUNDS,
		OBELISK_HILLS,
		ELVEN_RUINS_VALLEY
	};
}
