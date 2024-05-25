using CalamityMod;
using CalamityMod.Projectiles;
using Terraria;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksProjs : GlobalProjectile
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override void SetDefaults(Projectile proj)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.NoDefenseDamage)
			{
				CalamityGlobalProjectile calProj = proj.Calamity();
				calProj.DealsDefenseDamage = false;
			}
		}
	}
}