package com.shnok.javaserver.gameserver.network.serverpackets;

import java.util.List;

import com.shnok.javaserver.gameserver.data.manager.RankingManager.RankingEntry;

public class RankingListPacket extends L2GameServerPacket
{
	private final int _category;
	private final List<RankingEntry> _entries;

	public RankingListPacket(int category, List<RankingEntry> entries)
	{
		_category = category;
		_entries = entries;
	}

	@Override
	protected void writeImpl()
	{
		writeC(0xF1);
		writeD(_category);
		writeD(_entries.size());

		for (RankingEntry entry : _entries)
		{
			writeD(entry.getRank());
			writeS(entry.getName());
			writeD(entry.getClassId());
			writeD(entry.getLevel());
			writeQ(entry.getValue());
			writeD(entry.isOnline() ? 1 : 0);
		}
	}
}
