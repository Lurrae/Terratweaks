global using TepigCore;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terratweaks.Calamitweaks;
using Terratweaks.Items;
using Terratweaks.NPCs;

namespace Terratweaks
{
	public enum PacketType
	{
		SyncInferno = 0
	}

	public static class TerratweaksDropConditions
	{
		public class NightEoL : IItemDropRuleCondition, IProvideItemConditionDescription
		{
			public bool CanDrop(DropAttemptInfo info)
			{
				return !info.npc.AI_120_HallowBoss_IsGenuinelyEnraged();
			}

			public bool CanShowItemDropInUI()
			{
				return true;
			}

			public string GetConditionDescription()
			{
				return Language.GetTextValue("Mods.Terratweaks.Conditions.NightEoL");
			}
		}

		public class DownedMoonlord : IItemDropRuleCondition, IProvideItemConditionDescription
		{
			public bool CanDrop(DropAttemptInfo info)
			{
				return NPC.downedMoonlord;
			}

			public bool CanShowItemDropInUI()
			{
				return true;
			}

			public string GetConditionDescription()
			{
				return Language.GetTextValue("Mods.Terratweaks.Conditions.DownedMoonlord");
			}
		}
	}

	public class Terratweaks : Mod
	{
		public static bool playerHasChesterSafeOpened = false;

		public static ModKeybind InfernoToggleKeybind { get; private set; }
		public static ModKeybind RulerToggleKeybind { get; private set; }
		public static ModKeybind MechRulerToggleKeybind { get; private set; }
		public static readonly List<int> DyeItemsSoldByTrader = [];

		public static readonly Dictionary<int, FinalFractalHelper.FinalFractalProfile> defaultProfileList = new()
		{
			{ ItemID.Starfury, new FinalFractalHelper.FinalFractalProfile(48f, new Color(236, 62, 192)) },
			{ ItemID.BeeKeeper, new FinalFractalHelper.FinalFractalProfile(48f, Main.OurFavoriteColor) },
			{ ItemID.LightsBane, new FinalFractalHelper.FinalFractalProfile(48f, new Color(122, 66, 191)) },
			{ ItemID.FieryGreatsword, new FinalFractalHelper.FinalFractalProfile(76f, new Color(254, 158, 35)) },
			{ ItemID.BladeofGrass, new FinalFractalHelper.FinalFractalProfile(70f, new Color(107, 203, 0)) },
			{ ItemID.Excalibur, new FinalFractalHelper.FinalFractalProfile(70f, new Color(236, 200, 19)) },
			{ ItemID.TrueExcalibur, new FinalFractalHelper.FinalFractalProfile(70f, new Color(236, 200, 19)) },
			{ ItemID.NightsEdge, new FinalFractalHelper.FinalFractalProfile(70f, new Color(179, 54, 201)) },
			{ ItemID.TrueNightsEdge, new FinalFractalHelper.FinalFractalProfile(70f, new Color(179, 54, 201)) },
			{ ItemID.InfluxWaver, new FinalFractalHelper.FinalFractalProfile(70f, new Color(84, 234, 245)) },
			{ ItemID.EnchantedSword, new FinalFractalHelper.FinalFractalProfile(48f, new Color(91, 158, 232)) },
			{ ItemID.TheHorsemansBlade, new FinalFractalHelper.FinalFractalProfile(76f, new Color(252, 95, 4)) },
			{ ItemID.Meowmere, new FinalFractalHelper.FinalFractalProfile(76f, new Color(254, 194, 250)) },
			{ ItemID.StarWrath, new FinalFractalHelper.FinalFractalProfile(70f, new Color(237, 63, 133)) },
			{ ItemID.TerraBlade, new FinalFractalHelper.FinalFractalProfile(70f, new Color(80, 222, 122)) },
			{ ItemID.Muramasa, new FinalFractalHelper.FinalFractalProfile(70f, new Color(56, 78, 210)) },
			{ ItemID.BloodButcherer, new FinalFractalHelper.FinalFractalProfile(70f, new Color(237, 28, 36)) },
			{ ItemID.Seedler, new FinalFractalHelper.FinalFractalProfile(80f, new Color(143, 215, 29)) },
			{ ItemID.Terragrim, new FinalFractalHelper.FinalFractalProfile(45f, new Color(178, 255, 180)) },
			{ ItemID.CopperShortsword, new FinalFractalHelper.FinalFractalProfile(45f, new Color(235, 166, 135)) },
			{ ItemID.Zenith, new FinalFractalHelper.FinalFractalProfile(86f, new Color(178, 255, 180)) }
		};

		public override void Load()
		{
			InfernoToggleKeybind = KeybindLoader.RegisterKeybind(this, "InfernoToggle", "I");
			RulerToggleKeybind = KeybindLoader.RegisterKeybind(this, "RulerToggle", "NumPad1");
			MechRulerToggleKeybind = KeybindLoader.RegisterKeybind(this, "MechRulerToggle", "NumPad2");
			
			On_Main.DamageVar_float_int_float += DisableDamageVariance;
			On_Main.DrawInfernoRings += HideInfernoVisuals;
			On_Main.TryInteractingWithMoneyTrough += ChesterRework_OpenSafe;
			On_Player.HandleBeingInChestRange += ChesterRework_Variables;
			On_Player.TakeDamageFromJellyfish += IncreasedJellyfishDamage;
			On_Player.UpdateJumpHeight += RadiantInsigniaJumpHeight;
			On_NPC.CountKillForBannersAndDropThem += BannerCombatText;
			On_NPC.HitModifiers.GetDamage += CritsBypassDefense;
			On_ShopHelper.ProcessMood += CustomHappinessFactors;
		}

