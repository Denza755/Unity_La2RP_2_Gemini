package com.shnok.javaserver.gameserver.bots;

import java.util.List;

import com.shnok.javaserver.commons.logging.CLogger;
import com.shnok.javaserver.commons.random.Rnd;
import com.shnok.javaserver.gameserver.data.SkillTable;
import com.shnok.javaserver.gameserver.enums.SayType;
import com.shnok.javaserver.gameserver.enums.ZoneId;
import com.shnok.javaserver.gameserver.enums.items.WeaponType;
import com.shnok.javaserver.gameserver.geoengine.GeoEngine;
import com.shnok.javaserver.gameserver.model.WorldObject;
import com.shnok.javaserver.gameserver.model.actor.Creature;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.actor.ai.type.PlayerAI;
import com.shnok.javaserver.gameserver.model.actor.instance.Monster;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.model.location.Location;
import com.shnok.javaserver.gameserver.network.serverpackets.actor.DeleteObject;
import com.shnok.javaserver.gameserver.skills.L2Skill;

public class HotBotAI extends PlayerAI
{
	private static final CLogger LOGGER = new CLogger(HotBotAI.class.getName());
	private static int _loggedMoveCount = 0;

	private final FakePlayer _bot;
	private long _lastPatrolTime = 0;
	private long _lastPotionTime = 0;
	private long _castStartTime = 0;
	private long _attackStartTime = 0;

	public HotBotAI(FakePlayer bot)
	{
		super(bot);
		_bot = bot;
	}

	private void logMoveIfNeeded(String action, int tx, int ty, int tz)
	{
		if (_loggedMoveCount < 10)
		{
			_loggedMoveCount++;
			LOGGER.info("[BotAI] {} is {} towards ({}, {}, {}) from ({}, {}, {})",
				_bot.getName(), action, tx, ty, tz, _bot.getX(), _bot.getY(), _bot.getZ());
		}
	}

	public void think()
	{
		// 1. DEAD CHECK & RESPAWN
		if (_bot.isDead())
		{
			handleDeath();
			return;
		}

		// Watchdog for stuck casting / attacking states
		final long now = System.currentTimeMillis();
		if (_bot.getCast().isCastingNow())
		{
			if (_castStartTime == 0)
			{
				_castStartTime = now;
			}
			else if (now - _castStartTime > 10000)
			{
				_bot.getCast().stop();
				doIdleIntention();
				_castStartTime = 0;
			}
		}
		else
		{
			_castStartTime = 0;
		}

		if (_bot.getAttack().isAttackingNow())
		{
			if (_attackStartTime == 0)
			{
				_attackStartTime = now;
			}
			else if (now - _attackStartTime > 8000)
			{
				_bot.getAttack().stop();
				doIdleIntention();
				_attackStartTime = 0;
			}
		}
		else
		{
			_attackStartTime = 0;
		}

		// 2. STATIC TRADER
		if (_bot.getSimulationMode() == BotSimulationMode.STATIC || _bot.getRole() == BotRole.TRADER)
		{
			return;
		}

		// 3. RESTING RECOVERY (HP/MP)
		if (_bot.isResting())
		{
			handleResting();
			return;
		}

		// 3.1. TARGET DISENGAGEMENT & VALIDATION (PVP / PEACE / RANGE)
		final WorldObject currentTarget = _bot.getTarget();
		if (currentTarget instanceof Player targetPlayer)
		{
			if (targetPlayer.isDead()
				|| !targetPlayer.isVisible()
				|| !targetPlayer.isOnline()
				|| _bot.isInsideZone(ZoneId.PEACE)
				|| targetPlayer.isInsideZone(ZoneId.PEACE)
				|| _bot.distance2D(targetPlayer) > 1200)
			{
				_bot.setTarget(null);
				_bot.getAttack().stop();
				_bot.getCast().stop();
				_bot.getMove().stop();
				_bot.setBotState(BotState.RETURNING_TO_SPOT);

				if (targetPlayer.isDead() && Rnd.get(100) < 40)
				{
					final String[] PVP_WIN_CHAT = { "Good fight!", "gf", "Nice try!", "You need more practice :)" };
					_bot.sayInChat(Rnd.get(PVP_WIN_CHAT), SayType.ALL);
				}
				return;
			}

			// Active PvP combat with player target
			performCombatAction(targetPlayer);
			return;
		}

		// 4. RETURNING TO SPOT
		if (_bot.getBotState() == BotState.RETURNING_TO_SPOT)
		{
			handleReturningToSpot();
			return;
		}

		// 5. RESTOCKING CHECK
		if (!_bot.isInParty() && _bot.needsRestock())
		{
			_bot.performRestock();
			return;
		}

		// 6. LOW HEALTH / MANA SAFETY REST
		if (!_bot.getAttack().isAttackingNow() && !_bot.getCast().isCastingNow())
		{
			final double hpRatio = _bot.getStatus().getHpRatio();
			final double mpRatio = _bot.getStatus().getMpRatio();

			if (hpRatio < 0.25 || (mpRatio < 0.20 && _bot.getRole() == BotRole.MAGE))
			{
				_bot.setResting(true);
				_bot.setBotState(BotState.RESTING);
				tryToSit(null);
				return;
			}
		}

		// 7. PARTY ASSIST / FOLLOW (Stage 2)
		final Player followTarget = _bot.getFollowTarget();
		if (followTarget != null && followTarget.isOnline() && !followTarget.isDead() && _bot.isInParty() && _bot.getParty().containsPlayer(followTarget))
		{
			if (handlePartyBehavior(followTarget))
			{
				return;
			}
		}
		else if (_bot.getFollowTarget() != null)
		{
			_bot.setFollowTarget(null);
			_bot.setPartyAssistant(false);
			_bot.setBotState(BotState.RETURNING_TO_SPOT);
		}

		// 8. FARMING CYCLE
		handleFarming();
	}

