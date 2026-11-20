package com.shnok.javaserver.gameserver.network.serverpackets.item;

import java.util.Set;

import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.model.item.kind.Item;
import com.shnok.javaserver.gameserver.network.serverpackets.L2GameServerPacket;

public class CompanionInventoryPacket extends L2GameServerPacket
{
	private final int _companionObjectId;
	private final String _companionName;
	private final int _adena;
	private final Set<ItemInstance> _items;

	public CompanionInventoryPacket(Player companion)
	{
		_companionObjectId = companion.getObjectId();
		_companionName = companion.getName();
		_adena = companion.getAdena();
		_items = companion.getInventory().getItems();
	}

	@Override
	protected final void writeImpl()
	{
		writeC(0xef);
		writeD(_companionObjectId);
		writeS(_companionName);
		writeD(_adena);
		writeH(_items.size());

		for (ItemInstance temp : _items)
		{
			Item item = temp.getItem();

			writeH(item.getType1());
			writeD(temp.getObjectId());
			writeD(temp.getItemId());
			writeD(temp.getCount());
			writeH(item.getType2());
			writeH(temp.getCustomType1());
			writeH(temp.isEquipped() ? 0x01 : 0x00);
			writeD(item.getBodyPart());
			writeH(temp.getEnchantLevel());
			writeH(temp.getCustomType2());
			writeD(temp.isAugmented() ? temp.getAugmentation().getId() : 0x00);
			writeD(temp.getDisplayedManaLeft());
			writeD(temp.getLocationSlot());
		}
	}
}
