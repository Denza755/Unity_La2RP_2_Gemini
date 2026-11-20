package com.shnok.javaserver.gameserver.handler.voicedcommandhandlers;

import com.shnok.javaserver.gameserver.handler.IVoicedCommandHandler;
import com.shnok.javaserver.gameserver.model.WorldObject;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.network.serverpackets.item.CompanionInventoryPacket;

public class VoiceTrade implements IVoicedCommandHandler
{
	private static final String[] VOICED_COMMANDS =
	{
		"trade"
	};

	@Override
	public boolean useVoicedCommand(String command, Player player, String target)
	{
		if (command.equalsIgnoreCase("trade"))
		{
			if (player.isDead())
			{
				player.sendMessage("You cannot trade while dead.");
				return false;
			}

			final WorldObject tgt = player.getTarget();
			if (!(tgt instanceof Player targetPlayer) || !targetPlayer.isCompanion() || targetPlayer.getCompanionOwner() != player)
			{
				player.sendMessage("You must target your companion to manage inventory.");
				return false;
			}

			player.sendPacket(new CompanionInventoryPacket(targetPlayer));
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
