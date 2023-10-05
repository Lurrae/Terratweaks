using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terratweaks.Buffs;
using Terratweaks.NPCs;
using Terratweaks.Projectiles;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks
{
	// Handles custom armor set bonuses
	public class TerratweaksPlayer : ModPlayer
	{
		// Custom vanilla armor set bonuses
		public bool spiderWeb;
		public bool cobaltDefense;
		public bool mythrilFire;
		public int mythrilCD;
		public bool adamHearts;
		public float adamHeartSpin;
		public int adamCD;
		public bool spookyShots;
		public int spookyCD;

		// Custom boolean for the Ogre-dropped DD2 accessories
		public bool dd2Accessory2;

		public override void ResetEffects()
		{
			spiderWeb = false;
			cobaltDefense = false;
			mythrilFire = false;
			adamHearts = false;
			spookyShots = false;

			dd2Accessory2 = false;
		}

		public override void OnEnterWorld()
		{
			adamCD = 120; // Make the hearts start off on cooldown
		}

		public override void PostUpdateEquips()
		{
			// Cooldown ticking
			if (spookyCD > 0)
				spookyCD--;
			if (mythrilCD > 0)
				mythrilCD--;
			if (adamCD > 0)
				adamCD--;
			
			// Huntress' Buckler and Monk's Belt effects
			if (dd2Accessory2)
			{
				Player.maxTurrets++;
				Player.GetDamage(DamageClass.Summon) += 0.1f;
			}

			// Spooky armor's new set bonus
			if (spookyShots)
			{
				// Find an enemy to shoot; this will always target the NPC with the lowest index
				NPC target = null;

				for (int i = 0; i < Main.maxNPCs; i++)
				{
					if (Main.npc[i] != null && Main.npc[i].CanBeChasedBy())
					{
						target = Main.npc[i];
						break;
					}
				}

				// Spawn a projectile to shoot at enemies with the Spooky armor set bonus
				if (target != null && spookyCD == 0)
				{
					var source = Player.GetSource_Misc("SetBonus_Spooky");
					Projectile spookyNettle = Projectile.NewProjectileDirect(source, Player.position, Vector2.Zero, ProjectileID.FlamingWood, 40, 3f, Player.whoAmI);
					spookyNettle.DamageType = DamageClass.Summon;
					spookyNettle.hostile = false; // Make it not harm the player...
					spookyNettle.friendly = true; // ...and instead harm enemies!
					spookyNettle.tileCollide = true; // Don't allow it to go through blocks
					ProjectileID.Sets.SentryShot[spookyNettle.type] = true; // Make it count as a sentry projectile

					Vector2 dirToTarget = spookyNettle.DirectionTo(target.position);
					if (dirToTarget.HasNaNs())
						dirToTarget = Vector2.UnitX;

					spookyNettle.velocity = dirToTarget * 10f;
					SoundEngine.PlaySound(SoundID.Item42, Player.position);
					spookyCD = 30;
				}
			}

			// Adamantite Hearts
			int adamHeartCount = 0;
			foreach (NPC npc in Main.npc)
			{
				if (npc.type == NPCType<AdamantiteHeart>() && npc.ai[0] == Player.whoAmI)
				{
					adamHeartCount++;
				}
			}

			if (NPC.CountNPCS(NPCType<AdamantiteHeart>()) > 0)
				adamHeartSpin += 0.01f;

			if (adamHearts && adamCD == 0 && adamHeartCount < 5)
			{
				NPC.NewNPC(Player.GetSource_ReleaseEntity(), (int)Player.Center.X + 32, (int)Player.Center.Y, NPCType<AdamantiteHeart>(), ai0: Player.whoAmI);
				adamCD = 120; // Takes 2 seconds to respawn
			}
		}

		bool scalingUp;
		float scale;
		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
			if (spiderWeb)
			{
				Texture2D texture = (Texture2D)Request<Texture2D>($"Terraria/Images/Extra_32");
				Vector2 pos = Player.MountedCenter - Main.screenPosition - new Vector2((texture.Width * scale) / 2, (texture.Height * scale) / 2);
				Color color = Color.Multiply(drawInfo.colorArmorBody, 0.5f);

				Main.EntitySpriteDraw(texture, pos, null, color, 0f, Vector2.Zero, scale, drawInfo.playerEffect, 0);
				
				// Handle the pulsing effect
				if (scalingUp && scale < 1.25f)
					scale += 0.005f;
				else if (!scalingUp && scale > 1.15f)
					scale -= 0.005f;

				if (scale >= 1.25f || scale <= 1.15f)
					scalingUp = !scalingUp;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			TriggerOnHitBonus(target, hit);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			TriggerOnHitBonus(target, hit, proj);
		}

		public void TriggerOnHitBonus(NPC target, NPC.HitInfo hit, Projectile trigger = null)
		{
			int mythrilFireProj = ProjectileType<MythrilExplosion>();
			
			// Player gets a defense buff after hitting an enemy with Cobalt armor active
			if (cobaltDefense)
				Player.AddBuff(BuffType<CobaltBuff>(), 150);

			// Mythril's modified set bonus spawns a pillar of flames from the ground
			if (mythrilFire && mythrilCD == 0)
			{
				if (trigger != null && trigger.type == mythrilFireProj)
					return; // Prevents the Mythril set bonus from triggering off of itself

				var source = Player.GetSource_Misc("SetBonus_Mythril");
				Projectile mythrilFlame = Projectile.NewProjectileDirect(source, target.Center, Vector2.Zero, mythrilFireProj, 36, 0f, Player.whoAmI);

				for (int i = 0; i < 100; i++)
				{
					Dust dust = Dust.NewDustDirect(target.Center - new Vector2(10, 10), 20, 20, DustID.Firework_Green, Scale: Main.rand.NextFloat(1f, 2f));
					dust.velocity = Vector2.Normalize(mythrilFlame.Center - dust.position) * Main.rand.NextFloat(-3f, -1f);
					dust.noGravity = false;
				}
				mythrilCD = 30;
			}
		}

		// Despawn all active sentries when the player is slain, if they have the corresponding config setting enabled
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			if (GetInstance<TerratweaksConfig>().KillSentries)
			{
				foreach (Projectile proj in Main.projectile.Where(p => p.active && p.sentry && p.owner == Player.whoAmI))
				{
					proj.Kill();
				}
			}
		}
	}

	// This ModPlayer is used exclusively for checking if the player is in the Radiant Insignia cutscene, because APPARENTLY global items don't exist on dropped items for a frame or two
	public class CutscenePlayer : ModPlayer
	{
		public bool inCutscene = false;
		public int direction = 1;
	}

	// Handles any modded behaviors that stem from player inputs
	// Can also be used to handle custom keybinds, if we ever add any
	public class InputPlayer : ModPlayer
	{
		public override void PostUpdate()
		{
			bool chesterRework = GetInstance<TerratweaksConfig>().ChesterRework;

			if (Player.controlInv && chesterRework && Player.chest == -3)
			{
				Player.chest = -1;
				Main.PlayInteractiveProjectileOpenCloseSound(ProjectileID.ChesterPet, false);
				Player.piggyBankProjTracker.Clear();
				Recipe.FindRecipes();
			}
		}
	}
}