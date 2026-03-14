using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Projectiles
{
	public class FrostArrow : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.FrostburnArrow);
		}

		public override void AI()
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Alpha: 100);
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
			
			for (int i = 0; i < 20; i++)
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, Alpha: 100);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextBool(3))
				target.AddBuff(BuffID.Chilled, Conversions.ToFrames(3));
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			if (Main.rand.NextBool(3))
				target.AddBuff(BuffID.Chilled, Conversions.ToFrames(3));
		}
	}
}