package com.shnok.javaserver.gameserver.model.actor.instance;

import java.util.StringTokenizer;

import com.shnok.javaserver.Config;
import com.shnok.javaserver.gameserver.data.manager.CastleManager;
import com.shnok.javaserver.gameserver.data.manager.CastleManorManager;
import com.shnok.javaserver.gameserver.data.xml.ItemData;
import com.shnok.javaserver.gameserver.model.actor.Player;
import com.shnok.javaserver.gameserver.model.actor.template.NpcTemplate;
import com.shnok.javaserver.gameserver.model.item.instance.ItemInstance;
import com.shnok.javaserver.gameserver.model.item.kind.Item;
import com.shnok.javaserver.gameserver.model.manor.Seed;
import com.shnok.javaserver.gameserver.model.residence.castle.Castle;
import com.shnok.javaserver.gameserver.network.SystemMessageId;
import com.shnok.javaserver.gameserver.network.serverpackets.combat.ActionFailed;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.BuyListSeed;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.ExShowCropInfo;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.ExShowManorDefaultInfo;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.ExShowProcureCropDetail;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.ExShowSeedInfo;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.ExShowSellCropList;
import com.shnok.javaserver.gameserver.network.serverpackets.unused.NpcHtmlMessage;
import com.shnok.javaserver.gameserver.network.serverpackets.SystemMessage;

public class ManorManagerNpc extends Merchant
{
	public ManorManagerNpc(int objectId, NpcTemplate template)
	{
		super(objectId, template);
	}
	
	@Override
	public void onBypassFeedback(Player player, String command)
	{
		if (command.startsWith("ExchangeCrop_"))
		{
			try
			{
				String[] parts = command.split("_");
				int cropId = Integer.parseInt(parts[1]);
				int rewardType = Integer.parseInt(parts[2]);
				
				ItemInstance item = player.getInventory().getItemByItemId(cropId);
				if (item == null || item.getCount() <= 0)
				{
					player.sendMessage("You don't have this crop.");
					showCropSalesDialog(player);
					return;
				}
				
				Seed seed = CastleManorManager.getInstance().getSeedByCrop(cropId);
				int rewardId = (rewardType == 1) ? (seed != null ? seed.getReward1() : 1864) : (seed != null ? seed.getReward2() : 1865);
				Item rewardTmpl = ItemData.getInstance().getTemplate(rewardId);
				int rewardPrice = (rewardTmpl != null && rewardTmpl.getReferencePrice() > 0) ? rewardTmpl.getReferencePrice() : 100;
				int cropPrice = (seed != null && seed.getCropReferencePrice() > 0) ? seed.getCropReferencePrice() : 100;
				
				int count = item.getCount();
				int rewardCount = Math.max(1, (cropPrice * count) / rewardPrice);
				
				if (player.destroyItem(item, count, true))
				{
					player.addItem(rewardId, rewardCount, true);
					player.sendMessage("Exchanged " + count + " crops for " + rewardCount + " " + (rewardTmpl != null ? rewardTmpl.getName() : "items") + "!");
				}
				showCropSalesDialog(player);
			}
			catch (Exception e)
			{
				LOGGER.error("Error exchanging crop: " + command, e);
			}
			return;
		}
		
		if (command.startsWith("manor_menu_select"))
		{
			if (CastleManorManager.getInstance().isUnderMaintenance())
			{
				player.sendPacket(ActionFailed.STATIC_PACKET);
				player.sendPacket(SystemMessageId.THE_MANOR_SYSTEM_IS_CURRENTLY_UNDER_MAINTENANCE);
				return;
			}
			
			final StringTokenizer st = new StringTokenizer(command, "&");
			
			final int ask = Integer.parseInt(st.nextToken().split("=")[1]);
			final int state = Integer.parseInt(st.nextToken().split("=")[1]);
			final boolean time = st.nextToken().split("=")[1].equals("1");
			
			final int castleId = (state < 0) ? getCastle().getId() : state;
			
			switch (ask)
			{
				case 1: // Seed purchase
					showBuyWindow(player, 335104);
					player.sendPacket(new BuyListSeed(player.getAdena(), castleId));
					break;
				
				case 2: // Crop sales (Tally up indigenous product)
					showCropSalesDialog(player);
					break;
				
				case 3: // Current seeds (Manor info)
					player.sendPacket(new ExShowSeedInfo(castleId, time, false));
					break;
				
				case 4: // Current crops (Manor info)
					player.sendPacket(new ExShowCropInfo(castleId, time, false));
					break;
				
				case 5: // Basic info (Manor info)
					player.sendPacket(new ExShowManorDefaultInfo(false));
					break;
				
				case 6: // Buy harvester
					showBuyWindow(player, 335103);
					break;
				
				case 9: // Edit sales (Crop sales)
					player.sendPacket(new ExShowProcureCropDetail(state));
					break;
			}
		}
		else
			super.onBypassFeedback(player, command);
	}
	
