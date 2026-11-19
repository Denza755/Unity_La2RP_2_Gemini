package com.shnok.javaserver.gameserver.handler;

import com.shnok.javaserver.commons.logging.CLogger;
import com.shnok.javaserver.gameserver.model.actor.Player;

public interface IVoicedCommandHandler
{
	final CLogger LOGGER = new CLogger(IVoicedCommandHandler.class.getName());
	
	public boolean useVoicedCommand(String command, Player player, String target);
	
	public String[] getVoicedCommandList();
}
