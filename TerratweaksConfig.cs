using Terraria.ModLoader.Config;
using System.ComponentModel;

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

	public class TerratweaksConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		
		public ArmorReworks armorBonuses = new();

		public ExpertAccBuffs expertAccBuffs = new();

		public CraftableUncraftables craftableUncraftables = new();

		public CalTweaks calamitweaks = new();

		public ThorTweaks thoritweaks = new();

		public AlchTweaks alchemitweaks = new();

		[DefaultValue(true)]
		public bool BannersDontSpamChat { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool BetterBestiary { get; set; }

		[DefaultValue(true)]
		public bool BetterCrackedBricks { get; set; }

		[DefaultValue(true)]
		public bool BetterHappiness { get; set; }

		[DefaultValue(1)]
		[Range(0f, 10f)]
		[Increment(0.25f)]
		public float BossesLowerSpawnRates { get; set; }

		[DefaultValue(0)]
		[Range(0, 10)]
		public int PlayerDeathsToCallOffInvasion { get; set; }

		[DefaultValue(0)]
		[Range(0, 10)]
		public int NPCDeathsToCallOffInvasion { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ChesterRework { get; set; }

		[DefaultValue(false)]
		public bool CritsBypassDefense { get; set; }

		[DefaultValue(true)]
		public bool DeerclopsRegens { get; set; }

		[Increment(60)]
		[Range(60, 3600)]
		[DefaultValue(600)]
		public int DeerRegenAmt { get; set; }

		[DefaultValue(true)]
		public bool DeerWeaponsRework { get; set; }

		[DefaultValue(DummySetting.Limited)]
		public DummySetting DummyFix { get; set; }

		[DefaultValue(true)]
		public bool DyeTraderShopExpansion { get; set; }

		[DefaultValue(true)]
		public bool DangersenseHighlightsSilt { get; set; }

		[DefaultValue(true)]
		public bool DangersenseIgnoresThinIce { get; set; }

		[DefaultValue(false)]
		public bool ForceBossContactDamage { get; set; }

		[DefaultValue(false)]
		public bool HouseSizeAffectsHappiness { get; set; }

		[DefaultValue(true)]
		public bool KillSentries { get; set; }

		[DefaultValue(true)]
		public bool ManaFreeSummoner { get; set; }

		[DefaultValue(false)]
		public bool NoCasterContactDamage { get; set; }

		[ReloadRequired]
		[DefaultValue(CoinTheftSetting.On)]
		public CoinTheftSetting NoCoinTheft { get; set; }

		[DefaultValue(DamageVarianceSetting.On)]
		public DamageVarianceSetting NoDamageVariance { get; set; }

		[DefaultValue(false)]
		public bool NoDiminishingReturns { get; set; }

		[DefaultValue(true)]
		public bool NoEnemyInvulnerability { get; set; }

		[DefaultValue(false)]
		public bool NoExpertDebuffTimes { get; set; }

		[DefaultValue(false)]
		public bool NoExpertFreezingWater { get; set; }

		[DefaultValue(false)]
		public bool NoExpertScaling { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool NPCsSellMinecarts { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool OasisCrateBuff { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool OreUnification { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PillarEnemiesDropFragments { get; set; }

		[DefaultValue(true)]
		public bool PostEyeSandstorms { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ReaverSharkTweaks { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool SIRework { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool SoilSolutionsPreML { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool SolutionsOnGFB { get; set; }

		[DefaultValue(false)]
		public bool SmartMimics { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool SmartNymphs { get; set; }

		[DefaultValue(SentryAccSetting.Limited)]
		public SentryAccSetting StackableDD2Accs { get; set; }

		[ReloadRequired]
		[DefaultValue(0)]
		public int TerraprismaDropRate { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TownNPCsSellWeapons { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool UmbrellaHatRework { get; set; }

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
					craftableUncraftables.ShimmerBossDrops != pendingTtConfig.craftableUncraftables.ShimmerBossDrops)
				{
					return true;
				}

				// Check all Calamitweaks configs for forced reloads
				// TODO: Ditto the above todo (remove if/when tmod fixes the issue that makes this necessary)
				if (calamitweaks.AsgardsValorBuff != pendingTtConfig.calamitweaks.AsgardsValorBuff ||
					calamitweaks.CraftableHostileTurrets != pendingTtConfig.calamitweaks.CraftableHostileTurrets ||
					calamitweaks.DeificAmuletBuff != pendingTtConfig.calamitweaks.DeificAmuletBuff ||
					calamitweaks.DryadSellsSeeds != pendingTtConfig.calamitweaks.DryadSellsSeeds ||
					calamitweaks.EnemyFoodDrops != pendingTtConfig.calamitweaks.EnemyFoodDrops ||
					calamitweaks.EzCalBanners != pendingTtConfig.calamitweaks.EzCalBanners ||
					calamitweaks.NoSellingRoD != pendingTtConfig.calamitweaks.NoSellingRoD ||
					calamitweaks.OnionMasterMode != pendingTtConfig.calamitweaks.OnionMasterMode ||
					calamitweaks.RevertGraveyards != pendingTtConfig.calamitweaks.RevertGraveyards ||
					calamitweaks.RevertPickSpeedBuffs != pendingTtConfig.calamitweaks.RevertPickSpeedBuffs ||
					calamitweaks.RevertPillars != pendingTtConfig.calamitweaks.RevertPillars ||
					calamitweaks.RevertTerraprisma != pendingTtConfig.calamitweaks.RevertTerraprisma ||
					calamitweaks.RevertVanillaBossAIChanges != pendingTtConfig.calamitweaks.RevertVanillaBossAIChanges)
				{
					return true;
				}

				// Check all Alchemitweaks configs for forced reloads
				// TODO: Once again, ditto the above todos
				if (alchemitweaks.DisableCustomPotions != pendingTtConfig.alchemitweaks.DisableCustomPotions)
				{
					return true;
				}
			}

			return base.NeedsReload(pendingConfig);
		}
	}

	public class TerratweaksConfig_Client : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[DefaultValue(true)]
		public bool EstimatedDPS { get; set; }

		[DefaultValue(true)]
		public bool NoRandomCrit { get; set; }

		[DefaultValue(true)]
		public bool PermBuffTips { get; set; }

		[DefaultValue(true)]
		public bool StatsInTip { get; set; }
	}

	[SeparatePage]
	public class ExpertAccBuffs
	{
		[DefaultValue(true)]
		public bool RoyalGel { get; set; }

		[DefaultValue(true)]
		public bool HivePack { get; set; }

		[DefaultValue(true)]
		public bool BoneHelm { get; set; }
	}

	[SeparatePage]
	public class ArmorReworks
	{
		[DefaultValue(true)]
		public bool Spider { get; set; }

		[DefaultValue(true)]
		public bool Cobalt { get; set; }

		[DefaultValue(true)]
		public bool Mythril { get; set; }

		[DefaultValue(true)]
		public bool Adamantite { get; set; }

		[DefaultValue(true)]
		public bool Spooky { get; set; }
	}

	[SeparatePage]
	public class CraftableUncraftables
	{
		[ReloadRequired]
		[DefaultValue(true)]
		public bool GemCritters { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PlanterBoxes { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool DungeonFurniture { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ObsidianFurniture { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool StructureBanners { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Moss { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Gravestones { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Trophies { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ClothierVoodooDoll { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TempleTraps { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShimmerBottomlessAndSponges { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TeamBlocks { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PrehardUnobtainables { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShimmerBossDrops { get; set; }
	}

	[SeparatePage]
	public class CalTweaks
	{
		[DefaultValue(true)]
		public bool AquaticEmblemBuff { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool AsgardsValorBuff { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool CraftableHostileTurrets { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool DeificAmuletBuff { get; set; }

		[DefaultValue(false)]
		public bool DRBuffs { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool DryadSellsSeeds { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EnemyFoodDrops { get; set; }

		[DefaultValue(true)]
		public bool EnragedEoLInstakills { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EzCalBanners { get; set; }

		[DefaultValue(false)]
		public bool NoDefenseDamage { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool NoPatreonNPCNames { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool NoSellingRoD { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool OnionMasterMode { get; set; }

		[DefaultValue(true)]
		public bool RadiantInsigniaUpgradesFromAscendant { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool RevertGraveyards { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool RevertPickSpeedBuffs { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool RevertPillars { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool RevertTerraprisma { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool RevertVanillaBossAIChanges { get; set; }

		[DefaultValue(false)]
		public bool SummonerAccBuffs { get; set; }
	}

	[SeparatePage]
	public class ThorTweaks
	{
		[DefaultValue(false)]
		public bool EatCooksFoodInCombat { get; set; }
	}

	[SeparatePage]
	public class AlchTweaks
	{
		[ReloadRequired]
		[DefaultValue(false)]
		public bool DisableCustomPotions { get; set; }
	}
}
