using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Projectiles
{
	public class Probe : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projPet[Type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Type] = true; // Allows the probes to target tagged enemies
			ProjectileID.Sets.CultistIsResistantTo[Type] = true; // Cultist resists all "homing" projectiles, including all minions and similar projectiles
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Retanimini);
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.minionSlots = 0;
			AIType = ProjectileID.Retanimini;
		}

		// Prevent the probe dying to hitting a tile
		public override bool OnTileCollide(Vector2 oldVelocity) => false;

		// Probes shouldn't destroy grass and the like
		public override bool? CanCutTiles() => false;

		// Give the probes a slight "nudge" when overlapping other Probes, so that they spread out a bit more
		public override void AI()
		{
			float nudge = 0.04f; // The velocity at which probes nudge away from each other

			foreach (var other in Main.ActiveProjectiles)
			{
				// Within a half-width (12 px) of another probe owned by the same player
				if (other.whoAmI != Projectile.whoAmI && other.owner == Projectile.owner && other.type == Type &&
					Projectile.Distance(other.Center) < (Projectile.width / 2))
				{
					// Nudge left/right based on where we are in relation to the other probe
					if (Projectile.position.X < other.position.X)
					{
						Projectile.velocity.X -= nudge;
					}
					else
					{
						Projectile.velocity.X += nudge;
					}

					// Nudge up/down based on where we are in relation to the other probe
					if (Projectile.position.Y < other.position.Y)
					{
						Projectile.velocity.Y -= nudge;
					}
					else
					{
						Projectile.velocity.Y += nudge;
					}
				}
			}
		}
	}
}