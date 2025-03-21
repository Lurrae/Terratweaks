using Terraria.ModLoader.Config;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader;

namespace Terratweaks
{
	public enum DummySetting
	{
		Off,
		Limited,
		On
	}

	public enum SentryAccSetting
	{
		Off,
		Limited,
		On
	}

	public enum DamageVarianceSetting
	{
		Off,
		Limited,
		On
	}

	public enum CoinTheftSetting
	{
		Off,
		Limited,
		On
	}

	public enum SturdyLarvaeSetting
	{
		Off,
		Env,
		Peace,
		Bee
	}

	public class TerratweaksConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		// Extra tweak categories
		[Header("Categories")]

		public CraftableUncraftables craftableUncraftables = new();

		public CalTweaks calamitweaks = new();

		public ThorTweaks thoritweaks = new();

		public AlchTweaks alchemitweaks = new();

		// Block & biome tweaks
		[Header("BlockBiomeTweaks")]

		[DefaultValue(true)]
		public bool BetterCrackedBricks { get; set; }

		[DefaultValue(true)]
		public bool BombableMeteorite { get; set; }

		[DefaultValue(false)]
		public bool OverrideGraveyardRequirements { get; set; }

		[Increment(1)]
		[Range(1, 18)]
		[DefaultValue(4)]
		[Slider]
		public int GraveyardVisuals { get; set; }

		[Increment(1)]
		[Range(1, 18)]
		[DefaultValue(7)]
		[Slider]
		public int GraveyardFunctionality { get; set; }

		[Increment(1)]
		[Range(1, 18)]
		[DefaultValue(9)]
		[Slider]
		public int GraveyardMax { get; set; }

		[DefaultValue(SturdyLarvaeSetting.Env)]
		public SturdyLarvaeSetting SturdyLarvae { get; set; }

		// Boss and Event Tweaks
		[Header("BossTweaks")]

		[DefaultValue(0)]
		[Range(0, 10)]
		[Slider]
		public int NPCDeathsToCallOffInvasion { get; set; }

		[DefaultValue(0)]
		[Range(0, 10)]
		[Slider]
		public int PlayerDeathsToCallOffInvasion { get; set; }

		[DefaultValue(true)]
		public bool DeerclopsRegens { get; set; }

		[Increment(60)]
		[Range(60, 3600)]
		[DefaultValue(600)]
		public int DeerRegenAmt { get; set; }

		[DefaultValue(false)]
		public bool ForceBossContactDamage { get; set; }

		[DefaultValue(false)]
		public bool MoreOOAMedals { get; set; }

		[DefaultValue(true)]
		public bool PostEyeSandstorms { get; set; }

		[DefaultValue(1)]
		[Range(0f, 10f)]
		[Increment(0.25f)]
		public float BossesLowerSpawnRates { get; set; }

		[ReloadRequired]
		[DefaultValue(0)]
		public int TerraprismaDropRate { get; set; }

