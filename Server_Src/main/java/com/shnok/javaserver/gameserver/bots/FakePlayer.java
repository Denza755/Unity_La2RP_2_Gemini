package com.shnok.javaserver.gameserver.bots;

import com.shnok.javaserver.commons.random.Rnd;
import com.shnok.javaserver.gameserver.enums.MessageType;
import com.shnok.javaserver.gameserver.enums.SayType;
import com.shnok.javaserver.gameserver.enums.items.ShotType;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.actor.container.player.Appearance;
import com.shnok.javaserver.gameserver.model.actor.template.PlayerTemplate;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.network.SystemMessageId;
import com.shnok.javaserver.gameserver.network.serverpackets.CreatureSay;
import com.shnok.javaserver.gameserver.network.serverpackets.L2GameServerPacket;
import com.shnok.javaserver.gameserver.network.serverpackets.skill.MagicSkillUse;

public class FakePlayer extends Player
{
	private final BotRole _role;
	private BotSpot _assignedSpot;
	private BotSimulationMode _simulationMode = BotSimulationMode.HOT;
	private BotState _botState = BotState.IDLE;

	private long _lastAiTick = 0;
	private long _deathTimestamp = 0;
	private long _lastSocialActionTime = 0;
	private long _lastRestCheckTime = 0;
	private boolean _isResting = false;

	// Cooldowns for social interactions (chat)
	private long _lastBuffThankTime = 0;
	private long _lastKsReactionTime = 0;

	// Party leader / follow reference (for player-led or bot-led party)
	private Player _followTarget = null;
	private boolean _isPartyAssistant = false;

	public FakePlayer(int objectId, PlayerTemplate template, String accountName, Appearance app, BotRole role, BotSpot spot)
	{
		super(objectId, template, accountName, app);
		_role = role;
		_assignedSpot = spot;
	}

	@Override
	public boolean isFakePlayer()
	{
		return true;
	}

	@Override
	public void setAI()
	{
		_ai = new HotBotAI(this);
	}

	@Override
	public HotBotAI getAI()
	{
		return (HotBotAI) _ai;
	}

	@Override
	public void sendPacket(L2GameServerPacket packet)
	{
		// Bots do not hold a real client TCP socket connection.
		// Packets destined to the bot itself are safely ignored,
		// while all broadcast packets are normally sent to surrounding players.
	}

	@Override
	public void sendPacket(SystemMessageId id)
	{
		// Safely ignored for bots.
	}

	public BotRole getRole()
	{
		return _role;
	}

	public BotSpot getAssignedSpot()
	{
		return _assignedSpot;
	}

	public void setAssignedSpot(BotSpot spot)
	{
		_assignedSpot = spot;
	}

	public BotSimulationMode getSimulationMode()
	{
		return _simulationMode;
	}

	public void setSimulationMode(BotSimulationMode mode)
	{
		_simulationMode = mode;
	}

	public BotState getBotState()
	{
		return _botState;
	}

	public void setBotState(BotState state)
	{
		_botState = state;
	}

	public long getLastAiTick()
	{
		return _lastAiTick;
	}

	public void setLastAiTick(long time)
	{
		_lastAiTick = time;
	}

	public long getDeathTimestamp()
	{
		return _deathTimestamp;
	}

	public void setDeathTimestamp(long time)
	{
		_deathTimestamp = time;
	}

	public long getLastSocialActionTime()
	{
		return _lastSocialActionTime;
	}

	public void setLastSocialActionTime(long time)
	{
		_lastSocialActionTime = time;
	}

	public long getLastRestCheckTime()
	{
		return _lastRestCheckTime;
	}

	public void setLastRestCheckTime(long time)
	{
		_lastRestCheckTime = time;
	}

	public boolean isResting()
	{
		return _isResting;
	}

	public void setResting(boolean resting)
	{
		_isResting = resting;
	}

	public Player getFollowTarget()
	{
		return _followTarget;
	}

	public void setFollowTarget(Player target)
	{
		_followTarget = target;
	}

	public boolean isPartyAssistant()
	{
		return _isPartyAssistant;
	}

	public void setPartyAssistant(boolean isAssistant)
	{
		_isPartyAssistant = isAssistant;
	}

	/**
	 * Send a chat message visible to nearby players.
	 */
	public void sayInChat(String text, SayType channel)
	{
		if (text == null || text.isEmpty())
		{
			return;
		}

		final CreatureSay cs = new CreatureSay(this, channel, text);
		if (channel == SayType.SHOUT)
		{
			broadcastPacket(cs);
		}
		else
		{
			broadcastPacketInRadius(cs, 1250);
		}
	}

	/**
	 * Social response when healed or buffed by a real player.
	 */
	public void onBeneficialSkillReceived(Player caster, String skillName)
	{
		if (caster == null || caster == this)
		{
			return;
		}

		final long now = System.currentTimeMillis();
		if (now - _lastBuffThankTime < 30000)
		{
			return;
		}
		_lastBuffThankTime = now;

		final String[] THANKS =
		{
			"Thanks for the " + (skillName != null ? skillName : "buff") + ", " + caster.getName() + "!",
			"ty!",
			"Thanks!",
			"Appreciate the support :)",
			"Nice, thank you!"
		};

		sayInChat(Rnd.get(THANKS), SayType.ALL);
	}

