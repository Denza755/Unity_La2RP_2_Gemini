package com.shnok.javaserver.gameserver.bots;

import java.util.Random;

import com.shnok.javaserver.commons.random.Rnd;
import com.shnok.javaserver.gameserver.data.SkillTable;
import com.shnok.javaserver.gameserver.data.xml.PlayerData;
import com.shnok.javaserver.gameserver.enums.actors.ClassId;
import com.shnok.javaserver.gameserver.enums.actors.Sex;
import com.shnok.javaserver.gameserver.idfactory.IdFactory;
import com.shnok.javaserver.gameserver.model.actor.container.player.Appearance;
import com.shnok.javaserver.gameserver.model.actor.template.PlayerTemplate;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.skills.L2Skill;

public class BotAppearanceFactory
{
	private static final String[] BOT_NAMES =
	{
		"Aleron", "Valeria", "Brom", "Kaelen", "Morwen", "Drako", "Theron", "Sylvia",
		"Grom", "Breg", "Finbar", "Alysia", "Vane", "Korg", "Thorin", "Elowen",
		"Zarek", "Lyanna", "Dorn", "Seraph", "Morgath", "Jarek", "Faelar", "Kallan",
		"Thorgar", "Vesper", "Bran", "Rowan", "Fenris", "Tormund", "Elidor", "Orin",
		"Balthazar", "Kira", "Darion", "Grimjaw", "Xander", "Vael", "Althea", "Corvus",
		"Zephyr", "Rurik", "Selene", "Boros", "Eldrin", "Nyx", "Garret", "Sylvan",
		"Torin", "Malakor", "Garrick", "Sariel", "Lucan", "Vanya", "Mirena", "Borek",
		"Kallisto", "Talon", "Harek", "Brant"
	};

	private static final ClassId[] FIGHTER_CLASSES =
	{
		ClassId.HUMAN_FIGHTER,
		ClassId.ELVEN_FIGHTER,
		ClassId.DARK_FIGHTER,
		ClassId.ORC_FIGHTER,
		ClassId.DWARVEN_FIGHTER
	};

	private static final ClassId[] MAGE_CLASSES =
	{
		ClassId.HUMAN_MYSTIC,
		ClassId.ELVEN_MYSTIC,
		ClassId.DARK_MYSTIC,
		ClassId.ORC_MYSTIC
	};

	public static FakePlayer createBot(int index, BotRole role, BotSpot spot, int level)
	{
		final int objectId = IdFactory.getInstance().getNextId();
		final String botName = (index < BOT_NAMES.length) ? BOT_NAMES[index] : ("Bot_" + objectId);

		ClassId classId;
		if (role == BotRole.MAGE || role == BotRole.HEALER)
		{
			classId = MAGE_CLASSES[Rnd.get(MAGE_CLASSES.length)];
		}
		else
		{
			classId = FIGHTER_CLASSES[Rnd.get(FIGHTER_CLASSES.length)];
		}

		final Sex sex = Rnd.nextBoolean() ? Sex.MALE : Sex.FEMALE;

		// Strictly within valid client appearance limits (0..2)
		final byte face = (byte) Rnd.get(0, 2);
		final byte hairStyle = (byte) Rnd.get(0, 2);
		final byte hairColor = (byte) Rnd.get(0, 2);

		final Appearance appearance = new Appearance(face, hairColor, hairStyle, sex);
		final PlayerTemplate template = PlayerData.getInstance().getTemplate(classId);

		final FakePlayer bot = new FakePlayer(objectId, template, "bot_account", appearance, role, spot);
		bot.setName(botName);
		bot.setTitle(role == BotRole.TRADER ? "Trader" : "TI Explorer");
		bot.setBaseClass(classId);
		bot.setOnlineStatus(true, false);
		bot.setRunning(true);
		bot.setStanding(true);

		// Set bot level and stats
		bot.getStatus().setLevel(Math.max(1, Math.min(20, level)));
		bot.getStatus().setHp(bot.getStatus().getMaxHp());
		bot.getStatus().setMp(bot.getStatus().getMaxMp());
		bot.getStatus().setCp(bot.getStatus().getMaxCp());

		// Equip gear with verified 3D models in client
		equipBotGear(bot, role, level);

		// Give consumable items
		giveBotConsumables(bot, role, level);

		// Teach skills strictly from the Whitelist
		teachBotSkills(bot, role, level);

		return bot;
	}

