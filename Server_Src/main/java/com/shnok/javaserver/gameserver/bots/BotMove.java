package com.shnok.javaserver.gameserver.bots;

import java.util.List;

import com.shnok.javaserver.commons.logging.CLogger;
import com.shnok.javaserver.commons.pool.ThreadPool;
import com.shnok.javaserver.gameserver.enums.AiEventType;
import com.shnok.javaserver.gameserver.enums.actors.MoveType;
import com.shnok.javaserver.gameserver.geoengine.GeoEngine;
import com.shnok.javaserver.gameserver.geoengine.geodata.GeoStructure;
import com.shnok.javaserver.gameserver.model.World;
import com.shnok.javaserver.gameserver.model.WorldObject;
import com.shnok.javaserver.gameserver.model.actor.Creature;
import com.shnok.javaserver.gameserver.model.actor.Npc;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.actor.move.PlayerMove;
import com.shnok.javaserver.gameserver.model.location.Location;
import com.shnok.javaserver.gameserver.network.serverpackets.movement.MoveToLocation;
import com.shnok.javaserver.gameserver.network.serverpackets.movement.StopMove;

/**
 * Server-side movement controller for FakePlayer bots.
 * Restores full GeoEngine pathfinding, 100ms ticker updates, and MoveToLocation / StopMove packet dispatch.
 */
public class BotMove extends PlayerMove
{
	private static final CLogger LOGGER = new CLogger(BotMove.class.getName());

	private static final int FOLLOW_INTERVAL = 1000;
	private static final int ATTACK_FOLLOW_INTERVAL = 500;

	public BotMove(Player actor)
	{
		super(actor);
	}

	@Override
	public void moveToLocation(Location destination, boolean pathfinding)
	{
		if (_task != null)
		{
			updatePosition(true);
		}

		final Location position = _actor.getPosition().clone();
		_xAccurate = position.getX();
		_yAccurate = position.getY();
		_geoPath.clear();

		// Check Z with Shnok GeoEngine
		final int groundZ = GeoEngine.getInstance().getHeight(destination.getX(), destination.getY(), destination.getZ());
		destination.setZ(groundZ);

		if (pathfinding)
		{
			final Location loc = calculatePath(position.getX(), position.getY(), position.getZ(), destination.getX(), destination.getY(), destination.getZ());
			if (loc != null)
			{
				destination.set(loc);
			}
		}

		_destination.set(destination);
		_actor.getPosition().setHeadingTo(destination);

		registerMoveTask();

		_actor.broadcastPacket(new MoveToLocation(_actor, destination));
	}

	public void moveToPawn(WorldObject pawn, int offset)
	{
		final int tx = pawn.getX();
		final int ty = pawn.getY();
		final int tz = pawn.getZ();

		_pawn = pawn;
		_offset = offset;

		if (_task != null)
		{
			updatePosition(true);
		}

		final Location position = _actor.getPosition().clone();
		_xAccurate = position.getX();
		_yAccurate = position.getY();
		_geoPath.clear();

		final Location destination = new Location(tx, ty, tz);
		final int groundZ = GeoEngine.getInstance().getHeight(tx, ty, tz);
		destination.setZ(groundZ);

		final Location loc = calculatePath(position.getX(), position.getY(), position.getZ(), destination.getX(), destination.getY(), destination.getZ());
		if (loc != null)
		{
			destination.set(loc);
		}

		_destination.set(destination);
		_actor.getPosition().setHeadingTo(destination);

		registerMoveTask();

		_actor.broadcastPacket(new MoveToLocation(_actor, destination));
	}

	@Override
	public void registerMoveTask()
	{
		if (_task != null)
		{
			return;
		}

		_blocked = false;

		_task = ThreadPool.scheduleAtFixedRate(() ->
		{
			if (updatePosition(false) && !moveToNextRoutePoint())
			{
				ThreadPool.execute(() ->
				{
					cancelMoveTask();

					_actor.revalidateZone(true);
					if (!_blocked)
					{
						_actor.getAI().notifyEvent(AiEventType.ARRIVED, null, null);
					}
					else
					{
						_actor.getAI().notifyEvent(AiEventType.ARRIVED_BLOCKED, null, null);
					}
				});
			}
		}, 100, 100);
	}

