using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.NPCs;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class Calamitweaks : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		private static readonly MethodInfo _pillarEventProgressionEdit = typeof(CalamityGlobalNPC).GetMethod("PillarEventProgressionEdit", BindingFlags.Instance | BindingFlags.NonPublic);

		public override void Load()
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// Calamity changes how many tiles are needed to form a graveyard
			// These values are the vanilla amounts
			if (calamitweaks.RevertGraveyards)
			{
				SceneMetrics.GraveyardTileMax = 36;
				SceneMetrics.GraveyardTileMin = 16;
				SceneMetrics.GraveyardTileThreshold = 28;
			}

			if (calamitweaks.RevertVanillaBossAIChanges)
			{
				CalamityMod.CalamityMod.ExternalFlag_DisableNonRevBossAI = true;
			}

			MonoModHooks.Add(_pillarEventProgressionEdit, ProgressionEditRemover);
		}

		private static void ProgressionEditRemover(Action<CalamityGlobalNPC, NPC> orig, CalamityGlobalNPC self, NPC npc)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// By not calling the original function, it never runs, and therefore pillar enemies spawn as they do in vanilla!
			if (calamitweaks.RevertPillars)
				return;

			orig(self, npc);
		}

		public override void PostAddRecipes()
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// If neither of these configs are active, we don't need to change any recipes
			// This is done for performance reasons- there's no need to iterate over every recipe if we know we won't be changing any
			if (!calamitweaks.DeificAmuletBuff && !calamitweaks.AsgardsValorBuff)
				return;

			foreach (Recipe recipe in Main.recipe)
			{
				if (calamitweaks.DeificAmuletBuff && recipe.HasResult(ModContent.ItemType<DeificAmulet>()))
				{
					// Remove these ingredients so the recipe displays in the correct order
					recipe.RemoveIngredient(ModContent.ItemType<AstralBar>());
					recipe.RemoveIngredient(ModContent.ItemType<SeaPrism>());

					// Add Charm of Myths, then put the ingredients back (they had to be re-added so that they'll show up after Star Veil and Charm of Myths)
					recipe.AddIngredient(ItemID.CharmofMyths)
						.AddIngredient(ModContent.ItemType<AstralBar>(), 10)
						.AddIngredient(ModContent.ItemType<SeaPrism>(), 15);
				}

				if (calamitweaks.AsgardsValorBuff && recipe.HasResult(ModContent.ItemType<AsgardsValor>()))
				{
					// Remove these ingredients so the recipe displays in the correct order
					recipe.RemoveIngredient(ModContent.ItemType<CoreofCalamity>());

					// Add Shield of the Ocean and Deep Diver, then put the ingredients back
					recipe.AddIngredient(ModContent.ItemType<ShieldoftheOcean>())
						.AddIngredient(ModContent.ItemType<DeepDiver>())
						.AddIngredient(ModContent.ItemType<CoreofCalamity>());
				}
			}
		}
	}
}