package com.shnok.javaserver.gameserver.network.clientpackets.item;

import com.shnok.javaserver.gameserver.model.WorldObject;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.network.clientpackets.L2GameClientPacket;
import com.shnok.javaserver.gameserver.network.serverpackets.item.CompanionInventoryPacket;
import com.shnok.javaserver.gameserver.network.serverpackets.item.ItemList;

public class RequestCompanionItemAction extends L2GameClientPacket
{
	private int _action; // 0 = Player -> Companion, 1 = Companion -> Player, 2 = Equip/Unequip on Companion
	private int _companionObjectId;
	private int _itemObjectId;
	private int _count;

	@Override
	protected void readImpl()
	{
		_action = readC();
		_companionObjectId = readD();
		_itemObjectId = readD();
		_count = readD();
	}

	@Override
	protected void runImpl()
	{
		final Player player = getClient().getPlayer();
		if (player == null || player.isDead())
		{
			return;
		}

		final WorldObject target = player.getTarget();
		if (!(target instanceof Player companion) || !companion.isCompanion() || companion.getCompanionOwner() != player || companion.getObjectId() != _companionObjectId)
		{
			return;
		}

		switch (_action)
		{
			case 0: // Transfer from Player to Companion
			{
				final ItemInstance item = player.getInventory().getItemByObjectId(_itemObjectId);
				if (item == null || item.isEquipped() || _count <= 0)
				{
					return;
				}
				final int transferCount = Math.min(_count, item.getCount());
				player.transferItem(_itemObjectId, transferCount, companion);
				break;
			}
			case 1: // Transfer from Companion to Player
			{
				final ItemInstance item = companion.getInventory().getItemByObjectId(_itemObjectId);
				if (item == null || _count <= 0)
				{
					return;
				}
				if (item.isEquipped())
				{
					companion.getInventory().unequipItemInSlot(item.getLocationSlot());
				}
				final int transferCount = Math.min(_count, item.getCount());
				companion.transferItem(_itemObjectId, transferCount, player);
				companion.broadcastUserInfo();
				break;
			}
			case 2: // Equip / Unequip on Companion
			{
				final ItemInstance item = companion.getInventory().getItemByObjectId(_itemObjectId);
				if (item == null || !item.isEquipable())
				{
					return;
				}
				companion.useEquippableItem(item, false);
				companion.broadcastUserInfo();
				break;
			}
		}

		// Refresh inventories for both windows
		player.sendPacket(new ItemList(player, false));
		player.sendPacket(new CompanionInventoryPacket(companion));
	}
}