	private void showCropSalesDialog(Player player)
	{
		final StringBuilder sb = new StringBuilder();
		sb.append("<html><body><center><font color=\"LEVEL\">Manor Crop Exchange</font></center><br>");
		
		int found = 0;
		for (int cropId : CastleManorManager.getInstance().getCropIds())
		{
			ItemInstance item = player.getInventory().getItemByItemId(cropId);
			if (item != null && item.getCount() > 0)
			{
				found++;
				Seed seed = CastleManorManager.getInstance().getSeedByCrop(cropId);
				int cropPrice = (seed != null && seed.getCropReferencePrice() > 0) ? seed.getCropReferencePrice() : 100;
				int r1 = (seed != null) ? seed.getReward1() : 1864; // Stem
				int r2 = (seed != null) ? seed.getReward2() : 1865; // Varnish
				
				Item itemTmpl = ItemData.getInstance().getTemplate(cropId);
				Item r1Tmpl = ItemData.getInstance().getTemplate(r1);
				Item r2Tmpl = ItemData.getInstance().getTemplate(r2);
				
				int r1Price = (r1Tmpl != null && r1Tmpl.getReferencePrice() > 0) ? r1Tmpl.getReferencePrice() : 100;
				int r2Price = (r2Tmpl != null && r2Tmpl.getReferencePrice() > 0) ? r2Tmpl.getReferencePrice() : 200;
				
				int r1Count = Math.max(1, (cropPrice * item.getCount()) / r1Price);
				int r2Count = Math.max(1, (cropPrice * item.getCount()) / r2Price);
				
				String cropName = itemTmpl != null ? itemTmpl.getName() : ("Crop #" + cropId);
				String r1Name = r1Tmpl != null ? r1Tmpl.getName() : ("Item #" + r1);
				String r2Name = r2Tmpl != null ? r2Tmpl.getName() : ("Item #" + r2);
				
				sb.append("<table width=270><tr><td><b>").append(cropName).append("</b> (").append(item.getCount()).append(" pcs)</td></tr>");
				sb.append("<tr><td><a action=\"bypass -h npc_").append(getObjectId()).append("_ExchangeCrop_").append(cropId).append("_1\">")
				  .append("Exchange for ").append(r1Name).append(" (x").append(r1Count).append(")</a></td></tr>");
				sb.append("<tr><td><a action=\"bypass -h npc_").append(getObjectId()).append("_ExchangeCrop_").append(cropId).append("_2\">")
				  .append("Exchange for ").append(r2Name).append(" (x").append(r2Count).append(")</a></td></tr>");
				sb.append("</table><br>");
			}
		}
		
		if (found == 0)
		{
			sb.append("You do not have any harvested crops in your inventory.<br><br>");
			sb.append("To harvest crops:<br>");
			sb.append("1. Purchase seeds from me.<br>");
			sb.append("2. Purchase a Harvester.<br>");
			sb.append("3. Sow seeds into monsters and kill them.<br>");
			sb.append("4. Use the Harvester on their corpse to gather crops!<br><br>");
		}
		
		sb.append("<center><a action=\"bypass -h npc_").append(getObjectId()).append("_Chat 0\">Back</a></center></body></html>");
		
		final NpcHtmlMessage html = new NpcHtmlMessage(getObjectId());
		html.setHtml(sb.toString());
		player.sendPacket(html);
		
		player.sendPacket(new ExShowSellCropList(player.getInventory(), getCastle().getId()));
	}
	
	@Override
	public String getHtmlPath(int npcId, int val)
	{
		return "data/html/manormanager/manager.htm";
	}
	
	@Override
	public void showChatWindow(Player player)
	{
		if (!Config.ALLOW_MANOR)
		{
			showChatWindow(player, "data/html/npcdefault.htm");
			return;
		}
		
		if (getCastle() != null && player.getClan() != null && getCastle().getOwnerId() == player.getClanId() && player.isClanLeader())
			showChatWindow(player, "data/html/manormanager/manager-lord.htm");
		else
			showChatWindow(player, "data/html/manormanager/manager.htm");
	}
}