	private void handleDeath()
	{
		if (_bot.getDeathTimestamp() == 0)
		{
			_bot.setDeathTimestamp(System.currentTimeMillis());
			return;
		}

		// Revive after 6 seconds of death
		if (System.currentTimeMillis() - _bot.getDeathTimestamp() >= 6000)
		{
			_bot.setDeathTimestamp(0);
			_bot.broadcastPacket(new DeleteObject(_bot));
			_bot.doRevive(100.0);
			_bot.getStatus().setHp(_bot.getStatus().getMaxHp());
			_bot.getStatus().setMp(_bot.getStatus().getMaxMp());

			// Respawn strictly in Talking Island Village
			_bot.teleportTo(BotSpot.VILLAGE_TOWN.getCenter(), 80);
			_bot.setBotState(BotState.RETURNING_TO_SPOT);
		}
	}

	private void handleResting()
	{
		if (!_bot.isSitting())
		{
			tryToSit(null);
			return;
		}

		final double hpRatio = _bot.getStatus().getHpRatio();
		final double mpRatio = _bot.getStatus().getMpRatio();

		if (hpRatio >= 0.85 && mpRatio >= 0.80)
		{
			tryToStand();
			_bot.setResting(false);
			_bot.setBotState(BotState.FARMING);
		}
	}

	private void handleReturningToSpot()
	{
		BotSpot spot = _bot.getAssignedSpot();
		if (spot == null)
		{
			spot = BotSpot.KELTIR_PLAINS;
			_bot.setAssignedSpot(spot);
		}

		final Location center = spot.getCenter();
		final double dist = _bot.distance2D(center);
		if (dist > spot.getRadius())
		{
			if (dist > 5000)
			{
				_bot.teleportTo(center, 50);
			}
			else
			{
				logMoveIfNeeded("returning to spot " + spot.getName(), center.getX(), center.getY(), center.getZ());
				_bot.getMove().maybeMoveToLocation(center, 0, true, false);
			}
		}
		else
		{
			_bot.setBotState(BotState.FARMING);
		}
	}

