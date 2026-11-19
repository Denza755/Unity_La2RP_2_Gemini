package com.shnok.javaserver.gameserver.handler;

public class VoicedCommandHandler extends AbstractHandler<Integer, IVoicedCommandHandler>
{
	protected VoicedCommandHandler()
	{
		super(IVoicedCommandHandler.class, "voicedcommandhandlers");
	}
	
	@Override
	protected void registerHandler(IVoicedCommandHandler handler)
	{
		for (String id : handler.getVoicedCommandList())
		{
			_entries.put(id.toLowerCase().hashCode(), handler);
		}
	}
	
	@Override
	public IVoicedCommandHandler getHandler(Object key)
	{
		if (!(key instanceof String voicedCommand))
		{
			return null;
		}
		
		final int index = voicedCommand.indexOf(" ");
		final String command = (index == -1) ? voicedCommand : voicedCommand.substring(0, index);
		
		return super.getHandler(command.toLowerCase().hashCode());
	}
	
	public static VoicedCommandHandler getInstance()
	{
		return SingletonHolder.INSTANCE;
	}
	
	private static class SingletonHolder
	{
		protected static final VoicedCommandHandler INSTANCE = new VoicedCommandHandler();
	}
}
