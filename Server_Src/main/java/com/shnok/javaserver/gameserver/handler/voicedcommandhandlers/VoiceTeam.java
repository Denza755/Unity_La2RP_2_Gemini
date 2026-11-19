package com.shnok.javaserver.gameserver.handler.voicedcommandhandlers;

import com.shnok.javaserver.gameserver.bots.CompanionManager;
import com.shnok.javaserver.gameserver.handler.IVoicedCommandHandler;
import com.shnok.javaserver.gameserver.model.actor.Player;

public class VoiceTeam implements IVoicedCommandHandler
{
	private static final String[] VOICED_COMMANDS =
	{
		"team"
	};
	
	@Override
	public boolean useVoicedCommand(String command, Player player, String target)
	{
		if (command.equalsIgnoreCase("team"))
		{
			if (player.isDead())
			{
				player.sendMessage("You cannot summon or dismiss companions while dead.");
				return false;
			}
			
			if (CompanionManager.getInstance().hasActiveTeam(player))
			{
				CompanionManager.getInstance().despawnTeam(player);
				return true;
			}
			
			if (player.isInCombat())
			{
				player.sendMessage("You cannot summon companions while in combat.");
				return false;
			}
			
			CompanionManager.getInstance().spawnTeam(player);
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
