global using TepigCore;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
using Terraria.GameContent.Events;
using Terraria.GameContent.Generation;
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

		public class PostSkeletronOnFtw : IItemDropRuleCondition, IProvideItemConditionDescription
		{
			public bool CanDrop(DropAttemptInfo info)
			{
				return !Main.getGoodWorld || NPC.downedBoss3;
			}

			public bool CanShowItemDropInUI()
			{
				return true;
			}

			public string GetConditionDescription()
			{
				return Language.GetTextValue("Mods.Terratweaks.Conditions.PostSkeletronOnFtw");
			}
		}
	}

	public class Terratweaks : Mod
	{
		public static bool playerHasChesterSafeOpened = false;

		public static ModKeybind InfernoToggleKeybind { get; private set; }
		public static ModKeybind RulerToggleKeybind { get; private set; }
		public static ModKeybind MechRulerToggleKeybind { get; private set; }
		public static ModKeybind AutoFishingKeybind { get; private set; }
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

		public static TerratweaksConfig Config => ModContent.GetInstance<TerratweaksConfig>();
		public static TerratweaksConfig_Client ClientConfig => ModContent.GetInstance<TerratweaksConfig_Client>();
		public static AlchTweaks Alchemitweaks => Config.alchemitweaks;
		public static CalTweaks Calamitweaks => Config.calamitweaks;
		public static EmodeTweaks Eternitweaks => Config.eternitweaks;
		public static ThorTweaks Thoritweaks => Config.thoritweaks;

		public override void Load()
		{
			InfernoToggleKeybind = KeybindLoader.RegisterKeybind(this, "InfernoToggle", "I");
			RulerToggleKeybind = KeybindLoader.RegisterKeybind(this, "RulerToggle", "NumPad1");
			MechRulerToggleKeybind = KeybindLoader.RegisterKeybind(this, "MechRulerToggle", "NumPad2");
			AutoFishingKeybind = KeybindLoader.RegisterKeybind(this, "AutoFishing", "F");
			
			On_Main.DamageVar_float_int_float += DisableDamageVariance;
			On_Main.DrawInfernoRings += HideInfernoVisuals;
			On_Main.TryInteractingWithMoneyTrough += ChesterRework_OpenSafe;
			On_Player.HandleBeingInChestRange += ChesterRework_Variables;
			On_Player.TakeDamageFromJellyfish += IncreasedJellyfishDamage;
			On_Player.UpdateJumpHeight += RadiantInsigniaJumpHeight;
			On_NPC.CountKillForBannersAndDropThem += BannerCombatText;
			On_NPC.HitModifiers.GetDamage += CritsBypassDefense;
			On_ShopHelper.ProcessMood += CustomHappinessFactors;
			On_DD2Event.CheckProgress += DropMoreDefenderMedals;
			On_DD2Event.WinInvasionInternal += DropMoreDefenderMedals_Victory;
			On_Projectile.CanExplodeTile += HandleExplosives;
			On_Player.GetTileCutIgnorance += DontHurtLarvae;
			On_Player.BrainOfConfusionDodge += BuffedBrainDodge;
			On_NPC.AddBuff += BuffedBrainIchor;
			On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += ResetIchorState;
			On_Player.DashMovement += BuffedEyeShieldDash;
			On_WorldGen.RandHousePicture += WhereIsntWaldo;
			On_NPC.HitEffect_HitInfo += LavalessLavaSlime;
			On_NPC.GetNPCColorTintedByBuffs += HunterHighlightOverride;
			On_Main.StartSlimeRain += DisableSlimeRain;
			On_Projectile.TryGetContainerIndex += ChesterIndexChange;

			IL_Main.UpdateTime_StartDay += DisableEventSpawns_Day;
			IL_Main.UpdateTime_StartNight += DisableEventAndBossSpawns_Night;
			IL_Main.UpdateTime += DisableBossSpawns_Deerclops;
		}

		public override void PostSetupContent()
		{
			foreach (NPC npc in ContentSamples.NpcsByNetId.Values)
			{
				// Give all enemies with Granite Elemental AI defensive properties that reduce damage taken to 25% but increase knockback taken to 110%
				if (npc.aiStyle == NPCAIStyleID.GraniteElemental)
				{
					Tuple<float, float, Func<NPC, bool>> drEnemyStats = new(0.25f, 1.1f, (NPC npc) => npc.ai[0] == -1);
					TerratweaksContentSets.DefensiveEnemyProperties[npc.type] = drEnemyStats;
				}

				// Give all modded enemies with Fighter AI and the Granite Golem AI type defensive properties that reduce damage taken to 25% and nearly nullify knockback
				if (npc.aiStyle == NPCAIStyleID.Fighter && npc.ModNPC != null && npc.ModNPC.AIType == NPCID.GraniteGolem)
				{
					Tuple<float, float, Func<NPC, bool>> drEnemyStats = new(0.25f, 0.05f, (NPC npc) => npc.ai[2] < 0f);
					TerratweaksContentSets.DefensiveEnemyProperties[npc.type] = drEnemyStats;
				}

				// Give all Jellyfish-style enemies that can zap the player retalitory properties that deal 2x the enemy's base contact damage to the player
				if (NPCID.Sets.ZappingJellyfish[npc.type])
				{
					Tuple<float, Func<NPC, bool>> retEnemyStats = new(2.0f, (NPC npc) => npc.wet && npc.ai[1] == 1f);
					TerratweaksContentSets.RetalitoryEnemyProperties[npc.type] = retEnemyStats;
				}

				// Make all Caster AI enemies count as projectile attackers, and additionally add any enemies that use an AIType matching that of an enemy already in the set
				// The latter check accounts for things like hornets, archers, and Salamanders, as well as a few misc. enemies like Gastropods and Probes
				if (npc.aiStyle == NPCAIStyleID.Caster || (npc.ModNPC != null && npc.ModNPC.AIType > -1 && TerratweaksContentSets.ProjectileAttacker[npc.ModNPC.AIType]))
				{
					TerratweaksContentSets.ProjectileAttacker[npc.type] = true;
				}

				// Block only boss NPCs (and EoW) from stealing coins
				if (Config.NoCoinTheft == CoinTheftSetting.Limited)
				{
					if ((npc.type <= NPCID.EaterofWorldsHead && npc.type >= NPCID.EaterofWorldsTail) || npc.boss)
						NPCID.Sets.CantTakeLunchMoney[npc.type] = true;
				}
				// Block all enemies from stealing coins
				else if (Config.NoCoinTheft == CoinTheftSetting.On)
				{
					NPCID.Sets.CantTakeLunchMoney[npc.type] = true;
				}
			}

			if (Config.NonConsumableBossSummons)
			{
				foreach (Item item in ContentSamples.ItemsByType.Values)
				{
					// If we're checking a vanilla item, we need to make sure Calamity's changes aren't reverted,
					// because if they are we don't want to change the research count since they're still actually consumable
					if (item.ModItem == null)
					{
						if (ModLoader.HasMod("CalamityMod") && Calamitweaks.ConsumableCalBossSummons)
							continue;

						bool bossSummonFilter = ModLoader.HasMod("CalamityMod") ? !item.consumable && item.maxStack == 1 : item.consumable && item.ResearchUnlockCount == 3;

						if (item.TryGetGlobalItem(out BossSummonStuff globalItem))
						{
							var ogConsumable = globalItem.OriginalConsumableValue;

							// If Calamity isn't present or the config option isn't active, we can just check if the item has Calamity's boss summon stats
							if (ogConsumable.HasValue)
							{
								bossSummonFilter = ModLoader.HasMod("CalamityMod") ? !ogConsumable.Value && item.maxStack == 1 : ogConsumable.Value && item.ResearchUnlockCount == 3;

								if (ItemID.Sets.SortingPriorityBossSpawns[item.type] != -1 && bossSummonFilter)
								{
									item.ResearchUnlockCount = 1;
									continue;
								}
							}
						}

						if (ItemID.Sets.SortingPriorityBossSpawns[item.type] != -1 && bossSummonFilter)
						{
							item.ResearchUnlockCount = 1;
						}
					}
					// Non-vanilla items that aren't from Calamity just have their research count set to 1
					// Calamity items are handled by Calamitweaks code
					else if (ItemChanges.IsBossSummon(item))
					{
						item.ResearchUnlockCount = 1;
					}
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
							// Replace some symbols/words with others to make this more lenient
							settingToQuery = settingToQuery
							#region Aliases
								.Replace(" ", "").Replace("'", "").Replace("&", "").Replace("^", "").Replace("%", "").Replace("#", "").Replace("@", "")
								.Replace("!", "").Replace("?", "")
								.Replace("/", "_").Replace(":", "_").Replace(";", "_").Replace(".", "_").Replace("-", "_").Replace(",", "_")
								.ToLower() // Convert to lowercase so case sensitivity doesn't matter (this happens before checking for alias words for obvious reasons)
								// Secret Seed name abbreviations
								.Replace("fortheworthy", "ftw")
								.Replace("featherworthy", "ftw")
								.Replace("getfixedboi", "gfb")
								.Replace("everythingseed", "gfb")
								.Replace("zenithseed", "gfb")
								// Lunar wings aliases
								.Replace("celestialwings", "lunarwings")
								.Replace("solarwings", "lunarwings")
								.Replace("vortexwings", "lunarwings")
								.Replace("vortexbeater", "lunarwings")
								.Replace("nebulawings", "lunarwings")
								.Replace("nebulamantle", "lunarwings")
								.Replace("stardustwings", "lunarwings")
								// NPC/boss/area name aliases
								.Replace("deerclops", "deer")
								.Replace("jungletemple", "temple")
								.Replace("enragedeol", "dayeol")
								.Replace("aquaticdepths", "ad")
								.Replace("magmaslime", "lavaslime")
								.Replace("hellslime", "lavaslime")
								.Replace("hellstoneslime", "lavaslime")
								.Replace("underworldslime", "lavaslime")
								.Replace("fireslime", "lavaslime")
								// Content name aliases/abbreviations
								.Replace("oasismiragecrate", "oasiscrate")
								.Replace("miragecrate", "oasiscrate")
								.Replace("geysertrap", "geyser")
								.Replace("geysers", "geyser")
								.Replace("icesentry", "frosthydra")
								.Replace("icehydra", "frosthydra")
								.Replace("icedragon", "frosthydra")
								.Replace("staffofthefrosthydra", "frosthydra")
								.Replace("frosthydrastaff", "frosthydra")
								.Replace("eocshield", "eyeshield")
								.Replace("shieldofcthulhu", "eyeshield")
								.Replace("brainofconfusion", "boc")
								.Replace("boc", "wormbrain")
								.Replace("wormscarfboc", "wormbrain")
								.Replace("wormscarf", "wormbrain")
								.Replace("gravityglobe", "gravglobe")
								.Replace("calhostileturrets", "hostileturrets")
								.Replace("sliceofcake", "cakeslice")
								.Replace("encumberingstone", "encstone")
								.Replace("onedropyoyos", "onedrop")
								.Replace("highlightcolor", "glowcolor")
								.Replace("fallingstar", "fallenstar")
								.Replace("rainlightning", "lightning")
								.Replace("environmentaldebuff", "envdebuff")
								.Replace("classsealingdebuff", "classsealdebuff")
								.Replace("handofcreation", "hoc")
								.Replace("exobox", "gamerchair")
								.Replace("exocube", "gamerchair")
								.Replace("supremecalamitas", "scal")
								.Replace("spectreboot", "spectre")
								.Replace("spectres", "spectre")
								.Replace("duneriderboot", "dunerider")
								.Replace("duneriders", "dunerider")
								.Replace("prismaticlacewing", "lacewing")
								.Replace("tatteredcloth", "cloth")
								// Category name aliases
								.Replace("expertaccessorybuffs", "expertaccbuffs")
								.Replace("armortweaks", "armorreworks")
								.Replace("vanillaarmortweaks", "armorreworks")
								.Replace("armorsetbonuses", "armorreworks")
								.Replace("vanillaarmorsetbonuses", "armorreworks")
								.Replace("vanillaarmorsetreworks", "armorreworks")
								.Replace("vanillaarmorreworks", "armorreworks")
								.Replace("terratweaksclient", "client")
								.Replace("clientconfig", "client")
								.Replace("caltweaks", "calamitweaks")
								.Replace("calamityfables", "calfables")
								.Replace("wrathofthegods", "wotg")
								.Replace("wotgtweaks", "wotg")
								.Replace("thortweaks", "thoritweaks")
								.Replace("alchtweaks", "alchemitweaks")
								.Replace("eternitymode", "emode")
								.Replace("masochistmode", "masomode");
							#endregion

							return settingToQuery switch
							{
								#region Returns
								"bannersdontspamchat" => Config.BannersDontSpamChat,
								"betterbestiary" => Config.BetterBestiary,
								"bettercrackedbricks" => Config.BetterCrackedBricks,
								"betterhappiness" => Config.BetterHappiness,
								"bosseslowerspawnrates" => Config.BossesLowerSpawnRates,
								"playerdeathstocalloffinvasion" => Config.PlayerDeathsToCallOffInvasion,
								"npcdeathstocalloffinvasion" => Config.NPCDeathsToCallOffInvasion,
								"chesterrework" => Config.ChesterRework,
								"critsbypassdefense" => Config.CritsBypassDefense,
								"dangersensehighlightssilt" => Config.DangersenseHighlightsSilt,
								"dangersenseignoresthinice" => Config.DangersenseIgnoresThinIce,
								"deerregens" => Config.DeerclopsRegens,
								"deerregenamt" or "deerregenamount" => Config.DeerRegenAmt,
								"deerweaponsrework" => Config.DeerWeaponsRework,
								"dummyfix" => (int)Config.DummyFix,
								"dyetradershopexpansion" => Config.DyeTraderShopExpansion,
								"forcebosscontactdamage" => Config.ForceBossContactDamage,
								"housesizeaffectshappiness" => Config.HouseSizeAffectsHappiness,
								"killsentries" => Config.KillSentries,
								"manafreesummoner" => Config.ManaFreeSummoner,
								"nocastercontactdamage" => Config.NoCasterContactDamage,
								"nocointheft" => (int)Config.NoCoinTheft,
								"nodamagevariance" => (int)Config.NoDamageVariance,
								"nodiminishingreturns" => Config.NoDiminishingReturns,
								"noenemyinvulnerability" => Config.NoEnemyInvulnerability,
								"noexpertdebufftimes" => Config.NoExpertDebuffTimes,
								"noexpertfreezingwater" => Config.NoExpertFreezingWater,
								"noexpertscaling" => Config.NoExpertScaling,
								"npcssellminecarts" => Config.NPCsSellMinecarts,
								"oasiscratebuff" => Config.OasisCrateBuff,
								"oasiscrateodds" => Config.OasisCrateOdds,
								"oreunification" => Config.OreUnification,
								"overridegraveyardrequirements" => Config.OverrideGraveyardRequirements,
								"graveyardvisuals" => Config.GraveyardVisuals,
								"graveyardgunctionality" => Config.GraveyardFunctionality,
								"graveyardmax" => Config.GraveyardMax,
								"lunarwingspreml" or "earlylunarwings" => Config.LunarWingsPreML,
								"pillarenemiesdropfragments" => Config.PillarEnemiesDropFragments,
								"posteyesandstorms" => Config.PostEyeSandstorms,
								"reaversharktweaks" or "reavertweaks" => Config.ReaverSharkTweaks,
								"sirework" or "soaringinsigniarework" => Config.SIRework,
								"smartmimics" => Config.SmartMimics,
								"smartnymphs" => Config.SmartNymphs,
								"soilsolutionspreml" => Config.SoilSolutionsPreML,
								"solutionsongfb" => Config.SolutionsOnGFB,
								"stackabledd2accs" => (int)Config.StackableDD2Accs,
								"terraprismadroprate" => Config.TerraprismaDropRate,
								"townnpcssellweapons" => Config.TownNPCsSellWeapons,
								"umbrellahatrework" => Config.UmbrellaHatRework,
								"expertaccbuffs_royalgel" or "royalgelbuff" => Config.RoyalGel,
								"expertaccbuffs_hivepack" or "hivepackbuff" => Config.HivePack,
								"expertaccbuffs_bonehelm" or "bonehelmbuff" => Config.BoneHelm,
								"expertaccbuffs_boneglove" or "boneglovebuff" => Config.BoneGlove,
								"expertaccbuffs_eyeshield" or "eyeshieldbuff" => Config.EyeShield,
								"expertaccbuffs_wormbrain" or "wormbrainbuff" => Config.WormBrain,
								"armorreworks_spider" or "spiderarmorsetbonus" or "spidersetbonus" => Config.SpiderSetBonus,
								"armorreworks_cobalt" or "cobaltarmorsetbonus" or "cobaltsetbonus" => Config.CobaltSetBonus,
								"armorreworks_mythril" or "mythrilarmorsetbonus" or "mythrilsetbonus" => Config.MythrilSetBonus,
								"armorreworks_adamantite" or "adamantitearmorsetbonus" or "adamantitesetbonus" => Config.AdamantiteSetBonus,
								"armorreworks_spooky" or "spookyarmorsetbonus" or "spookysetbonus" => Config.SpookySetBonus,
								"armorreworks_monk" or "whipmonkarmor" or "convertmonkarmor" => Config.ConvertMonkArmor,
								"armorreworks_stardust" or "stardustarmorbuff" or "buffstardustarmor" => Config.StardustArmorBuff,
								"oldchestdungeon" => Config.OldChestDungeon,
								"boundnpcsimmune" or "boundnpcsnodamage" or "invulnerableboundnpcs" => Config.BoundNPCsImmune,
								"lessgrindydefendermedals" or "lessgrindyooa" or "moreooamedals" or "moredefendermedals" => Config.MoreOOAMedals,
								"bombablemeteorite" => Config.BombableMeteorite,
								"junglebossbags" => Config.JungleBossBags,
								"placeablegravglobe" => Config.PlaceableGravGlobe,
								"gravgloberange" => Config.GravGlobeRange,
								"cultistgravglobe" => Config.CultistGravGlobe,
								"sturdylarvae" => (int)Config.SturdyLarvae,
								"ftw_nerfskycrate" or "nerfskycrate" => Config.NerfSkyCrates,
								"ftw_nomobgriefing" or "nomobgriefing" => Config.NoMobGriefing,
								"ftw_bombpots" or "bombpots" => Config.FtwBombPots,
								"ftw_bombtrees" or "bombtrees" => Config.FtwBombTrees,
								"papercuts" => Config.PaperCuts,
								"frosthydrabuff" or "frosthydraminigun" => Config.FrostHydraBuff,
								"spelunkerhighlightshellstone" => Config.SpelunkerHighlightsHellstone,
								"infinitecakeslice" => Config.InfiniteCakeSlice,
								"persistentstationbuffs" => Config.PersistentStationBuffs,
								"coinsbypassencstone" => Config.CoinsBypassEncStone,
								"ezdyebanners" => Config.EzDyeBanners,
								"lavalesslavaslimes" => Config.LavalessLavaSlimes,
								"harmlessfallenstars" or "nofallenstardamage" => Config.HarmlessFallenStars,
								"resummonminions" => Config.ResummonMinions,
								"nobiomerequirements" => Config.NoBiomeRequirements,
								"nonconsumablebosssummons" => Config.NonConsumableBossSummons,
								"nobossspawns" or "nonaturalbossspawns" => Config.NoBossSpawns,
								"noeventspawns" or "nonaturaleventspawns" => Config.NoEventSpawns,
								"autofishing" => Config.AutoFishing,
								"autofishingitem" or "autofishingrequirement" => Config.AutoFishingItem, // Returns an ItemDefinition, not an item or an ID!
								"nolingeringprobes" => Config.NoLingeringProbes,
								"goldcrittersdropgold" => Config.GoldCrittersDropGold,
								"goldcrittersminvalue" or "goldcrittersmingold" => Config.GoldCritterMinValue,
								"goldcrittersmaxvalue" or "goldcrittersmaxgold" => Config.GoldCritterMaxValue,
								"toolboxhoc" or "toolbelthoc" or "toolbelttoolboxhoc" or "toolboxtoolbelthoc" => Config.ToolboxHoC,
								"mechanicsellstoolbox" => Config.MechanicSellsToolbox,
								"spectreneedsdunerider" or "spectreneeddunerider" => Config.SpectreNeedsDunerider,
								"bossblacklist" => Config.BossBlacklist, // Returns a list of NPCDefinitions, not a list of NPCs or IDs!
								"bosswhitelist" => Config.BossWhitelist, // Returns a list of NPCDefinitions, not a list of NPCs or IDs!
								"earlylacewing" => Config.EarlyLacewing,
								"goblinsdropcloth" or "allgoblinsdropcloth" => Config.GoblinsDropCloth,
								"warmthgivescoldimmunity" => Config.WarmthGivesColdImmunity,
								"obsidianskullonfireimmunity" or "obsidianskullgivesonfireimmunity" => Config.ObsidianSkullOnFireImmunity,
								"allencompassingankhshield" => Config.AllEncompassingAnkhShield,

								"client_estimateddps" or "estimateddps" => ClientConfig.EstimatedDPS,
								"client_grammarcorrections" or "grammarcorrections" => ClientConfig.GrammarCorrections,
								"client_norandomcrit" or "norandomcrit" => ClientConfig.NoRandomCrit,
								"client_permbufftips" or "permbufftips" => ClientConfig.PermBuffTips,
								"client_statsintip" or "statsintip" => ClientConfig.StatsInTip,
								"client_wingstatsintip" or "wingstatsintip" => ClientConfig.WingStatsInTip,
								"client_noonedroplogo" or "noonedroplogo" => ClientConfig.NoOneDropLogo,
								"client_overridespelunkerglow" or "overridespelunkerglow" => ClientConfig.OverrideSpelunkerGlow,
								"client_treasureglowcolor" or "treasureglowcolor" => ClientConfig.TreasureGlowColor,
								"client_overridedangerglow" or "overridedangerglow" => ClientConfig.OverrideDangerGlow,
								"client_dangerglowcolor" or "dangerglowcolor" => ClientConfig.DangerGlowColor,
								"client_overridehunterglow" or "overridehunterglow" => ClientConfig.OverrideHunterGlow,
								"client_enemyglowcolor" or "enemyglowcolor" => ClientConfig.EnemyGlowColor,
								"client_friendlyglowcolor" or "friendlyglowcolor" => ClientConfig.FriendlyGlowColor,
								"client_hideitemmodifiedtips" or "hideitemmodifiedtips" => ClientConfig.HideItemModifiedTips,
								"client_hidemilestonetips" or "hidemilestonetips" => ClientConfig.HideMilestoneTips,
								"client_hammushtip" or "hammushtip" => ClientConfig.HammushTooltip,
								"client_hidefavoritedtips" => ClientConfig.HideFavoritedTips,

								"craftableuncraftables_planterboxes" or "planterboxesrecipe" => Config.craftableUncraftables.PlanterBoxes,
								"craftableuncraftables_gemcritters" or "gemcrittersrecipe" => Config.craftableUncraftables.GemCritters,
								"craftableuncraftables_dungeonfurniture" or "dungeonfurniturerecipe" => Config.craftableUncraftables.DungeonFurniture,
								"craftableuncraftables_obsidianfurniture" or "obsidianfurniturerecipe" => Config.craftableUncraftables.ObsidianFurniture,
								"craftableuncraftables_structurebanners" or "structurebannersrecipe" => Config.craftableUncraftables.StructureBanners,
								"craftableuncraftables_moss" or "mossrecipe" => Config.craftableUncraftables.Moss,
								"craftableuncraftables_gravestones" or "gravestonesrecipe" => Config.craftableUncraftables.Gravestones,
								"craftableuncraftables_geyser" or "geyserrecipe" => Config.craftableUncraftables.GeyserTraps,
								"craftableuncraftables_trophies" or "trophiesrecipe" => Config.craftableUncraftables.Trophies,
								"craftableuncraftables_clothiervoodoodoll" or "clothiervoodoodollrecipe" or "clothierdollrecipe" => Config.craftableUncraftables.ClothierVoodooDoll,
								"craftableuncraftables_templetraps" or "templetrapsrecipe" => Config.craftableUncraftables.TempleTraps,
								"craftableuncraftables_shimmerbottomlessandsponges" or "bottomlessandspongesshimmer" => Config.craftableUncraftables.ShimmerBottomlessAndSponges,
								"craftableuncraftables_teamblocks" or "teamblocksrecipe" => Config.craftableUncraftables.TeamBlocks,
								"craftableuncraftables_prehardunobtainables" or "prehardunobtainablesshimmer" => Config.craftableUncraftables.PrehardUnobtainables,
								"craftableuncraftables_shimmerbossdrops" or "bossdropsshimmer" => Config.craftableUncraftables.ShimmerBossDrops,
								"craftableuncraftables_shimmerblacklens" or "blacklensshimmer" => Config.craftableUncraftables.ShimmerBlackLens,
								"craftableuncraftables_earlyechoblocks" or "earlyechoblocks" => Config.craftableUncraftables.EarlyEchoBlocks,

								"calamitweaks_asgardsvalorbuff" or "asgardsvalorbuff" => Calamitweaks.AsgardsValorBuff,
								"calamitweaks_combinedstationsupport" or "calcombinedstationsupport" => Calamitweaks.CombinedStationSupport,
								"calamitweaks_craftablehostileturrets" or "craftableuncraftables_hostileturrets" or "hostileturretsrecipe" or "hostileturretsshimmer" => Calamitweaks.CraftableHostileTurrets,
								"calamitweaks_deificamuletbuff" or "deificamuletbuff" => Calamitweaks.DeificAmuletBuff,
								"calamitweaks_drbuffs" or "caldrbuffs" => Calamitweaks.DRBuffs,
								"calamitweaks_ezcalbanners" or "calamitweaks_ezbanners" or "ezcalbanners" => Calamitweaks.EzCalBanners,
								"calamitweaks_nodefensedamage" or "nodefensedamage" => Calamitweaks.NoDefenseDamage,
								"calamitweaks_nopatreonnpcnames" or "nocalpatreonnpcnames" => Calamitweaks.NoPatreonNPCNames,
								"calamitweaks_noplantparticles" or "noplantparticles" => Calamitweaks.NoPlantParticles,
								"calamitweaks_nowormparticles" or "nowormparticles" => Calamitweaks.NoWormParticles,
								"calamitweaks_notimeddr" or "notimeddr" => Calamitweaks.SummonerAccBuffs,
								"calamitweaks_onionmastermode" or "onionmastermode" => Calamitweaks.OnionMasterMode,
								"calamitweaks_radiantinsigniaupgradesfromascendant" or "radiantinsigniaupgradesfromascendant" => Calamitweaks.RadiantInsigniaUpgradesFromAscendant,
								"calamitweaks_revertpickspeedbuffs" or "pickspeedcalreversion" => Calamitweaks.RevertPickSpeedBuffs,
								"calamitweaks_summoneraccbuffs" or "calsummoneraccbuffs" => Calamitweaks.SummonerAccBuffs,
								"calamitweaks_zenithrecipeoverhaul" or "calzenithrecipeoverhaul" or "calamitweaks_zenithrecipe" or "calzenithrecipe" => Calamitweaks.ZenithRecipeOverhaul,
								"calamitweaks_wotg_nosilentrift" or "wotg_nosilentrift" or "calamitweaks_nosilentrift" or "nosilentrift" => Calamitweaks.NoSilentRift,
								"calamitweaks_fables_ezbanners" or "calamitweaks_fables_ezfablesbanners" or "fables_ezbanners" or "fables_ezfablesbanners" or "ezfablesbanners" => Calamitweaks.EzFablesBanners,
								"calamitweaks_consumablebosssummons" or "consumablecalbosssummons" => Calamitweaks.ConsumableCalBossSummons,
								"calamitweaks_earlygrandgelatin" or "earlygrandgelatin" => Calamitweaks.EarlyGrandGelatin,
								"calamitweaks_allowgamerchairininfernumscal" or "allowgamerchairininfernumscal" => Calamitweaks.AllowGamerChairInInfernumScal,

								"thoritweaks_bombableadblocks" or "bombableadblocks" => Thoritweaks.BombableADBlocks,
								"thoritweaks_combinedstationsupport" or "thorcombinedstationsupport" => Thoritweaks.CombinedStationSupport,
								"thoritweaks_eatcooksfoodincombat" or "thoritweaks_cookbuff" or "eatcooksfoodincombat" or "cookbuff" => Thoritweaks.EatCooksFoodInCombat,
								"thoritweaks_zenithrecipeoverhaul" or "thorzenithrecipeoverhaul" or "thoritweaks_zenithrecipe" or "thorzenithrecipe" => Thoritweaks.ZenithRecipeOverhaul,

								"alchemitweaks_disablecustompotions" or "disablecustomalchpotions" => Alchemitweaks.DisableCustomPotions,
								"alchemitweaks_anticheese" or "alchemitweaks_infmoneyfix" or "architectinfmoneyfix" or "architectanticheese" => Alchemitweaks.AntiCheese,

								"eternitweaks_lightningbuff" or "eternitweaks_emodelightningbuff" or "emodelightningbuff" => Eternitweaks.EmodeLightningBuff,
								"eternitweaks_noenvdebuffs" or "noemodeenvdebuffs" => Eternitweaks.NoEnvironmentalDebuffs,
								"eternitweaks_noclasssealdebuffs" or "noemodeclasssealdebuffs" => Eternitweaks.NoClassSealDebuffs,
								"eternitweaks_eternalenergyaccessibility" or "eternitweaks_eternalenergyoutsideemode" or "eternalenergyaccessibility" or "eternalenergyoutsideemode" => Eternitweaks.EternalEnergyAccessibility,

								_ => throw new Exception($"Could not find Terratweaks config option with name \"{args[1]}\" or alias \"{settingToQuery}\"."),
								#endregion
							};
						}
						throw new ArgumentException($"Expected an argument of type string for 'Query', but got type {args[1].GetType().Name} instead.");
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
					case "AddBiomeCondition":
					case "AddBiomeConditions":
						if (args[1] is Condition biomeCondition)
						{
							ShopHandler.BiomeConditions.Add(biomeCondition);

							return true;
						}
						else if (args[1] is IEnumerable<Condition> biomeConditions)
						{
							ShopHandler.BiomeConditions.AddRange(biomeConditions);

							return true;
						}
						else
						{
							throw new ArgumentException($"Expected argument of type Condition or any IEnumerable<Condition> for 'AddBiomeCondition', but got type {args[1].GetType().Name} instead.");
						}
					default:
						Logger.Error($"Could not find a Terratweaks mod call under the name \"{args[0]}\".");
						return false;
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
			// Clear out all keybind data
			InfernoToggleKeybind = null;
			RulerToggleKeybind = null;
			MechRulerToggleKeybind = null;
			AutoFishingKeybind = null;

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

			if (Config.HouseSizeAffectsHappiness)
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
			if (Config.NoDamageVariance == DamageVarianceSetting.On)
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
			if (Config.CritsBypassDefense && !ClientConfig.NoRandomCrit && crit)
			{
				self.DefenseEffectiveness *= 0f;
			}

			return orig(ref self, baseDamage, crit, damageVariation, luck);
		}

		private void BannerCombatText(On_NPC.orig_CountKillForBannersAndDropThem orig, NPC self)
		{
			if (Config.BannersDontSpamChat)
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
			// Multiply the NPC's base damage by a higher value if electrified while this config is enabled
			// By default, the multiplier is increased from 1.3x to 2x, but it can be customized per-enemy in theory, and doesn't even have to be limited to jellyfish-AI enemies!
			if (Config.NoEnemyInvulnerability)
			{
				// Code adapted from the original method
				NPC jelly = Main.npc[npcIndex];
				Tuple<float, Func<NPC, bool>> retData = TerratweaksContentSets.RetalitoryEnemyProperties[jelly.type];

				double dmg = self.Hurt(PlayerDeathReason.ByNPC(npcIndex), (int)Math.Round(jelly.damage * retData.Item1), -self.direction);
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
			if (!Config.ChesterRework) // No need to run any special code if the rework is disabled, other than setting the chester safe bool to false
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
			
			// Make sure the game closes the safe when needed
			// This SHOULD only trigger on safe Chester
			if (proj.type == ProjectileID.ChesterPet && Main.mouseRight && Main.mouseRightRelease && playerHasChesterSafeOpened
				&& Main.MouseWorld.Distance(proj.Center) < proj.width / 2)
			{
				Main.LocalPlayer.chest = -2;
				playerHasChesterSafeOpened = false;
			}

			int originalReturn = orig(proj);

			// Check if the player's currently opened chest is Chester, and if so, set the player's opened chest to the safe instead of the piggy bank
			if (Config.ChesterRework && proj.type == ProjectileID.ChesterPet
				&& player.chestX == (int)(proj.Center.X / 16) && player.chestY == (int)(proj.Center.Y / 16)
				&& player.chest == -2)
			{
				player.chest = -3;
				playerHasChesterSafeOpened = true;
				Recipe.FindRecipes();
			}

			return originalReturn;
		}

		private bool[] DontHurtLarvae(On_Player.orig_GetTileCutIgnorance orig, Player self, bool allowRegrowth, bool fromTrap)
		{
			bool[] origResult = orig(self, allowRegrowth, fromTrap);

			if (Config.SturdyLarvae != SturdyLarvaeSetting.Off)
			{
				bool[] clone = (bool[])origResult.Clone();

				switch (Config.SturdyLarvae)
				{
					case SturdyLarvaeSetting.Env:
						if (self.dontHurtNature)
							clone[TileID.Larva] = true;
						break;
					case SturdyLarvaeSetting.Peace:
						if (self.HasItemInInventoryOrOpenVoidBag(ItemID.DontHurtComboBook) || HasComboBookUpgrade(self))
							clone[TileID.Larva] = true;
						break;
					case SturdyLarvaeSetting.Bee:
						if (NPC.downedQueenBee && self.dontHurtNature)
							clone[TileID.Larva] = true;
						break;
				}

				return clone;
			}

			return origResult;
		}

		private static bool HasComboBookUpgrade(Player player)
		{
			// Check for any of the vanilla books; if we have them, we probably don't have a hypothetical modded upgrade
			if (player.HasItemInInventoryOrOpenVoidBag(ItemID.DontHurtNatureBook) ||
				player.HasItemInInventoryOrOpenVoidBag(ItemID.DontHurtCrittersBook) ||
				player.HasItemInInventoryOrOpenVoidBag(ItemID.DontHurtComboBook))
				return false;

			// If we don't have any of the books, yet somehow still have both books' effects, we probably have a modded upgrade
			if (player.dontHurtCritters && player.dontHurtNature)
				return true;

			return false;
		}

		private bool HandleExplosives(On_Projectile.orig_CanExplodeTile orig, Projectile self, int i, int j)
		{
			// Allow all explosives to destroy Meteorite if an evil boss has been downed
			if (Config.BombableMeteorite)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.Meteorite && (NPC.downedBoss2 || Main.hardMode))
				{
					return true;
				}
			}

			// Disable Skeletron Prime bombs breaking blocks in For the Worthy worlds
			if (Config.NoMobGriefing && Main.getGoodWorld && self.type == ProjectileID.BombSkeletronPrime)
				return false;

			return orig(self, i, j);
		}

		private void DropMoreDefenderMedals_Victory(On_DD2Event.orig_WinInvasionInternal orig)
		{
			// Let the original method run on its own, that way we don't interrupt any standard vanilla behavior
			orig();

			// If the config option to drop extra medals is enabled,
			// drop two extra medals if the player just beat the tier 1 event, or three extra medals for tier 2
			// (they'll drop separately from vanilla's amount, but should still stack since they spawn on top of each other)
			// Tier 3 doesn't drop extra medals from the final wave
			if (Config.MoreOOAMedals)
			{
				if (DD2Event.OngoingDifficulty == 1)
					DD2Event.DropMedals(2);
				else if (DD2Event.OngoingDifficulty == 2)
					DD2Event.DropMedals(3);
			}
		}

		private bool spawnedMedalsThisWave = false;

		private void DropMoreDefenderMedals(On_DD2Event.orig_CheckProgress orig, int slainMonsterID)
		{
			// Let the original method run on its own, that way we don't interrupt any standard vanilla behavior
			orig(slainMonsterID);

			// If we just killed the first enemy in a wave, reset the variable for spawning medals so we can spawn more when the wave's done
			if (!DD2Event.EnemySpawningIsOnHold && spawnedMedalsThisWave)
				spawnedMedalsThisWave = false;

			// If the config option to drop extra medals is enabled, drop extra medals based on the current wave
			// (they'll drop separately from vanilla's amount, but should still stack since they spawn on top of each other)
			if (Config.MoreOOAMedals && DD2Event.EnemySpawningIsOnHold && !spawnedMedalsThisWave)
			{
				spawnedMedalsThisWave = true;

				// Note that this variable is NOT the number of the wave the player just cleared, but the number of the next wave to spawn!
				// So if the player just cleared wave 1, this value will be 2
				int upcomingWave = NPC.waveNumber;
				int bonusMedals = 1;

				switch (DD2Event.OngoingDifficulty)
				{
					// Tier 1 drops 1 extra medal on every wave except wave 3, which drops no extra
					case 1:
						if (upcomingWave == 4)
							bonusMedals = 0;
						break;
					// Tier 2 drops 1 extra medal on every wave except wave 6, which drops two extra
					case 2:
						if (upcomingWave == 7)
							bonusMedals = 2;
						break;
					// Tier 3 drops 1 extra medal on wave 1, 3 extra medals on waves 2 and 5, 4 extra medals on waves 3 and 4, and no extra medals on wave 6
					case 3:
						if (upcomingWave == 3 || upcomingWave == 6)
							bonusMedals = 3;
						else if (upcomingWave == 4 || upcomingWave == 5)
							bonusMedals = 4;
						else if (upcomingWave == 7)
							bonusMedals = 0;
						break;
				}

				// Medals don't drop on final wave since the DropMoreDefenderMedals_Victory method handles that
				if (bonusMedals > 0 && (upcomingWave <= 5 || (DD2Event.OngoingDifficulty > 1 && upcomingWave <= 7)))
					DD2Event.DropMedals(bonusMedals);
			}
		}

		private static bool ApplyIchorFromBuffedBrainDodge = false;

		// When the Brain of Confusion projectile spawns, reset ApplyIchorFromBuffedBrainDodge so that other sources of Confusion don't start applying Ichor
		private int ResetIchorState(On_Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2)
		{
			ApplyIchorFromBuffedBrainDodge = false;

			return orig(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
		}

		private void BuffedBrainDodge(On_Player.orig_BrainOfConfusionDodge orig, Player self)
		{
			orig(self);

			// If config is active and the requirements are met, also spawn three temporary probes
			// They'll automatically despawn once the Cerebral Mindtrick buff wears off
			TerratweaksPlayer tPlr = self.GetModPlayer<TerratweaksPlayer>();
			if (Config.WormBrain && tPlr.IsBuffedBrainEquipped())
			{
				// Also tell the game that we should apply Ichor when NPCs next get hit with Confused
				ApplyIchorFromBuffedBrainDodge = true;

				for (int i = 0; i < 3; i++)
				{
					Vector2 velocity = Main.rand.NextVector2Unit(); // Returns a vector between (-1, -1) and (1, 1)
					Projectile.NewProjectile(self.GetSource_Accessory(tPlr.buffedBrainOfConfusion), self.Center, velocity, TerratweaksPlayer.PROBE_ID, 20, 0, self.whoAmI);
				}

				// Also apply Cerebral Mindtrick for 12 seconds instead of 4
				self.AddBuff(BuffID.BrainOfConfusionBuff, Conversions.ToFrames(12), quiet: false);
			}
		}

		private void BuffedBrainIchor(On_NPC.orig_AddBuff orig, NPC self, int type, int time, bool quiet)
		{
			if (Config.WormBrain && type == BuffID.Confused && ApplyIchorFromBuffedBrainDodge)
			{
				// Apply the Ichor debuff in addition to Confused
				orig(self, BuffID.Ichor, time, quiet);
			}

			orig(self, type, time, quiet);
		}

		private void BuffedEyeShieldDash(On_Player.orig_DashMovement orig, Player self)
		{
			orig(self);

			// If we have the buffed Shield of Cthulhu and just hit an enemy, inflict Cursed Inferno onto them
			if (Config.EyeShield && NPC.downedMechBoss2 && self.eocHit > -1)
			{
				NPC npc = Main.npc[self.eocHit];

				// Make sure the enemy is one we should be able to hit (CanNPCBeHit just checks for critters with the Guide to Peaceful Coexistence)
				if (npc.active && !npc.friendly && !npc.dontTakeDamage && self.CanNPCBeHitByPlayerOrPlayerProjectile(npc))
				{
					npc.AddBuff(BuffID.CursedInferno, Conversions.ToFrames(3)); // Apply 3 seconds Cursed Inferno
					self.dashDelay = Math.Min(self.dashDelay, 15); // Prevent the dash delay from being above 15 ticks, effectively halving it since it's normally 30
				}
			}
		}

		private PaintingEntry WhereIsntWaldo(On_WorldGen.orig_RandHousePicture orig)
		{
			if (Config.DrunkWaldo && (WorldGen.drunkWorldGen || WorldGen.everythingWorldGen))
			{
				// 0.49% chance, same as the normal odds of Waldo
				// This effectively flips the script- 99.51% of paintings will be Waldo, instead of 0.49%
				if (WorldGen.genRand.NextBool(49, 10000))
				{
					return orig();
				}

				// Guarantees a Waldo painting- 2x3 painting with a style value of 0
				return new PaintingEntry
				{
					tileType = TileID.Painting2X3,
					style = 0
				};
			}

			return orig();
		}

		private void LavalessLavaSlime(On_NPC.orig_HitEffect_HitInfo orig, NPC self, NPC.HitInfo hit)
		{
			int lavaTileX = (int)(self.Center.X / 16f);
			int lavaTileY = (int)(self.Center.Y / 16f);
			Tile lavaTile = Main.tile[lavaTileX, lavaTileY];
			byte oldLiquid = lavaTile.LiquidAmount;

			orig(self, hit);

			// Do nothing if the config isn't enabled, we aren't dealing with a lava slime, or lava slimes wouldn't drop lava in the first place
			if (!Config.LavalessLavaSlimes || self.type != NPCID.LavaSlime || self.life > 0 || !Main.expertMode || Main.remixWorld || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			// If any lava was created, delete it
			if (oldLiquid == 0 && lavaTile.LiquidAmount > 0)
			{
				lavaTile.LiquidAmount = 0;
			}
		}

		private Color HunterHighlightOverride(On_NPC.orig_GetNPCColorTintedByBuffs orig, NPC self, Color npcColor)
		{
			if (self.CanApplyHunterPotionEffects() && self.lifeMax > 1)
			{
				if (ClientConfig.OverrideHunterGlow && Main.LocalPlayer.detectCreature)
				{
					Color color = IsFriendlyOrCritter(self) ? ClientConfig.FriendlyGlowColor : ClientConfig.EnemyGlowColor;
					color.A = npcColor.A;
					return color;
				}
			}

			return orig(self, npcColor);
		}

		private static bool IsFriendlyOrCritter(NPC npc)
		{
			return npc.friendly || npc.catchItem > 0 || (npc.damage == 0 && npc.lifeMax == 5);
		}

		public static bool CheckNoEvents(bool vanillaValue) => vanillaValue && !Config.NoEventSpawns;

		public static bool CheckNoEvents_Invert(bool vanillaValue) => vanillaValue || Config.NoEventSpawns;

		public static bool CheckNoBosses(bool vanillaValue) => vanillaValue && !Config.NoBossSpawns;

		public static bool CheckNoBosses_Invert(bool vanillaValue) => vanillaValue || Config.NoBossSpawns;

		private void DisableEventSpawns_Day(ILContext il)
		{
			var c = new ILCursor(il);

			if (!c.TryGotoNext(i => i.MatchLdsfld("Terraria.Main", "hardMode")))
			{
				Logger.Warn("Terratweaks IL edit failed to find Main.hardMode check when trying to disable Solar Eclipse spawns! Dumping IL logs...");
				MonoModHooks.DumpIL(this, il);
				return;
			}

			c.Index++;
			c.Emit(OpCodes.Call, GetType().GetMethod(nameof(CheckNoEvents), BindingFlags.Public | BindingFlags.Static));

			if (!c.TryGotoNext(i => i.MatchLdsfld("Terraria.Main", "snowMoon")))
			{
				Logger.Warn("Terratweaks IL edit failed to find Main.snowMoon check when trying to disable invasion spawns! Dumping IL logs...");
				MonoModHooks.DumpIL(this, il);
				return;
			}

			c.Index++;
			c.Emit(OpCodes.Call, GetType().GetMethod(nameof(CheckNoEvents_Invert), BindingFlags.Public | BindingFlags.Static));
		}

		private void DisableEventAndBossSpawns_Night(ILContext il)
		{
			var c = new ILCursor(il);

			if (!c.TryGotoNext(i => i.MatchLdsfld("Terraria.NPC", "downedBoss1")))
			{
				Logger.Warn("Terratweaks IL edit failed to find NPC.downedBoss1 check when trying to disable EoC spawns! Dumping IL logs...");
				MonoModHooks.DumpIL(this, il);
				return;
			}

			c.Index++;
			c.Emit(OpCodes.Call, GetType().GetMethod(nameof(CheckNoBosses_Invert), BindingFlags.Public | BindingFlags.Static));

			if (!c.TryGotoNext(i => i.MatchLdsfld("Terraria.WorldGen", "spawnEye")))
			{
				Logger.Warn("Terratweaks IL edit failed to find WorldGen.spawnEye check when trying to disable mech boss spawns! Dumping IL logs...");
				MonoModHooks.DumpIL(this, il);
				return;
			}

			c.Index++;
			c.Emit(OpCodes.Call, GetType().GetMethod(nameof(CheckNoBosses_Invert), BindingFlags.Public | BindingFlags.Static));

			if (!c.TryGotoNext(i => i.MatchLdsfld("Terraria.WorldGen", "spawnEye")))
			{
				Logger.Warn("Terratweaks IL edit failed to find WorldGen.spawnEye check when trying to disable Blood Moon spawns! Dumping IL logs...");
				MonoModHooks.DumpIL(this, il);
				return;
			}

			c.Index++;
			c.Emit(OpCodes.Call, GetType().GetMethod(nameof(CheckNoEvents_Invert), BindingFlags.Public | BindingFlags.Static));
		}

		private void DisableBossSpawns_Deerclops(ILContext il)
		{
			var c = new ILCursor(il);

			if (!c.TryGotoNext(i => i.MatchLdsfld("Terraria.Main", "time")))
			{
				Logger.Warn("Terratweaks IL edit failed to find Main.time check when trying to disable Deerclops spawns! Dumping IL logs...");
				MonoModHooks.DumpIL(this, il);
				return;
			}

			if (!c.TryGotoNext(i => i.MatchLdsfld("Terraria.Main", "raining")))
			{
				Logger.Warn("Terratweaks IL edit failed to find Main.raining check when trying to disable Deerclops spawns! Dumping IL logs...");
				MonoModHooks.DumpIL(this, il);
				return;
			}

			c.Index++;
			c.Emit(OpCodes.Call, GetType().GetMethod(nameof(CheckNoBosses), BindingFlags.Public | BindingFlags.Static));
		}

		private void DisableSlimeRain(On_Main.orig_StartSlimeRain orig, bool announce)
		{
			// Prevents any code related to slime rain from running
			if (Config.NoEventSpawns)
				return;

			orig(announce);
		}

		private bool ChesterIndexChange(On_Projectile.orig_TryGetContainerIndex orig, Projectile proj, out int containerIndex)
		{
			// This makes Chester count as a safe (index -3) instead of a piggy bank (index -2)
			// This SHOULD reduce some of the jank surrounding him, but definitely not *everything*
			if (Config.ChesterRework && proj.type == ProjectileID.ChesterPet)
			{
				containerIndex = -3;
				return true;
			}

			return orig(proj, out containerIndex);
		}
	}
}