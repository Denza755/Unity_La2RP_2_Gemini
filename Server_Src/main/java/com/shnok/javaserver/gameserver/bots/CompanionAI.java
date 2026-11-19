package com.shnok.javaserver.gameserver.bots;

import java.util.List;

import com.shnok.javaserver.commons.logging.CLogger;
import com.shnok.javaserver.commons.random.Rnd;
import com.shnok.javaserver.gameserver.data.SkillTable;
import com.shnok.javaserver.gameserver.enums.ZoneId;
import com.shnok.javaserver.gameserver.enums.items.ShotType;
import com.shnok.javaserver.gameserver.enums.items.WeaponType;
import com.shnok.javaserver.gameserver.model.WorldObject;
import com.shnok.javaserver.gameserver.model.actor.Creature;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.actor.ai.type.PlayerAI;
import com.shnok.javaserver.gameserver.model.actor.instance.Monster;
import com.shnok.javaserver.gameserver.model.group.Party;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.network.serverpackets.skill.MagicSkillUse;
import com.shnok.javaserver.gameserver.skills.L2Skill;

/**
 * Intelligent AI controller for account companions (.team system).
 * Handles combat assist, party support/healing, owner defense, and automatic revival/stand-guard logic.
 */
public class CompanionAI extends PlayerAI
{
	private static final CLogger LOGGER = new CLogger(CompanionAI.class.getName());

	private final Player _companion;
	private final Player _owner;
	private long _deathTime = 0;
	private long _lastPotionTime = 0;
	private Creature _lastAttacker = null;

	public CompanionAI(Player companion, Player owner)
	{
		super(companion);
		_companion = companion;
		_owner = owner;
	}

	public Player getOwner()
	{
		return _owner;
	}

	public Player getCompanion()
	{
		return _companion;
	}

	public boolean hasSkill(int skillId)
	{
		return _companion.getSkill(skillId) != null;
	}

	public int getSkillLevel(int skillId)
	{
		final L2Skill skill = _companion.getSkill(skillId);
		return skill != null ? skill.getLevel() : 1;
	}

	public boolean canCastSkill(int skillId)
	{
		final L2Skill skill = _companion.getSkill(skillId);
		if (skill == null)
		{
			return false;
		}
		return _companion.getStatus().getMp() >= skill.getMpConsume();
	}

	public void autoUseShots(boolean isMagic)
	{
		final ItemInstance weaponInst = _companion.getActiveWeaponInstance();
		if (weaponInst == null)
		{
			return;
		}

		if (!isMagic)
		{
			if (!weaponInst.isChargedShot(ShotType.SOULSHOT))
			{
				final ItemInstance ss = _companion.getInventory().getItemByItemId(2039); // No-grade Soulshot
				if (ss != null && ss.getCount() > 0)
				{
					_companion.destroyItem(ss, 1, false);
					weaponInst.setChargedShot(ShotType.SOULSHOT, true);
					_companion.broadcastPacketInRadius(new MagicSkillUse(_companion, _companion, 2039, 1, 0, 0), 600);
				}
			}
		}
		else
		{
			if (!weaponInst.isChargedShot(ShotType.SPIRITSHOT))
			{
				final ItemInstance sps = _companion.getInventory().getItemByItemId(2047); // No-grade Spiritshot
				if (sps != null && sps.getCount() > 0)
				{
					_companion.destroyItem(sps, 1, false);
					weaponInst.setChargedShot(ShotType.SPIRITSHOT, true);
					_companion.broadcastPacketInRadius(new MagicSkillUse(_companion, _companion, 2047, 1, 0, 0), 600);
				}
			}
		}
	}

	public boolean useHealingPotion()
	{
		ItemInstance potion = _companion.getInventory().getItemByItemId(1060); // Lesser Healing Potion
		if (potion == null || potion.getCount() <= 0)
		{
			potion = _companion.getInventory().getItemByItemId(1061); // Healing Potion
		}

		if (potion == null || potion.getCount() <= 0)
		{
			return false;
		}

		_companion.destroyItem(potion, 1, false);
		_companion.broadcastPacketInRadius(new MagicSkillUse(_companion, _companion, 2031, 1, 0, 0), 600);
		final double maxHp = _companion.getStatus().getMaxHp();
		_companion.getStatus().setHp(Math.min(maxHp, _companion.getStatus().getHp() + 90));
		return true;
	}

