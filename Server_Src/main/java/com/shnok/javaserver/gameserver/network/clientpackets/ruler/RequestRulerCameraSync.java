package com.shnok.javaserver.gameserver.network.clientpackets.ruler;

import com.shnok.javaserver.gameserver.model.World;
import com.shnok.javaserver.gameserver.model.WorldObject;
import com.shnok.javaserver.gameserver.model.WorldRegion;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.network.clientpackets.L2GameClientPacket;

public class RequestRulerCameraSync extends L2GameClientPacket
{
	private boolean _active;
	private int _l2X;
	private int _l2Y;
	private int _l2Z;
	
	@Override
	protected void readImpl()
	{
		_active = readC() != 0;
		_l2X = readD();
		_l2Y = readD();
		_l2Z = readD();
	}
	
	@Override
	protected void runImpl()
	{
		final Player player = getClient().getPlayer();
		if (player == null)
		{
			return;
		}
		
		if (!_active)
		{
			return;
		}
		
		final WorldRegion region = World.getInstance().getRegion(_l2X, _l2Y);
		if (region != null)
		{
			for (WorldRegion r : region.getSurroundingRegions())
			{
				if (r == null)
				{
					continue;
				}
				
				for (WorldObject obj : r.getObjects())
				{
					if (obj != null && obj != player)
					{
						obj.sendInfo(player);
					}
				}
			}
		}
	}
}