		public override void PostSetupContent()
		{
			foreach (NPC npc in ContentSamples.NpcsByNetId.Values.Where(n => n.aiStyle == NPCAIStyleID.GraniteElemental))
			{
				StatChangeHandler.DREnemy drEnemyStats = new(0.25f, 1.1f, (NPC npc) => npc.ai[0] == -1);
				if (!StatChangeHandler.damageResistantEnemies.TryAdd(npc.type, drEnemyStats))
				{
					StatChangeHandler.damageResistantEnemies[npc.type] = drEnemyStats;
				}
			}

			// Block only boss NPCs (and EoW) from stealing coins
			TerratweaksConfig config = ModContent.GetInstance<TerratweaksConfig>();
			if (config.NoCoinTheft == CoinTheftSetting.Limited)
			{
				foreach (NPC npc in ContentSamples.NpcsByNetId.Values)
				{
					if ((npc.type <= NPCID.EaterofWorldsHead && npc.type >= NPCID.EaterofWorldsTail) || npc.boss)
						NPCID.Sets.CantTakeLunchMoney[npc.type] = true;
				}
			}
			// Block all enemies from stealing coins
			else if (config.NoCoinTheft == CoinTheftSetting.On)
			{
				foreach (NPC npc in ContentSamples.NpcsByNetId.Values)
				{
					NPCID.Sets.CantTakeLunchMoney[npc.type] = true;
				}
			}
		}