	public void think()
	{
		// 1. COMPANION DEATH (Scheme 2A)
		if (_companion.isDead())
		{
			if (_deathTime == 0)
			{
				_deathTime = System.currentTimeMillis();
			}
			else if (System.currentTimeMillis() - _deathTime >= 15000)
			{
				CompanionManager.getInstance().onCompanionDied(_companion, _owner);
			}
			return;
		}
		_deathTime = 0;

		// 2. OWNER VALIDATION
		if (_owner == null || !_owner.isOnline())
		{
			CompanionManager.getInstance().despawnTeam(_owner);
			return;
		}

		// 3. OWNER DEATH (Scheme 1A)
		if (_owner.isDead())
		{
			final WorldObject curTarget = _companion.getTarget();
			if (curTarget == _owner || (curTarget instanceof Creature c && c.isDead()))
			{
				_companion.setTarget(null);
				_companion.getAttack().stop();
				_companion.getCast().stop();
			}

			// 3.1. Finish off nearby hostile enemies attacking owner or companion
			Creature activeThreat = null;
			if (_lastAttacker != null && !_lastAttacker.isDead() && _lastAttacker.isVisible() && _companion.distance2D(_lastAttacker) <= 800)
			{
				activeThreat = _lastAttacker;
			}
			else
			{
				final List<Creature> threats = _companion.getKnownTypeInRadius(Creature.class, 800,
					c -> !c.isDead() && c.isVisible() && (c.getTarget() == _owner || c.getTarget() == _companion));
				if (!threats.isEmpty())
				{
					activeThreat = threats.get(0);
				}
			}

			if (activeThreat != null)
			{
				_companion.setTarget(activeThreat);
				performCombatAction(activeThreat);
				return;
			}

			// 3.2. No threats: cast Resurrection (1016) on owner if learned and MP available
			if (canCastSkill(1016))
			{
				_companion.setTarget(_owner);
				autoUseShots(true);
				tryToCast(_owner, 1016, getSkillLevel(1016));
				return;
			}

			// 3.3. Otherwise stand guard near owner
			final double distToOwner = _companion.distance2D(_owner);
			if (distToOwner > 150)
			{
				_companion.getMove().maybeStartFriendlyFollow(_owner, 100);
			}
			else if (_companion.isMoving())
			{
				_companion.getMove().stop();
			}
			return;
		}

		// 4. OWNER REVIVAL / TELEPORT (Distance > 1800)
		if (_companion.distance2D(_owner) > 1800)
		{
			_companion.teleportTo(_owner.getPosition(), 60);
			return;
		}

		// 5. WAIT IF ACTION IS IN PROGRESS
		if (_companion.getAttack().isAttackingNow() || _companion.getCast().isCastingNow())
		{
			return;
		}

		// 6. POTION USAGE
		final long now = System.currentTimeMillis();
		if (_companion.getStatus().getHpRatio() < 0.60 && (now - _lastPotionTime >= 15000))
		{
			if (useHealingPotion())
			{
				_lastPotionTime = now;
			}
		}

		// 7. SUPPORT / HEALING / BUFFS
		if (handleSupportAndBuffs())
		{
			return;
		}

		// 8. COMBAT / DEFENSE / ASSIST
		if (handleCombatAndDefense())
		{
			return;
		}

		// 9. FOLLOW OWNER OUTSIDE COMBAT
		handleFollowOwner();
	}

