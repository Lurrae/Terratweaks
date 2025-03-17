using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terratweaks.Items;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks.Projectiles
{
	public class TerratweaksGlobalProj : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public int HitAdamHeart = -1; // Tracks if the projectile has hit an Adamantite Heart
		public Item sourceItem = null;

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (source is EntitySource_ItemUse src1)
			{
				sourceItem = src1.Item;
			}
			else if (source is EntitySource_ItemUse_WithAmmo src2)
			{
				sourceItem = src2.Item;
			}

			// Increase Bone Helm damage, increase it further and increase armor pen and pierce in Hardmode
			if (GetInstance<TerratweaksConfig>().BoneHelm && projectile.type == ProjectileID.InsanityShadowFriendly)
			{
				projectile.damage += 6;

				if (Main.hardMode)
				{
					projectile.damage *= 2;
					projectile.ArmorPenetration += 10;
					projectile.penetrate *= 2;
					projectile.maxPenetrate *= 2;
				}
			}

			if (GetInstance<TerratweaksConfig>().DeerWeaponsRework && projectile.type == ProjectileID.WeatherPainShot)
			{
				projectile.idStaticNPCHitCooldown = 10; // Reduced from 25 to 10, so it can now hit 6 times per second instead of about 2 times per second
				projectile.penetrate = 45; // Hits about 3x more, allowing it to remain active for about as long as it did before
			}
		}

		public override void AI(Projectile projectile)
		{
			Player player = Main.player[projectile.owner];
			TerratweaksPlayer tPlr = player.GetModPlayer<TerratweaksPlayer>();

			// If a hostile projectile is near a player with modified Spider Armor equipped, slow it down
			if (tPlr.spiderWeb && projectile.hostile && projectile.Distance(Main.player[Main.myPlayer].Center) <= 64)
			{
				projectile.position -= projectile.velocity * 0.5f; // Effectively halves the speed of the projectile
			}

			// If the player has a buffed Hive Pack equipped, display some Acid Venom particles to clarify the effect is active!
			if (tPlr.buffedHivePack && (projectile.IsBeeRelated() || projectile.type == ProjectileID.Hornet || projectile.type == ProjectileID.BeeArrow))
			{
				if (Main.rand.NextBool(6)) // 1/6 chance to spawn dust
					Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Venom);
			}

			// Houndius Shootius buff - Now fires a projectile every 3/4 second instead of 1.5 seconds
			if (GetInstance<TerratweaksConfig>().DeerWeaponsRework && projectile.type == ProjectileID.HoundiusShootius)
			{
				if (projectile.ai[0] > 45)
					projectile.ai[0] = 45;
			}
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (GetInstance<TerratweaksConfig>().NoDamageVariance == DamageVarianceSetting.Limited)
				modifiers.DamageVariationScale *= 0;

			if (GetInstance<TerratweaksConfig_Client>().NoRandomCrit && sourceItem != null && sourceItem.TryGetGlobalItem(out TooltipChanges globalItem))
			{
				globalItem.hitsDone += projectile.CritChance;

				if (globalItem.hitsDone >= 100)
				{
					modifiers.SetCrit();
					globalItem.hitsDone = 0;

					if (GetInstance<TerratweaksConfig>().CritsBypassDefense)
						modifiers.DefenseEffectiveness *= 0;
				}
				else
					modifiers.DisableCrit();
			}
		}
	}

	public class CalamityTeslaVisibilitySupport : GlobalProjectile
	{
		public override bool PreDraw(Projectile projectile, ref Color lightColor)
		{
			// Hide Calamity's Tesla Potion aura
			if (ModLoader.TryGetMod("CalamityMod", out Mod calamity) && calamity.TryFind("TeslaAura", out ModProjectile teslaAura))
			{
				if (projectile.type == teslaAura.Type && !Main.player[projectile.owner].GetModPlayer<InputPlayer>().showInferno)
					return false;
			}

			return base.PreDraw(projectile, ref lightColor);
		}
	}
}