		public override object Call(params object[] args)
		{
			if (args[0] is string content)
			{
				switch (content)
				{
					case "Query":
						if (args[1] is string settingToQuery)
						{
							TerratweaksConfig config = ModContent.GetInstance<TerratweaksConfig>();
							TerratweaksConfig_Client clientConfig = ModContent.GetInstance<TerratweaksConfig_Client>();

							return settingToQuery switch
							{
								#region Returns
								"BannersDontSpamChat" => config.BannersDontSpamChat,
								"BetterBestiary" => config.BetterBestiary,
								"BetterCrackedBricks" => config.BetterCrackedBricks,
								"BetterHappiness" => config.BetterHappiness,
								"BossesLowerSpawnRates" => config.BossesLowerSpawnRates,
								"PlayerDeathsToCallOffInvasion" => config.PlayerDeathsToCallOffInvasion,
								"NPCDeathsToCallOffInvasion" => config.NPCDeathsToCallOffInvasion,
								"ChesterRework" => config.ChesterRework,
								"CritsBypassDefense" => config.CritsBypassDefense,
								"DangersenseHighlightsSilt" => config.DangersenseHighlightsSilt,
								"DangersenseIgnoresThinIce" => config.DangersenseIgnoresThinIce,
								"DeerclopsRegens" => config.DeerclopsRegens,
								"DeerRegenAmt" => config.DeerRegenAmt,
								"DeerWeaponsRework" => config.DeerWeaponsRework,
								"DummyFix" => (int)config.DummyFix,
								"DyeTraderShopExpansion" => config.DyeTraderShopExpansion,
								"ForceBossContactDamage" => config.ForceBossContactDamage,
								"HouseSizeAffectsHappiness" => config.HouseSizeAffectsHappiness,
								"KillSentries" => config.KillSentries,
								"ManaFreeSummoner" => config.ManaFreeSummoner,
								"NoCasterContactDamage" => config.NoCasterContactDamage,
								"NoCoinTheft" => (int)config.NoCoinTheft,
								"NoDamageVariance" => (int)config.NoDamageVariance,
								"NoDiminishingReturns" => config.NoDiminishingReturns,
								"NoEnemyInvulnerability" => config.NoEnemyInvulnerability,
								"NoExpertDebuffTimes" => config.NoExpertDebuffTimes,
								"NoExpertFreezingWater" => config.NoExpertFreezingWater,
								"NoExpertScaling" => config.NoExpertScaling,
								"NPCsSellMinecarts" => config.NPCsSellMinecarts,
								"OasisCrateBuff" => config.OasisCrateBuff,
								"OreUnification" => config.OreUnification,
								"OverrideGraveyardRequirements" => config.OverrideGraveyardRequirements,
								"GraveyardVisuals" => config.GraveyardVisuals,
								"GraveyardFunctionality" => config.GraveyardFunctionality,
								"GraveyardMax" => config.GraveyardMax,
								"LunarWingsPreML" or "EarlyLunarWings" => config.LunarWingsPreML,
								"PillarEnemiesDropFragments" => config.PillarEnemiesDropFragments,
								"PostEyeSandstorms" => config.PostEyeSandstorms,
								"ReaverSharkTweaks" or "ReaverTweaks" => config.ReaverSharkTweaks,
								"SIRework" or "SoaringInsigniaRework" => config.SIRework,
								"SmartMimics" => config.SmartMimics,
								"SmartNymphs" => config.SmartNymphs,
								"SoilSolutionsPreML" => config.SoilSolutionsPreML,
								"SolutionsOnGFB" => config.SolutionsOnGFB,
								"StackableDD2Accs" => (int)config.StackableDD2Accs,
								"TerraprismaDropRate" => config.TerraprismaDropRate,
								"TownNPCsSellWeapons" => config.TownNPCsSellWeapons,
								"UmbrellaHatRework" => config.UmbrellaHatRework,
								"ExpertAccBuffs_RoyalGel" or "RoyalGelBuff" => config.RoyalGel,
								"ExpertAccBuffs_HivePack" or "HivePackBuff" => config.HivePack,
								"ExpertAccBuffs_BoneHelm" or "BoneHelmBuff" => config.BoneHelm,
								"ArmorReworks_Spider" or "SpiderArmorSetBonus" or "SpiderSetBonus" => config.SpiderSetBonus,
								"ArmorReworks_Cobalt" or "CobaltArmorSetBonus" or "CobaltSetBonus" => config.CobaltSetBonus,
								"ArmorReworks_Mythril" or "MythrilArmorSetBonus" or "MythrilSetBonus" => config.MythrilSetBonus,
								"ArmorReworks_Adamantite" or "AdamantiteArmorSetBonus" or "AdamantiteSetBonus" => config.AdamantiteSetBonus,
								"ArmorReworks_Spooky" or "SpookyArmorSetBonus" or "SpookySetBonus" => config.SpookySetBonus,
								"OldChestDungeon" => config.OldChestDungeon,

								"Client_EstimatedDPS" or "EstimatedDPS" => clientConfig.EstimatedDPS,
								"Client_GrammarCorrections" or "GrammarCorrections" => clientConfig.GrammarCorrections,
								"Client_NoRandomCrit" or "NoRandomCrit" => clientConfig.NoRandomCrit,
								"Client_PermBuffTips" or "PermBuffTips" => clientConfig.PermBuffTips,
								"Client_StatsInTip" or "StatsInTip" => clientConfig.StatsInTip,
								"Client_WingStatsInTip" or "WingStatsInTip" => clientConfig.WingStatsInTip,

								"CraftableUncraftables_PlanterBoxes" or "PlanterBoxesRecipe" => config.craftableUncraftables.PlanterBoxes,
								"CraftableUncraftables_GemCritters" or "GemCrittersRecipe" => config.craftableUncraftables.GemCritters,
								"CraftableUncraftables_DungeonFurniture" or "DungeonFurnitureRecipe" => config.craftableUncraftables.DungeonFurniture,
								"CraftableUncraftables_ObsidianFurniture" or "ObsidianRecipe" => config.craftableUncraftables.ObsidianFurniture,
								"CraftableUncraftables_StructureBanners" or "StructureBannersRecipe" => config.craftableUncraftables.StructureBanners,
								"CraftableUncraftables_Moss" or "MossRecipe" => config.craftableUncraftables.Moss,
								"CraftableUncraftables_Gravestones" or "GravestonesRecipe" => config.craftableUncraftables.Gravestones,
								"CraftableUncraftables_GeyserTraps" or "GeyserTrapsRecipe" or "CraftableUncraftables_Geysers" or "GeysersRecipe" or "CraftableUncraftables_GeyserTrap" or "GeyserTrapRecipe" or "CraftableUncraftables_Geyser" or "GeyserRecipe" => config.craftableUncraftables.GeyserTraps,
								"CraftableUncraftables_Trophies" or "TrophiesRecipe" => config.craftableUncraftables.Trophies,
								"CraftableUncraftables_ClothierVoodooDoll" or "ClothierVoodooDollRecipe" or "ClothierDollRecipe" => config.craftableUncraftables.ClothierVoodooDoll,
								"CraftableUncraftables_TempleTraps" or "TempleTrapsRecipe" => config.craftableUncraftables.TempleTraps,
								"CraftableUncraftables_ShimmerBottomlessAndSponges" or "BottomlessAndSpongesShimmer" => config.craftableUncraftables.ShimmerBottomlessAndSponges,
								"CraftableUncraftables_TeamBlocks" or "TeamBlocksRecipe" => config.craftableUncraftables.TeamBlocks,
								"CraftableUncraftables_PrehardUnobtainables" or "PrehardUnobtainablesShimmer" => config.craftableUncraftables.PrehardUnobtainables,
								"CraftableUncraftables_ShimmerBossDrops" or "BossDropsShimmer" => config.craftableUncraftables.ShimmerBossDrops,

								"Calamitweaks_AquaticEmblemBuff" or "AquaticEmblemBuff" => config.calamitweaks.AquaticEmblemBuff,
								"Calamitweaks_AsgardsValorBuff" or "AsgardsValorBuff" => config.calamitweaks.AsgardsValorBuff,
								"Calamitweaks_CraftableHostileTurrets" or "CraftableUncraftables_HostileTurrets" or "HostileTurretsRecipe" or "HostileTurretsShimmer" => config.calamitweaks.CraftableHostileTurrets,
								"Calamitweaks_DeificAmuletBuff" or "DeificAmuletBuff" => config.calamitweaks.DeificAmuletBuff,
								"Calamitweaks_DRBuffs" or "CalDRBuffs" => config.calamitweaks.DRBuffs,
								"Calamitweaks_DryadSellsSeeds" or "DryadSellsCalSeeds" => config.calamitweaks.DryadSellsSeeds,
								"Calamitweaks_EnemyFoodDrops" or "EnemyFoodDrops" => config.calamitweaks.EnemyFoodDrops,
								"Calamitweaks_EnragedEoLInstakills" or "Calamitweaks_DayEoLInstakills" or "EnragedEoLInstakills" or "DayEoLInstakills" => config.calamitweaks.EnragedEoLInstakills,
								"Calamitweaks_EzCalBanners" or "EzCalBanners" => config.calamitweaks.EzCalBanners,
								"Calamitweaks_ForceWormContactDamage" or "ForceWormContactDamage" => config.calamitweaks.ForceWormContactDamage,
								"Calamitweaks_NoDefenseDamage" or "NoDefenseDamage" => config.calamitweaks.NoDefenseDamage,
								"Calamitweaks_NoPatreonNPCNames" or "NoCalPatreonNPCNames" => config.calamitweaks.NoPatreonNPCNames,
								"Calamitweaks_NoPlantParticles" or "NoPlantParticles" => config.calamitweaks.NoPlantParticles,
								"Calamitweaks_NoSellingRoD" or "NoSellingRoD" => config.calamitweaks.NoSellingRoD,
								"Calamitweaks_NoWormParticles" or "NoWormParticles" => config.calamitweaks.NoWormParticles,
								"Calamitweaks_OnionMasterMode" or "OnionMasterMode" => config.calamitweaks.OnionMasterMode,
								"Calamitweaks_RadiantInsigniaUpgradesFromAscendant" or "RadiantInsigniaUpgradesFromAscendant" => config.calamitweaks.RadiantInsigniaUpgradesFromAscendant,
								"Calamitweaks_RevertPickSpeedBuffs" or "PickSpeedCalReversion" => config.calamitweaks.RevertPickSpeedBuffs,
								"Calamitweaks_RevertPillars" or "PillarCalReversion" => config.calamitweaks.RevertPillars,
								"Calamitweaks_RevertTerraprisma" or "TerraprismaCalReversion" => config.calamitweaks.RevertTerraprisma,
								"Calamitweaks_RevertVanillaBossAIChanges" or "VanillaBossAICalReversion" => config.calamitweaks.RevertVanillaBossAIChanges,
								"Calamitweaks_SummonerAccBuffs" or "CalSummonerAccBuffs" => config.calamitweaks.SummonerAccBuffs,
								"Calamitweaks_ZenithRecipeOverhaul" or "CalZenithRecipeOverhaul" or "Calamitweaks_ZenithRecipe" or "CalZenithRecipe" => config.calamitweaks.ZenithRecipeOverhaul,

								"Thoritweaks_EatCooksFoodInCombat" or "Thoritweaks_CookBuff" or "EatCooksFoodInCombat" or "CookBuff" => config.thoritweaks.EatCooksFoodInCombat,
								"Thoritweaks_ZenithRecipeOverhaul" or "ThorZenithRecipeOverhaul" or "Thoritweaks_ZenithRecipe" or "ThorZenithRecipe" => config.thoritweaks.ZenithRecipeOverhaul,

								"Alchemitweaks_DisableCustomPotions" or "DisableCustomAlchPotions" => config.alchemitweaks.DisableCustomPotions,

								_ => throw new Exception($"Could not find Terratweaks config option named {args[1]}."),
								#endregion
							};
						}
						throw new ArgumentException($"Expected an argument of type string for 'Query', but got type {args[1].GetType().Name} instead.");
					case "AddPermConsumable":
						try
						{
							int itemID = Convert.ToInt32(args[1]);

							if (args[2] is Func<Player, bool> hasBeenUsed)
							{
								if (!TooltipChanges.PermBuffBools.TryAdd(itemID, hasBeenUsed))
								{
									TooltipChanges.PermBuffBools[itemID] = hasBeenUsed;
								}
								
								return true;
							}
							else if (args[2] is Func<Player, Vector2> amtConsumed)
							{
								if (!TooltipChanges.MultiPermBuffs.TryAdd(itemID, amtConsumed))
								{
									TooltipChanges.MultiPermBuffs[itemID] = amtConsumed;
								}
								
								return true;
							}
							else
							{
								throw new ArgumentException($"Expected a second argument of type Func<Player, bool> or Func<Player, Vector2> for 'AddPermConsumable', but got type {args[2].GetType().Name} instead.");
							}
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected a first argument of type int for 'AddPermConsumable', but got type {args[1].GetType().Name} instead.");
						}
					case "AddDefensiveEnemy":
						if (args[1] is string type)
						{
							switch (type)
							{
								case "DamageResistant":
									try
									{
										int npcID = Convert.ToInt32(args[2]);

										if (args[3] is float dmgResist && args[4] is float kbResist && args[5] is Func<NPC, bool> defensiveState)
										{
											StatChangeHandler.DREnemy drEnemyStats = new(dmgResist, kbResist, defensiveState);
											if (!StatChangeHandler.damageResistantEnemies.TryAdd(npcID, drEnemyStats))
											{
												StatChangeHandler.damageResistantEnemies[npcID] = drEnemyStats;
											}

											return true;
										}

										throw new ArgumentException($"Expected arguments of type int, float, float, and Func<NPC, bool> for 'AddDefensiveEnemy', but got types {args[2].GetType().Name}, {args[3].GetType().Name}, {args[4].GetType().Name}, and {args[5].GetType().Name} instead.");
									}
									catch (OverflowException)
									{
										throw new ArgumentException($"Expected arguments of type int, float, float, and Func<NPC, bool> for 'AddDefensiveEnemy', but got types {args[2].GetType().Name}, {args[3].GetType().Name}, {args[4].GetType().Name}, and {args[5].GetType().Name} instead.");
									}
								// TODO: Add more defensive enemy types in the future, maybe projectile-reflecting ones like Selenians and Legendary Mode EoC could be cool?
							}
						}
						throw new ArgumentException($"Expected a first argument of type string for 'AddDefensiveEnemy', but got type {args[1].GetType().Name} instead.");
					case "RemoveDefensiveEnemy":
						if (args[1] is string type2)
						{
							switch (type2)
							{
								case "DamageResistant":
									try
									{
										int npcID = Convert.ToInt32(args[2]);
										
										StatChangeHandler.damageResistantEnemies.Remove(npcID);

										return true;
									}
									catch (OverflowException)
									{
										throw new ArgumentException($"Expected a second argument of type int for 'RemoveDefensiveEnemy', but got type {args[2].GetType().Name} instead.");
									}
							}
						}
						throw new ArgumentException($"Expected a first argument of type string for 'RemoveDefensiveEnemy', but got type {args[1].GetType().Name} instead.");
					case "AddNoContactDamageEnemy":
						try
						{
							int npcID = Convert.ToInt32(args[1]);

							if (!StatChangeHandler.npcTypesThatShouldNotDoContactDamage.Contains(npcID))
								StatChangeHandler.npcTypesThatShouldNotDoContactDamage.Add(npcID);

							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected an argument of type int for 'AddNoContactDamageEnemy', but got type {args[1].GetType().Name} instead.");
						}
					case "RemoveNoContactDamageEnemy":
						try
						{
							int npcID = Convert.ToInt32(args[1]);

							StatChangeHandler.npcTypesThatShouldNotDoContactDamage.Remove(npcID);
							StatChangeHandler.ignoreNoContactDmg.Add(npcID);

							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected an argument of type int for 'RemoveNoContactDamageEnemy', but got type {args[1].GetType().Name} instead.");
						}
					case "AddIgnoredSummonWeapon":
						try
						{
							int itemID = Convert.ToInt32(args[1]);

							if (!ItemChanges.IgnoredSummonWeapons.Contains(itemID))
								ItemChanges.IgnoredSummonWeapons.Add(itemID);

							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected an argument of type int for 'AddIgnoredSummonWeapon', but got type {args[1].GetType().Name} instead.");
						}
					case "RemoveIgnoredSummonWeapon":
						try
						{
							int itemID = Convert.ToInt32(args[1]);

							ItemChanges.IgnoredSummonWeapons.Remove(itemID);

							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected an argument of type int for 'RemoveIgnoredSummonWeapon', but got type {args[1].GetType().Name} instead.");
						}
					case "AddHotDebuff":
						try
						{
							int buffID = Convert.ToInt32(args[1]);

							if (!TerratweaksPlayer.HotDebuffs.Contains(buffID))
								TerratweaksPlayer.HotDebuffs.Add(buffID);
							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected an argument of type int for 'AddHotDebuff', but got type {args[1].GetType().Name} instead.");
						}
					case "RemoveHotDebuff":
						try
						{
							int buffID = Convert.ToInt32(args[1]);

							TerratweaksPlayer.HotDebuffs.Remove(buffID);
							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected an argument of type int for 'RemoveHotDebuff', but got type {args[1].GetType().Name} instead.");
						}
					case "AddColdDebuff":
						try
						{
							int buffID = Convert.ToInt32(args[1]);

							if (!TerratweaksPlayer.ColdDebuffs.Contains(buffID))
								TerratweaksPlayer.ColdDebuffs.Add(buffID);
							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected an argument of type int for 'AddColdDebuff', but got type {args[1].GetType().Name} instead.");
						}
					case "RemoveColdDebuff":
						try
						{
							int buffID = Convert.ToInt32(args[1]);

							TerratweaksPlayer.ColdDebuffs.Remove(buffID);
							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected an argument of type int for 'RemoveColdDebuff', but got type {args[1].GetType().Name} instead.");
						}
					case "AddShimmerableBossDrop":
						if (args[1] is string listToEdit && args[2] is List<int> items)
						{
							ShimmerTransmutationHandler.ShimmerableBossDrops.TryAdd(listToEdit, new List<int>());

							foreach (int item in items)
							{
								if (ShimmerTransmutationHandler.ShimmerableBossDrops[listToEdit].Contains(item))
									continue;

								ShimmerTransmutationHandler.ShimmerableBossDrops[listToEdit].Add(item);
							}

							return true;
						}
						throw new ArgumentException($"Expected arguments of type string and List<int> for 'AddShimmerableBossDrop', but got types {args[1].GetType().Name} and {args[2].GetType().Name} instead.");
					case "RemoveShimmerableBossDrop":
						if (args[1] is string listToEdit2 && args[2] is List<int> items2)
						{
							foreach (int item in items2)
							{
								ShimmerTransmutationHandler.ShimmerableBossDrops[listToEdit2].Remove(item);
							}

							return true;
						}
						throw new ArgumentException($"Expected arguments of type string and List<int> for 'RemoveShimmerableBossDrop', but got types {args[1].GetType().Name} and {args[2].GetType().Name} instead.");
					case "AddHappinessFactorBlacklistedNPC":
						try
						{
							int npcID = Convert.ToInt32(args[1]);

							happinessFactorBlacklist.Add(npcID);

							return true;
						}
						catch (OverflowException)
						{
							if (args[1] is List<int> blacklistNpcIDs)
							{
								foreach (int npcID in blacklistNpcIDs)
									happinessFactorBlacklist.Add(npcID);

								return true;
							}
							throw new ArgumentException($"Expected argument of type int or List<int> for 'AddHappinessFactorBlacklistedNPC', but got type {args[1].GetType().Name} instead.");
						}
					case "AddHappinessFactorLocalization":
						try
						{
							int npcID = Convert.ToInt32(args[1]);

							if (args[2] is string npcLocKey)
							{
								if (!npcHappinessKeys.TryAdd(npcID, npcLocKey))
								{
									npcHappinessKeys[npcID] = npcLocKey;
								}

								return true;
							}

							throw new ArgumentException($"Expected arguments of type int and string for 'AddHappinessFactorLocalization', but got types {args[1].GetType().Name} and {args[2].GetType().Name} instead.");
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected arguments of type int and string for 'AddHappinessFactorLocalization', but got types {args[1].GetType().Name} and {args[2].GetType().Name} instead.");
						}
					case "AddSellableWeapon":
						try
						{
							int itemID = Convert.ToInt32(args[1]);
							int npcID = Convert.ToInt32(args[2]);

							if (args.Length > 3 && args[3] is List<Condition>)
							{
								ShopHandler.SellableWeaponConditions.Add(itemID, args[3] as List<Condition>);
							}
							else if (args.Length > 3)
							{
								Logger.Warn($"Expected third argument of 'AddSellableWeapon' to be a List<Condition>, but got type {args[3].GetType().Name} instead. The additional conditions for this weapon will be ignored, and loading will proceed as normal.");
							}

							ShopHandler.SellableWeapons.Add(itemID, npcID);

							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected arguments of type int and int for 'AddSellableWeapon', but got types {args[1].GetType().Name} and {args[2].GetType().Name} instead.");
						}
				}
			}

			throw new ArgumentException($"Expected argument of type string for Terratweaks mod call, but got type {args[0].GetType().Name} instead.");
		}