	private boolean handleSupportAndBuffs()
	{
		// Heal (1011)
		if (canCastSkill(1011))
		{
			final int healLvl = getSkillLevel(1011);

			// Owner heal priority
			if (_owner.getStatus().getHpRatio() < 0.65 && _companion.distance2D(_owner) <= 800)
			{
				autoUseShots(true);
				tryToCast(_owner, 1011, healLvl);
				return true;
			}

			// Self heal
			if (_companion.getStatus().getHpRatio() < 0.60)
			{
				autoUseShots(true);
				tryToCast(_companion, 1011, healLvl);
				return true;
			}

			// Party members heal
			final Party party = _companion.getParty();
			if (party != null)
			{
				for (Player member : party.getMembers())
				{
					if (member != null && !member.isDead() && member.getStatus().getHpRatio() < 0.60 && _companion.distance2D(member) <= 800)
					{
						autoUseShots(true);
						tryToCast(member, 1011, healLvl);
						return true;
					}
				}
			}
		}

		// Cure Poison (1012)
		if (canCastSkill(1012))
		{
			if (_owner.getFirstEffect(1168) != null && _companion.distance2D(_owner) <= 600)
			{
				autoUseShots(true);
				tryToCast(_owner, 1012, 1);
				return true;
			}
			if (_companion.getFirstEffect(1168) != null)
			{
				autoUseShots(true);
				tryToCast(_companion, 1012, 1);
				return true;
			}
		}

		// Buffs (outside intense combat)
		if (_companion.distance2D(_owner) <= 600)
		{
			// Shield (1040)
			if (_owner.getFirstEffect(1040) == null && canCastSkill(1040))
			{
				autoUseShots(true);
				tryToCast(_owner, 1040, 1);
				return true;
			}

			// Might (1068)
			if (_owner.getFirstEffect(1068) == null && canCastSkill(1068))
			{
				autoUseShots(true);
				tryToCast(_owner, 1068, 1);
				return true;
			}
		}

		// Soul Shield (1010) on self
		if (_companion.getFirstEffect(1010) == null && canCastSkill(1010))
		{
			autoUseShots(true);
			tryToCast(_companion, 1010, 1);
			return true;
		}

		return false;
	}

	private boolean handleCombatAndDefense()
	{
		// 1. Self defense against attacker
		if (_lastAttacker != null)
		{
			if (_lastAttacker.isDead()
				|| !_lastAttacker.isVisible()
				|| _companion.distance2D(_lastAttacker) > 1000
				|| _companion.isInsideZone(ZoneId.PEACE)
				|| _lastAttacker.isInsideZone(ZoneId.PEACE))
			{
				_lastAttacker = null;
			}
			else
			{
				_companion.setTarget(_lastAttacker);
				performCombatAction(_lastAttacker);
				return true;
			}
		}

		// 2. Protect owner if owner is attacked
		final List<Creature> threats = _companion.getKnownTypeInRadius(Creature.class, 900,
			c -> !c.isDead() && c.isVisible() && c.getTarget() == _owner);
		if (!threats.isEmpty())
		{
			final Creature threat = threats.get(0);
			if (!(threat instanceof Player p && (p.isInsideZone(ZoneId.PEACE) || _companion.isInsideZone(ZoneId.PEACE))))
			{
				_companion.setTarget(threat);
				performCombatAction(threat);
				return true;
			}
		}

		// 3. Assist owner target
		final WorldObject ownerTarget = _owner.getTarget();
		if (ownerTarget instanceof Creature target && !target.isDead() && target.isVisible() && target != _companion && target != _owner)
		{
			if (target instanceof Player p && (p.isInsideZone(ZoneId.PEACE) || _companion.isInsideZone(ZoneId.PEACE)))
			{
				// In peace zone, do not attack players
			}
			else
			{
				_companion.setTarget(target);
				performCombatAction(target);
				return true;
			}
		}

		return false;
	}

	private void handleFollowOwner()
	{
		final WorldObject currentTarget = _companion.getTarget();
		if (currentTarget instanceof Creature c && c.isDead())
		{
			_companion.setTarget(null);
		}

		final double dist = _companion.distance2D(_owner);
		if (dist > 220)
		{
			_companion.getMove().maybeStartFriendlyFollow(_owner, 120);
		}
		else if (dist < 100 && _companion.isMoving())
		{
			_companion.getMove().stop();
		}
	}

