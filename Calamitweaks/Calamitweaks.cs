using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Items.Placeables;
using CalamityMod.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
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

		private static readonly MethodInfo _addNewNames = typeof(CalamityGlobalNPC).GetMethod("AddNewNames", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo _pillarEventProgressionEdit = typeof(CalamityGlobalNPC).GetMethod("PillarEventProgressionEdit", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo _blockDrops = typeof(DropHelper).GetMethod("BlockDrops", BindingFlags.Static | BindingFlags.Public);
		private static readonly MethodInfo _canUseOnion = typeof(CelestialOnion).GetMethod("CanUseItem", BindingFlags.Instance | BindingFlags.Public);
		private static readonly MethodInfo _onionSlotIsEnabled = typeof(CelestialOnionAccessorySlot).GetMethod("IsEnabled", BindingFlags.Instance | BindingFlags.Public);
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

			MonoModHooks.Add(_addNewNames, DisablePatreonNames);
			MonoModHooks.Add(_pillarEventProgressionEdit, ProgressionEditRemover);
			MonoModHooks.Add(_blockDrops, PreventFoodDropBlocking);
			MonoModHooks.Add(_onionSlotIsEnabled, EnableOnionSlotInMasterMode);
			MonoModHooks.Add(_canUseOnion, EnableOnionUseInMasterMode);
		}

		private static void DisablePatreonNames(Action<CalamityGlobalNPC, List<string>, string[]> orig, CalamityGlobalNPC self, List<string> nameList, string[] patreonNames)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// By not calling the original function, it never runs, which should mean no patreon names!
			// This likely won't change the names of NPCs already in the world but-
			if (calamitweaks.NoPatreonNPCNames)
				return;

			orig(self, nameList, patreonNames);
		}

		private static void ProgressionEditRemover(Action<CalamityGlobalNPC, NPC> orig, CalamityGlobalNPC self, NPC npc)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// By not calling the original function, it never runs, and therefore pillar enemies spawn as they do in vanilla!
			if (calamitweaks.RevertPillars)
				return;

			orig(self, npc);
		}

		private static void PreventFoodDropBlocking(Action<int[]> orig, params int[] itemIDs)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// Like with ProgressionEditRemover, not calling the original function prevents it from running
			// In this case, it should always run the original function UNLESS trying to block enemy food drops, that way we don't run into issues
			// with other items being left unblocked (such as the evil ores and materials from EoW's segments or BoC's creepers)
			if (itemIDs.Contains(ItemID.ApplePie) && calamitweaks.EnemyFoodDrops)
				return;

			orig(itemIDs);
		}

		private static bool EnableOnionUseInMasterMode(Func<ModItem, Player, bool> orig, ModItem self, Player player)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.OnionMasterMode)
			{
				return !player.Calamity().extraAccessoryML;
			}

			return orig(self, player);
		}

		private static bool EnableOnionSlotInMasterMode(Func<ModAccessorySlot, bool> orig, ModAccessorySlot self)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.OnionMasterMode)
			{
				if (!ModAccessorySlot.Player.active)
					return false;

				return ModAccessorySlot.Player.Calamity().extraAccessoryML;
			}
			
			return orig(self);
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