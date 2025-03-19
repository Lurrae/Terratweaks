using AlchemistNPCLite.NPCs;
using Terraria.ModLoader;

namespace Terratweaks.Alchemitweaks
{
	[JITWhenModsEnabled("AlchemistNPCLite")]
	public class AlchemitweaksNPCs : GlobalNPC
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("AlchemistNPCLite");

		public override void ModifyShop(NPCShop shop)
		{
			if (Terratweaks.Alchemitweaks.DisableCustomPotions)
			{
				if (shop.NpcType == ModContent.NPCType<Brewer>())
				{
					foreach (NPCShop.Entry entry in shop.ActiveEntries)
					{
						// Item is a modded item from Alch NPC Lite
						if (entry.Item.ModItem != null && entry.Item.ModItem.Mod == AlchemistNPCLite.AlchemistNPCLite.Instance)
						{
							// Check if the item's actually a potion
							if (entry.Item.buffType > 0)
							{
								// Disable the item, because it's almost certainly one of the alch mod's potions
								entry.Disable();
							}
						}
					}
				}
			}

			if (Terratweaks.Alchemitweaks.AntiCheese)
			{
				if (shop.NpcType == ModContent.NPCType<Architect>())
				{
					foreach (NPCShop.Entry entry in shop.ActiveEntries)
					{
						// Any item that has a sell price above 0 should be sold for its sell price
						// The Architect normally sells items for less than the regular sell price, which causes an infinite money exploit
						if (entry.Item.value > 0)
						{
							entry.Item.shopCustomPrice = null;
						}
					}
				}
			}
		}
	}
}