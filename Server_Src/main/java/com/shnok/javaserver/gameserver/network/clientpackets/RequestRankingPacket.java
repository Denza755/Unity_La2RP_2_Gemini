package com.shnok.javaserver.gameserver.network.clientpackets;

import java.util.List;

import com.shnok.javaserver.gameserver.data.manager.RankingManager;
import com.shnok.javaserver.gameserver.data.manager.RankingManager.RankingEntry;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.network.serverpackets.RankingListPacket;

public final class RequestRankingPacket extends L2GameClientPacket
{
	private int _category;

	@Override
	protected void readImpl()
	{
		_category = readD();
	}

	@Override
	protected void runImpl()
	{
		final Player player = getClient().getPlayer();
		if (player == null)
		{
			return;
		}

		List<RankingEntry> entries = RankingManager.getInstance().getRanking(_category);
		player.sendPacket(new RankingListPacket(_category, entries));
	}
}
