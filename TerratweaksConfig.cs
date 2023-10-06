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
		public bool KillSentries { get; set; }

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

		[DefaultValue(SentryAccSetting.Limited)]
		public SentryAccSetting StackableDD2Accs { get; set; }
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
		public bool SpiderRework { get; set; }

		[DefaultValue(true)]
		public bool CobaltRework { get; set; }

		[DefaultValue(true)]
		public bool MythrilRework { get; set; }

		[DefaultValue(true)]
		public bool AdamantiteRework { get; set; }

		[DefaultValue(true)]
		public bool SpookyRework { get; set; }
	}
}