	private boolean handlePartyBehavior(Player leader)
	{
		// Healer in party priorities
		if (_bot.getRole() == BotRole.HEALER)
		{
			// Check leader HP
			if (leader.getStatus().getHpRatio() < 0.70)
			{
				_bot.autoUseShots(true);
				tryToCast(leader, 1011, getSkillLevel(1011)); // Heal
				return true;
			}

			// Check own HP
			if (_bot.getStatus().getHpRatio() < 0.60)
			{
				_bot.autoUseShots(true);
				tryToCast(_bot, 1011, getSkillLevel(1011)); // Heal
				return true;
			}

			// Buff leader with Shield or Might if missing
			if (leader.getFirstEffect(1040) == null && hasSkill(1040))
			{
				_bot.autoUseShots(true);
				tryToCast(leader, 1040, 1);
				return true;
			}
			if (leader.getFirstEffect(1068) == null && hasSkill(1068))
			{
				_bot.autoUseShots(true);
				tryToCast(leader, 1068, 1);
				return true;
			}
		}

		// Assist leader target
		final WorldObject leaderTarget = leader.getTarget();
		if (leaderTarget instanceof Creature && !((Creature) leaderTarget).isDead() && ((Creature) leaderTarget).isVisible())
		{
			_bot.setTarget(leaderTarget);
			performCombatAction((Creature) leaderTarget);
			return true;
		}

		// Follow leader if far
		if (_bot.distance2D(leader) > 250)
		{
			logMoveIfNeeded("following party leader " + leader.getName(), leader.getX(), leader.getY(), leader.getZ());
			_bot.getMove().maybeStartFriendlyFollow(leader, 120);
			return true;
		}

		return false;
	}

	private void handleFarming()
	{
		// 1. Pick up drops nearby
		if (!_bot.getAttack().isAttackingNow() && !_bot.getCast().isCastingNow())
		{
			final List<ItemInstance> drops = _bot.getKnownTypeInRadius(ItemInstance.class, 350);
			if (!drops.isEmpty())
			{
				for (ItemInstance item : drops)
				{
					if (item != null && item.isVisible() && GeoEngine.getInstance().canSeeTarget(_bot, item))
					{
						tryToPickUp(item.getObjectId(), false);
						return;
					}
				}
			}
		}

		// 2. Check / find target
		WorldObject target = _bot.getTarget();
		if (target == null || !(target instanceof Creature) || ((Creature) target).isDead() || !((Creature) target).isVisible())
		{
			target = findNearestMonster();
			if (target != null)
			{
				_bot.setTarget(target);
			}
		}

		if (target == null)
		{
			// Patrol / Random walk within spot
			handlePatrol();
			return;
		}

		// 3. Perform attack or cast
		performCombatAction((Creature) target);
	}

	private void handlePatrol()
	{
		// Don't patrol if already moving
		if (_bot.isMoving())
		{
			return;
		}

		// Wait 3 to 7 seconds between patrol movements
		final long now = System.currentTimeMillis();
		if (now - _lastPatrolTime < Rnd.get(3000, 7000))
		{
			return;
		}
		_lastPatrolTime = now;

		BotSpot spot = _bot.getAssignedSpot();
		if (spot == null)
		{
			spot = BotSpot.KELTIR_PLAINS;
			_bot.setAssignedSpot(spot);
		}

		final int curX = _bot.getX();
		final int curY = _bot.getY();
		final int curZ = _bot.getZ();

		// If drifted outside spot radius, run back towards spot center
		if (_bot.distance2D(spot.getCenter()) > spot.getRadius())
		{
			final int tx = spot.getCenter().getX() + Rnd.get(-150, 150);
			final int ty = spot.getCenter().getY() + Rnd.get(-150, 150);
			final int tz = GeoEngine.getInstance().getHeight(tx, ty, spot.getCenter().getZ());
			logMoveIfNeeded("patrolling back to " + spot.getName(), tx, ty, tz);
			_bot.getMove().maybeMoveToLocation(new Location(tx, ty, tz), 0, true, false);
			return;
		}

		// Pick a random walk point within 300..800 units, verified by GeoEngine
		for (int attempts = 0; attempts < 6; attempts++)
		{
			final double angle = Rnd.nextDouble() * 2 * Math.PI;
			final int dist = Rnd.get(300, 800);
			final int tx = (int) (curX + dist * Math.cos(angle));
			final int ty = (int) (curY + dist * Math.sin(angle));
			final int tz = GeoEngine.getInstance().getHeight(tx, ty, curZ);

			if (spot.getCenter().distance2D(new Location(tx, ty, tz)) <= spot.getRadius()
				&& Math.abs(tz - curZ) < 250
				&& GeoEngine.getInstance().canMoveToTarget(curX, curY, curZ, tx, ty, tz))
			{
				logMoveIfNeeded("patrolling random spot", tx, ty, tz);
				_bot.getMove().maybeMoveToLocation(new Location(tx, ty, tz), 0, true, false);
				break;
			}
		}
	}

