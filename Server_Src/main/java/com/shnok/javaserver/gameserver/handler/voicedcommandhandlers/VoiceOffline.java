package com.shnok.javaserver.gameserver.handler.voicedcommandhandlers;

import com.shnok.javaserver.gameserver.handler.IVoicedCommandHandler;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.network.serverpackets.auth.ServerClose;

public class VoiceOffline implements IVoicedCommandHandler
{
	private static final String[] VOICED_COMMANDS =
	{
		"offline"
	};

	@Override
	public boolean useVoicedCommand(String command, Player player, String target)
	{
		if (command.equalsIgnoreCase("offline"))
		{
			if (player.isDead())
			{
				player.sendMessage("You cannot use offline store while dead.");
				return false;
			}

			if (!player.isInStoreMode())
			{
				player.sendMessage("You can only use .offline while running a private store (Sell/Buy).");
				return false;
			}

			player.setIsOfflineShop(true);
			player.store();
			player.sendMessage("Your private store is now in offline mode. Disconnecting...");

			if (player.getClient() != null)
			{
				player.getClient().close(ServerClose.STATIC_PACKET);
			}
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
