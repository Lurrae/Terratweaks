using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Items
{
	public class RadiantInsignia : ModItem
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return Terratweaks.Config.SIRework; // Item is only loaded in if SI rework is enabled
		}

		public override void SetStaticDefaults()
		{
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.EmpressFlightBooster; // Enable Shimmer decrafting into regular Soaring Insignia
		}

		public override void SetDefaults() 
		{
			Item.CloneDefaults(ItemID.EmpressFlightBooster);
			Item.width = 34;
			Item.height = 42;

			// Remove R.Insignia from the list of items that should display a "modified by Terratweaks" tooltip
			Item.StatsModifiedBy.Remove(Mod);
		}
		
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<TerratweaksPlayer>().radiantInsignia = true;
			player.moveSpeed += 0.1f;
		}

		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
		{
			if (equippedItem.type == ItemID.EmpressFlightBooster || incomingItem.type == ItemID.EmpressFlightBooster)
			{
				return false;
			}

			return base.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
		}
	}
}