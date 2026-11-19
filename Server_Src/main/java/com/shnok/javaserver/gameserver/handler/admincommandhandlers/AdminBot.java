package com.shnok.javaserver.gameserver.handler.admincommandhandlers;

import java.util.StringTokenizer;

import com.shnok.javaserver.gameserver.bots.BotManager;
import com.shnok.javaserver.gameserver.bots.BotRole;
import com.shnok.javaserver.gameserver.bots.BotSpot;
import com.shnok.javaserver.gameserver.handler.IAdminCommandHandler;
import com.shnok.javaserver.gameserver.model.actor.Player;

public class AdminBot implements IAdminCommandHandler
{
	private static final String[] ADMIN_COMMANDS =
	{
		"admin_bot_status",
		"admin_bot_spawn",
		"admin_bot_despawn_all"
	};

	@Override
	public void useAdminCommand(String command, Player player)
	{
		final StringTokenizer st = new StringTokenizer(command);
		final String actualCommand = st.nextToken();

		switch (actualCommand)
		{
			case "admin_bot_status":
			{
				final int total = BotManager.getInstance().getTotalCount();
				final int hot = BotManager.getInstance().getHotCount();
				final int cold = BotManager.getInstance().getColdCount();
				final int stat = BotManager.getInstance().getStaticCount();

				player.sendMessage("=== Fake Players Status ===");
				player.sendMessage(String.format("Total: %d | HOT: %d | COLD: %d | STATIC: %d", total, hot, cold, stat));
				player.sendMessage("Active Farming Spots: Keltir Plains, Wolf Forest, Orc Outpost, Cedric Grounds, Obelisk Hills, Elven Ruins Valley.");
				player.sendMessage("Trading Hub: Talking Island Village Square (8 Traders).");
				break;
			}

			case "admin_bot_spawn":
			{
				int count = 1;
				if (st.hasMoreTokens())
				{
					try
					{
						count = Integer.parseInt(st.nextToken());
					}
					catch (NumberFormatException e)
					{
						count = 1;
					}
				}

				for (int i = 0; i < count; i++)
				{
					BotManager.getInstance().spawnBot(BotRole.MELEE_DD, BotSpot.KELTIR_PLAINS, 3);
				}
				player.sendMessage("Spawned " + count + " bot(s) at Keltir Plains.");
				break;
			}

			case "admin_bot_despawn_all":
			{
				BotManager.getInstance().despawnAll();
				player.sendMessage("All fake player bots have been despawned.");
				break;
			}
		}
	}

	@Override
	public String[] getAdminCommandList()
	{
		return ADMIN_COMMANDS;
	}
}