	private static void equipBotGear(FakePlayer bot, BotRole role, int level)
	{
		// 1. WEAPON & SHIELD
		int weaponId;
		int shieldId = 0;

		if (level <= 5) // Tier 1: Lvl 1-5
		{
			switch (role)
			{
				case TANK:
					weaponId = Rnd.nextBoolean() ? 2369 : 1; // Squire's Sword (2369) or Short Sword (1)
					shieldId = 19; // Small Shield (19)
					break;
				case ARCHER:
					weaponId = 13; // Short Bow (13)
					break;
				case MAGE:
				case HEALER:
					weaponId = Rnd.nextBoolean() ? 6 : 7; // Apprentice's Wand (6) or Apprentice's Rod (7)
					break;
				case MELEE_DD:
				default:
					weaponId = Rnd.nextBoolean() ? 216 : 4; // Dirk (216) or Club (4)
					break;
			}
		}
		else if (level <= 11) // Tier 2: Lvl 6-11
		{
			switch (role)
			{
				case TANK:
					weaponId = 1; // Short Sword (1)
					shieldId = 19; // Small Shield (19)
					break;
				case ARCHER:
					weaponId = Rnd.nextBoolean() ? 13 : 271; // Short Bow (13) or Hunting Bow (271)
					break;
				case MAGE:
				case HEALER:
					weaponId = Rnd.nextBoolean() ? 6 : 7; // Apprentice's Wand (6) or Apprentice's Rod (7)
					break;
				case MELEE_DD:
				default:
					weaponId = Rnd.nextBoolean() ? 216 : 4; // Dirk (216) or Club (4)
					break;
			}
		}
		else // Tier 3: Lvl 12-18
		{
			switch (role)
			{
				case TANK:
					weaponId = Rnd.nextBoolean() ? 3 : 2; // Broadsword (3) or Long Sword (2)
					shieldId = Rnd.nextBoolean() ? 20 : 19; // Buckler (20) or Small Shield (19)
					break;
				case ARCHER:
					weaponId = Rnd.nextBoolean() ? 14 : 271; // Bow (14) or Hunting Bow (271)
					break;
				case MAGE:
				case HEALER:
					weaponId = 8; // Willow Staff (8)
					break;
				case MELEE_DD:
				default:
					weaponId = Rnd.nextBoolean() ? 10 : 4; // Dagger (10) or Club (4)
					break;
			}
		}

		ItemInstance weapon = bot.getInventory().addItem(weaponId, 1);
		if (weapon != null)
		{
			bot.getInventory().equipItem(weapon);
		}

		if (shieldId > 0)
		{
			ItemInstance shield = bot.getInventory().addItem(shieldId, 1);
			if (shield != null)
			{
				bot.getInventory().equipItem(shield);
			}
		}

		// 2. ARMOR
		if (role == BotRole.MAGE || role == BotRole.HEALER)
		{
			if (level <= 11)
			{
				// Apprentice's set (425 Tunic, 461 Stockings, 36 Sandals)
				equipItemSafe(bot, 425);
				equipItemSafe(bot, 461);
				equipItemSafe(bot, 36);
			}
			else
			{
				// Leather Tunic set (429 Tunic, 464 Stockings, 36 Sandals)
				equipItemSafe(bot, 429);
				equipItemSafe(bot, 464);
				equipItemSafe(bot, 36);
			}
		}
		else
		{
			// Safe Wooden armor set with 100% verified 3D meshes across all races (including Dark Elf Male)
			equipItemSafe(bot, 23); // Wooden Breastplate
			equipItemSafe(bot, 30); // Wooden Gaiters
			equipItemSafe(bot, 37); // Shoes
		}
	}

	private static void equipItemSafe(FakePlayer bot, int itemId)
	{
		ItemInstance item = bot.getInventory().addItem(itemId, 1);
		if (item != null && item.isEquipable())
		{
			bot.getInventory().equipItem(item);
		}
	}

	private static void giveBotConsumables(FakePlayer bot, BotRole role, int level)
	{
		// Adena & Potions by tier
		if (level <= 5)
		{
			bot.getInventory().addItem(57, Rnd.get(500, 1000)); // Adena
			bot.getInventory().addItem(1060, 2); // Lesser Healing Potion
			if (role == BotRole.ARCHER)
			{
				bot.getInventory().addItem(17, 500); // Wooden Arrow
			}
			// Tier 1 has minimal or no shots
			if (role == BotRole.MAGE || role == BotRole.HEALER)
			{
				bot.getInventory().addItem(2047, 50); // Spiritshot No-Grade
			}
			else
			{
				bot.getInventory().addItem(2039, 50); // Soulshot No-Grade
			}
		}
		else if (level <= 11)
		{
			bot.getInventory().addItem(57, Rnd.get(3000, 5000)); // Adena
			bot.getInventory().addItem(1060, 5); // Lesser Healing Potion
			if (role == BotRole.ARCHER)
			{
				bot.getInventory().addItem(17, 1000); // Wooden Arrow
			}
			if (role == BotRole.MAGE || role == BotRole.HEALER)
			{
				bot.getInventory().addItem(2047, 300); // Spiritshot No-Grade
			}
			else
			{
				bot.getInventory().addItem(2039, 300); // Soulshot No-Grade
			}
		}
		else
		{
			bot.getInventory().addItem(57, Rnd.get(10000, 20000)); // Adena
			bot.getInventory().addItem(1060, 10); // Lesser Healing Potion
			if (role == BotRole.ARCHER)
			{
				bot.getInventory().addItem(17, 2000); // Wooden Arrow
			}
			if (role == BotRole.MAGE || role == BotRole.HEALER)
			{
				bot.getInventory().addItem(2047, 800); // Spiritshot No-Grade
			}
			else
			{
				bot.getInventory().addItem(2039, 800); // Soulshot No-Grade
			}
		}
	}

