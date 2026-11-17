package com.shnok.javaserver.gameserver.network.clientpackets.ruler;

import com.shnok.javaserver.gameserver.data.manager.RulerBuildingManager;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.network.clientpackets.L2GameClientPacket;

public class RequestRulerBuildAction extends L2GameClientPacket
{
	private int _action; // 1 = build, 2 = demolish
	private String _prefabName;
	private double _posX;
	private double _posY;
	private double _posZ;
	private float _rotY;
	private float _scale;
	private int _buildingId;
	
	@Override
	protected void readImpl()
	{
		_action = readC();
		if (_action == 1) // Build
		{
			_prefabName = readS();
			_posX = readF();
			_posY = readF();
			_posZ = readF();
			_rotY = (float) readF();
			_scale = (float) readF();
		}
		else if (_action == 2) // Demolish
		{
			_buildingId = readD();
		}
	}
	
	@Override
	protected void runImpl()
	{
		final Player player = getClient().getPlayer();
		if (player == null)
		{
			return;
		}
		
		if (!RulerBuildingManager.ALLOW_ALL_PLAYERS_BUILD && !player.isGM())
		{
			LOGGER.warn("Player {} tried to perform ruler build action {} without permission.", player.getName(), _action);
			return;
		}
		
		if (_action == 1)
		{
			if (_prefabName == null || _prefabName.trim().isEmpty())
			{
				return;
			}
			RulerBuildingManager.getInstance().addBuilding(_prefabName, _posX, _posY, _posZ, _rotY, _scale, player.getName());
		}
		else if (_action == 2)
		{
			if (_buildingId <= 0)
			{
				return;
			}
			RulerBuildingManager.getInstance().removeBuilding(_buildingId);
		}
	}
}