	private Creature findNearestMonster()
	{
		BotSpot spot = _bot.getAssignedSpot();
		if (spot == null)
		{
			spot = BotSpot.KELTIR_PLAINS;
			_bot.setAssignedSpot(spot);
		}

		final int searchRadius = Math.max(1200, spot.getRadius());
		final List<Monster> mobs = _bot.getKnownTypeInRadius(Monster.class, searchRadius,
			m -> !m.isDead() && m.isVisible() && GeoEngine.getInstance().canSeeTarget(_bot, m));

		if (mobs.isEmpty())
		{
			return null;
		}

		Monster best = null;
		double bestDistance = Double.MAX_VALUE;

		for (Monster m : mobs)
		{
			double dist = _bot.distance2D(m);
			if (dist < bestDistance)
			{
				bestDistance = dist;
				best = m;
			}
		}

		return best;
	}

	private void performCombatAction(Creature target)
	{
		if (_bot.getAttack().isAttackingNow() || _bot.getCast().isCastingNow())
		{
			return;
		}

		// Potion usage in combat: if HP < 55% and cooldown expired (15 sec)
		final long now = System.currentTimeMillis();
		if (_bot.getStatus().getHpRatio() < 0.55 && (now - _lastPotionTime >= 15000))
		{
			if (_bot.useHealingPotion())
			{
				_lastPotionTime = now;
			}
		}

		// Kiting for Archers & Mages if player target is too close (< 120 units)
		if (target instanceof Player targetPlayer && (_bot.getRole() == BotRole.ARCHER || _bot.getRole() == BotRole.MAGE))
		{
			final double distToPlayer = _bot.distance2D(targetPlayer);
			if (distToPlayer < 120 && Rnd.get(100) < 50 && !_bot.isMoving())
			{
				final int bx = _bot.getX();
				final int by = _bot.getY();
				final int bz = _bot.getZ();
				double dx = bx - targetPlayer.getX();
				double dy = by - targetPlayer.getY();
				double len = Math.hypot(dx, dy);
				if (len < 1)
				{
					final double angle = Rnd.nextDouble() * 2 * Math.PI;
					dx = Math.cos(angle);
					dy = Math.sin(angle);
					len = 1;
				}
				final double ux = dx / len;
				final double uy = dy / len;
				final int retreatDist = Rnd.get(200, 250);
				final int tx = (int) (bx + ux * retreatDist);
				final int ty = (int) (by + uy * retreatDist);
				final int tz = GeoEngine.getInstance().getHeight(tx, ty, bz);

				if (Math.abs(tz - bz) < 200 && GeoEngine.getInstance().canMoveToTarget(bx, by, bz, tx, ty, tz))
				{
					logMoveIfNeeded("kiting player " + targetPlayer.getName(), tx, ty, tz);
					_bot.getMove().maybeMoveToLocation(new Location(tx, ty, tz), 0, true, false);
					return;
				}
			}
		}

		// KS Detection: check if any real player is attacking or has aggro on this monster
		if (target instanceof Monster monster)
		{
			for (Creature c : monster.getAI().getAggroList().keySet())
			{
				if (c instanceof Player p && !p.isFakePlayer() && p != _bot)
				{
					_bot.onKsDetected(p);
					break;
				}
			}
		}

		// Shots: only Tier 2 and Tier 3 (level >= 6) or in active PvP
		final boolean useShots = _bot.getStatus().getLevel() >= 6 || target instanceof Player;

		switch (_bot.getRole())
		{
			case HEALER:
				if (_bot.getStatus().getHpRatio() < 0.60 && canUseSkill(1011))
				{
					if (useShots)
					{
						_bot.autoUseShots(true);
					}
					tryToCast(_bot, 1011, getSkillLevel(1011)); // Heal
				}
				else if (target instanceof Player && canUseSkill(1177) && Rnd.get(100) < 60)
				{
					if (useShots)
					{
						_bot.autoUseShots(true);
					}
					tryToCast(target, 1177, getSkillLevel(1177)); // Wind Strike
				}
				else if (canUseSkill(1177) && Rnd.get(100) < 50)
				{
					if (useShots)
					{
						_bot.autoUseShots(true);
					}
					tryToCast(target, 1177, 1); // Wind Strike
				}
				else
				{
					if (useShots)
					{
						_bot.autoUseShots(false);
					}
					tryToAttack(target);
				}
				break;

			case MAGE:
				if (useShots)
				{
					_bot.autoUseShots(true);
				}
				if (_bot.getStatus().getHpRatio() < 0.50 && canUseSkill(1090)) // Life Drain
				{
					tryToCast(target, 1090, 1);
				}
				else if (target instanceof Player && canUseSkill(1168) && target.getFirstEffect(1168) == null && Rnd.get(100) < 50) // Curse Poison
				{
					tryToCast(target, 1168, 1);
				}
				else if (canUseSkill(1177) && Rnd.get(100) < 65) // Wind Strike
				{
					tryToCast(target, 1177, getSkillLevel(1177));
				}
				else if (canUseSkill(1184)) // Ice Bolt
				{
					tryToCast(target, 1184, getSkillLevel(1184));
				}
				else
				{
					tryToAttack(target);
				}
				break;

			case ARCHER:
				if (useShots)
				{
					_bot.autoUseShots(false);
				}
				if (canUseSkill(56) && Rnd.get(100) < 35) // Power Shot
				{
					tryToCast(target, 56, getSkillLevel(56));
				}
				else
				{
					tryToAttack(target);
				}
				break;

			case TANK:
				if (useShots)
				{
					_bot.autoUseShots(false);
				}
				if (canUseSkill(1040) && _bot.getFirstEffect(1040) == null && Rnd.get(100) < 50) // Shield
				{
					if (useShots)
					{
						_bot.autoUseShots(true);
					}
					tryToCast(_bot, 1040, 1);
				}
				else if (canUseSkill(3) && Rnd.get(100) < 40) // Power Strike
				{
					tryToCast(target, 3, getSkillLevel(3));
				}
				else
				{
					tryToAttack(target);
				}
				break;

			case MELEE_DD:
			default:
				if (useShots)
				{
					_bot.autoUseShots(false);
				}
				if (canUseSkill(78) && _bot.getFirstEffect(78) == null && Rnd.get(100) < 50) // War Cry
				{
					if (useShots)
					{
						_bot.autoUseShots(true);
					}
					tryToCast(_bot, 78, 1);
				}
				else if (target instanceof Monster && canUseSkill(254) && !((Monster) target).getSpoilState().isSpoiled()) // Spoil
				{
					tryToCast(target, 254, 1);
				}
				else if (canUseSkill(16) && Rnd.get(100) < 30) // Mortal Blow
				{
					tryToCast(target, 16, getSkillLevel(16));
				}
				else if (canUseSkill(3) && Rnd.get(100) < 35) // Power Strike
				{
					tryToCast(target, 3, getSkillLevel(3));
				}
				else if (canUseSkill(29) && Rnd.get(100) < 25) // Iron Punch
				{
					tryToCast(target, 29, getSkillLevel(29));
				}
				else
				{
					tryToAttack(target);
				}
				break;
		}
	}

