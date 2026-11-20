package com.shnok.javaserver.gameserver.bots;

import com.shnok.javaserver.gameserver.geoengine.GeoEngine;
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
		final int realZ = GeoEngine.getInstance().getHeight(_center.getX(), _center.getY(), _center.getZ());
		_center.setZ(realZ);
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

	// Preset spots distributed across Talking Island sectors (17_25, 16_25, 17_24, 16_24)
	public static final BotSpot VILLAGE_TOWN = new BotSpot("TI Village Town", -84141, 244623, -3729, 600, 1, 20);
	public static final BotSpot KELTIR_PLAINS = new BotSpot("Keltir Plains (Lvl 1-4)", -86500, 239500, -3600, 1800, 1, 4);
	public static final BotSpot WOLF_FOREST = new BotSpot("Wolf Forest (Lvl 4-8)", -79500, 243500, -3500, 1800, 4, 8);
	public static final BotSpot ORC_HILLS = new BotSpot("Orc Hills (Lvl 6-10)", -88000, 250500, -3550, 1800, 6, 10);
	public static final BotSpot ORC_OUTPOST = ORC_HILLS;
	public static final BotSpot CEDRIC_GROUNDS = new BotSpot("Cedric Grounds (Lvl 8-12)", -71417, 258270, -3104, 1800, 8, 12);
	public static final BotSpot MAGIC_SCHOOL_VALLEY = new BotSpot("School of Magic Valley (Lvl 11-15)", -90875, 248162, -3570, 1800, 11, 15);
	public static final BotSpot ELVEN_RUINS_VALLEY = MAGIC_SCHOOL_VALLEY;
	public static final BotSpot OBELISK_PLAINS = new BotSpot("Obelisk Plains (Lvl 14-18)", -80000, 251000, -3450, 1800, 14, 18);
	public static final BotSpot OBELISK_HILLS = OBELISK_PLAINS;

	public static final BotSpot[] FARM_SPOTS =
	{
		KELTIR_PLAINS,
		WOLF_FOREST,
		ORC_HILLS,
		CEDRIC_GROUNDS,
		MAGIC_SCHOOL_VALLEY,
		OBELISK_PLAINS
	};
}
