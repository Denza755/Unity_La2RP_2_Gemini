package com.shnok.javaserver.gameserver.bots;

import java.util.List;

import com.shnok.javaserver.commons.random.Rnd;
import com.shnok.javaserver.gameserver.data.SkillTable;
import com.shnok.javaserver.gameserver.geoengine.GeoEngine;
import com.shnok.javaserver.gameserver.model.WorldObject;
import com.shnok.javaserver.gameserver.model.actor.Creature;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.actor.ai.type.PlayerAI;
import com.shnok.javaserver.gameserver.model.actor.instance.Monster;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.model.location.Location;
import com.shnok.javaserver.gameserver.skills.L2Skill;

public class HotBotAI extends PlayerAI
{
	private final FakePlayer _bot;

	public HotBotAI(FakePlayer bot)
	{
		super(bot);
		_bot = bot;
	}

	public void think()
	{
		// 1. DEAD CHECK & RESPAWN
		if (_bot.isDead())
		{
			handleDeath();
			return;
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

		final double dist = _bot.distance2D(spot.getCenter());
		if (dist > spot.getRadius())
		{
			if (dist > 5000)
			{
				_bot.teleportTo(spot.getCenter(), 50);
			}
			else
			{
				tryToMoveTo(spot.getCenter(), null);
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
			tryToMoveTo(leader.getPosition(), null);
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
			// Roam inside spot
			final BotSpot spot = _bot.getAssignedSpot();
			if (spot != null && _bot.distance2D(spot.getCenter()) > spot.getRadius())
			{
				tryToMoveTo(spot.getCenter(), null);
			}
			return;
		}

		// 3. Perform attack or cast
		performCombatAction((Creature) target);
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

		switch (_bot.getRole())
		{
			case HEALER:
				if (_bot.getStatus().getHpRatio() < 0.60)
				{
					_bot.autoUseShots(true);
					tryToCast(_bot, 1011, getSkillLevel(1011)); // Heal
				}
				else if (hasSkill(1177) && Rnd.get(100) < 50)
				{
					_bot.autoUseShots(true);
					tryToCast(target, 1177, 1); // Wind Strike
				}
				else
				{
					_bot.autoUseShots(false);
					tryToAttack(target);
				}
				break;

			case MAGE:
				_bot.autoUseShots(true);
				if (_bot.getStatus().getHpRatio() < 0.50 && hasSkill(1090)) // Life Drain
				{
					tryToCast(target, 1090, 1);
				}
				else if (hasSkill(1177) && Rnd.get(100) < 65) // Wind Strike
				{
					tryToCast(target, 1177, getSkillLevel(1177));
				}
				else if (hasSkill(1184)) // Ice Bolt
				{
					tryToCast(target, 1184, getSkillLevel(1184));
				}
				else
				{
					tryToAttack(target);
				}
				break;

			case ARCHER:
				_bot.autoUseShots(false);
				if (hasSkill(56) && Rnd.get(100) < 30) // Power Shot
				{
					tryToCast(target, 56, getSkillLevel(56));
				}
				else
				{
					tryToAttack(target);
				}
				break;

			case TANK:
			case MELEE_DD:
			default:
				_bot.autoUseShots(false);
				if (target instanceof Monster && hasSkill(254) && !((Monster) target).getSpoilState().isSpoiled()) // Spoil
				{
					tryToCast(target, 254, 1);
				}
				else if (hasSkill(3) && Rnd.get(100) < 35) // Power Strike
				{
					tryToCast(target, 3, getSkillLevel(3));
				}
				else if (hasSkill(16) && Rnd.get(100) < 25) // Mortal Blow
				{
					tryToCast(target, 16, getSkillLevel(16));
				}
				else if (hasSkill(29) && Rnd.get(100) < 20) // Iron Punch
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

	private boolean hasSkill(int skillId)
	{
		return _bot.getSkill(skillId) != null;
	}

	private int getSkillLevel(int skillId)
	{
		final L2Skill skill = _bot.getSkill(skillId);
		return skill != null ? skill.getLevel() : 1;
	}
}