	/**
	 * Retaliate in self-defense when attacked by a real player.
	 */
	public void onAttackedByPlayer(Player attacker)
	{
		if (_bot.isDead() || attacker == null || attacker.isDead() || !attacker.isVisible())
		{
			return;
		}

		if (_bot.isInsideZone(ZoneId.PEACE) || attacker.isInsideZone(ZoneId.PEACE))
		{
			return;
		}

		if (_bot.isResting())
		{
			_bot.setResting(false);
			tryToStand();
		}

		// Interrupt current monster farming
		_bot.getAttack().stop();
		_bot.getCast().stop();

		// Switch target to player
		_bot.setTarget(attacker);
		_bot.setBotState(BotState.FARMING);

		// Retaliate
		performCombatAction(attacker);
	}

	/**
	 * Defend a party member that is being attacked by a player.
	 */
	public void notifyPartyMemberUnderAttack(FakePlayer victim, Player attacker)
	{
		if (_bot.isDead() || attacker == null || attacker.isDead() || !attacker.isVisible())
		{
			return;
		}

		if (_bot.isInsideZone(ZoneId.PEACE) || attacker.isInsideZone(ZoneId.PEACE))
		{
			return;
		}

		if (_bot.isResting())
		{
			_bot.setResting(false);
			tryToStand();
		}

		if (_bot.getRole() == BotRole.HEALER)
		{
			// Healer assists victim
			_bot.setTarget(victim);
			_bot.autoUseShots(true);
			tryToCast(victim, 1011, getSkillLevel(1011));
		}
		else
		{
			// DD / Tank assists by attacking the player
			_bot.setTarget(attacker);
			attacker.updatePvPStatus(_bot);
			_bot.updatePvPStatus(attacker);
			if (_bot.getPvpFlag() == 0)
			{
				_bot.updatePvPFlag(1);
			}
			performCombatAction(attacker);
		}
	}