	@Override
	public boolean updatePosition(boolean firstRun)
	{
		if (_task == null || !_actor.isVisible())
		{
			return true;
		}

		if (_pawn != null && !_actor.knows(_pawn))
		{
			return true;
		}

		final MoveType type = getMoveType();

		final int curX = _actor.getX();
		final int curY = _actor.getY();
		final int curZ = _actor.getZ();

		if (type == MoveType.GROUND)
		{
			_destination.setZ(GeoEngine.getInstance().getHeight(_destination));
		}

		final double dx = _destination.getX() - _xAccurate;
		final double dy = _destination.getY() - _yAccurate;
		final double dz = _destination.getZ() - curZ;

		final double leftDistance = (type == MoveType.GROUND) ? Math.sqrt(dx * dx + dy * dy) : Math.sqrt(dx * dx + dy * dy + dz * dz);
		final double passedDistance = _actor.getStatus().getMoveSpeed() / 10.0;

		final int nextX;
		final int nextY;
		final int nextZ;

		if (passedDistance < leftDistance)
		{
			final double fraction = passedDistance / leftDistance;

			_xAccurate += dx * fraction;
			_yAccurate += dy * fraction;

			nextX = (int) Math.round(_xAccurate);
			nextY = (int) Math.round(_yAccurate);
			nextZ = Math.min((type == MoveType.GROUND) ? GeoEngine.getInstance().getHeight(nextX, nextY, curZ + 2 * GeoStructure.CELL_HEIGHT) : (curZ + (int) (dz * fraction + 0.5)), World.WORLD_Z_MAX);
		}
		else
		{
			nextX = _destination.getX();
			nextY = _destination.getY();
			nextZ = Math.min(_destination.getZ(), World.WORLD_Z_MAX);
		}

		if (type == MoveType.GROUND && !GeoEngine.getInstance().canMoveToTarget(curX, curY, curZ, nextX, nextY, nextZ))
		{
			_blocked = true;
			return true;
		}

		_actor.setXYZ(nextX, nextY, nextZ);
		_actor.revalidateZone(false);

		if (isOnLastPawnMoveGeoPath() && ((type == MoveType.GROUND) ? _actor.isIn2DRadius(_pawn, _offset) : _actor.isIn3DRadius(_pawn, _offset)))
		{
			return true;
		}

		return (passedDistance >= leftDistance);
	}

	@Override
	public boolean maybeMoveToLocation(Location destination, int offset, boolean pathfinding, boolean isShiftPressed)
	{
		if (_actor.isIn3DRadius(destination, offset))
		{
			return false;
		}

		if (!_actor.isMovementDisabled() && !isShiftPressed)
		{
			_pawn = null;
			_offset = 0;

			moveToLocation(destination, pathfinding);
		}

		return true;
	}

	@Override
	public boolean maybeMoveToPawn(WorldObject target, int offset, boolean isShiftPressed)
	{
		if (offset < 0 || _actor == target)
		{
			return false;
		}

		final int collisionTotal = (int) (offset + _actor.getCollisionRadius() + ((target instanceof Creature targetCreature) ? targetCreature.getCollisionRadius() : 0));
		if (_actor.isIn3DRadius(target, collisionTotal))
		{
			return false;
		}

		if (!_actor.isMovementDisabled() && !isShiftPressed)
		{
			_pawn = target;
			_offset = offset;

			moveToPawn(target, offset);
		}

		return true;
	}

