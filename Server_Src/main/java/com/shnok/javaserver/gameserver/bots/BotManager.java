package com.shnok.javaserver.gameserver.bots;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ScheduledFuture;

import com.shnok.javaserver.commons.logging.CLogger;
import com.shnok.javaserver.commons.pool.ThreadPool;
import com.shnok.javaserver.commons.random.Rnd;
import com.shnok.javaserver.gameserver.enums.LootRule;
import com.shnok.javaserver.gameserver.enums.SayType;
import com.shnok.javaserver.gameserver.enums.actors.OperateType;
import com.shnok.javaserver.gameserver.model.World;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.group.Party;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.model.location.Location;
import com.shnok.javaserver.gameserver.model.trade.TradeList;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.PrivateStoreMsgBuy;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.PrivateStoreMsgSell;

public class BotManager
{
	private static final CLogger LOGGER = new CLogger(BotManager.class.getName());

	private final Map<Integer, FakePlayer> _bots = new ConcurrentHashMap<>();

	private ScheduledFuture<?> _hotAiTask = null;
	private ScheduledFuture<?> _lodEvaluationTask = null;
	private ScheduledFuture<?> _ambientChatTask = null;

	private int _botNameCounter = 0;

	private static final String[] AMBIENT_SHOUTS =
	{
		"LF party Cedric Grounds 10+!",
		"Any buffer in village square?",
		"Selling Soulshots cheap near square!",
		"Good hunting today in Obelisk Hills :)",
		"Need a group for Elven Ruins!",
		"WTB Willow Staff!",
		"Selling starter gear, check my store in square!",
		"Buying bones and varnish, PM me or visit my store!"
	};

	public static BotManager getInstance()
	{
		return SingletonHolder.INSTANCE;
	}

	private static class SingletonHolder
	{
		protected static final BotManager INSTANCE = new BotManager();
	}

	public void init()
	{
		LOGGER.info("Initializing Fake Player (Bot) System...");

		// 1. Spawn 42 Farm Bots across 6 Talking Island spots
		spawnFarmBots();

		// 2. Spawn 8 Static Traders in Talking Island Village
		spawnStaticTraders();

		// 3. Form preset bot parties
		formPresetBotParties();

		// 4. Start simulation schedules
		startSimulationTasks();

		LOGGER.info("Fake Player System started: {} total bots (HOT: {}, COLD: {}, STATIC: {}).",
			_bots.size(), getHotCount(), getColdCount(), getStaticCount());
	}

	private void spawnFarmBots()
	{
		final BotRole[] roles =
		{
			BotRole.TANK,
			BotRole.MELEE_DD,
			BotRole.ARCHER,
			BotRole.MAGE,
			BotRole.HEALER,
			BotRole.MELEE_DD,
			BotRole.ARCHER
		};

		for (BotSpot spot : BotSpot.FARM_SPOTS)
		{
			for (int i = 0; i < 7; i++)
			{
				final BotRole role = roles[i % roles.length];
				final int level = Rnd.get(spot.getMinLevel(), spot.getMaxLevel());
				spawnBot(role, spot, level);
			}
		}
	}

	private void spawnStaticTraders()
	{
		final Location villageCenter = BotSpot.VILLAGE_TOWN.getCenter();

		final int[][] offsets =
		{
			{ 150, -150 },
			{ 150, 150 },
			{ -150, -150 },
			{ -150, 150 },
			{ 50, 300 },
			{ -50, -300 },
			{ 250, 0 },
			{ -250, 0 }
		};

		for (int i = 0; i < 8; i++)
		{
			final FakePlayer trader = BotAppearanceFactory.createBot(_botNameCounter++, BotRole.TRADER, BotSpot.VILLAGE_TOWN, 20);
			trader.setSimulationMode(BotSimulationMode.STATIC);
			trader.setBotState(BotState.TRADING);

			final int x = villageCenter.getX() + offsets[i][0];
			final int y = villageCenter.getY() + offsets[i][1];
			final int z = villageCenter.getZ();

			trader.spawnMe(x, y, z);
			_bots.put(trader.getObjectId(), trader);

			setupTraderStore(trader, i);
		}
	}