		public override void HandlePacket(BinaryReader reader, int fromWho)
		{
			PacketType type = (PacketType)reader.ReadByte();

			if (type == PacketType.SyncInferno)
			{
				if (Main.netMode == NetmodeID.Server) // Server needs to send out packets to all other players
				{
					int playerIdxToIgnore = fromWho;
					bool value = reader.ReadBoolean();

					ModPacket packet = GetPacket();
					packet.Write((byte)PacketType.SyncInferno);
					packet.Write(value);
					packet.Write(playerIdxToIgnore);
					packet.Send(ignoreClient: fromWho);
				}
				else // Multiplayer client needs to update the inferno visuals for the player they just received data from
				{
					int senderWhoAmI = reader.ReadInt32();
					bool value = reader.ReadBoolean();
					Main.player[senderWhoAmI].GetModPlayer<InputPlayer>().showInferno = value;
				}
			}
		}

		public override void Unload()
		{
			InfernoToggleKeybind = null;
			RulerToggleKeybind = null;
			MechRulerToggleKeybind = null;

			// Clear the modified Zenith profiles when the mod unloads
			// This is done here instead of in either Calamitweaks or Thoritweaks' code so it doesn't happen twice
			ResetZenithProfiles();
		}

		public static void ResetZenithProfiles()
		{
			FieldInfo _zenithProfiles = typeof(FinalFractalHelper).GetField("_fractalProfiles", BindingFlags.Static | BindingFlags.NonPublic);
			Dictionary<int, FinalFractalHelper.FinalFractalProfile> profiles = (Dictionary<int, FinalFractalHelper.FinalFractalProfile>)_zenithProfiles.GetValue(null);
			profiles.Clear();
			profiles.AddRange(defaultProfileList);
		}

