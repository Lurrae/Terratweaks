using MagicStorage.Stations;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.ArcaneArmor;
using ThoriumMod.Items.Placeable;
using ThoriumMod.Items.Thorium;

namespace Terratweaks.Thoritweaks
{
	[JITWhenModsEnabled("ThoriumMod", "MagicStorage")]
	public class ThoritweaksMagicStorage : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("ThoriumMod") && ModLoader.HasMod("MagicStorage");

		public override void PostAddRecipes()
		{
			// Do nothing if the Thoritweaks option isn't enabled ofc
			if (!Terratweaks.Thoritweaks.CombinedStationSupport)
				return;

			// Combined Stations Tier 1 - Now requires Arcane Armor Fabricator, and Thorium Anvil in place of the Iron/Lead Anvil
			Recipe comboStations1 = Main.recipe.First(r => r.HasResult(ModContent.ItemType<CombinedStations1Item>()));

			// Remove Iron/Lead Anvil since it's no longer needed
			comboStations1.RemoveRecipeGroup(RecipeGroup.recipeGroupIDs["MagicStorage:AnyPreHmAnvil"]);
			comboStations1.RemoveIngredient(ItemID.IronAnvil);
			comboStations1.RemoveIngredient(ItemID.LeadAnvil);

			// Add the new stations
			// The Thorium Anvil won't be placed where the regular anvils are in the recipe, but that's fine
			comboStations1
				.AddIngredient(ModContent.ItemType<ThoriumAnvil>())
				.AddIngredient(ModContent.ItemType<ArcaneArmorFabricator>());

			// Combined Stations Tier 3 - Now requires Soul Forge in place of the Adamantite/Titanium Forge
			Recipe comboStations3 = Main.recipe.First(r => r.HasResult(ModContent.ItemType<CombinedStations3Item>()));

			// Remove Adamantite/Titanium Forge
			comboStations3.RemoveRecipeGroup(RecipeGroup.recipeGroupIDs["MagicStorage:AnyHmFurnace"]);
			comboStations3.RemoveIngredient(ItemID.AdamantiteForge);
			comboStations3.RemoveIngredient(ItemID.TitaniumForge);

			// Add the Soul Forge
			// Like the Thorium Anvil, it'll be in a different spot in the recipe, but that's ok
			comboStations3
				.AddIngredient(ModContent.ItemType<SoulForge>());
		}
	}
}