		// Enemy Tweaks
		[Header("EnemyTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PillarEnemiesDropFragments { get; set; }

		[DefaultValue(true)]
		public bool NoEnemyInvulnerability { get; set; }

		[DefaultValue(false)]
		public bool NoCasterContactDamage { get; set; }

		[DefaultValue(false)]
		public bool SmartMimics { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool SmartNymphs { get; set; }

		[DefaultValue(DummySetting.Limited)]
		public DummySetting DummyFix { get; set; }

		// Expert Mode Accessory Buffs
		[Header("ExpertAccTweaks")]

		[DefaultValue(true)]
		public bool BoneGlove { get; set; }

		[DefaultValue(true)]
		public bool BoneHelm { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool CultistGravGlobe { get; set; }

		[DefaultValue(true)]
		public bool HivePack { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PlaceableGravGlobe { get; set; }

		[Slider]
		[Increment(5)]
		[Range(5, 200)]
		[DefaultValue(125)]
		public int GravGlobeRange { get; set; }

		[DefaultValue(true)]
		public bool RoyalGel { get; set; }

		[DefaultValue(true)]
		public bool EyeShield { get; set; }

		[DefaultValue(true)]
		public bool WormBrain { get; set; }

		// Expert Mode Tweaks
		[Header("ExpertTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool JungleBossBags { get; set; }

		[ReloadRequired]
		[DefaultValue(CoinTheftSetting.Limited)]
		public CoinTheftSetting NoCoinTheft { get; set; }

		[DefaultValue(false)]
		public bool NoExpertScaling { get; set; }

		[DefaultValue(false)]
		public bool NoExpertDebuffTimes { get; set; }

		[DefaultValue(false)]
		public bool NoExpertFreezingWater { get; set; }

		// For the Worthy Tweaks
		[Header("FtwTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool NerfSkyCrates { get; set; }

		[DefaultValue(false)]
		public bool NoMobGriefing { get; set; }

		// General Item Tweaks
		[Header("ItemTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ChesterRework { get; set; }

		[DefaultValue(DamageVarianceSetting.Off)]
		public DamageVarianceSetting NoDamageVariance { get; set; }

		[DefaultValue(false)]
		public bool CritsBypassDefense { get; set; }

		[DefaultValue(true)]
		public bool DangersenseHighlightsSilt { get; set; }

		[DefaultValue(true)]
		public bool DangersenseIgnoresThinIce { get; set; }

		[DefaultValue(true)]
		public bool DeerWeaponsRework { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool LunarWingsPreML { get; set; }

		[DefaultValue(false)]
		public bool NoDiminishingReturns { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool OasisCrateBuff { get; set; }

		[ReloadRequired]
		[Slider]
		[Range(1, 50)]
		[DefaultValue(35)]
		public int OasisCrateOdds { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ReaverSharkTweaks { get; set; }

		[DefaultValue(true)]
		public bool KillSentries { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool SIRework { get; set; }

		[DefaultValue(SentryAccSetting.Limited)]
		public SentryAccSetting StackableDD2Accs { get; set; }

		[DefaultValue(true)]
		public bool ManaFreeSummoner { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool UmbrellaHatRework { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool OreUnification { get; set; }

		// Joke Tweaks
		[Header("JokeTweaks")]

		[DefaultValue(false)]
		public bool PaperCuts { get; set; }

		// Misc. Tweaks
		[Header("MiscTweaks")]

		[DefaultValue(true)]
		public bool BannersDontSpamChat { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool BetterBestiary { get; set; }

		// Town NPC Tweaks
		[Header("TownNpcTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool SolutionsOnGFB { get; set; }

		[DefaultValue(true)]
		public bool DyeTraderShopExpansion { get; set; }

		[DefaultValue(false)]
		public bool HouseSizeAffectsHappiness { get; set; }

		[DefaultValue(true)]
		public bool BetterHappiness { get; set; }

		[DefaultValue(false)]
		public bool BoundNPCsImmune { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool NPCsSellMinecarts { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TownNPCsSellWeapons { get; set; }

		[DefaultValue(true)]
		public bool OldChestDungeon { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool SoilSolutionsPreML { get; set; }

		// Vanilla Armorset Reworks
		[Header("ArmorTweaks")]

		[DefaultValue(true)]
		public bool AdamantiteSetBonus { get; set; }

		[DefaultValue(true)]
		public bool CobaltSetBonus { get; set; }

		[DefaultValue(true)]
		public bool ConvertMonkArmor { get; set; }

		[DefaultValue(true)]
		public bool MythrilSetBonus { get; set; }

		[DefaultValue(true)]
		public bool SpiderSetBonus { get; set; }

		[DefaultValue(true)]
		public bool SpookySetBonus { get; set; }

		[DefaultValue(true)]
		public bool StardustArmorBuff { get; set; }

		public override bool NeedsReload(ModConfig pendingConfig)
		{
			if (pendingConfig is TerratweaksConfig pendingTtConfig)
			{
				// Check all craftable uncraftable configs for forced reloads
				// TODO: This will need to be removed if/when tmod updates to check nested ReloadRequireds
				if (craftableUncraftables.GemCritters != pendingTtConfig.craftableUncraftables.GemCritters ||
					craftableUncraftables.PlanterBoxes != pendingTtConfig.craftableUncraftables.PlanterBoxes ||
					craftableUncraftables.DungeonFurniture != pendingTtConfig.craftableUncraftables.DungeonFurniture ||
					craftableUncraftables.ObsidianFurniture != pendingTtConfig.craftableUncraftables.ObsidianFurniture ||
					craftableUncraftables.StructureBanners != pendingTtConfig.craftableUncraftables.StructureBanners ||
					craftableUncraftables.Moss != pendingTtConfig.craftableUncraftables.Moss ||
					craftableUncraftables.Gravestones != pendingTtConfig.craftableUncraftables.Gravestones ||
					craftableUncraftables.Trophies != pendingTtConfig.craftableUncraftables.Trophies ||
					craftableUncraftables.ClothierVoodooDoll != pendingTtConfig.craftableUncraftables.ClothierVoodooDoll ||
					craftableUncraftables.TempleTraps != pendingTtConfig.craftableUncraftables.TempleTraps ||
					craftableUncraftables.ShimmerBottomlessAndSponges != pendingTtConfig.craftableUncraftables.ShimmerBottomlessAndSponges ||
					craftableUncraftables.TeamBlocks != pendingTtConfig.craftableUncraftables.TeamBlocks ||
					craftableUncraftables.PrehardUnobtainables != pendingTtConfig.craftableUncraftables.PrehardUnobtainables ||
					craftableUncraftables.ShimmerBossDrops != pendingTtConfig.craftableUncraftables.ShimmerBossDrops ||
					craftableUncraftables.GeyserTraps != pendingTtConfig.craftableUncraftables.GeyserTraps ||
					craftableUncraftables.ShimmerBlackLens != pendingTtConfig.craftableUncraftables.ShimmerBlackLens)
				{
					return true;
				}

				// Check all Calamitweaks configs for forced reloads
				// TODO: Ditto the above todo (remove if/when tmod fixes the issue that makes this necessary)
				if (calamitweaks.AsgardsValorBuff != pendingTtConfig.calamitweaks.AsgardsValorBuff ||
					calamitweaks.CombinedStationSupport != pendingTtConfig.calamitweaks.CombinedStationSupport ||
					calamitweaks.CraftableHostileTurrets != pendingTtConfig.calamitweaks.CraftableHostileTurrets ||
					calamitweaks.DeificAmuletBuff != pendingTtConfig.calamitweaks.DeificAmuletBuff ||
					calamitweaks.DryadSellsSeeds != pendingTtConfig.calamitweaks.DryadSellsSeeds ||
					calamitweaks.EnemyFoodDrops != pendingTtConfig.calamitweaks.EnemyFoodDrops ||
					calamitweaks.ForceWormContactDamage != pendingTtConfig.calamitweaks.ForceWormContactDamage ||
					calamitweaks.EzCalBanners != pendingTtConfig.calamitweaks.EzCalBanners ||
					calamitweaks.NoSellingRoD != pendingTtConfig.calamitweaks.NoSellingRoD ||
					calamitweaks.OnionMasterMode != pendingTtConfig.calamitweaks.OnionMasterMode ||
					calamitweaks.RevertPickSpeedBuffs != pendingTtConfig.calamitweaks.RevertPickSpeedBuffs ||
					calamitweaks.RevertPillars != pendingTtConfig.calamitweaks.RevertPillars ||
					calamitweaks.RevertTerraprisma != pendingTtConfig.calamitweaks.RevertTerraprisma ||
					calamitweaks.RevertVanillaBossAIChanges != pendingTtConfig.calamitweaks.RevertVanillaBossAIChanges)
				{
					return true;
				}

				// Check all Thoritweaks configs for forced reloads
				// TODO: Once again, ditto the above todos
				if (thoritweaks.CombinedStationSupport != pendingTtConfig.thoritweaks.CombinedStationSupport ||
					thoritweaks.ZenithRecipeOverhaul != pendingTtConfig.thoritweaks.ZenithRecipeOverhaul)
				{
					return true;
				}

				// Check all Alchemitweaks configs for forced reloads
				// TODO: Once again, ditto the above todos
				if (alchemitweaks.AntiCheese != pendingTtConfig.alchemitweaks.AntiCheese ||
					alchemitweaks.DisableCustomPotions != pendingTtConfig.alchemitweaks.DisableCustomPotions)
				{
					return true;
				}
			}

			return base.NeedsReload(pendingConfig);
		}

		public override void OnChanged()
		{
			if (OverrideGraveyardRequirements)
			{
				SceneMetrics.GraveyardTileMax = GraveyardMax * 4;
				SceneMetrics.GraveyardTileMin = GraveyardVisuals * 4;
				SceneMetrics.GraveyardTileThreshold = GraveyardFunctionality * 4;
			}
			else
			{
				if (ModLoader.HasMod("CalamityMod"))
				{
					SceneMetrics.GraveyardTileMax = 15 * 4;
					SceneMetrics.GraveyardTileMin = 10 * 4;
					SceneMetrics.GraveyardTileThreshold = 13 * 4;
				}
				else
				{
					SceneMetrics.GraveyardTileMax = 7 * 4;
					SceneMetrics.GraveyardTileMin = 4 * 4;
					SceneMetrics.GraveyardTileThreshold = 9 * 4;
				}
			}
		}
	}

	public class TerratweaksConfig_Client : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Misc. Tweaks
		[Header("MiscTweaks")]

		[DefaultValue(true)]
		public bool NoRandomCrit { get; set; }

		// Tooltip Tweaks
		[Header("TooltipTweaks")]

		[DefaultValue(true)]
		public bool EstimatedDPS { get; set; }

		[DefaultValue(true)]
		public bool GrammarCorrections { get; set; }

		[DefaultValue(true)]
		public bool PermBuffTips { get; set; }

		[DefaultValue(true)]
		public bool StatsInTip { get; set; }

		[DefaultValue(true)]
		public bool WingStatsInTip { get; set; }
	}

	[SeparatePage]
	public class CraftableUncraftables
	{
		// Crafting Recipes
		[Header("Crafting")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ClothierVoodooDoll { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool DungeonFurniture { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool GemCritters { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool GeyserTraps { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TempleTraps { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ObsidianFurniture { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PlanterBoxes { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool StructureBanners { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TeamBlocks { get; set; }

		// Shimmer Transmutations
		[Header("Shimmer")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShimmerBlackLens { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShimmerBossDrops { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShimmerBottomlessAndSponges { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Gravestones { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PrehardUnobtainables { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Moss { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Trophies { get; set; }
	}

	[SeparatePage]
	public class CalTweaks
	{
		// Accessory Tweaks
		[Header("AccTweaks")]

		[DefaultValue(true)]
		public bool AquaticEmblemBuff { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool AsgardsValorBuff { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool DeificAmuletBuff { get; set; }

		[DefaultValue(true)]
		public bool RadiantInsigniaUpgradesFromAscendant { get; set; }

		[DefaultValue(false)]
		public bool SummonerAccBuffs { get; set; }

		// Block and Biome Tweaks
		//[Header("BlockBiomeTweaks")]
		
		// Boss and Event Tweaks
		[Header("BossTweaks")]

		[DefaultValue(true)]
		public bool EnragedEoLInstakills { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool RevertPillars { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool RevertTerraprisma { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool RevertVanillaBossAIChanges { get; set; }

		// Enemy Tweaks
		[Header("EnemyTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EnemyFoodDrops { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EzCalBanners { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ForceWormContactDamage { get; set; }

		[DefaultValue(true)]
		public bool NoPlantParticles { get; set; }

		[DefaultValue(true)]
		public bool NoWormParticles { get; set; }

		// Gameplay Tweaks
		[Header("GameplayTweaks")]

		[DefaultValue(false)]
		public bool DRBuffs { get; set; }

		[DefaultValue(false)]
		public bool NoDefenseDamage { get; set; }

		// General Item Tweaks
		[Header("ItemTweaks")]

		[ReloadRequired]
		[DefaultValue(false)]
		public bool OnionMasterMode { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool RevertPickSpeedBuffs { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ZenithRecipeOverhaul { get; set; }
		
		// Misc. tweaks
		[Header("MiscTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool CombinedStationSupport { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool CraftableHostileTurrets { get; set; }

		// Town NPC tweaks
		[Header("TownNpcTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool DryadSellsSeeds { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool NoPatreonNPCNames { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool NoSellingRoD { get; set; }

		// Wrath of the Gods Tweaks
		//[Header("WotgTweaks")]
	}

	[SeparatePage]
	public class ThorTweaks
	{
		// Accessory Tweaks
		//[Header("AccTweaks")]

		// Block and Biome Tweaks
		[Header("BlockBiomeTweaks")]

		[DefaultValue(false)]
		public bool BombableADBlocks { get; set; }

		// Boss and Event Tweaks
		//[Header("BossTweaks")]

		// Enemy Tweaks
		//[Header("EnemyTweaks")]

		// Gameplay Tweaks
		//[Header("GameplayTweaks")]

		// General Item Tweaks
		[Header("ItemTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ZenithRecipeOverhaul { get; set; }

		// Misc. Tweaks
		[Header("MiscTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool CombinedStationSupport { get; set; }

		// Town NPC Tweaks
		[Header("TownNpcTweaks")]

		[DefaultValue(false)]
		public bool EatCooksFoodInCombat { get; set; }
	}

	[SeparatePage]
	public class AlchTweaks
	{
		// Accessory Tweaks
		//[Header("AccTweaks")]

		// Block and Biome Tweaks
		//[Header("BlockBiomeTweaks")]

		// Boss and Event Tweaks
		//[Header("BossTweaks")]

		// Enemy Tweaks
		//[Header("EnemyTweaks")]

		// Gameplay Tweaks
		//[Header("GameplayTweaks")]

		// General Item Tweaks
		//[Header("ItemTweaks")]

		// Misc. Tweaks
		//[Header("MiscTweaks")]

		// Town NPC Tweaks
		[Header("TownNpcTweaks")]

		[ReloadRequired]
		[DefaultValue(false)]
		public bool AntiCheese { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool DisableCustomPotions { get; set; }
	}
}