	private void setupTraderStore(FakePlayer trader, int index)
	{
		trader.getMove().stop();
		trader.sitDown();

		switch (index)
		{
			case 0: // Soulshot NG Seller
			{
				final ItemInstance ss = trader.getInventory().addItem(2039, 50000);
				final TradeList sellList = trader.getSellList();
				sellList.clear();
				sellList.setTitle("Soulshots NG 12a");
				sellList.addItem(ss.getObjectId(), 50000, 12);
				trader.setOperateType(OperateType.SELL);
				trader.broadcastUserInfo();
				trader.broadcastPacket(new PrivateStoreMsgSell(trader));
				break;
			}
			case 1: // Spiritshot NG Seller
			{
				final ItemInstance sps = trader.getInventory().addItem(2047, 30000);
				final TradeList sellList = trader.getSellList();
				sellList.clear();
				sellList.setTitle("Spiritshots NG 25a");
				sellList.addItem(sps.getObjectId(), 30000, 25);
				trader.setOperateType(OperateType.SELL);
				trader.broadcastUserInfo();
				trader.broadcastPacket(new PrivateStoreMsgSell(trader));
				break;
			}
			case 2: // Arrows & Potions Seller
			{
				final ItemInstance arr = trader.getInventory().addItem(17, 10000);
				final ItemInstance pot = trader.getInventory().addItem(1060, 500);
				final TradeList sellList = trader.getSellList();
				sellList.clear();
				sellList.setTitle("Arrows & Potions");
				sellList.addItem(arr.getObjectId(), 10000, 3);
				sellList.addItem(pot.getObjectId(), 500, 80);
				trader.setOperateType(OperateType.SELL);
				trader.broadcastUserInfo();
				trader.broadcastPacket(new PrivateStoreMsgSell(trader));
				break;
			}
			case 3: // Starter Weapons Seller
			{
				final ItemInstance sword = trader.getInventory().addItem(1, 5);
				final ItemInstance dagger = trader.getInventory().addItem(223, 5);
				final ItemInstance club = trader.getInventory().addItem(3, 5);
				final TradeList sellList = trader.getSellList();
				sellList.clear();
				sellList.setTitle("Starter Weapons");
				sellList.addItem(sword.getObjectId(), 5, 1500);
				sellList.addItem(dagger.getObjectId(), 5, 1500);
				sellList.addItem(club.getObjectId(), 5, 1500);
				trader.setOperateType(OperateType.SELL);
				trader.broadcastUserInfo();
				trader.broadcastPacket(new PrivateStoreMsgSell(trader));
				break;
			}
			case 4: // Starter Armors Seller
			{
				final ItemInstance plate = trader.getInventory().addItem(22, 5);
				final ItemInstance armor = trader.getInventory().addItem(23, 5);
				final ItemInstance pants = trader.getInventory().addItem(24, 5);
				final TradeList sellList = trader.getSellList();
				sellList.clear();
				sellList.setTitle("Starter Armor");
				sellList.addItem(plate.getObjectId(), 5, 2200);
				sellList.addItem(armor.getObjectId(), 5, 3000);
				sellList.addItem(pants.getObjectId(), 5, 1800);
				trader.setOperateType(OperateType.SELL);
				trader.broadcastUserInfo();
				trader.broadcastPacket(new PrivateStoreMsgSell(trader));
				break;
			}
			case 5: // Mystic Gear Seller
			{
				final ItemInstance staff = trader.getInventory().addItem(65, 5);
				final ItemInstance wand = trader.getInventory().addItem(15, 5);
				final ItemInstance tunic = trader.getInventory().addItem(425, 5);
				final ItemInstance stock = trader.getInventory().addItem(426, 5);
				final TradeList sellList = trader.getSellList();
				sellList.clear();
				sellList.setTitle("Mage Gear & Robes");
				sellList.addItem(staff.getObjectId(), 5, 4500);
				sellList.addItem(wand.getObjectId(), 5, 1200);
				sellList.addItem(tunic.getObjectId(), 5, 2500);
				sellList.addItem(stock.getObjectId(), 5, 1500);
				trader.setOperateType(OperateType.SELL);
				trader.broadcastUserInfo();
				trader.broadcastPacket(new PrivateStoreMsgSell(trader));
				break;
			}
			case 6: // Materials Buyer 1
			{
				trader.getInventory().addItem(57, 200000); // Adena
				final TradeList buyList = trader.getBuyList();
				buyList.clear();
				buyList.setTitle("Buying Bones & Varnish");
				buyList.addItemByItemId(1872, 500, 40, 0); // Animal Bone
				buyList.addItemByItemId(1865, 500, 50, 0); // Varnish
				trader.setOperateType(OperateType.BUY);
				trader.broadcastUserInfo();
				trader.broadcastPacket(new PrivateStoreMsgBuy(trader));
				break;
			}
			case 7: // Materials Buyer 2
			{
				trader.getInventory().addItem(57, 200000); // Adena
				final TradeList buyList = trader.getBuyList();
				buyList.clear();
				buyList.setTitle("Buying Iron & Charcoal");
				buyList.addItemByItemId(1869, 500, 45, 0); // Iron Ore
				buyList.addItemByItemId(1870, 500, 40, 0); // Charcoal
				trader.setOperateType(OperateType.BUY);
				trader.broadcastUserInfo();
				trader.broadcastPacket(new PrivateStoreMsgBuy(trader));
				break;
			}
		}
	}

