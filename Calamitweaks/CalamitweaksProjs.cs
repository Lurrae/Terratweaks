using CalamityMod;
using CalamityMod.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksProjs : GlobalProjectile
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override bool InstancePerEntity => true;
		
		public override void SetDefaults(Projectile proj)
		{
			if (Terratweaks.Calamitweaks.NoDefenseDamage)
			{
				CalamityGlobalProjectile calProj = proj.Calamity();
				calProj.DealsDefenseDamage = false;
			}
		}
	}
}