	/**
	 * Whitelisted skills for bots strictly matching C1 progression.
	 */
	private static void teachBotSkills(FakePlayer bot, BotRole role, int level)
	{
		if (level <= 5)
		{
			// Tier 1: Basic level 1 skills, autoattack
			switch (role)
			{
				case MAGE:
					addSkillSafe(bot, 1177, 1); // Wind Strike lvl 1
					break;
				case HEALER:
					addSkillSafe(bot, 1011, 1); // Heal lvl 1
					addSkillSafe(bot, 1177, 1); // Wind Strike lvl 1
					break;
				default:
					// Physical auto-attack mainly
					addSkillSafe(bot, 3, 1); // Power Strike lvl 1
					break;
			}
		}
		else if (level <= 11)
		{
			// Tier 2: Basic lvl 1-2 skills
			switch (role)
			{
				case ARCHER:
					addSkillSafe(bot, 56, 1); // Power Shot lvl 1
					break;
				case TANK:
					addSkillSafe(bot, 3, 1); // Power Strike lvl 1
					addSkillSafe(bot, 1040, 1); // Shield lvl 1
					break;
				case MELEE_DD:
					addSkillSafe(bot, 3, 1); // Power Strike lvl 1
					addSkillSafe(bot, 16, 1); // Mortal Blow lvl 1
					addSkillSafe(bot, 29, 1); // Iron Punch lvl 1
					if (bot.getClassId() == ClassId.DWARVEN_FIGHTER)
					{
						addSkillSafe(bot, 254, 1); // Spoil lvl 1
					}
					break;
				case MAGE:
					addSkillSafe(bot, 1177, 2); // Wind Strike lvl 2
					addSkillSafe(bot, 1184, 1); // Ice Bolt lvl 1
					break;
				case HEALER:
					addSkillSafe(bot, 1011, 2); // Heal lvl 2
					addSkillSafe(bot, 1040, 1); // Shield lvl 1
					addSkillSafe(bot, 1068, 1); // Might lvl 1
					addSkillSafe(bot, 1177, 1); // Wind Strike lvl 1
					break;
				default:
					break;
			}
		}
		else
		{
			// Tier 3: Advanced C1 whitelist lvl 2-3
			switch (role)
			{
				case ARCHER:
					addSkillSafe(bot, 56, 2); // Power Shot lvl 2
					break;
				case TANK:
					addSkillSafe(bot, 3, 2); // Power Strike lvl 2
					addSkillSafe(bot, 1040, 1); // Shield lvl 1
					addSkillSafe(bot, 1068, 1); // Might lvl 1
					break;
				case MELEE_DD:
					addSkillSafe(bot, 3, 2); // Power Strike lvl 2
					addSkillSafe(bot, 16, 2); // Mortal Blow lvl 2
					addSkillSafe(bot, 29, 2); // Iron Punch lvl 2
					addSkillSafe(bot, 78, 1); // War Cry lvl 1
					if (bot.getClassId() == ClassId.DWARVEN_FIGHTER)
					{
						addSkillSafe(bot, 254, 1); // Spoil lvl 1
					}
					break;
				case MAGE:
					addSkillSafe(bot, 1177, 3); // Wind Strike lvl 3
					addSkillSafe(bot, 1184, 2); // Ice Bolt lvl 2
					addSkillSafe(bot, 1090, 1); // Life Drain lvl 1
					addSkillSafe(bot, 1168, 1); // Curse Poison lvl 1
					break;
				case HEALER:
					addSkillSafe(bot, 1011, 3); // Heal lvl 3
					addSkillSafe(bot, 1012, 1); // Cure Poison lvl 1
					addSkillSafe(bot, 1040, 1); // Shield lvl 1
					addSkillSafe(bot, 1068, 1); // Might lvl 1
					addSkillSafe(bot, 1177, 2); // Wind Strike lvl 2
					break;
				default:
					break;
			}
		}
	}

	private static void addSkillSafe(FakePlayer bot, int skillId, int level)
	{
		L2Skill skill = SkillTable.getInstance().getInfo(skillId, level);
		if (skill == null)
		{
			skill = SkillTable.getInstance().getInfo(skillId, 1);
		}
		if (skill != null)
		{
			bot.addSkill(skill, true);
		}
	}
}
