package com.shnok.javaserver.gameserver.network.serverpackets.ruler;

import java.util.Collection;
import java.util.Collections;

import com.shnok.javaserver.gameserver.model.ruler.RulerBuildingData;
import com.shnok.javaserver.gameserver.network.serverpackets.L2GameServerPacket;

public class RulerBuildingsPacket extends L2GameServerPacket
{
	private final int _mode; // 0 = chunk list, 1 = single add, 2 = single remove
	private final Collection<RulerBuildingData> _buildings;
	private final RulerBuildingData _singleBuilding;
	private final int _demolishedId;
	
	// Mode 0: Chunk list
	public RulerBuildingsPacket(Collection<RulerBuildingData> buildings)
	{
		_mode = 0;
		_buildings = buildings != null ? buildings : Collections.emptyList();
		_singleBuilding = null;
		_demolishedId = 0;
	}
	
	// Mode 1: Single building added
	public RulerBuildingsPacket(RulerBuildingData building)
	{
		_mode = 1;
		_buildings = Collections.emptyList();
		_singleBuilding = building;
		_demolishedId = 0;
	}
	
	// Mode 2: Single building demolished
	public RulerBuildingsPacket(int demolishedId)
	{
		_mode = 2;
		_buildings = Collections.emptyList();
		_singleBuilding = null;
		_demolishedId = demolishedId;
	}
	
	@Override
	protected void writeImpl()
	{
		writeC(0xed);
		writeC(_mode);
		
		if (_mode == 0)
		{
			writeH(_buildings.size());
			for (RulerBuildingData b : _buildings)
			{
				writeD(b.getId());
				writeS(b.getPrefabName());
				writeF(b.getPosX());
				writeF(b.getPosY());
				writeF(b.getPosZ());
				writeF(b.getRotY());
				writeF(b.getScale());
				writeS(b.getOwnerName() != null ? b.getOwnerName() : "");
			}
		}
		else if (_mode == 1)
		{
			if (_singleBuilding != null)
			{
				writeD(_singleBuilding.getId());
				writeS(_singleBuilding.getPrefabName());
				writeF(_singleBuilding.getPosX());
				writeF(_singleBuilding.getPosY());
				writeF(_singleBuilding.getPosZ());
				writeF(_singleBuilding.getRotY());
				writeF(_singleBuilding.getScale());
				writeS(_singleBuilding.getOwnerName() != null ? _singleBuilding.getOwnerName() : "");
			}
			else
			{
				writeD(0);
				writeS("");
				writeF(0);
				writeF(0);
				writeF(0);
				writeF(0);
				writeF(0);
				writeS("");
			}
		}
		else if (_mode == 2)
		{
			writeD(_demolishedId);
		}
	}
}
