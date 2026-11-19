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
		giveBotConsumables(bot, role);

		// Teach skills strictly from the Whitelist
		teachBotSkills(bot, role, level);

		return bot;
	}

	private static void equipBotGear(FakePlayer bot, BotRole role, int level)
	{
		// 1. WEAPON & SHIELD
		int weaponId;
		int shieldId = 0;

		switch (role)
		{
			case TANK:
				weaponId = (level >= 10) ? 71 : 1; // Long Sword (D-grade) or Short Sword
				shieldId = (level >= 10) ? 20 : 19; // Buckler or Small Shield
				break;
			case ARCHER:
				weaponId = (level >= 10) ? 276 : 13; // Hunting Bow or Short Bow
				break;
			case MAGE:
			case HEALER:
				weaponId = (level >= 10) ? 65 : 14; // Willow Staff or Apprentice's Staff
				break;
			case MELEE_DD:
			default:
				if (Rnd.nextBoolean())
				{
					weaponId = (level >= 10) ? 7 : 223; // Dagger or Dirk
				}
				else
				{
					weaponId = (level >= 10) ? 72 : 3; // Bastard Sword or Club
				}
				break;
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
			// Magic Robe: Tunic of Magic (425), Stockings of Magic (426), Low Boots (57)
			equipItemSafe(bot, 425);
			equipItemSafe(bot, 426);
			equipItemSafe(bot, 57);
		}
		else
		{
			// Leather Armor: Leather Armor (23), Leather Pants (24), Leather Helmet (42), Leather Gloves (48), Leather Shoes (56)
			equipItemSafe(bot, 23);
			equipItemSafe(bot, 24);
			equipItemSafe(bot, 42);
			equipItemSafe(bot, 48);
			equipItemSafe(bot, 56);
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

	private static void giveBotConsumables(FakePlayer bot, BotRole role)
	{
		// Adena
		bot.getInventory().addItem(57, 50000);

		// Shots
		if (role == BotRole.MAGE || role == BotRole.HEALER)
		{
			bot.getInventory().addItem(2047, 1000); // Spiritshot No-Grade
		}
		else
		{
			bot.getInventory().addItem(2039, 1000); // Soulshot No-Grade
		}

		// Arrows for archers
		if (role == BotRole.ARCHER)
		{
			bot.getInventory().addItem(17, 2500); // Wooden Arrow
		}
	}

	/**
	 * Whitelisted skills for bots:
	 * Physical: 3 (Power Strike), 16 (Mortal Blow), 29 (Iron Punch), 56 (Power Shot), 78 (War Cry), 254 (Spoil)
	 * Magic / Buffs: 70 (Drain HP), 1010 (Soul Shield), 1011 (Heal), 1012 (Cure Poison), 1040 (Shield), 1068 (Might),
	 *                1090 (Life Drain), 1092 (Fear), 1168 (Curse Poison), 1177 (Wind Strike), 1184 (Ice Bolt), 1244 (Freezing Flame)
	 */
	private static void teachBotSkills(FakePlayer bot, BotRole role, int level)
	{
		int skillLevel = Math.max(1, Math.min(3, level / 5 + 1));

		switch (role)
		{
			case ARCHER:
				addSkillSafe(bot, 56, skillLevel); // Power Shot
				break;

			case TANK:
				addSkillSafe(bot, 3, skillLevel);  // Power Strike
				addSkillSafe(bot, 1040, 1);        // Shield
				break;

			case MELEE_DD:
				addSkillSafe(bot, 3, skillLevel);  // Power Strike
				addSkillSafe(bot, 16, skillLevel); // Mortal Blow
				addSkillSafe(bot, 29, skillLevel); // Iron Punch
				addSkillSafe(bot, 78, 1);          // War Cry
				if (bot.getClassId() == ClassId.DWARVEN_FIGHTER)
				{
					addSkillSafe(bot, 254, 1);     // Spoil
				}
				break;

			case MAGE:
				addSkillSafe(bot, 1177, skillLevel); // Wind Strike
				addSkillSafe(bot, 1184, skillLevel); // Ice Bolt
				addSkillSafe(bot, 1090, 1);          // Life Drain
				addSkillSafe(bot, 1168, 1);          // Curse Poison
				break;

			case HEALER:
				addSkillSafe(bot, 1011, skillLevel); // Heal
				addSkillSafe(bot, 1012, 1);          // Cure Poison
				addSkillSafe(bot, 1040, 1);          // Shield
				addSkillSafe(bot, 1068, 1);          // Might
				addSkillSafe(bot, 1177, 1);          // Wind Strike (for defensive attacks)
				break;

			default:
				break;
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
