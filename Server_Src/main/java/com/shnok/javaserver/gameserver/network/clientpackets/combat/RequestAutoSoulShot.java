package com.shnok.javaserver.gameserver.network.clientpackets.combat;

import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.network.SystemMessageId;
import com.shnok.javaserver.gameserver.network.clientpackets.L2GameClientPacket;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.ExAutoSoulShot;
import com.shnok.javaserver.gameserver.network.serverpackets.SystemMessage;

public final class RequestAutoSoulShot extends L2GameClientPacket
{
	private int _itemId;
	private int _type; // 1 = on : 0 = off;
	
	@Override
	protected void readImpl()
	{
		_itemId = readD();
		_type = readD();
	}
	
	@Override
	protected void runImpl()
	{
		final Player player = getClient().getPlayer();
		if (player == null)
			return;
		
		if (!player.isOperating() && player.getActiveRequester() == null && !player.isDead())
		{
			// Lineage 2 C1 standard: Auto soulshot / spiritshot is disabled.
			// Automatically reset auto-shot mode on client and clear from active set.
			player.removeAutoSoulShot(_itemId);
			player.sendPacket(new ExAutoSoulShot(_itemId, 0));
		}
	}
}