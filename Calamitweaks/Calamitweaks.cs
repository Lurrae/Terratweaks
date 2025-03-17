using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Items.Placeables;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
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
		private static readonly MethodInfo _calGlobalNpcPostAi = typeof(CalamityGlobalNPC).GetMethod("PostAI", BindingFlags.Instance | BindingFlags.Public);
		
		public override void Load()
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.RevertVanillaBossAIChanges)
			{
				CalamityMod.CalamityMod.ExternalFlag_DisableNonRevBossAI = true;
			}

			MonoModHooks.Add(_addNewNames, DisablePatreonNames);
			MonoModHooks.Add(_pillarEventProgressionEdit, ProgressionEditRemover);
			MonoModHooks.Add(_blockDrops, PreventFoodDropBlocking);
			MonoModHooks.Add(_onionSlotIsEnabled, EnableOnionSlotInMasterMode);
			MonoModHooks.Add(_canUseOnion, EnableOnionUseInMasterMode);
			
			//IL.CalamityMod.NPCs.CalamityGlobalNPC.PostAI += DisableEnemyParticles;
			MonoModHooks.Modify(_calGlobalNpcPostAi, DisableEnemyParticles);

			// Add more swords to Zenith profiles
			if (calamitweaks.ZenithRecipeOverhaul)
			{
				FieldInfo _zenithProfiles = typeof(FinalFractalHelper).GetField("_fractalProfiles", BindingFlags.Static | BindingFlags.NonPublic);
				Dictionary<int, FinalFractalHelper.FinalFractalProfile> profiles = (Dictionary<int, FinalFractalHelper.FinalFractalProfile>)_zenithProfiles.GetValue(null);
				AddCalamityZenithProfiles(profiles);
			}
		}

		public static void AddCalamityZenithProfiles(Dictionary<int, FinalFractalHelper.FinalFractalProfile> profiles)
		{
			#region Terratomere crafting tree
			profiles.Add(ModContent.ItemType<Terratomere>(), new FinalFractalHelper.FinalFractalProfile(66f, new Color(80, 222, 122)));
			profiles.Add(ModContent.ItemType<Hellkite>(), new FinalFractalHelper.FinalFractalProfile(84f, new Color(254, 158, 35)));
			profiles.Add(ModContent.ItemType<Floodtide>(), new FinalFractalHelper.FinalFractalProfile(80f, new Color(91, 158, 232)));
			#endregion

			#region Ark of the Cosmos crafting tree
			profiles.Add(ModContent.ItemType<ArkoftheCosmos>(), new FinalFractalHelper.FinalFractalProfile(132f, new Color(255, 255, 255)));
			profiles.Add(ModContent.ItemType<ArkoftheElements>(), new FinalFractalHelper.FinalFractalProfile(112f, new Color(255, 255, 255)));
			profiles.Add(ModContent.ItemType<TrueArkoftheAncients>(), new FinalFractalHelper.FinalFractalProfile(72f, new Color(255, 255, 255)));
			profiles.Add(ModContent.ItemType<FracturedArk>(), new FinalFractalHelper.FinalFractalProfile(60f, new Color(255, 255, 255)));
			profiles.Add(ModContent.ItemType<FourSeasonsGalaxia>(), new FinalFractalHelper.FinalFractalProfile(126f, new Color(255, 255, 255)));
			profiles.Add(ModContent.ItemType<TrueBiomeBlade>(), new FinalFractalHelper.FinalFractalProfile(68f, new Color(255, 255, 255)));
			profiles.Add(ModContent.ItemType<OmegaBiomeBlade>(), new FinalFractalHelper.FinalFractalProfile(86f, new Color(255, 255, 255)));
			profiles.Add(ModContent.ItemType<BrokenBiomeBlade>(), new FinalFractalHelper.FinalFractalProfile(36f, new Color(255, 255, 255)));
			profiles.Add(ItemID.WoodenSword, new FinalFractalHelper.FinalFractalProfile(32f, new Color(255, 255, 255)));
			profiles.Add(ItemID.BorealWoodSword, new FinalFractalHelper.FinalFractalProfile(32f, new Color(255, 255, 255)));
			profiles.Add(ItemID.RichMahoganySword, new FinalFractalHelper.FinalFractalProfile(32f, new Color(255, 255, 255)));
			profiles.Add(ItemID.PalmWoodSword, new FinalFractalHelper.FinalFractalProfile(32f, new Color(255, 255, 255)));
			profiles.Add(ItemID.EbonwoodSword, new FinalFractalHelper.FinalFractalProfile(32f, new Color(255, 255, 255)));
			profiles.Add(ItemID.ShadewoodSword, new FinalFractalHelper.FinalFractalProfile(32f, new Color(255, 255, 255)));
			profiles.Add(ItemID.PearlwoodSword, new FinalFractalHelper.FinalFractalProfile(32f, new Color(255, 255, 255)));
			profiles.Add(ItemID.AshWoodSword, new FinalFractalHelper.FinalFractalProfile(32f, new Color(255, 255, 255)));
			#endregion

			#region Plague Keeper crafting tree
			profiles.Add(ModContent.ItemType<PlagueKeeper>(), new FinalFractalHelper.FinalFractalProfile(90f, new Color(80, 222, 122)));
			profiles.Add(ModContent.ItemType<Virulence>(), new FinalFractalHelper.FinalFractalProfile(62f, new Color(80, 222, 122)));
			#endregion

			#region Swords that are not part of any grand crafting tree
			profiles.Add(ModContent.ItemType<Excelsus>(), new FinalFractalHelper.FinalFractalProfile(78f, new Color(255, 231, 255)));
			profiles.Add(ModContent.ItemType<GalactusBlade>(), new FinalFractalHelper.FinalFractalProfile(60f, new Color(236, 62, 152)));
			profiles.Add(ModContent.ItemType<VoidEdge>(), new FinalFractalHelper.FinalFractalProfile(122f, new Color(62, 0, 66)));
			profiles.Add(ModContent.ItemType<HolyCollider>(), new FinalFractalHelper.FinalFractalProfile(140f, new Color(255, 231, 69)));
			profiles.Add(ModContent.ItemType<AstralBlade>(), new FinalFractalHelper.FinalFractalProfile(80f, new Color(255, 231, 255)));
			profiles.Add(ModContent.ItemType<Greentide>(), new FinalFractalHelper.FinalFractalProfile(62f, new Color(80, 222, 122)));
			profiles.Add(ModContent.ItemType<BrimstoneSword>(), new FinalFractalHelper.FinalFractalProfile(52f, new Color(237, 28, 36)));
			profiles.Add(ModContent.ItemType<PerfectDark>(), new FinalFractalHelper.FinalFractalProfile(50f, new Color(122, 66, 191)));
			profiles.Add(ModContent.ItemType<VeinBurster>(), new FinalFractalHelper.FinalFractalProfile(52f, new Color(237, 28, 36)));
			profiles.Add(ModContent.ItemType<SeashineSword>(), new FinalFractalHelper.FinalFractalProfile(40f, new Color(91, 158, 232)));
			#endregion
		}

		public static bool CheckNoWormParticles(bool calamityResult)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;
			return calamityResult && !calamitweaks.NoWormParticles;
		}

		public static bool CheckNoPlantParticles(bool calamityResult)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;
			return calamityResult && !calamitweaks.NoPlantParticles;
		}

		private void DisableEnemyParticles(ILContext il)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			var c = new ILCursor(il);

			for (int i = 0; i < 3; i++)
			{
				if (!c.TryGotoNext(i => i.MatchCall("Terraria.Utils", "NextBool")))
				{
					Mod.Logger.Warn($"Calamitweaks IL edit failed when trying to disable {(i == 0 ? "worm" : "plant")} particles! Dumping IL logs...");
					MonoModHooks.DumpIL(Mod, il);
					return;
				}

				c.Index++;
				if (i == 0)
					c.Emit(OpCodes.Call, GetType().GetMethod(nameof(CheckNoWormParticles), BindingFlags.Public | BindingFlags.Static));
				else
					c.Emit(OpCodes.Call, GetType().GetMethod(nameof(CheckNoPlantParticles), BindingFlags.Public | BindingFlags.Static));
			}
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

		public override void AddRecipeGroups()
		{
			RecipeGroup group = new(() => Language.GetTextValue("Mods.Terratweaks.RecipeGroups.CalEvilSwords"), ModContent.ItemType<PerfectDark>(), ModContent.ItemType<VeinBurster>());
			RecipeGroup.RegisterGroup("CalEvilSwords", group);
		}

		public override void PostAddRecipes()
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// If none of these configs are active, we don't need to change any recipes
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