	/**
	 * Social response when another player kill-steals (KS) bot's target mob.
	 */
	public void onKsDetected(Player player)
	{
		if (player == null || player == this)
		{
			return;
		}

		final long now = System.currentTimeMillis();
		if (now - _lastKsReactionTime < 45000)
		{
			return;
		}
		_lastKsReactionTime = now;

		final String[] KS_MSGS =
		{
			"Hey, my mob!",
			"Nice KS :)",
			"Leave some for me, " + player.getName() + "!",
			"Find your own spot :D",
			"Come on man, I was hitting that!"
		};

		sayInChat(Rnd.get(KS_MSGS), SayType.ALL);
	}

	/**
	 * Check whether bot needs to make a restocking trip to village.
	 */
	public boolean needsRestock()
	{
		if (_botState == BotState.RESTOCKING)
		{
			return true;
		}

		// Shots check
		final int shotId = (_role == BotRole.MAGE || _role == BotRole.HEALER) ? 2047 : 2039;
		final ItemInstance shots = getInventory().getItemByItemId(shotId);
		if (shots == null || shots.getCount() <= 5)
		{
			return true;
		}

		// Arrows check for archer
		if (_role == BotRole.ARCHER)
		{
			final ItemInstance arrows = getInventory().getItemByItemId(17);
			if (arrows == null || arrows.getCount() <= 20)
			{
				return true;
			}
		}

		return false;
	}

	/**
	 * Perform a restocking trip: head to village, refill shots and arrows, return to spot.
	 */
	public void performRestock()
	{
		_botState = BotState.RESTOCKING;

		// Teleport to village town
		teleportTo(BotSpot.VILLAGE_TOWN.getCenter(), 80);

		// Add shots
		final int shotId = (_role == BotRole.MAGE || _role == BotRole.HEALER) ? 2047 : 2039;
		getInventory().addItem(shotId, 1000);

		// Add arrows for archers
		if (_role == BotRole.ARCHER)
		{
			getInventory().addItem(17, 2000);
		}

		// Friendly ambient chatter in town
		if (Rnd.get(100) < 30)
		{
			sayInChat("Restocked and ready!", SayType.ALL);
		}

		// Resume travel back to assigned spot
		_botState = BotState.RETURNING_TO_SPOT;
	}

	/**
	 * Automatically consume Soulshot or Spiritshot before attack/skill if available.
	 */
	public void autoUseShots(boolean isMagic)
	{
		final ItemInstance weaponInst = getActiveWeaponInstance();
		if (weaponInst == null)
		{
			return;
		}

		if (!isMagic)
		{
			if (!weaponInst.isChargedShot(ShotType.SOULSHOT))
			{
				ItemInstance ss = getInventory().getItemByItemId(2039); // No-grade Soulshot
				if (ss != null && ss.getCount() > 0)
				{
					destroyItem(ss, 1, false);
					weaponInst.setChargedShot(ShotType.SOULSHOT, true);
					broadcastPacketInRadius(new MagicSkillUse(this, this, 2039, 1, 0, 0), 600);
				}
			}
		}
		else
		{
			if (!weaponInst.isChargedShot(ShotType.SPIRITSHOT))
			{
				ItemInstance sps = getInventory().getItemByItemId(2047); // No-grade Spiritshot
				if (sps != null && sps.getCount() > 0)
				{
					destroyItem(sps, 1, false);
					weaponInst.setChargedShot(ShotType.SPIRITSHOT, true);
					broadcastPacketInRadius(new MagicSkillUse(this, this, 2047, 1, 0, 0), 600);
				}
			}
		}
	}

	/**
	 * Lightweight progression simulation for COLD bots without nearby players.
	 */
	public void simulateColdProgress(int intervalSec)
	{
		if (isDead())
		{
			if (_deathTimestamp == 0)
			{
				_deathTimestamp = System.currentTimeMillis();
			}
			else if (System.currentTimeMillis() - _deathTimestamp >= 6000)
			{
				_deathTimestamp = 0;
				doRevive(100.0);
				getStatus().setHp(getStatus().getMaxHp());
				getStatus().setMp(getStatus().getMaxMp());
				if (_assignedSpot != null)
				{
					teleportTo(_assignedSpot.getCenter(), 50);
				}
			}
			return;
		}

		// Light HP/MP recovery
		getStatus().setHp(Math.min(getStatus().getMaxHp(), getStatus().getHp() + 15 * intervalSec));
		getStatus().setMp(Math.min(getStatus().getMaxMp(), getStatus().getMp() + 10 * intervalSec));

		// Small exp & adena progression
		final int lvl = getStatus().getLevel();
		final int expGain = (lvl * 12 + 10) * intervalSec;
		final int adenaGain = (lvl * 2 + 1) * intervalSec;

		addExpAndSp(expGain, expGain / 10);
		getInventory().addItem(57, adenaGain);
	}

	/**
	 * Despawn bot cleanly from world.
	 */
	public void despawn()
	{
		if (getParty() != null)
		{
			getParty().removePartyMember(this, MessageType.LEFT);
		}

		setTarget(null);
		getAttack().stop();
		getCast().stop();
		decayMe();
	}
}
