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
using Terraria.GameContent.Events;
using Terraria.GameContent.Generation;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terratweaks.Buffs;
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
		public static ThorTweaks Thoritweaks => Config.thoritweaks;

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
			On_DD2Event.CheckProgress += DropMoreDefenderMedals;
			On_DD2Event.WinInvasionInternal += DropMoreDefenderMedals_Victory;
			On_Projectile.CanExplodeTile += HandleExplosives;
			On_Player.GetTileCutIgnorance += DontHurtLarvae;
			On_Player.BrainOfConfusionDodge += BuffedBrainDodge;
			On_NPC.AddBuff += BuffedBrainIchor;
			On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += ResetIchorState;
			On_Player.DashMovement += BuffedEyeShieldDash;
			On_WorldGen.RandHousePicture += WhereIsntWaldo;
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

		public override void PostSetupContent()
		{
			foreach (NPC npc in ContentSamples.NpcsByNetId.Values)
			{
				if (npc.aiStyle == NPCAIStyleID.GraniteElemental)
				{
					StatChangeHandler.DREnemy drEnemyStats = new(0.25f, 1.1f, (NPC npc) => npc.ai[0] == -1);
					if (!StatChangeHandler.damageResistantEnemies.TryAdd(npc.type, drEnemyStats))
					{
						StatChangeHandler.damageResistantEnemies[npc.type] = drEnemyStats;
					}
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
								.Replace(" ", "").Replace("'", "").Replace("&", "").Replace("^", "").Replace("%", "").Replace("#", "").Replace("@", "")
								.Replace("!", "").Replace("?", "").Replace(",", "").Replace(";", "")
								.Replace("/", "_").Replace(":", "_").Replace(".", "_").Replace("-", "_")
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
								// Item name aliases/abbreviations
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
								.Replace("wormscarf", "wormbrain")
								.Replace("brainofconfusion", "boc")
								.Replace("boc", "wormbrain")
								.Replace("wormscarfboc", "wormbrain")
								.Replace("gravityglobe", "gravglobe")
								.Replace("calhostileturrets", "hostileturrets")
								.Replace("sliceofcake", "cakeslice")
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
								.Replace("wrathofthegods", "wotg")
								.Replace("wotgtweaks", "wotg")
								.Replace("thortweaks", "thoritweaks")
								.Replace("alchtweaks", "alchemitweaks");

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
								"infinitecakeslice" => Config.InfiniteCakeSlice,
								"persistentstationbuffs" => Config.PersistentStationBuffs,

								"client_estimateddps" or "estimateddps" => ClientConfig.EstimatedDPS,
								"client_grammarcorrections" or "grammarcorrections" => ClientConfig.GrammarCorrections,
								"client_norandomcrit" or "norandomcrit" => ClientConfig.NoRandomCrit,
								"client_permbufftips" or "permbufftips" => ClientConfig.PermBuffTips,
								"client_statsintip" or "statsintip" => ClientConfig.StatsInTip,
								"client_wingstatsintip" or "wingstatsintip" => ClientConfig.WingStatsInTip,

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

								"calamitweaks_aquaticemblembuff" or "aquaticemblembuff" => Calamitweaks.AquaticEmblemBuff,
								"calamitweaks_asgardsvalorbuff" or "asgardsvalorbuff" => Calamitweaks.AsgardsValorBuff,
								"calamitweaks_combinedstationsupport" or "calcombinedstationsupport" => Calamitweaks.CombinedStationSupport,
								"calamitweaks_craftablehostileturrets" or "craftableuncraftables_hostileturrets" or "hostileturretsrecipe" or "hostileturretsshimmer" => Calamitweaks.CraftableHostileTurrets,
								"calamitweaks_deificamuletbuff" or "deificamuletbuff" => Calamitweaks.DeificAmuletBuff,
								"calamitweaks_drbuffs" or "caldrbuffs" => Calamitweaks.DRBuffs,
								"calamitweaks_dryadsellsseeds" or "dryadsellscalseeds" => Calamitweaks.DryadSellsSeeds,
								"calamitweaks_enemyfooddrops" or "enemyfooddrops" => Calamitweaks.EnemyFoodDrops,
								"calamitweaks_dayeolinstakills" or "dayeolinstakills" => Calamitweaks.EnragedEoLInstakills,
								"calamitweaks_ezcaldanners" or "ezcaldanners" => Calamitweaks.EzCalBanners,
								"calamitweaks_forcewormcontactdamage" or "forcewormcontactdamage" => Calamitweaks.ForceWormContactDamage,
								"calamitweaks_nodefensedamage" or "nodefensedamage" => Calamitweaks.NoDefenseDamage,
								"calamitweaks_nopatreonnpcnames" or "nocalpatreonnpcnames" => Calamitweaks.NoPatreonNPCNames,
								"calamitweaks_noplantparticles" or "noplantparticles" => Calamitweaks.NoPlantParticles,
								"calamitweaks_nosellingrod" or "nosellingrod" => Calamitweaks.NoSellingRoD,
								"calamitweaks_nowormparticles" or "nowormparticles" => Calamitweaks.NoWormParticles,
								"calamitweaks_notimeddr" or "notimeddr" => Calamitweaks.SummonerAccBuffs,
								"calamitweaks_onionmastermode" or "onionmastermode" => Calamitweaks.OnionMasterMode,
								"calamitweaks_radiantinsigniaupgradesfromascendant" or "radiantinsigniaupgradesfromascendant" => Calamitweaks.RadiantInsigniaUpgradesFromAscendant,
								"calamitweaks_revertpickspeedbuffs" or "pickspeedcalreversion" => Calamitweaks.RevertPickSpeedBuffs,
								"calamitweaks_revertpillars" or "pillarcalreversion" => Calamitweaks.RevertPillars,
								"calamitweaks_revertterraprisma" or "terraprismacalreversion" => Calamitweaks.RevertTerraprisma,
								"calamitweaks_revertvanillabossaichanges" or "vanillabossaicalreversion" => Calamitweaks.RevertVanillaBossAIChanges,
								"calamitweaks_summoneraccbuffs" or "calsummoneraccbuffs" => Calamitweaks.SummonerAccBuffs,
								"calamitweaks_zenithrecipeoverhaul" or "calzenithrecipeoverhaul" or "calamitweaks_zenithrecipe" or "calzenithrecipe" => Calamitweaks.ZenithRecipeOverhaul,
								"calamitweaks_wotg_nosilentrift" or "wotg_nosilentrift" or "calamitweaks_nosilentrift" or "nosilentrift" => Calamitweaks.NoSilentRift,

								"thoritweaks_bombableadblocks" or "bombableadblocks" => Thoritweaks.BombableADBlocks,
								"thoritweaks_combinedstationsupport" or "thorcombinedstationsupport" => Thoritweaks.CombinedStationSupport,
								"thoritweaks_eatcooksfoodincombat" or "thoritweaks_cookbuff" or "eatcooksfoodincombat" or "cookbuff" => Thoritweaks.EatCooksFoodInCombat,
								"thoritweaks_zenithrecipeoverhaul" or "thorzenithrecipeoverhaul" or "thoritweaks_zenithrecipe" or "thorzenithrecipe" => Thoritweaks.ZenithRecipeOverhaul,

								"alchemitweaks_disablecustompotions" or "disablecustomalchpotions" => Alchemitweaks.DisableCustomPotions,
								"alchemitweaks_anticheese" or "alchemitweaks_infmoneyfix" or "architectinfmoneyfix" or "architectanticheese" => Alchemitweaks.AntiCheese,

								_ => throw new Exception($"Could not find Terratweaks config option with name \"{args[1]}\" or alias \"{settingToQuery}\"."),
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
					case "AddStationBuff":
						try
						{
							int buffID = Convert.ToInt32(args[1]);
							BuffChanges.StationBuffs.Add(buffID);

							return true;
						}
						catch (OverflowException)
						{
							throw new ArgumentException($"Expected arguments of type int for 'AddStationBuff', but got type {args[1].GetType().Name} instead.");
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
			// Deal 2x NPC's base damage if electrified while this config is enabled, instead of 1.3x
			if (Config.NoEnemyInvulnerability)
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
			if (proj.type == ProjectileID.ChesterPet && Main.mouseRight && Main.mouseRightRelease && playerHasChesterSafeOpened)
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
	}
}