		private static readonly List<int> happinessFactorBlacklist = new()
		{
			NPCID.Princess
		};

		private static readonly string mainKey = "Mods.Terratweaks.HappinessFactors";

		private static readonly Dictionary<int, string> npcHappinessKeys = new()
		{
			{ NPCID.Angler, mainKey + ".Angler" },
			{ NPCID.ArmsDealer, mainKey + ".ArmsDealer" },
			{ NPCID.Clothier, mainKey + ".Clothier" },
			{ NPCID.Cyborg, mainKey + ".Cyborg" },
			{ NPCID.Demolitionist, mainKey + ".Demolitionist" },
			{ NPCID.Dryad, mainKey + ".Dryad" },
			{ NPCID.DyeTrader, mainKey + ".DyeTrader" },
			{ NPCID.GoblinTinkerer, mainKey + ".GoblinTinkerer" },
			{ NPCID.Golfer, mainKey + ".Golfer" },
			{ NPCID.Guide, mainKey + ".Guide" },
			{ NPCID.Mechanic, mainKey + ".Mechanic" },
			{ NPCID.Merchant, mainKey + ".Merchant" },
			{ NPCID.Nurse, mainKey + ".Nurse" },
			{ NPCID.Painter, mainKey + ".Painter" },
			{ NPCID.PartyGirl, mainKey + ".PartyGirl" },
			{ NPCID.Pirate, mainKey + ".Pirate" },
			{ NPCID.SantaClaus, mainKey + ".SantaClaus" },
			{ NPCID.Steampunker, mainKey + ".Steampunker" },
			{ NPCID.Stylist, mainKey + ".Stylist" },
			{ NPCID.DD2Bartender, mainKey + ".Tavernkeep" },
			{ NPCID.TaxCollector, mainKey + ".TaxCollector" },
			{ NPCID.Truffle, mainKey + ".Truffle" },
			{ NPCID.WitchDoctor, mainKey + ".WitchDoctor" },
			{ NPCID.Wizard, mainKey + ".Wizard" },
			{ NPCID.BestiaryGirl, mainKey + ".Zoologist" }
		};

