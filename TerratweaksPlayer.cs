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
		public Item buffedWormScarf;
		public Item buffedBrainOfConfusion;
		public int buffedBoneGloveCounter = 0;
		public bool radiantInsignia; // Literally only needed because Calamity is dumb
		public bool summonsDisabled;
		public bool werebeaver;
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
			buffedWormScarf = null;
			buffedBrainOfConfusion = null;
			radiantInsignia = false;
			summonsDisabled = false;
		}

		public override void OnEnterWorld()
		{
			adamCD = 120; // Make the hearts start off on cooldown
			werenessMeter = 0; // Make the player have 0 wereness by default, this builds up by hitting enemies

			// Not sure if this is a bug in vanilla or something I messed up, but eocHit starts at 0 instead of -1,
			// making the game think the player hit the first enemy loaded into the game before they dash
			// Because the buffed SoC config option relies on that variable being -1 to not erroneously burn NPCs,
			// I have to fix it with that config option active
			if (Terratweaks.Config.EyeShield)
			{
				Player.eocHit = -1;
			}
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
			// This duration can be extended by hitting enemies with Lucy while transformed, but that's handled elsewhere
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
			if (Terratweaks.Config.NoExpertFreezingWater)
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
			if (Terratweaks.Config.RoyalGel && NPC.downedQueenSlime)
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
			if (Terratweaks.Config.HivePack && NPC.downedPlantBoss)
			{
				if (Player.strongBees)
					buffedHivePack = true;
			}

			// Radiant Insignia - Infinite wing/rocket boot flight time and increased acceleration
			// These effects are identical to those of Soaring Insignia
			if (radiantInsignia)
			{
				bool noInfiniteFlight = false;

				// If Calamity is enabled, we need to avoid providing infinite flight with any debuffs that are meant to strip infinite flight:
				// Weak Petrification, Icarus' Folly, Extreme Gravity
				if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
				{
					if (calamity.TryFind("WeakPetrification", out ModBuff weakPetri) &&
						calamity.TryFind("IcarusFolly", out ModBuff icarus) &&
						calamity.TryFind("DoGExtremeGravity", out ModBuff exGravity))
					{
						// Don't apply infinite flight if the player has any of the anti-inf.flight debuffs
						if (Player.HasBuff(weakPetri.Type) || Player.HasBuff(icarus.Type) || Player.HasBuff(exGravity.Type))
						{
							noInfiniteFlight = true;
						}
						// If we do provide infinite flight, make sure Calamity knows that
						else
						{
							calamity.Call("ToggleInfiniteFlight", Player, true);
						}
					}
				}

				// If we can provide infinite flight, do so
				if (!noInfiniteFlight)
				{
					if (Player.wingTime != 0)
						Player.wingTime = Player.wingTimeMax;

					Player.rocketTime = Player.rocketTimeMax;
					Player.runAcceleration *= 1.75f;
				}
				// Otherwise, just increase flight time in the same way as base Soaring Insignia
				else
				{
					Player.empressBrooch = true;
				}
			}

			// Despawn all probes owned by this player if the accessory is unequipped or the config option is reverted
			// Unsetting the config should set buffedWormScarf to null automatically, but doesn't hurt to be careful
			if ((!IsBuffedWormScarfEquipped() && !IsBuffedBrainEquipped()) || !Terratweaks.Config.WormBrain)
			{
				foreach (Projectile proj in Main.projectile.Where(p => p.owner == Player.whoAmI && p.type == PROBE_ID))
				{
					proj.Kill();
				}
			}
			else
			{
				// Kill all the probes once the Cerebral Mindtrick buff wears off, unless a buffed Worm Scarf is also equipped
				if (IsBuffedBrainEquipped() && !IsBuffedWormScarfEquipped() && !Player.HasBuff(BuffID.BrainOfConfusionBuff))
				{
					foreach (Projectile proj in Main.projectile.Where(p => p.owner == Player.whoAmI && p.type == PROBE_ID))
					{
						proj.Kill();
					}
				}

				// Spawn or despawn probes to hit the intended cap based on max HP
				if (IsBuffedWormScarfEquipped())
				{
					//Main.NewText($"Player has {Player.statLife} / {Player.statLifeMax2} ({Player.statLife / Player.statLifeMax2}%) HP remaining");
					float lifePercent = Player.statLife / (float)Player.statLifeMax2;
					int probeCount = lifePercent <= 0.33 ? 3 : lifePercent <= 0.66 ? 2 : 1;

					// Despawn probes if we're above the intended limit, unless we have Cerebral Mindtrick active
					// The Mindtrick check is needed so that if both accessories are equipped, the Brain can spawn probes above the Scarf's limit
					if (!Player.HasBuff(BuffID.BrainOfConfusionBuff) && Player.ownedProjectileCounts[PROBE_ID] > probeCount)
					{
						// Kill the oldest probes until we are back under the limit
						int activeProbes = Player.ownedProjectileCounts[PROBE_ID];
						while (activeProbes > probeCount)
						{
							int idx = FindOldestProbe(PROBE_ID);

							// Break out early if we kill every probe or can't find a probe
							if (activeProbes < 1 || idx == Main.maxProjectiles)
							{
								break;
							}

							Main.projectile[idx].Kill();
							activeProbes--;
						}
					}

					int numActiveScarfProbes = Player.ownedProjectileCounts[PROBE_ID];

					// Ignore three active probes if the Cerebral Mindtrick buff is active, since it spawns three extras that shouldn't
					// count towards the scarf's limit
					if (Player.HasBuff(BuffID.BrainOfConfusionBuff))
						numActiveScarfProbes -= 3;

					// Spawn extra probes up to the calculated limit
					if (IsBuffedWormScarfEquipped() && numActiveScarfProbes < probeCount)
					{
						for (int i = 0; i < probeCount - numActiveScarfProbes; i++)
						{
							Vector2 velocity = Main.rand.NextVector2Unit(); // Returns a vector between (-1, -1) and (1, 1)
							Projectile.NewProjectile(Player.GetSource_Accessory(buffedWormScarf), Player.Center, velocity, PROBE_ID, 20, 0, Player.whoAmI);
						}
					}
				}
			}
		}

		public static readonly int PROBE_ID = ProjectileType<Probe>();

		public bool IsBuffedWormScarfEquipped()
		{
			return buffedWormScarf != null && !buffedWormScarf.IsAir;
		}

		public bool IsBuffedBrainEquipped()
		{
			return buffedBrainOfConfusion != null && !buffedBrainOfConfusion.IsAir;
		}

		private int FindOldestProbe(int probeType)
		{
			int resultIdx = Main.maxProjectiles;
			int shortestTimeLeft = 9999999;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.owner == Player.whoAmI && proj.type == probeType && proj.timeLeft < shortestTimeLeft)
				{
					resultIdx = i;
					shortestTimeLeft = Main.projectile[i].timeLeft;
				}
			}

			return resultIdx;
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
			// TODO: It might be nice to have a custom sprite for this for convenience?
			if (Terratweaks.Config.DeerWeaponsRework && werenessMeter > 0)
			{
				Main.instance.DrawHealthBar(Player.Center.X, Player.position.Y - 16, werenessMeter, MAX_WERENESS, 1);
			}

			// Spawn Cursed Inferno dust while dashing with the buffed Shield of Cthulhu
			if (Terratweaks.Config.EyeShield && NPC.downedMechBoss2 && Player.eocDash > 0)
			{
				Dust dust = Main.dust[Dust.NewDust(Player.position, Player.width, Player.height, DustID.CursedTorch, Scale: Main.rand.Next(3, 6))];
				dust.noGravity = true;
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

			if (item.type == ItemID.LucyTheAxe && Terratweaks.Config.DeerWeaponsRework)
			{
				// Ignore target dummies and any enemies that don't drop coins
				if (target.value > 0 && !target.immortal)
				{
					if (!werebeaver)
						werenessMeter += 25;
					else
						werenessMeter += 10;

					if (werenessMeter >= MAX_WERENESS && !werebeaver)
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
		public bool inGravGlobeRange;

		public override void ResetEffects()
		{
			umbrellaHat = false;
			umbrellaHatVanity = false;
		}

		public override void PostUpdateEquips()
		{
			if (inGravGlobeRange)
			{
				Player.gravControl = false;
				Player.gravControl2 = false;
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (Terratweaks.InfernoToggleKeybind.JustPressed)
			{
				showInferno = !showInferno;
				
				if (Main.netMode == NetmodeID.MultiplayerClient) // Sync changed value in multiplayer
				{
					ModPacket packet = Mod.GetPacket();
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

			if (Terratweaks.AutoFishingKeybind.JustPressed)
			{
				FishingPlayer fPlr = Player.GetModPlayer<FishingPlayer>();
				if (fPlr.CanAutoFish())
				{
					fPlr.isAutoFishing = !fPlr.isAutoFishing;

					CombatText.NewText(Player.getRect(), Color.White, Language.GetTextValue(fPlr.isAutoFishing ? "Mods.Terratweaks.Common.AutoFish_On" : "Mods.Terratweaks.Common.AutoFish_Off"));
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
			if (Player.controlInv && Terratweaks.Config.ChesterRework && Player.chest == -3 && Terratweaks.playerHasChesterSafeOpened)
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
			// Despawn all active sentries when the player is slain, if they have the corresponding config setting enabled
			if (Terratweaks.Config.KillSentries)
			{
				foreach (Projectile proj in Main.projectile.Where(p => p.active && p.sentry && p.owner == Player.whoAmI))
				{
					proj.Kill();
				}
			}

			// Call off invasions if the player dies too many times to the same one
			if (Terratweaks.Config.PlayerDeathsToCallOffInvasion > 0 && Main.invasionType != InvasionID.None)
			{
				deathsThisInvasion++;

				if (deathsThisInvasion >= Terratweaks.Config.PlayerDeathsToCallOffInvasion)
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
		public static readonly List<int> ValidAccessoryTypes = new();

		public bool isAutoFishing = false;
		public bool hasAutoFishAcc = false;

		public bool HasAnyBait()
		{
			int numBaitStacks = 0;

			foreach (Item item in Player.inventory)
			{
				if (item.bait > 0)
					numBaitStacks++;
			}

			return numBaitStacks > 0;
		}

		public override void ResetEffects()
		{
			hasAutoFishAcc = false;
		}

		public bool CanAutoFish()
		{
			// You can't auto-fish if you're dead
			if (Player.dead || Player.ghost)
				return false;

			// Otherwise, as long as the config is enabled, the proper accessory is equipped (if one is set), and the player has bait, they can auto-fish
			return Terratweaks.Config.AutoFishing && (hasAutoFishAcc || ValidAccessoryTypes.Count == 0) && HasAnyBait();
		}

		public override void PostUpdate()
		{
			// If we don't have the ability to auto-fish for any reason, stop auto-fishing and display a message
			if (!CanAutoFish() && isAutoFishing)
			{
				isAutoFishing = false;
				CombatText.NewText(Player.getRect(), Color.White, Language.GetTextValue("Mods.Terratweaks.Common.AutoFish_Off"));
			}

			// All of the code below is for auto-fishing, so if we aren't auto-fishing, don't run any of it
			// This also means that if we can't auto-fish, nothing below this point will run
			if (!isAutoFishing)
				return;

			// Don't do anything if the player is not holding a fishing pole
			if (Player.HeldItem.fishingPole <= 0)
				return;

			// This needs to be done to handle the custom bobber accessories
			int bobberID = Player.overrideFishingBobber > 0 ? Player.overrideFishingBobber : Player.HeldItem.shoot;

			// While auto-fishing, if we don't have a bobber out and aren't using our fishing rod already, use it
			if (Player.ownedProjectileCounts[bobberID] == 0 && Player.itemAnimation <= 0)
			{
				Player.controlUseItem = true;
				Player.ItemCheck();
				Player.SetItemAnimation(Player.HeldItem.useAnimation);
				Player.SetItemTime(Player.HeldItem.useTime);
				Player.controlUseItem = false;
			}

			// If we *do* have a bobber, we need to figure out whether or not the bobber has caught something, and if it has, reel it in!
			// Luckily, bobbers use their ai[1] field to detect whether or not they have a catch, so we can just check if that's not 0
			// on any bobber, and then retract them all if one has an item
			int numBobbers = Player.ownedProjectileCounts[bobberID];
			int numBobbersWithItems = 0;

			foreach (Projectile proj in Main.ActiveProjectiles)
			{
				// Ignore non-bobber projectiles, or projectiles not owned by this player
				if (proj.owner != Player.whoAmI || !proj.bobber)
					continue;

				// If the bobber has an item, we've got a fish and need to reel all the bobbers in!
				if (proj.ai[1] < 0)
				{
					numBobbersWithItems++;
				}
			}

			// If at least a configurable percentage of the player's bobbers have an item, reel them all in
			if (numBobbersWithItems >= numBobbers * (Terratweaks.Config.AutoFishingBobberCount / 100.0f))
			{
				Player.controlUseItem = true;
				Player.ItemCheck();
				Player.controlUseItem = false;
			}
		}

		public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
		{
			if (Terratweaks.Config.ReaverSharkTweaks)
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

	// Tracks what minions the player has summoned while they're alive, and then resummons those minions on death
	public class MinionPlayer : ModPlayer
	{
		// Tracks how many minions of a particular type are summoned
		public readonly Dictionary<Item, int> SummonedMinions = new();

		// Immediately upon respawning, summon every minion the player had before they died
		public override void OnRespawn()
		{
			// Don't do this if the config option is disabled or the player had no summoned minions
			if (!Terratweaks.Config.ResummonMinions || SummonedMinions.Count <= 0)
				return;

			// Force the player's accessory effects to apply, so that all of their extra minion slots can be used
			Player.UpdateEquips(Player.whoAmI);

			// Create a copy of the original list, then clear the original list
			// This way, the old list can be updated to match what minions are successfully resummoned
			KeyValuePair<Item, int>[] oldSummonedMinions = (KeyValuePair<Item, int>[])SummonedMinions.ToArray().Clone();
			SummonedMinions.Clear();

			foreach (KeyValuePair<Item, int> pair in oldSummonedMinions)
			{
				Item item = pair.Key;
				int amount = pair.Value;
				
				// The code for summoning minions was adapted from the Lan's Auto Summon mod;
				// LansAutoSummon.cs, lines 278-345
				for (int i = 0; i < amount; i++)
				{
					// This is needed so that items in the player's inventory won't get overwritten for no reason
					Item oldItem = Player.inventory[Player.selectedItem];

					// Since we don't know what values are going to be set by simulating item use,
					// we need to track basically EVERYTHING that could feasibly change so we can restore it all later
					var oldControlUseItem = Player.controlUseItem;
					var oldReleaseUseItem = Player.releaseUseItem;
					var oldItemAnimation = Player.itemAnimation;
					var oldItemTime = Player.itemTime;
					var oldItemAnimationMax = Player.itemAnimationMax;
					var oldItemLocation = Player.itemLocation;
					var oldItemRotation = Player.itemRotation;
					var oldDirection = Player.direction;
					var oldToolTime= Player.toolTime;
					var oldChannel = Player.channel;
					var oldAttackCooldown = Player.attackCD;

					// By changing the item in the selected slot of the player's inventory, we can basically force them to use whatever item we want
					// In this case, a minion-summoning item!
					Player.inventory[Player.selectedItem] = item;

					// Simulate the player using this item
					Player.controlUseItem = true;
					Player.itemAnimation = 0;
					Player.ItemCheck();
					Player.controlUseItem = false;

					while (Player.itemAnimation > 0)
						Player.ItemCheck();

					// Restore the item in the selected slot
					Player.inventory[Player.selectedItem] = oldItem;

					// Restore all the other variables
					Player.controlUseItem = oldControlUseItem;
					Player.releaseUseItem = oldReleaseUseItem;
					Player.itemAnimation = oldItemAnimation;
					Player.itemTime = oldItemTime;
					Player.itemAnimationMax = oldItemAnimationMax;
					Player.itemLocation = oldItemLocation;
					Player.itemRotation = oldItemRotation;
					Player.direction = oldDirection;
					Player.toolTime = oldToolTime;
					Player.channel = oldChannel;
					Player.attackCD = oldAttackCooldown;

					// Check the player's current minion count- if we've reached or exceeded the minion cap, stop summoning minions
					// This is to ensure that the last minion summoned will not be re-summoned if the player had a Summoning Potion, rather than the first
					if (Player.numMinions >= Player.maxMinions)
					{
						return;
					}
				}
			}
		}
	}
}