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

	public class TerratweaksConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		
		public ArmorReworks armorBonuses = new();

		public ExpertAccBuffs expertAccBuffs = new();

		public CraftableUncraftables craftableUncraftables = new();

		[DefaultValue(true)]
		public bool BannersDontSpamChat { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ChesterRework { get; set; }

		[DefaultValue(true)]
		public bool DeerclopsRegens { get; set; }

		[Increment(60)]
		[Range(60, 3600)]
		[DefaultValue(600)]
		public int DeerRegenAmt { get; set; }

		[DefaultValue(DummySetting.Limited)]
		public DummySetting DummyFix { get; set; }

		[DefaultValue(true)]
		public bool DyeTraderShopExpansion { get; set; }

		[DefaultValue(true)]
		public bool KillSentries { get; set; }

		[DefaultValue(false)]
		public bool NoCasterContactDamage { get; set; }

		[DefaultValue(false)]
		public bool NoDiminishingReturns { get; set; }

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

		[DefaultValue(true)]
		public bool PostEyeSandstorms { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool SIRework { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool SoilSolutionsPreML { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool SolutionsOnGFB { get; set; }

		[DefaultValue(SentryAccSetting.Limited)]
		public SentryAccSetting StackableDD2Accs { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool UmbrellaHatRework { get; set; }
	}

	public class TerratweaksConfig_Client : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		
		[DefaultValue(true)]
		public bool NoDamageVariance { get; set; }

		[DefaultValue(true)]
		public bool NoRandomCrit { get; set; }

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
	}
}