	private void formPresetBotParties()
	{
		// Party 1: Cedric Grounds (Fighter + Healer)
		formPartyAtSpot(BotSpot.CEDRIC_GROUNDS, BotRole.MELEE_DD, BotRole.HEALER);

		// Party 2: Obelisk Hills (Tank + Archer + Healer)
		formPartyAtSpot(BotSpot.OBELISK_HILLS, BotRole.TANK, BotRole.ARCHER, BotRole.HEALER);

		// Party 3: Elven Ruins Valley (Mage + Healer)
		formPartyAtSpot(BotSpot.ELVEN_RUINS_VALLEY, BotRole.MAGE, BotRole.HEALER);
	}

	private void formPartyAtSpot(BotSpot spot, BotRole leaderRole, BotRole... memberRoles)
	{
		FakePlayer leader = null;
		final List<FakePlayer> members = new ArrayList<>();

		for (FakePlayer bot : _bots.values())
		{
			if (bot.getAssignedSpot() == spot && !bot.isInParty() && bot.getRole() != BotRole.TRADER)
			{
				if (leader == null && bot.getRole() == leaderRole)
				{
					leader = bot;
				}
				else
				{
					for (BotRole mRole : memberRoles)
					{
						if (bot.getRole() == mRole && !members.contains(bot) && members.size() < memberRoles.length)
						{
							members.add(bot);
							break;
						}
					}
				}
			}
		}

		if (leader != null && !members.isEmpty())
		{
			Party party = new Party(leader, members.get(0), LootRule.ITEM_LOOTER);
			leader.setBotState(BotState.IN_PARTY);

			for (FakePlayer member : members)
			{
				if (member != members.get(0))
				{
					party.addPartyMember(member);
				}
				member.setFollowTarget(leader);
				member.setPartyAssistant(true);
				member.setBotState(BotState.IN_PARTY);
			}
		}
	}

	private void startSimulationTasks()
	{
		// 1. HOT AI Execution: Every 500 ms
		_hotAiTask = ThreadPool.scheduleAtFixedRate(this::tickHotBots, 1000, 500);

		// 2. LOD Evaluation & COLD Progression: Every 5000 ms
		_lodEvaluationTask = ThreadPool.scheduleAtFixedRate(this::tickLodAndColdBots, 5000, 5000);

		// 3. Ambient Chat & Town Shouts: Every 35000 ms
		_ambientChatTask = ThreadPool.scheduleAtFixedRate(this::tickAmbientChatter, 15000, 35000);
	}

	private void tickHotBots()
	{
		for (FakePlayer bot : _bots.values())
		{
			if (bot.getSimulationMode() == BotSimulationMode.HOT)
			{
				try
				{
					bot.getAI().think();
				}
				catch (Exception e)
				{
					LOGGER.error("Error ticking bot AI for {}: {}", bot.getName(), e.getMessage());
				}
			}
		}
	}