	private void performCombatAction(Creature target)
	{
		if (_companion.getAttack().isAttackingNow() || _companion.getCast().isCastingNow())
		{
			return;
		}

		// Shots
		final boolean isMagicUser = hasSkill(1177) || hasSkill(1184) || hasSkill(1090) || hasSkill(70);
		autoUseShots(isMagicUser);

		// Magic attacks
		if (_companion.getStatus().getHpRatio() < 0.50 && canCastSkill(1090)) // Life Drain
		{
			tryToCast(target, 1090, getSkillLevel(1090));
			return;
		}
		if (_companion.getStatus().getHpRatio() < 0.50 && canCastSkill(70)) // Drain HP
		{
			tryToCast(target, 70, getSkillLevel(70));
			return;
		}
		if (canCastSkill(1168) && target.getFirstEffect(1168) == null && Rnd.get(100) < 50) // Curse Poison
		{
			tryToCast(target, 1168, 1);
			return;
		}
		if (canCastSkill(1177) && Rnd.get(100) < 65) // Wind Strike
		{
			tryToCast(target, 1177, getSkillLevel(1177));
			return;
		}
		if (canCastSkill(1184)) // Ice Bolt
		{
			tryToCast(target, 1184, getSkillLevel(1184));
			return;
		}
		if (canCastSkill(1244) && target.getFirstEffect(1244) == null) // Freezing Flame
		{
			tryToCast(target, 1244, getSkillLevel(1244));
			return;
		}

		// Physical skills
		if (_companion.getFirstEffect(78) == null && canCastSkill(78) && Rnd.get(100) < 50) // War Cry
		{
			autoUseShots(true);
			tryToCast(_companion, 78, 1);
			return;
		}
		if (target instanceof Monster m && !m.getSpoilState().isSpoiled() && canCastSkill(254)) // Spoil
		{
			tryToCast(target, 254, 1);
			return;
		}
		if (_companion.getAttackType() == WeaponType.BOW && canCastSkill(56) && Rnd.get(100) < 40) // Power Shot
		{
			tryToCast(target, 56, getSkillLevel(56));
			return;
		}
		if (canCastSkill(16) && Rnd.get(100) < 35) // Mortal Blow
		{
			tryToCast(target, 16, getSkillLevel(16));
			return;
		}
		if (canCastSkill(3) && Rnd.get(100) < 35) // Power Strike
		{
			tryToCast(target, 3, getSkillLevel(3));
			return;
		}
		if (canCastSkill(29) && Rnd.get(100) < 25) // Iron Punch
		{
			tryToCast(target, 29, getSkillLevel(29));
			return;
		}

		// Default physical attack
		tryToAttack(target);
	}

	@Override
	protected void onEvtAttacked(Creature attacker)
	{
		super.onEvtAttacked(attacker);
		if (_companion.isDead() || attacker == null || attacker == _companion)
		{
			return;
		}
		_lastAttacker = attacker;
	}

	@Override
	protected void thinkAttack()
	{
		if (_companion.denyAiAction())
		{
			return;
		}

		final Creature target = _currentIntention.getFinalTarget();
		if (isTargetLost(target))
		{
			doIdleIntention();
			return;
		}

		if (_companion.getMove().maybeStartOffensiveFollow(target, _companion.getStatus().getPhysicalAttackRange()))
		{
			return;
		}

		if ((_companion.getAttackType() == WeaponType.BOW && _companion.getAttack().isBowCoolingDown()) || _companion.getAttack().isAttackingNow())
		{
			setNextIntention(_currentIntention);
			return;
		}

		if (!_companion.getAttack().canAttack(target))
		{
			return;
		}

		_companion.getMove().stop();
		_companion.getAttack().doAttack(target);
	}

	@Override
	protected void thinkCast()
	{
		if (_companion.denyAiAction() || _companion.getAllSkillsDisabled() || _companion.getCast().isCastingNow())
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

		if (!_companion.getCast().canAttemptCast(target, skill))
		{
			return;
		}

		if (_companion.getMove().maybeStartOffensiveFollow(target, skill.getCastRange()))
		{
			_companion.setWalkOrRun(true);
			return;
		}

		if (skill.getHitTime() > 50)
		{
			_companion.getMove().stop();
			if (target != _companion)
			{
				_companion.getPosition().setHeadingTo(target);
			}
		}

		if (!_companion.getCast().canCast(target, skill, _currentIntention.isCtrlPressed(), _currentIntention.getItemObjectId()))
		{
			return;
		}

		_companion.getCast().doCast(skill, target, null);
	}

	@Override
	protected void thinkMoveTo()
	{
		if (_companion.denyAiAction() || _companion.isMovementDisabled())
		{
			doIdleIntention();
			return;
		}

		_companion.getMove().maybeMoveToLocation(_currentIntention.getLoc(), 0, true, false);
	}
}
