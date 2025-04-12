using FargowiltasSouls.Content.Projectiles.Masomode;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Terratweaks.FargoTweaks
{
	[JITWhenModsEnabled("FargowiltasSouls")]
	public class EternitweaksProjs : GlobalProjectile
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("FargowiltasSouls");

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (Terratweaks.Eternitweaks.EmodeLightningBuff)
			{
				// Lightning normally deals 60 damage per hit, which is doubled to 120 in Hardmode
				// Let's change that :)
				if (projectile.type == ModContent.GetInstance<RainLightning>().Type)
				{
					// This base damage is the same as that of a boulder
					// Normally, emode makes lightning deal 2x damage in Hardmode as mentioned above,
					// but with how much damage it does already I don't think that's necessary
					int newDamage = Main.masterMode ? 420 : Main.expertMode ? 280 : 140;

					// I'm not sure why, but lightning damage is divided by 4 after all other calculations
					// Despite this, it still actually does the expected ~60 damage normally,
					// so I'm doing the same here since it seems important somehow
					// I don't want lightning dealing upwards of a thousand damage, after all XD
					newDamage /= 4;

					// Finally, set the damage so that lightning actually does the amount of damage we want
					projectile.damage = newDamage;
				}
			}
		}
	}
}