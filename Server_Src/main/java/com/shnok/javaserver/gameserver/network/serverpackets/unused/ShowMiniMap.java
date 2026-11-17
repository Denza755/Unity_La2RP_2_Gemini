package com.shnok.javaserver.gameserver.network.serverpackets.unused;

import com.shnok.javaserver.gameserver.data.manager.SevenSignsManager;
import com.shnok.javaserver.gameserver.network.serverpackets.L2GameServerPacket;

public class ShowMiniMap extends L2GameServerPacket
{
	public static final ShowMiniMap REGULAR_MAP = new ShowMiniMap(1665);
	
	private final int _mapId;
	
	public ShowMiniMap(int mapId)
	{
		_mapId = mapId;
	}
	
	@Override
	protected final void writeImpl()
	{
		writeC(0x9d);
		writeD(_mapId);
		
		int period = 0;
		try
		{
			SevenSignsManager mgr = SevenSignsManager.getInstance();
			if (mgr != null && mgr.getCurrentPeriod() != null)
				period = mgr.getCurrentPeriod().ordinal();
		}
		catch (Exception e)
		{
			LOGGER.warn("ShowMiniMap: failed to get SevenSigns period, defaulting to 0. {}", e.getMessage());
		}
		writeD(period);
	}
}