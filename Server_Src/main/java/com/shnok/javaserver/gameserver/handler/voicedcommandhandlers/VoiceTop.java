package com.shnok.javaserver.gameserver.handler.voicedcommandhandlers;

import java.util.List;

import com.shnok.javaserver.gameserver.data.manager.RankingManager;
import com.shnok.javaserver.gameserver.data.manager.RankingManager.RankingEntry;
import com.shnok.javaserver.gameserver.handler.IVoicedCommandHandler;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.network.serverpackets.RankingListPacket;

public class VoiceTop implements IVoicedCommandHandler
{
	private static final String[] VOICED_COMMANDS =
	{
		"top",
		"ranking"
	};

	@Override
	public boolean useVoicedCommand(String command, Player player, String target)
	{
		if (command.equalsIgnoreCase("top") || command.equalsIgnoreCase("ranking"))
		{
			List<RankingEntry> entries = RankingManager.getInstance().getRanking(0);
			player.sendPacket(new RankingListPacket(0, entries));
			return true;
		}
		return false;
	}

	@Override
	public String[] getVoicedCommandList()
	{
		return VOICED_COMMANDS;
	}
}
