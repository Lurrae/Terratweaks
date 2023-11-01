using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Security.Policy;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
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

		// Other custom booleans
		public bool dd2Accessory2;
		public bool buffedHivePack;
		public bool radiantInsignia; // Literally only needed because Calamity is dumb
		
		public override void ResetEffects()
		{
			spiderWeb = false;
			cobaltDefense = false;
			mythrilFire = false;
			adamHearts = false;
			spookyShots = false;

			dd2Accessory2 = false;
			buffedHivePack = false;
			radiantInsignia = false;
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

			// Provide immunity to chilling water in Expert mode
			if (GetInstance<TerratweaksConfig>().NoExpertFreezingWater)
				Player.arcticDivingGear = true;

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

			// Royal Gel buff - If Queen Slime is dead, it protects against Spiked Slimes and Queen Slime's three minions
			var expertAccBuffs = GetInstance<TerratweaksConfig>().expertAccBuffs;

			if (expertAccBuffs.RoyalGel && NPC.downedQueenSlime)
			{
				if (Player.npcTypeNoAggro[NPCID.BlueSlime]) // Player has an accessory that gives Royal Gel's effects
				{
					Player.npcTypeNoAggro[NPCID.SlimeSpiked] = true;
					Player.npcTypeNoAggro[NPCID.QueenSlimeMinionBlue] = true;
					Player.npcTypeNoAggro[NPCID.QueenSlimeMinionPink] = true;
					Player.npcTypeNoAggro[NPCID.QueenSlimeMinionPurple] = true;
				}
			}

			// Hive Pack buff - If Plantera is dead, buff damage of all bee/wasp weapons and sometimes spawn a swarm of large bees
			if (expertAccBuffs.HivePack && NPC.downedPlantBoss)
			{
				if (Player.strongBees)
					buffedHivePack = true;
			}

			// Radiant Insignia - Infinite wing/rocket boot flight time and increased acceleration
			// These effects are identical to those of Soaring Insignia
			if (radiantInsignia)
			{
				if (Player.wingTime != 0)
					Player.wingTime = Player.wingTimeMax;

				Player.rocketTime = Player.rocketTimeMax;
				Player.runAcceleration *= 1.75f;
				
				// Tell Calamity we have infinite flight time if that mod's enabled
				if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
				{
					calamity.Call("ToggleInfiniteFlight", Player, true);
				}
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

			// Buffed Hive Pack makes bee weapons inflict Acid Venom for a random duration between 0.5-1.5 seconds
			if (buffedHivePack && proj.IsBeeRelated())
			{
				int duration = 30 * Main.rand.Next(new IntRange(1, 3));
				target.AddBuff(BuffID.Venom, duration);
			}
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
	// Can also be used to handle custom keybinds
	public class InputPlayer : ModPlayer
	{
		public bool showInferno = true;
		public bool umbrellaHat;
		public bool umbrellaHatVanity;

		public override void ResetEffects()
		{
			umbrellaHat = false;
			umbrellaHatVanity = false;
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (Terratweaks.InfernoToggleKeybind.JustPressed)
			{
				showInferno = !showInferno;
				
				if (Main.netMode == NetmodeID.MultiplayerClient) // Sync changed value in multiplayer
				{
					ModPacket packet = GetInstance<Terratweaks>().GetPacket();
					packet.Write((byte)PacketType.SyncInferno); // Packet ID - What type of message is this?
					packet.Write(showInferno); // Packet message - Does this player have the inferno ring visible or not?
					packet.Send();
				}
			}

			if (Terratweaks.RulerToggleKeybind.JustPressed)
			{
				switch (Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine])
				{
					case 0:
						Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine] = 1;
						break;
					case 1:
						Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerLine] = 0;
						break;
				}
			}

			if (Terratweaks.MechRulerToggleKeybind.JustPressed)
			{
				switch (Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid])
				{
					case 0:
						Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid] = 1;
						break;
					case 1:
						Player.builderAccStatus[Player.BuilderAccToggleIDs.RulerGrid] = 0;
						break;
				}
			}
		}

		public override void FrameEffects()
		{
			// Draw Umbrella Hat on the player's head if it's equipped as an accessory
			if (umbrellaHatVanity)
			{
				Player.head = ArmorIDs.Head.UmbrellaHat;
			}
		}

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

			// Player is falling while holding jump
			// If they have the Umbrella Hat equipped and aren't wearing wings, slow their descent
			if (Player.controlJump && Player.velocity.Y > 0 && Player.wingsLogic == 0 && umbrellaHat)
			{
				Player.velocity.Y += Player.gravity / 3 * Player.gravDir;

				if (Player.gravDir == 1)
				{
					if (Player.velocity.Y > Player.maxFallSpeed / 3 && !Player.TryingToHoverDown)
						Player.velocity.Y = Player.maxFallSpeed / 3;
				}
				else if (Player.velocity.Y < (0 - Player.maxFallSpeed) / 3 && !Player.TryingToHoverUp)
					Player.velocity.Y = (0 - Player.maxFallSpeed) / 3;
			}
		}
	}
}