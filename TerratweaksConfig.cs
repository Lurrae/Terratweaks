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
		public static TerratweaksConfig Instance;

		public VanillaChanges vanillaChanges = new();
	}

	public class TerratweaksConfig_Client : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		public static TerratweaksConfig_Client Instance;

		[DefaultValue(true)]
		public bool NoDamageVariance { get; set; }

		[DefaultValue(true)]
		public bool NoRandomCrit { get; set; }

		[DefaultValue(true)]
		public bool StatsInTip { get; set; }
	}

	public class VanillaChanges
	{
		public ArmorReworks armorBonuses = new();

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
