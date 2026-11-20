package com.shnok.javaserver.gameserver.network.serverpackets;

import java.util.ArrayList;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;

import com.shnok.javaserver.gameserver.model.actor.Attackable;
import com.shnok.javaserver.gameserver.model.item.DropCategory;
import com.shnok.javaserver.gameserver.model.item.DropData;

public class NpcDropListPacket extends L2GameServerPacket
{
	private final int _npcId;
	private final String _npcName;
	private final List<Integer> _itemIds;

	public NpcDropListPacket(Attackable attackable)
	{
		_npcId = attackable.getNpcId();
		_npcName = attackable.getName();
		
		Set<Integer> uniqueItems = new LinkedHashSet<>();
		List<DropCategory> categories = attackable.getTemplate().getDropData();
		if (categories != null)
		{
			for (DropCategory cat : categories)
			{
				if (cat != null)
				{
					for (DropData drop : cat)
					{
						if (drop != null)
						{
							uniqueItems.add(drop.itemId());
						}
					}
				}
			}
		}
		_itemIds = new ArrayList<>(uniqueItems);
	}

	@Override
	protected final void writeImpl()
	{
		writeC(0xf0);
		writeD(_npcId);
		writeS(_npcName);
		writeD(_itemIds.size());
		for (int itemId : _itemIds)
		{
			writeD(itemId);
		}
	}
}