	@Override
	public boolean maybeStartOffensiveFollow(Creature target, int weaponAttackRange)
	{
		if (weaponAttackRange < 0)
		{
			return false;
		}

		int totalRadius = (int) (weaponAttackRange + _actor.getCollisionRadius() + target.getCollisionRadius());
		if (_actor.isIn2DRadius(target, totalRadius))
		{
			if (!GeoEngine.getInstance().canSeeTarget(_actor, target) && !_actor.isMovementDisabled())
			{
				startOffensiveFollow(target, (int) target.getCollisionRadius());
				return true;
			}
			return false;
		}

		if (!_actor.isMovementDisabled())
		{
			startOffensiveFollow(target, weaponAttackRange);
			return true;
		}

		return false;
	}

	@Override
	public boolean maybeStartFriendlyFollow(Creature target, int range)
	{
		if (_actor.isMovementDisabled())
		{
			return false;
		}

		startFriendlyFollow(target, range);
		return true;
	}

	@Override
	public void startOffensiveFollow(Creature pawn, int offset)
	{
		cancelFollowTask();
		_followTask = ThreadPool.scheduleAtFixedRate(() -> offensiveFollowTask(pawn, offset), 5, ATTACK_FOLLOW_INTERVAL);
	}

	@Override
	public void startFriendlyFollow(Creature pawn, int offset)
	{
		cancelFollowTask();
		_followTask = ThreadPool.scheduleAtFixedRate(() -> friendlyFollowTask(pawn, offset), 5, FOLLOW_INTERVAL);
	}

	@Override
	protected void offensiveFollowTask(Creature target, int offset)
	{
		if (_followTask == null)
		{
			return;
		}

		if (!_actor.knows(target) || target.isDead())
		{
			cancelFollowTask();
			_actor.getAI().tryToIdle();
			return;
		}

		final Location destination = target.getPosition().clone();
		// Separation offset based on actor's objectId and collision radius to avoid stacking
		final double angle = Math.toRadians((_actor.getObjectId() * 59) % 360);
		final double sepDist = Math.max(20.0, _actor.getCollisionRadius() * 0.95);
		final int newX = destination.getX() + (int) (Math.cos(angle) * sepDist);
		final int newY = destination.getY() + (int) (Math.sin(angle) * sepDist);
		destination.set(newX, newY, GeoEngine.getInstance().getHeight(newX, newY, destination.getZ()));

		final int realOffset = (int) (offset + _actor.getCollisionRadius() + target.getCollisionRadius());

		if ((getMoveType() == MoveType.GROUND) ? _actor.isIn2DRadius(destination, realOffset) : _actor.isIn3DRadius(destination, realOffset))
		{
			return;
		}

		_pawn = target;
		_offset = realOffset;

		moveToLocation(destination, true);
	}

	@Override
	protected void friendlyFollowTask(Creature target, int offset)
	{
		if (_followTask == null)
		{
			return;
		}

		if (!_actor.knows(target) || target.isDead())
		{
			cancelFollowTask();
			_actor.getAI().tryToIdle();
			return;
		}

		final Location destination = target.getPosition().clone();
		// Separation offset based on actor's objectId and collision radius to avoid stacking
		final double angle = Math.toRadians((_actor.getObjectId() * 59) % 360);
		final double sepDist = Math.max(25.0, _actor.getCollisionRadius() * 1.1);
		final int newX = destination.getX() + (int) (Math.cos(angle) * sepDist);
		final int newY = destination.getY() + (int) (Math.sin(angle) * sepDist);
		destination.set(newX, newY, GeoEngine.getInstance().getHeight(newX, newY, destination.getZ()));

		final int realOffset = (int) (offset + _actor.getCollisionRadius() + target.getCollisionRadius());

		if ((getMoveType() == MoveType.GROUND) ? _actor.isIn2DRadius(destination, realOffset) : _actor.isIn3DRadius(destination, realOffset))
		{
			return;
		}

		_pawn = null;
		_offset = 0;

		moveToLocation(destination, true);
	}

	@Override
	public void stop()
	{
		if (_task == null && _followTask == null)
		{
			return;
		}

		cancelFollowTask();
		cancelMoveTask();

		_actor.revalidateZone(true);
		_actor.broadcastPacket(new StopMove(_actor));
	}
}
