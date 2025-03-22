using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terratweaks.Items;

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
			if (Terratweaks.Config.BoneHelm && projectile.type == ProjectileID.InsanityShadowFriendly)
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

			if (Terratweaks.Config.DeerWeaponsRework && projectile.type == ProjectileID.WeatherPainShot)
			{
				projectile.idStaticNPCHitCooldown = 10; // Reduced from 25 to 10, so it can now hit 6 times per second instead of about 2 times per second
				projectile.penetrate = 45; // Hits about 3x more, allowing it to remain active for about as long as it did before
			}

			// Buff Bone Glove post-Skeletron Prime
			if (Terratweaks.Config.BoneGlove && projectile.type == ProjectileID.BoneGloveProj && NPC.downedMechBoss3)
			{
				// Increase damage, armor penetration, and pierce
				projectile.damage += 20; // 45 total damage
				projectile.ArmorPenetration += 20; // 45 total AP
				projectile.penetrate += 1; // 4 total pierce

				Player player = Main.player[projectile.owner];
				TerratweaksPlayer tPlr = player.GetModPlayer<TerratweaksPlayer>();

				// Increment the counter...
				tPlr.buffedBoneGloveCounter++;

				// ...and replace this crossbone with a laser if this is the fourth one!
				if (tPlr.buffedBoneGloveCounter >= 4)
				{
					tPlr.buffedBoneGloveCounter = 0;
					projectile.type = ProjectileID.MiniRetinaLaser;

					// Extra damage, but less armor pen.
					projectile.damage += 30; // 75 total damage
					projectile.ArmorPenetration = 20; // Only 20 armor penetration tho

					// Laser has infinite pierce cuz why not
					projectile.penetrate = -1;

					// More extraUpdates = laser moves faster
					projectile.extraUpdates = 3;

					// Also set the velocity to aim directly at the mouse
					projectile.velocity = projectile.DirectionTo(Main.MouseWorld) * 10f;
				}

				// Reduce the delay on the Bone Glove- vanilla value is 60 ticks (1 sec), this is half that
				player.boneGloveTimer = 30;
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
			if (Terratweaks.Config.DeerWeaponsRework && projectile.type == ProjectileID.HoundiusShootius)
			{
				if (projectile.ai[0] > 45)
					projectile.ai[0] = 45;
			}

			// Frost Hydra buff - Fires faster the longer it remains locked on to the same target
			if (Terratweaks.Config.FrostHydraBuff && projectile.type == ProjectileID.FrostHydra)
			{
				FindTarget_FrostHydra(projectile, out int targetIdx);

				// Found a target this frame, and had a target last frame
				if (targetIdx != -1 && projectile.ai[1] != -1)
				{
					// We have a different target, reset fire rate timer
					if (targetIdx != projectile.ai[1])
					{
						projectile.ai[2] = 0;
					}
					else // Same target, increment timer and increase fire rate if necessary
					{
						projectile.ai[2]++;

						int shotCooldown = 60;
						shotCooldown -= Math.Min((int)Math.Ceiling(projectile.ai[2] / 10), 55); // Every 10 ticks, reduce cooldown by a tick to a minimum of 5 ticks

						projectile.ai[0] = Math.Min(projectile.ai[0], shotCooldown); // Lower shooting cooldown so it can fire faster
					}
				}
				else // Reset fire rate if we lost our target
				{
					projectile.ai[2] = 0;
				}

				projectile.ai[1] = targetIdx;
			}
		}

		// Vanilla code for detecting a target, since only the Houndius Shootius stores the target index in an ai field unfortunately
		// Code was adapted from Projectile.cs, lines 25859-25900
		static void FindTarget_FrostHydra(Projectile proj, out int targetIdx)
		{
			bool foundTarget = false;
			float maxRange = 1000f;
			targetIdx = -1;
			NPC targetNPC = proj.OwnerMinionAttackTargetNPC;
			if (targetNPC != null && targetNPC.CanBeChasedBy(proj))
			{
				float num437 = targetNPC.position.X + (targetNPC.width / 2);
				float num438 = targetNPC.position.Y + (targetNPC.height / 2);
				float num439 = Math.Abs(proj.position.X + (proj.width / 2) - num437) + Math.Abs(proj.position.Y + (proj.height / 2) - num438);
				if (num439 < maxRange && Collision.CanHit(proj.position, proj.width, proj.height, targetNPC.position, targetNPC.width, targetNPC.height))
				{
					maxRange = num439;
					foundTarget = true;
					targetIdx = targetNPC.whoAmI;
				}
			}
			if (!foundTarget)
			{
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy(proj))
					{
						float dist = Math.Abs(proj.position.X + (proj.width / 2) - npc.Center.X) + Math.Abs(proj.position.Y + (proj.height / 2) - npc.Center.Y);
						if (dist < maxRange && Collision.CanHit(proj.position, proj.width, proj.height, npc.position, npc.width, npc.height))
						{
							maxRange = dist;
							targetIdx = npc.whoAmI;
						}
					}
				}
			}
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			// Disable damage variation
			if (Terratweaks.Config.NoDamageVariance == DamageVarianceSetting.Limited)
				modifiers.DamageVariationScale *= 0;

			// Disable random crits, and allow them to ignore defense if that config option is active
			// TODO: Verify this works as expected in multiplayer; checking a client-side option and then a server-side option may not be safe
			if (Terratweaks.ClientConfig.NoRandomCrit && sourceItem != null && sourceItem.TryGetGlobalItem(out TooltipChanges globalItem))
			{
				globalItem.hitsDone += projectile.CritChance;

				if (globalItem.hitsDone >= 100)
				{
					modifiers.SetCrit();
					globalItem.hitsDone = 0;

					if (Terratweaks.Config.CritsBypassDefense)
						modifiers.DefenseEffectiveness *= 0;
				}
				else
					modifiers.DisableCrit();
			}

			// Paper cuts from paper airplanes, cuz why not?
			if (Terratweaks.Config.PaperCuts && projectile.aiStyle == ProjAIStyleID.PaperPlane)
			{
				target.AddBuff(BuffID.Bleeding, Conversions.ToFrames(4)); // 4 seconds of Bleeding
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