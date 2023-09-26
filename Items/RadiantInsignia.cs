using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks.Items
{
	public class RadiantInsignia : ModItem
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return GetInstance<TerratweaksConfig>().vanillaChanges.SIRework; // Item is only loaded in if SI rework is enabled
		}

		public override void SetStaticDefaults()
		{
			ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.EmpressFlightBooster; // Enable Shimmer decrafting into regular Soaring Insignia
		}

		public override void SetDefaults() 
		{
			Item.CloneDefaults(ItemID.EmpressFlightBooster);
		}
		
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.empressBrooch = true;
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