	private void tickLodAndColdBots()
	{
		final Collection<Player> onlinePlayers = World.getInstance().getPlayers();
		final List<Player> realPlayers = new ArrayList<>();
		for (Player p : onlinePlayers)
		{
			if (p != null && p.isOnline() && !p.isFakePlayer())
			{
				realPlayers.add(p);
			}
		}

		for (FakePlayer bot : _bots.values())
		{
			if (bot.getRole() == BotRole.TRADER || bot.getSimulationMode() == BotSimulationMode.STATIC)
			{
				continue;
			}

			// If in party with a real player, always HOT
			if (bot.isInParty() && bot.getParty().containsPlayer(bot.getFollowTarget()))
			{
				bot.setSimulationMode(BotSimulationMode.HOT);
				continue;
			}

			// Evaluate proximity to real players
			boolean nearRealPlayer = false;
			for (Player realPlayer : realPlayers)
			{
				if (bot.distance2D(realPlayer) <= 3500)
				{
					nearRealPlayer = true;
					break;
				}
			}

			if (nearRealPlayer)
			{
				bot.setSimulationMode(BotSimulationMode.HOT);
			}
			else
			{
				bot.setSimulationMode(BotSimulationMode.COLD);
				// Simulate lightweight progression
				bot.simulateColdProgress(5);
			}
		}
	}

	private void tickAmbientChatter()
	{
		if (_bots.isEmpty())
		{
			return;
		}

		final List<FakePlayer> botList = new ArrayList<>(_bots.values());
		final FakePlayer chatter = botList.get(Rnd.get(botList.size()));

		if (chatter.isOnline() && !chatter.isDead())
		{
			final String message = AMBIENT_SHOUTS[Rnd.get(AMBIENT_SHOUTS.length)];
			chatter.sayInChat(message, SayType.SHOUT);
		}
	}

	public FakePlayer spawnBot(BotRole role, BotSpot spot, int level)
	{
		final FakePlayer bot = BotAppearanceFactory.createBot(_botNameCounter++, role, spot, level);
		final Location center = spot.getCenter();

		final int offsetX = Rnd.get(-120, 120);
		final int offsetY = Rnd.get(-120, 120);
		final int x = center.getX() + offsetX;
		final int y = center.getY() + offsetY;
		final int z = center.getZ();

		bot.setSimulationMode(BotSimulationMode.HOT);
		bot.setBotState(BotState.FARMING);
		bot.spawnMe(x, y, z);

		_bots.put(bot.getObjectId(), bot);
		return bot;
	}

	public void despawnAll()
	{
		if (_hotAiTask != null)
		{
			_hotAiTask.cancel(true);
			_hotAiTask = null;
		}
		if (_lodEvaluationTask != null)
		{
			_lodEvaluationTask.cancel(true);
			_lodEvaluationTask = null;
		}
		if (_ambientChatTask != null)
		{
			_ambientChatTask.cancel(true);
			_ambientChatTask = null;
		}

		for (FakePlayer bot : _bots.values())
		{
			bot.despawn();
		}

		_bots.clear();
		LOGGER.info("All fake players despawned.");
	}

	public int getTotalCount()
	{
		return _bots.size();
	}

	public int getHotCount()
	{
		int count = 0;
		for (FakePlayer bot : _bots.values())
		{
			if (bot.getSimulationMode() == BotSimulationMode.HOT)
			{
				count++;
			}
		}
		return count;
	}

	public int getColdCount()
	{
		int count = 0;
		for (FakePlayer bot : _bots.values())
		{
			if (bot.getSimulationMode() == BotSimulationMode.COLD)
			{
				count++;
			}
		}
		return count;
	}

	public int getStaticCount()
	{
		int count = 0;
		for (FakePlayer bot : _bots.values())
		{
			if (bot.getSimulationMode() == BotSimulationMode.STATIC)
			{
				count++;
			}
		}
		return count;
	}

	public Collection<FakePlayer> getBots()
	{
		return _bots.values();
	}
}