		[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_currentPriceAdjustment")]
		private static extern ref float CurrentPriceAdjustment(ShopHelper self);

		[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_currentHappiness")]
		private static extern ref string CurrentHappiness(ShopHelper self);

		private void CustomHappinessFactors(On_ShopHelper.orig_ProcessMood orig, ShopHelper self, Player player, NPC npc)
		{
			orig(self, player, npc);

			if (ModContent.GetInstance<TerratweaksConfig>().HouseSizeAffectsHappiness)
			{
				// Do nothing if the NPC doesn't have a home; it makes no sense for them to complain about the home size in that case
				// Also ignore any NPC in the blacklist (only the Princess by default)
				if (npc.homeless || happinessFactorBlacklist.Contains(npc.type))
					return;

				bool houseHasThreeTileWidth = false;
				bool houseHasThreeTileHeight = false;
				
				// Calculate the size of the current NPC's house
				WorldGen.StartRoomCheck(npc.homeTileX, npc.homeTileY - 1);
				Vector2 houseCorner1 = new(WorldGen.roomX1, WorldGen.roomY1);
				Vector2 houseCorner2 = new(WorldGen.roomX2, WorldGen.roomY2);
				int houseWidth = (int)Math.Abs(houseCorner1.X - houseCorner2.X) + 1;
				int houseHeight = (int)Math.Abs(houseCorner1.Y - houseCorner2.Y) + 1;
				int curHouseSize = WorldGen.numRoomTiles;

				if (houseWidth <= 3)
				{
					houseHasThreeTileWidth = true;
				}

				if (houseHeight <= 3)
				{
					houseHasThreeTileHeight = true;
				}

				string locKey = "Mods.Terratweaks.HappinessFactors.Default";
				bool priceAdjusted = false;
				
				if (npcHappinessKeys.TryGetValue(npc.type, out string value))
				{
					locKey = value;

					if (npc.type == NPCID.BestiaryGirl && npc.ShouldBestiaryGirlBeLycantrope())
						locKey += "_Transformed";
				}

				// Add the relevant happiness dialogue to the NPC's message and multiply their prices
				if (houseHasThreeTileHeight || houseHasThreeTileWidth) // House is too cramped = -10 happiness
				{
					CurrentPriceAdjustment(self) *= 1.1f;
					locKey += ".TinySpace";
					priceAdjusted = true;
				}
				else if (curHouseSize < 75) // 50-75 tiles = -5 happiness
				{
					CurrentPriceAdjustment(self) *= 1.05f;
					locKey += ".SmallSpace";
					priceAdjusted = true;
				}
				else if (curHouseSize >= 150 && curHouseSize < 200) // 150-199 tiles = +5 happiness
				{
					CurrentPriceAdjustment(self) *= 0.95f;
					locKey += ".BigSpace";
					priceAdjusted = true;
				}
				else if (curHouseSize >= 200) // 200+ tiles = +10 happiness
				{
					CurrentPriceAdjustment(self) *= 0.9f;
					locKey += ".HugeSpace";
					priceAdjusted = true;
				}

				if (priceAdjusted)
				{
					string textValueWith = Language.GetTextValueWith(locKey, null);
					CurrentHappiness(self) += textValueWith + " ";
				}
			}
		}

		private int DisableDamageVariance(On_Main.orig_DamageVar_float_int_float orig, float dmg, int percent, float luck)
		{
			if (ModContent.GetInstance<TerratweaksConfig>().NoDamageVariance == DamageVarianceSetting.On)
			{
				return (int)Math.Round(dmg);
			}
			else
			{
				return orig(dmg, percent, luck);
			}
		}

		private void RadiantInsigniaJumpHeight(On_Player.orig_UpdateJumpHeight orig, Player self)
		{
			orig(self);

			if (self.GetModPlayer<TerratweaksPlayer>().radiantInsignia) // Mimic SI's effect
			{
				self.jumpSpeedBoost += 1.8f;
			}
		}

		private int CritsBypassDefense(On_NPC.HitModifiers.orig_GetDamage orig, ref NPC.HitModifiers self, float baseDamage, bool crit, bool damageVariation, float luck)
		{
			// Nullify defense on crits, if the corresponding config option is enabled
			// Don't apply this change with "Consistent Critical Hits", as that config handles armor piercing itself
			if (ModContent.GetInstance<TerratweaksConfig>().CritsBypassDefense && !ModContent.GetInstance<TerratweaksConfig_Client>().NoRandomCrit && crit)
			{
				self.DefenseEffectiveness *= 0f;
			}

			return orig(ref self, baseDamage, crit, damageVariation, luck);
		}

		private void BannerCombatText(On_NPC.orig_CountKillForBannersAndDropThem orig, NPC self)
		{
			if (ModContent.GetInstance<TerratweaksConfig>().BannersDontSpamChat)
			{
				// Code adapted from the original method, just modified to use CombatText instead of printing to chat
				int num = Item.NPCtoBanner(self.BannerID());
				if (num > 0 && !self.ExcludedFromDeathTally())
				{
					NPC.killCount[num]++;
					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendData(MessageID.NPCKillCountDeathTally, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
					}
					int num2 = ItemID.Sets.KillsToBanner[Item.BannerToItem(num)];
					if (NPC.killCount[num] % num2 == 0 && num > 0)
					{
						int num3 = Item.BannerToNPC(num);
						//int num4 = self.lastInteraction;
						Player player = Main.player[self.lastInteraction];
						if (!player.active || player.dead)
						{
							player = Main.player[self.FindClosestPlayer()];
						}
						string message = Language.GetTextValue("Game.EnemiesDefeatedAnnouncement", NPC.killCount[num], Lang.GetNPCName(num3));
						if (player.whoAmI >= 0 && player.whoAmI < 255)
						{
							message = Language.GetTextValue("Game.EnemiesDefeatedByAnnouncement", player.name, NPC.killCount[num], Lang.GetNPCName(num3));
						}

						Rectangle rect = new((int)player.position.X, (int)player.position.Y + 192, 16, 16);
						Color color = new(250, 250, 0);

						if (Main.netMode == NetmodeID.SinglePlayer)
						{
							CombatText.NewText(rect, color, message, true);
						}
						else if (Main.netMode == NetmodeID.Server)
						{
							foreach (Player plr in Main.player.Where(p => p.active))
							{
								CombatText.NewText(rect, color, message, true);
							}
						}
						int num5 = Item.BannerToItem(num);
						Vector2 position = self.position;
						if (player.whoAmI >= 0 && player.whoAmI < 255)
						{
							position = player.position;
						}
						Item.NewItem(self.GetSource_Loot(), (int)position.X, (int)position.Y, self.width, self.height, num5, 1, false, 0, false, false);
					}
				}
			}
			// Just run the original method if the config is disabled, no need to do anything special
			else
			{
				orig(self);
			}
		}

		private void IncreasedJellyfishDamage(On_Player.orig_TakeDamageFromJellyfish orig, Player self, int npcIndex)
		{
			// Deal 2x NPC's base damage if electrified while this config is enabled, instead of 1.3x
			if (ModContent.GetInstance<TerratweaksConfig>().NoEnemyInvulnerability)
			{
				// Code adapted from the original method
				NPC jelly = Main.npc[npcIndex];
				double dmg = self.Hurt(PlayerDeathReason.ByNPC(npcIndex), jelly.damage * 2, -self.direction);
				self.SetMeleeHitCooldown(npcIndex, self.itemAnimation);
				self.ApplyAttackCooldown();
			}
			// Just run the original method if the config is disabled
			else
			{
				orig(self, npcIndex);
			}
		}

		private void HideInfernoVisuals(On_Main.orig_DrawInfernoRings orig, Main self)
		{
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];

				if (player.active && !player.outOfRange && !player.dead)
				{
					if (!player.GetModPlayer<InputPlayer>().showInferno)
						player.inferno = false;
				}
			}

			orig(self);
		}

