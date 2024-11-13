global using TepigCore;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
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
	}

	public class Terratweaks : Mod
	{
		public static bool playerHasChesterSafeOpened = false;

		public static ModKeybind InfernoToggleKeybind { get; private set; }
		public static ModKeybind RulerToggleKeybind { get; private set; }
		public static ModKeybind MechRulerToggleKeybind { get; private set; }
		public static readonly List<int> DyeItemsSoldByTrader = [];

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
							return settingToQuery switch
							{
								#region Returns
								"BannersDontSpamChat" => ModContent.GetInstance<TerratweaksConfig>().BannersDontSpamChat,
								"BetterBestiary" => ModContent.GetInstance<TerratweaksConfig>().BetterBestiary,
								"BetterCrackedBricks" => ModContent.GetInstance<TerratweaksConfig>().BetterCrackedBricks,
								"BetterHappiness" => ModContent.GetInstance<TerratweaksConfig>().BetterHappiness,
								"BossesLowerSpawnRates" => ModContent.GetInstance<TerratweaksConfig>().BossesLowerSpawnRates,
								"PlayerDeathsToCallOffInvasion" => ModContent.GetInstance<TerratweaksConfig>().PlayerDeathsToCallOffInvasion,
								"NPCDeathsToCallOffInvasion" => ModContent.GetInstance<TerratweaksConfig>().NPCDeathsToCallOffInvasion,
								"ChesterRework" => ModContent.GetInstance<TerratweaksConfig>().ChesterRework,
								"CritsBypassDefense" => ModContent.GetInstance<TerratweaksConfig>().CritsBypassDefense,
								"DeerclopsRegens" => ModContent.GetInstance<TerratweaksConfig>().DeerclopsRegens,
								"DeerRegenAmt" => ModContent.GetInstance<TerratweaksConfig>().DeerRegenAmt,
								"DeerWeaponsRework" => ModContent.GetInstance<TerratweaksConfig>().DeerWeaponsRework,
								"DummyFix" => (int)ModContent.GetInstance<TerratweaksConfig>().DummyFix,
								"DyeTraderShopExpansion" => ModContent.GetInstance<TerratweaksConfig>().DyeTraderShopExpansion,
								"ForceBossContactDamage" => ModContent.GetInstance<TerratweaksConfig>().ForceBossContactDamage,
								"HouseSizeAffectsHappiness" => ModContent.GetInstance<TerratweaksConfig>().HouseSizeAffectsHappiness,
								"KillSentries" => ModContent.GetInstance<TerratweaksConfig>().KillSentries,
								"ManaFreeSummoner" => ModContent.GetInstance<TerratweaksConfig>().ManaFreeSummoner,
								"NoCasterContactDamage" => ModContent.GetInstance<TerratweaksConfig>().NoCasterContactDamage,
								"NoCoinTheft" => (int)ModContent.GetInstance<TerratweaksConfig>().NoCoinTheft,
								"NoDamageVariance" => (int)ModContent.GetInstance<TerratweaksConfig>().NoDamageVariance,
								"NoDiminishingReturns" => ModContent.GetInstance<TerratweaksConfig>().NoDiminishingReturns,
								"NoEnemyInvulnerability" => ModContent.GetInstance<TerratweaksConfig>().NoEnemyInvulnerability,
								"NoExpertDebuffTimes" => ModContent.GetInstance<TerratweaksConfig>().NoExpertDebuffTimes,
								"NoExpertFreezingWater" => ModContent.GetInstance<TerratweaksConfig>().NoExpertFreezingWater,
								"NoExpertScaling" => ModContent.GetInstance<TerratweaksConfig>().NoExpertScaling,
								"NPCsSellMinecarts" => ModContent.GetInstance<TerratweaksConfig>().NPCsSellMinecarts,
								"OasisCrateBuff" => ModContent.GetInstance<TerratweaksConfig>().OasisCrateBuff,
								"OreUnification" => ModContent.GetInstance<TerratweaksConfig>().OreUnification,
								"PostEyeSandstorms" => ModContent.GetInstance<TerratweaksConfig>().PostEyeSandstorms,
								"ReaverSharkTweaks" or "ReaverTweaks" => ModContent.GetInstance<TerratweaksConfig>().ReaverSharkTweaks,
								"SIRework" or "SoaringInsigniaRework" => ModContent.GetInstance<TerratweaksConfig>().SIRework,
								"SmartMimics" => ModContent.GetInstance<TerratweaksConfig>().SmartMimics,
								"SmartNymphs" => ModContent.GetInstance<TerratweaksConfig>().SmartNymphs,
								"SoilSolutionsPreML" => ModContent.GetInstance<TerratweaksConfig>().SoilSolutionsPreML,
								"SolutionsOnGFB" => ModContent.GetInstance<TerratweaksConfig>().SolutionsOnGFB,
								"StackableDD2Accs" => (int)ModContent.GetInstance<TerratweaksConfig>().StackableDD2Accs,
								"TerraprismaDropRate" => ModContent.GetInstance<TerratweaksConfig>().TerraprismaDropRate,
								"UmbrellaHatRework" => ModContent.GetInstance<TerratweaksConfig>().UmbrellaHatRework,

								"Client_EstimatedDPS" or "EstimatedDPS" => ModContent.GetInstance<TerratweaksConfig_Client>().EstimatedDPS,
								"Client_NoRandomCrit" or "NoRandomCrit" => ModContent.GetInstance<TerratweaksConfig_Client>().NoRandomCrit,
								"Client_PermBuffTips" or "PermBuffTips" => ModContent.GetInstance<TerratweaksConfig_Client>().PermBuffTips,
								"Client_StatsInTip" or "StatsInTip" => ModContent.GetInstance<TerratweaksConfig_Client>().StatsInTip,

								"ExpertAccBuffs_RoyalGel" or "RoyalGelBuff" => ModContent.GetInstance<TerratweaksConfig>().expertAccBuffs.RoyalGel,
								"ExpertAccBuffs_HivePack" or "HivePackBuff" => ModContent.GetInstance<TerratweaksConfig>().expertAccBuffs.HivePack,
								"ExpertAccBuffs_BoneHelm" or "BoneHelmBuff" => ModContent.GetInstance<TerratweaksConfig>().expertAccBuffs.BoneHelm,

								"ArmorReworks_Spider" or "SpiderArmorSetBonus" => ModContent.GetInstance<TerratweaksConfig>().armorBonuses.Spider,
								"ArmorReworks_Cobalt" or "CobaltArmorSetBonus" => ModContent.GetInstance<TerratweaksConfig>().armorBonuses.Cobalt,
								"ArmorReworks_Mythril" or "MythrilArmorSetBonus" => ModContent.GetInstance<TerratweaksConfig>().armorBonuses.Mythril,
								"ArmorReworks_Adamantite" or "AdamantiteArmorSetBonus" => ModContent.GetInstance<TerratweaksConfig>().armorBonuses.Adamantite,
								"ArmorReworks_Spooky" or "SpookyArmorSetBonus" => ModContent.GetInstance<TerratweaksConfig>().armorBonuses.Spooky,

								"CraftableUncraftables_PlanterBoxes" or "PlanterBoxesRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.PlanterBoxes,
								"CraftableUncraftables_GemCritters" or "GemCrittersRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.GemCritters,
								"CraftableUncraftables_DungeonFurniture" or "DungeonFurnitureRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.DungeonFurniture,
								"CraftableUncraftables_ObsidianFurniture" or "ObsidianRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.ObsidianFurniture,
								"CraftableUncraftables_StructureBanners" or "StructureBannersRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.StructureBanners,
								"CraftableUncraftables_Moss" or "MossRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.Moss,
								"CraftableUncraftables_Gravestones" or "GravestonesRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.Gravestones,
								"CraftableUncraftables_Trophies" or "TrophiesRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.Trophies,
								"CraftableUncraftables_ClothierVoodooDoll" or "ClothierVoodooDollRecipe" or "ClothierDollRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.ClothierVoodooDoll,
								"CraftableUncraftables_TempleTraps" or "TempleTrapsRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.TempleTraps,
								"CraftableUncraftables_ShimmerBottomlessAndSponges" or "BottomlessAndSpongesShimmer" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.ShimmerBottomlessAndSponges,
								"CraftableUncraftables_TeamBlocks" or "TeamBlocksRecipe" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.TeamBlocks,
								"CraftableUncraftables_PrehardUnobtainables" or "PrehardUnobtainablesShimmer" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.PrehardUnobtainables,
								"CraftableUncraftables_ShimmerBossDrops" or "BossDropsShimmer" => ModContent.GetInstance<TerratweaksConfig>().craftableUncraftables.ShimmerBossDrops,

								"Calamitweaks_AquaticEmblemBuff" or "AquaticEmblemBuff" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.AquaticEmblemBuff,
								"Calamitweaks_AsgardsValorBuff" or "AsgardsValorBuff" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.AsgardsValorBuff,
								"Calamitweaks_CraftableHostileTurrets" or "CraftableUncraftables_HostileTurrets" or "HostileTurretsRecipe" or "HostileTurretsShimmer" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.CraftableHostileTurrets,
								"Calamitweaks_DeificAmuletBuff" or "DeificAmuletBuff" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.DeificAmuletBuff,
								"Calamitweaks_DRBuffs" or "CalDRBuffs" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.DRBuffs,
								"Calamitweaks_DryadSellsSeeds" or "DryadSellsCalSeeds" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.DryadSellsSeeds,
								"Calamitweaks_EnemyFoodDrops" or "EnemyFoodDrops" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.EnemyFoodDrops,
								"Calamitweaks_EnragedEoLInstakills" or "Calamitweaks_DayEoLInstakills" or "EnragedEoLInstakills" or "DayEoLInstakills" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.EnragedEoLInstakills,
								"Calamitweaks_EzCalBanners" or "EzCalBanners" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.EzCalBanners,
								"Calamitweaks_NoDefenseDamage" or "NoDefenseDamage" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.NoDefenseDamage,
								"Calamitweaks_NoPatreonNPCNames" or "NoCalPatreonNPCNames" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.NoPatreonNPCNames,
								"Calamitweaks_NoSellingRoD" or "NoSellingRoD" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.NoSellingRoD,
								"Calamitweaks_OnionMasterMode" or "OnionMasterMode" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.OnionMasterMode,
								"Calamitweaks_RadiantInsigniaUpgradesFromAscendant" or "RadiantInsigniaUpgradesFromAscendant" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.RadiantInsigniaUpgradesFromAscendant,
								"Calamitweaks_RevertGraveyards" or "GraveyardCalReversion" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.RevertGraveyards,
								"Calamitweaks_RevertPillars" or "PillarCalReversion" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.RevertPillars,
								"Calamitweaks_RevertTerraprisma" or "TerraprismaCalReversion" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.RevertTerraprisma,
								"Calamitweaks_RevertVanillaBossAIChanges" or "VanillaBossAICalReversion" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.RevertVanillaBossAIChanges,
								"Calamitweaks_SummonerAccBuffs" or "CalSummonerAccBuffs" => ModContent.GetInstance<TerratweaksConfig>().calamitweaks.SummonerAccBuffs,

								"Thoritweaks_EatCooksFoodInCombat" or "Thoritweaks_CookBuff" or "EatCooksFoodInCombat" or "CookBuff" => ModContent.GetInstance<TerratweaksConfig>().thoritweaks.EatCooksFoodInCombat,

								"Alchemitweaks_DisableCustomPotions" or "DisableCustomAlchPotions" => ModContent.GetInstance<TerratweaksConfig>().alchemitweaks.DisableCustomPotions,

								_ => throw new Exception($"Could not find Terratweaks config option named {args[1]}."),
								#endregion
							};
						}
						throw new ArgumentException($"Expected an argument of type string for 'Query', but got type {args[1].GetType().Name} instead.");
					case "AddPermConsumable":
						if (args[1] is int itemType)
						{
							if (args[2] is Func<Player, bool> hasBeenUsed)
							{
								if (!TooltipChanges.PermBuffBools.TryAdd(itemType, hasBeenUsed))
								{
									TooltipChanges.PermBuffBools[itemType] = hasBeenUsed;
								}
								
								return true;
							}
							else if (args[2] is Func<Player, Vector2> amtConsumed)
							{
								if (!TooltipChanges.MultiPermBuffs.TryAdd(itemType, amtConsumed))
								{
									TooltipChanges.MultiPermBuffs[itemType] = amtConsumed;
								}
								
								return true;
							}
							else
							{
								throw new ArgumentException($"Expected a second argument of type Func<Player, bool> or Func<Player, Vector2> for 'AddPermConsumable', but got type {args[2].GetType().Name} instead.");
							}
						}
						throw new ArgumentException($"Expected a first argument of type int for 'AddPermConsumable', but got type {args[1].GetType().Name} instead.");
					case "AddDefensiveEnemy":
						if (args[1] is string type)
						{
							switch (type)
							{
								case "DamageResistant":
									if (args[2] is int npcID && args[3] is float dmgResist && args[4] is float kbResist && args[5] is Func<NPC, bool> defensiveState)
									{
										StatChangeHandler.DREnemy drEnemyStats = new(dmgResist, kbResist, defensiveState);
										if (!StatChangeHandler.damageResistantEnemies.TryAdd(npcID, drEnemyStats))
										{
											StatChangeHandler.damageResistantEnemies[npcID] = drEnemyStats;
										}

										return true;
									}
									else
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
									if (args[2] is int npcID)
									{
										StatChangeHandler.damageResistantEnemies.Remove(npcID);

										return true;
									}
									else
									{
										throw new ArgumentException($"Expected a second argument of type int for 'RemoveDefensiveEnemy', but got type {args[2].GetType().Name} instead.");
									}
							}
						}
						throw new ArgumentException($"Expected a first argument of type string for 'RemoveDefensiveEnemy', but got type {args[1].GetType().Name} instead.");
					case "AddNoContactDamageEnemy":
						if (args[1] is int npcType)
						{
							if (!StatChangeHandler.npcTypesThatShouldNotDoContactDamage.Contains(npcType))
								StatChangeHandler.npcTypesThatShouldNotDoContactDamage.Add(npcType);
							
							return true;
						}
						throw new ArgumentException($"Expected an argument of type int for 'AddNoContactDamageEnemy', but got type {args[1].GetType().Name} instead.");
					case "RemoveNoContactDamageEnemy":
						if (args[1] is int npcType2)
						{
							StatChangeHandler.npcTypesThatShouldNotDoContactDamage.Remove(npcType2);
							StatChangeHandler.ignoreNoContactDmg.Add(npcType2);
							return true;
						}
						throw new ArgumentException($"Expected an argument of type int for 'RemoveNoContactDamageEnemy', but got type {args[1].GetType().Name} instead.");
					case "AddIgnoredSummonWeapon":
						if (args[1] is int summonWeaponType)
						{
							if (!ItemChanges.IgnoredSummonWeapons.Contains(summonWeaponType))
								ItemChanges.IgnoredSummonWeapons.Add(summonWeaponType);

							return true;
						}
						throw new ArgumentException($"Expected an argument of type int for 'AddIgnoredSummonWeapon', but got type {args[1].GetType().Name} instead.");
					case "RemoveIgnoredSummonWeapon":
						if (args[1] is int summonWeaponType2)
						{
							ItemChanges.IgnoredSummonWeapons.Remove(summonWeaponType2);
							return true;
						}
						throw new ArgumentException($"Expected an argument of type int for 'RemoveIgnoredSummonWeapon', but got type {args[1].GetType().Name} instead.");
					case "AddHotDebuff":
						if (args[1] is int hotDebuffID)
						{
							if (!TerratweaksPlayer.HotDebuffs.Contains(hotDebuffID))
								TerratweaksPlayer.HotDebuffs.Add(hotDebuffID);
							return true;
						}
						throw new ArgumentException($"Expected an argument of type int for 'AddHotDebuff', but got type {args[1].GetType().Name} instead.");
					case "RemoveHotDebuff":
						if (args[1] is int hotDebuffID2)
						{
							TerratweaksPlayer.HotDebuffs.Remove(hotDebuffID2);
							return true;
						}
						throw new ArgumentException($"Expected an argument of type int for 'RemoveHotDebuff', but got type {args[1].GetType().Name} instead.");
					case "AddColdDebuff":
						if (args[1] is int coldDebuffID)
						{
							if (!TerratweaksPlayer.ColdDebuffs.Contains(coldDebuffID))
								TerratweaksPlayer.ColdDebuffs.Add(coldDebuffID);
							return true;
						}
						throw new ArgumentException($"Expected an argument of type int for 'AddColdDebuff', but got type {args[1].GetType().Name} instead.");
					case "RemoveColdDebuff":
						if (args[1] is int coldDebuffID2)
						{
							TerratweaksPlayer.ColdDebuffs.Remove(coldDebuffID2);
							return true;
						}
						throw new ArgumentException($"Expected an argument of type int for 'RemoveColdDebuff', but got type {args[1].GetType().Name} instead.");
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
						if (args[1] is int blacklistNpcID)
						{
							happinessFactorBlacklist.Add(blacklistNpcID);

							return true;
						}
						else if (args[1] is List<int> blacklistNpcIDs)
						{
							foreach (int blacklistNpcID2 in blacklistNpcIDs)
								happinessFactorBlacklist.Add(blacklistNpcID2);

							return true;
						}
						throw new ArgumentException($"Expected argument of type int or List<int> for 'AddHappinessFactorBlacklistedNPC', but got type {args[1].GetType().Name} instead.");
					case "AddHappinessFactorLocalization":
						if (args[1] is int npcType3 && args[2] is string npcLocKey)
						{
							if (!npcHappinessKeys.TryAdd(npcType3, npcLocKey))
							{
								npcHappinessKeys[npcType3] = npcLocKey;
							}

							return true;
						}
						throw new ArgumentException($"Expected arguments of type int and string for 'AddHappinessFactorLocalization', but got types {args[1].GetType().Name} and {args[2].GetType().Name} instead.");
				}
			}

			return true;
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