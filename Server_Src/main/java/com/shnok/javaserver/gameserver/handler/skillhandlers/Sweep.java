package com.shnok.javaserver.gameserver.handler.skillhandlers;

import java.util.Map;
import java.util.Map.Entry;

import com.shnok.javaserver.gameserver.enums.skills.SkillType;
import com.shnok.javaserver.gameserver.handler.ISkillHandler;
import com.shnok.javaserver.gameserver.model.WorldObject;
import com.shnok.javaserver.gameserver.model.actor.Creature;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.actor.instance.Monster;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.skills.L2Skill;
import com.shnok.javaserver.gameserver.taskmanager.DecayTaskManager;

public class Sweep implements ISkillHandler
{
	private static final SkillType[] SKILL_IDS =
	{
		SkillType.SWEEP
	};
	
	@Override
	public void useSkill(Creature creature, L2Skill skill, WorldObject[] targets, ItemInstance item)
	{
		// Must be called by a Player.
		if (!(creature instanceof Player player))
			return;
		
		for (WorldObject target : targets)
		{
			if (!(target instanceof Monster targetMonster))
				continue;
			
			final Map<Integer, Integer> items = targetMonster.getSpoilState();
			if (items.isEmpty())
				continue;
			
			// Reward spoiler, based on sweep items retained on List.
			for (Entry<Integer, Integer> entry : items.entrySet())
			{
				if (player.isInParty())
					player.getParty().distributeItem(player, entry.getKey(), entry.getValue(), true, targetMonster);
				else
					player.addEarnedItem(entry.getKey(), entry.getValue(), true);
			}
			
			// Reset variables.
			targetMonster.getSpoilState().clear();
			
			// Immediately remove corpse from the world on successful sweep
			DecayTaskManager.getInstance().cancel(targetMonster);
			targetMonster.onDecay();
		}
		
		if (skill.hasSelfEffects())
			skill.getEffectsSelf(creature);
	}
	
	@Override
	public SkillType[] getSkillIds()
	{
		return SKILL_IDS;
	}
}