	@Override
	protected void onEvtAttacked(Creature attacker)
	{
		super.onEvtAttacked(attacker);

		if (_bot.isDead() || attacker == null || attacker == _bot)
		{
			return;
		}

		if (attacker instanceof Player realPlayer && !realPlayer.isFakePlayer())
		{
			if (!_bot.isInsideZone(ZoneId.PEACE) && !realPlayer.isInsideZone(ZoneId.PEACE))
			{
				onAttackedByPlayer(realPlayer);
			}
		}
	}

	private boolean hasSkill(int skillId)
	{
		return _bot.getSkill(skillId) != null;
	}

	private boolean canUseSkill(int skillId)
	{
		final L2Skill skill = _bot.getSkill(skillId);
		if (skill == null)
		{
			return false;
		}
		if (_bot.getStatus().getMp() < skill.getMpConsume())
		{
			return false;
		}
		if (_bot.isSkillDisabled(skill))
		{
			return false;
		}
		return true;
	}

	private int getSkillLevel(int skillId)
	{
		final L2Skill skill = _bot.getSkill(skillId);
		return skill != null ? skill.getLevel() : 1;
	}

	@Override
	protected void thinkAttack()
	{
		if (_bot.denyAiAction())
		{
			return;
		}

		final Creature target = _currentIntention.getFinalTarget();
		if (isTargetLost(target))
		{
			doIdleIntention();
			return;
		}

		if (_bot.getMove().maybeStartOffensiveFollow(target, _bot.getStatus().getPhysicalAttackRange()))
		{
			return;
		}

		if ((_bot.getAttackType() == WeaponType.BOW && _bot.getAttack().isBowCoolingDown()) || _bot.getAttack().isAttackingNow())
		{
			setNextIntention(_currentIntention);
			return;
		}

		if (!_bot.getAttack().canAttack(target))
		{
			return;
		}

		_bot.getMove().stop();
		_bot.getAttack().doAttack(target);
	}

	@Override
	protected void thinkCast()
	{
		if (_bot.denyAiAction() || _bot.getAllSkillsDisabled() || _bot.getCast().isCastingNow())
		{
			return;
		}

		final Creature target = _currentIntention.getFinalTarget();
		final L2Skill skill = _currentIntention.getSkill();

		if (isTargetLost(target, skill))
		{
			doIdleIntention();
			return;
		}

		if (!_bot.getCast().canAttemptCast(target, skill))
		{
			doIdleIntention();
			tryToAttack(target);
			return;
		}

		if (_bot.getMove().maybeStartOffensiveFollow(target, skill.getCastRange()))
		{
			_bot.setWalkOrRun(true);
			return;
		}

		if (skill.getHitTime() > 50)
		{
			_bot.getMove().stop();
			if (target != _bot)
			{
				_bot.getPosition().setHeadingTo(target);
			}
		}

		if (!_bot.getCast().canCast(target, skill, _currentIntention.isCtrlPressed(), _currentIntention.getItemObjectId()))
		{
			doIdleIntention();
			tryToAttack(target);
			return;
		}

		_bot.getCast().doCast(skill, target, null);
	}

	@Override
	protected void thinkMoveTo()
	{
		if (_bot.denyAiAction() || _bot.isMovementDisabled())
		{
			doIdleIntention();
			return;
		}

		_bot.getMove().maybeMoveToLocation(_currentIntention.getLoc(), 0, true, false);
	}
}
