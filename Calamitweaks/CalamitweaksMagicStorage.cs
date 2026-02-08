using CalamityMod.Items.Placeables.Furniture.CraftingStations;
using CatalystMod.Items.Placeable.Furniture.CraftingStations;
using MagicStorage.Stations;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod", "MagicStorage")]
	public class CalamitweaksMagicStorage : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod") && ModLoader.HasMod("MagicStorage");

		public override void PostAddRecipes()
		{
			// Do nothing if the Calamitweaks option isn't enabled ofc
			if (!Terratweaks.Calamitweaks.CombinedStationSupport)
				return;

			// Combined Furniture Stations Tier 2 - Now requires Ashen Altar, Plague Infuser, and Void Condenser
			Recipe comboFurniture2 = Main.recipe.First(r => r.HasResult(ModContent.ItemType<CombinedFurnitureStations2Item>()));
			comboFurniture2
				.AddIngredient(ModContent.ItemType<AshenAltar>())
				.AddIngredient(ModContent.ItemType<PlagueInfuser>())
				.AddIngredient(ModContent.ItemType<VoidCondenser>());

			// Combined Stations Final Tier - Now requires Profaned Crucible and Altar of the Accursed
			// Also requires Draedon's Forge in place of the Ancient Manipulator
			Recipe final = Main.recipe.First(r => r.HasResult(ModContent.ItemType<CombinedStations4Item>()));
			
			// Remove Ancient Manipulator since it's no longer needed, and remove lava and honey buckets so they appear after the other crafting stations
			final.RemoveIngredient(ItemID.LunarCraftingStation);
			final.RemoveIngredient(ItemID.LavaBucket);
			final.RemoveIngredient(ItemID.HoneyBucket);

			// Now we can actually add stuff!
			final
				.AddIngredient(ModContent.ItemType<DraedonsForge>())
				.AddIngredient(ModContent.ItemType<ProfanedCrucible>())
				.AddIngredient(ModContent.ItemType<AltarOfTheAccursedItem>())
				.AddIngredient(ItemID.LavaBucket, 10)
				.AddIngredient(ItemID.HoneyBucket, 10);
		}
	}

	[JITWhenModsEnabled("CatalystMod", "MagicStorage")]
	public class CatalystMagicStorage : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CatalystMod") && ModLoader.HasMod("MagicStorage");

		public override void PostAddRecipes()
		{
			// Do nothing if the Calamitweaks option isn't enabled ofc
			if (!Terratweaks.Calamitweaks.CombinedStationSupport)
				return;

			// Combined Stations Final Tier - Now requires Astral Transmogrifier
			Recipe final = Main.recipe.First(r => r.HasResult(ModContent.ItemType<CombinedStations4Item>()));

			// Remove lava and honey so we can shift them to be placed after the transmogrifier
			final.RemoveIngredient(ItemID.LavaBucket);
			final.RemoveIngredient(ItemID.HoneyBucket);

			// Add the actual items!
			final
				.AddIngredient(ModContent.ItemType<AstralTransmogrifier>())
				.AddIngredient(ItemID.LavaBucket, 10)
				.AddIngredient(ItemID.HoneyBucket, 10);
		}
	}
}