using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
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
		public bool summonsDisabled;
		public bool werebeaver;
		public bool werebeaverLastFrame;
		public int werenessMeter = 0;

		public static readonly int MAX_WERENESS = 500;

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
			summonsDisabled = false;
		}

		public override void OnEnterWorld()
		{
			adamCD = 120; // Make the hearts start off on cooldown
			werenessMeter = 0; // Make the player have 0 wereness by default, this builds up by hitting enemies
		}

		private void SpawnTransformationSmoke()
		{
			// Only spawn dusts in singleplayer or on clients
			if (Main.netMode == NetmodeID.Server)
				return;

			// This code was adapted from the Minecart mounting/dismounting code
			// Vanilla Mount.cs, lines 5695-5754
			for (int i = 0; i < 100; i++)
			{
				if (i % 10 == 0)
				{
					int type = Main.rand.Next(61, 64);
					Gore gore = Gore.NewGoreDirect(Player.GetSource_FromThis(), new Vector2(Player.position.X, Player.position.Y), Vector2.Zero, type);
					gore.alpha = 100;
					gore.velocity = Vector2.Transform(new Vector2(1f, 0f), Matrix.CreateRotationZ((float)(Main.rand.NextDouble() * MathHelper.TwoPi)));
				}
			}
		}

		public static readonly List<int> HotDebuffs = new()
		{
			BuffID.OnFire,
			BuffID.CursedInferno,
			BuffID.ShadowFlame,
			BuffID.Daybreak,
			BuffID.OnFire3
		};

		public static readonly List<int> ColdDebuffs = new()
		{
			BuffID.Frostburn,
			BuffID.Frozen,
			BuffID.Frostburn2
		};

		private void AdjustRemainingHotAndColdDebuffTimes()
		{
			// Only run on the client of this player
			if (Player.whoAmI != Main.myPlayer)
				return;
			
			float insulation = 0.75f; // Werebeaver reduces duration of cold debuffs by 25%

			for (int i = 0; i < Player.buffType.Length; i++)
			{
				int buffType = Player.buffType[i];

				if (buffType == 0)
					continue;

				if (Main.debuff[buffType] && (HotDebuffs.Contains(buffType) || ColdDebuffs.Contains(buffType)))
				{
					if (werebeaver)
						Player.buffTime[i] = (int)(Player.buffTime[i] * insulation);
					else
						Player.buffTime[i] = (int)(Player.buffTime[i] / insulation);
				}
			}
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

			// Wereness meter decays at a rate of 1 unit per tick while in werebeaver form or not in combat
			// This means the werebeaver form lasts about 8 seconds (500 / 60 = 8.333...)
			if (werebeaver || !Player.GetModPlayer<CombatPlayer>().IsInCombat())
			{
				werenessMeter--;

				if (werenessMeter <= 0 && werebeaver)
				{
					werebeaver = false;
					werenessMeter = 0;
					SpawnTransformationSmoke();
				}
			}

			// Make sure the wereness meter always starts at 0
			if (werenessMeter < 0)
				werenessMeter = 0;

			// Provide immunity to chilling water in Expert mode
			if (GetInstance<TerratweaksConfig>().NoExpertFreezingWater)
				Player.arcticDivingGear = true;

			// Huntress' Buckler and Monk's Belt effects
			if (dd2Accessory2)
			{
				Player.maxTurrets++;
				Player.GetDamage(DamageClass.Summon) += 0.1f;
			}

			// Werebeaver effects- because this runs after the previous werebeaver check, the player won't have these stats the frame their meter runs out
			if (werebeaver)
			{
				Player.GetDamage(DamageClass.Melee) += 0.15f; // +15% melee damage
				Player.endurance += 0.25f; // +25% damage resistance
				Player.moveSpeed += 0.1f; // +10% movement speed
				Player.nightVision = true; // Night vision
			}

			if (werebeaver != werebeaverLastFrame)
			{
				AdjustRemainingHotAndColdDebuffTimes();
			}
			
			werebeaverLastFrame = werebeaver;

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
			// Do nothing if drawing an afterimage of the player
			if (drawInfo.shadow != 0f)
				return;

			// Draw spider web around the player
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

			// Draw a healthbar above the player's head to indicate their wereness meter while the meter is above 0
			if (GetInstance<TerratweaksConfig>().DeerWeaponsRework && werenessMeter > 0)
			{
				Main.instance.DrawHealthBar(Player.Center.X, Player.position.Y - 16, werenessMeter, MAX_WERENESS, 1);
			}
		}

		public override void FrameEffects()
		{
			if (werebeaver)
			{
				Player.head = ArmorIDs.Head.Werewolf;
				Player.body = ArmorIDs.Body.Werewolf;
				Player.legs = 20;
			}
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			TriggerOnHitBonus(target, hit);

			if (item.type == ItemID.LucyTheAxe && GetInstance<TerratweaksConfig>().DeerWeaponsRework && !werebeaver)
			{
				// Ignore target dummies and any enemies that don't drop coins
				if (target.value > 0 && !target.immortal)
				{
					werenessMeter += 25;

					if (werenessMeter >= MAX_WERENESS)
					{
						werenessMeter = MAX_WERENESS;
						werebeaver = true;
						SpawnTransformationSmoke();
					}
				}
			}
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

			if (Player.controlInv && chesterRework && Player.chest == -3 && Terratweaks.playerHasChesterSafeOpened)
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

	// Tracks whether or not the player is considered "in combat"
	// This system is based on the way Thorium does it, though I didn't reference their code for this
	public class CombatPlayer : ModPlayer
	{
		public int combatTimer = 0;

		public bool IsInCombat() => combatTimer > 0;

		public override void PostUpdateEquips()
		{
			if (IsInCombat())
				combatTimer--;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			combatTimer = Conversions.ToFrames(5);
		}

		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
		{
			combatTimer = Conversions.ToFrames(5);
		}
	}

	// Effects that happen when the player dies
	public class DeathEffectPlayer : ModPlayer
	{
		public int deathsThisInvasion = 0;

		public override void PostUpdate()
		{
			// Reset death counter if an invasion is not active
			if (Main.invasionType == InvasionID.None)
				deathsThisInvasion = 0;
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			// Despawn all active sentries when the player is slain, if they have the corresponding config setting enabled
			if (config.KillSentries)
			{
				foreach (Projectile proj in Main.projectile.Where(p => p.active && p.sentry && p.owner == Player.whoAmI))
				{
					proj.Kill();
				}
			}

			// Call off invasions if the player dies too many times to the same one
			if (config.PlayerDeathsToCallOffInvasion > 0 && Main.invasionType != InvasionID.None)
			{
				deathsThisInvasion++;

				if (deathsThisInvasion >= config.PlayerDeathsToCallOffInvasion)
				{
					Main.invasionType = InvasionID.None;
					Color color = new(175, 75, 255);
					Main.NewText(Language.GetTextValue("Mods.Terratweaks.Common.InvasionFailPlayer"), color);
				}
			}
		}
	}

	// Handles changes to fishing loot tables and fishing quest rewards
	public class FishingPlayer : ModPlayer
	{
		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

            if (config.ReaverSharkTweaks)
            {
				// Prevent catching Reaver Sharks if an evil boss or Skeletron hasn't been killed yet
				// This is just done by replacing any Reaver Sharks caught with a Sawtooth Shark
				// This may not be the best approach, but at least it works!
				if (itemDrop == ItemID.ReaverShark && !NPC.downedBoss2 && !NPC.downedBoss3)
				{
					itemDrop = ItemID.SawtoothShark;
				}
            }
        }
	}
}