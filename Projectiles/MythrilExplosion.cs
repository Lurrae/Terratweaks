using Terraria.ModLoader;

namespace Terratweaks.Projectiles
{
	public class MythrilExplosion : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.width = 75;
			Projectile.height = 75;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 10;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.friendly = true;
			Projectile.alpha = 255;
		}
	}
}