		private void ChesterRework_Variables(On_Player.orig_HandleBeingInChestRange orig, Player self)
		{
			bool chesterRework = ModContent.GetInstance<TerratweaksConfig>().ChesterRework;
			if (!chesterRework) // No need to run any special code if the rework is disabled, other than setting the chester safe bool to false
			{
				orig(self);
				playerHasChesterSafeOpened = false;
				return;
			}

			if (playerHasChesterSafeOpened)
				self.chest = -2;

			orig(self);

			if (playerHasChesterSafeOpened)
			{
				if (self.chest == -1)
					playerHasChesterSafeOpened = false;
				else
					self.chest = -3;
			}
		}

		private int ChesterRework_OpenSafe(On_Main.orig_TryInteractingWithMoneyTrough orig, Projectile proj)
		{
			Player player = Main.LocalPlayer;
			bool chesterRework = ModContent.GetInstance<TerratweaksConfig>().ChesterRework;

			// Make sure the game closes the safe when needed
			// This SHOULD only trigger on safe Chester
			if (proj.type == ProjectileID.ChesterPet && Main.mouseRight && Main.mouseRightRelease && playerHasChesterSafeOpened)
			{
				Main.LocalPlayer.chest = -2;
				playerHasChesterSafeOpened = false;
			}

			int originalReturn = orig(proj);

			// Check if the player's currently opened chest is Chester, and if so, set the player's opened chest to the safe instead of the piggy bank
			if (chesterRework && proj.type == ProjectileID.ChesterPet
				&& player.chestX == (int)(proj.Center.X / 16) && player.chestY == (int)(proj.Center.Y / 16)
				&& player.chest == -2)
			{
				player.chest = -3;
				playerHasChesterSafeOpened = true;
				Recipe.FindRecipes();
			}

			return originalReturn;
		}
	}
}