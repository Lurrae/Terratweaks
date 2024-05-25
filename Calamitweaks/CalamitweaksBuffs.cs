using Terraria.ModLoader;
using Terraria.ID;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksBuffs : GlobalBuff
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.DRBuffs)
			{
				if (type == BuffID.IceBarrier)
					tip = tip.Replace("15%", "25%");
			}
		}
	}
}