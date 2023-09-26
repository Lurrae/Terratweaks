using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terratweaks.Projectiles;

namespace Terratweaks.NPCs
{
	public class AdamantiteHeart : ModNPC
	{
		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 5;

			// Setting some basic things about this NPC
			NPCID.Sets.CantTakeLunchMoney[Type] = true; // Prevents this NPC from stealing the player's coins when they die
			NPCID.Sets.DontDoHardmodeScaling[Type] = true; // No Expert Hardmode scaling, because 1) it's dumb, and 2) this NPC doesn't need its health scaled by difficulty
			NPCID.Sets.NeedsExpertScaling[Type] = false; // Prevent Expert Mode scaling from occurring, because again, this NPC should have a set amount of HP regardless of mode

			// Prevents this NPC from counting towards the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new(0) { Hide = true };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
		}

		public override void SetDefaults()
		{
			// Basic stats (AI, damage, defense, hp, etc.)
			NPC.aiStyle = -1;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.lifeMax = 200;
			NPC.width = 22;
			NPC.height = 22;
			NPC.value = 0;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.Item25;
			NPC.alpha = 255;

			// NPC properties, like knockback/lava immunity and no tile collision/gravity
			NPC.knockBackResist = 0f; // 0f = fully immune, 1f = no resistance
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.lavaImmune = true;
			NPC.chaseable = false; // Prevents minions or homing weapons from targeting the hearts
		}

		private float TargetPlayer
		{
			get => NPC.ai[0];
			set => NPC.ai[0] = value;
		}

		public override void OnSpawn(IEntitySource source)
		{
			NPC.frame.Y = Main.rand.Next(new IntRange(0, Main.npcFrameCount[NPC.type] - 1)) * NPC.height;
		}

		public override void AI()
		{
			Player player = Main.player[(int)TargetPlayer];

			// Delete the NPC if the player dies or doesn't exist
			if (player.dead || !player.active || !player.GetModPlayer<TerratweaksPlayer>().adamHearts)
			{
				NPC.active = false;
				for (int i = 0; i < 10; i++)
				{
					Dust dust = Dust.NewDustDirect(NPC.Center - new Vector2(5, 5), 10, 10, DustID.Firework_Red);
					dust.velocity = Vector2.Normalize(NPC.Center - dust.position) * Main.rand.NextFloat(3f) * -1f;
					dust.noGravity = true;
				}
				SoundEngine.PlaySound(SoundID.Item25, NPC.position);

				return; // Don't run AI when dying
			}

			// The hearts spawn fully invisible, so this makes them quickly fade in
			if (NPC.alpha > 75)
				NPC.alpha -= 15;

			// Spin in a circle around the player
			// Hopefully this should work in mp too? Since it's based on the target player it should be okay to not have a netcode/local player check
			//if (player.whoAmI == Main.myPlayer)
			FindIndex(out int index, out int totalIndexes);
			Vector2 center = FindPos(player, index, totalIndexes);
			NPC.Center = center;
		}

		private void FindIndex(out int index, out int totalIndexes)
		{
			index = 0;
			totalIndexes = 0;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && npc.ai[0] == TargetPlayer && npc.type == NPC.type)
				{
					if (NPC.whoAmI > i)
					{
						index++;
					}
					totalIndexes++;
				}
			}
		}

		private static Vector2 FindPos(Player player, int index, int totalIndexes)
		{
			float off = MathHelper.ToRadians((360 / totalIndexes) * index);
			int radius = 64;
			float spinSpd = 2f;

			TerratweaksPlayer tPlr = player.GetModPlayer<TerratweaksPlayer>();

			Vector2 pos = player.MountedCenter + Vector2.One.RotatedBy(off + (tPlr.adamHeartSpin * spinSpd)) * radius;
			return pos;
		}

		// This should stop finite-pierce projectiles from dying when they hit the heart
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.penetrate > -1) // If a projectile doesn't pierce infinitely, increase the pierce amount so that it can go through the Adamantite Heart
			{
				projectile.penetrate++;

				TerratweaksGlobalProj tProj = projectile.GetGlobalProjectile<TerratweaksGlobalProj>();
				tProj.HitAdamHeart = projectile.whoAmI;
			}
		}

		// Prevent projectiles from hitting this Adamantite Heart if it already hit it, or if the projectile was fired by a different player
		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			TerratweaksGlobalProj tProj = projectile.GetGlobalProjectile<TerratweaksGlobalProj>();

			if (tProj.HitAdamHeart == projectile.whoAmI || projectile.owner != TargetPlayer)
				return false;

			return null;
		}

		public override bool? CanBeHitByItem(Player player, Item item)
		{
			if (player.whoAmI != TargetPlayer)
				return false;

			return null;
		}

		// Visual pulsating effect
		// TODO: Figure out how to make this not pulse faster (or sometimes refuse to move entirely) when multiple hearts are alive at once
		/*private readonly List<Vector2> shadowPos = new();
		private bool returning = false;
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (shadowPos.Count == 0)
			{
				shadowPos.Add(Vector2.UnitX * 10);
				shadowPos.Add(-10 * Vector2.UnitX);
				shadowPos.Add(Vector2.UnitY * 10);
				shadowPos.Add(-10 * Vector2.UnitY);
			}

			for (int i = 0; i < 4; i++)
			{
				Texture2D texture = (Texture2D)Request<Texture2D>(Texture);
				Vector2 pos = shadowPos[i];

				Color color = Color.Multiply(drawColor, 0.3f);

				spriteBatch.Draw(texture, NPC.Center + pos - screenPos, NPC.frame, color, 0f, new Vector2(NPC.width / 2, NPC.height / 2), 1f, SpriteEffects.None, 0f);
			}

			if (!returning)
			{
				shadowPos[0] += Vector2.UnitX / 2;
				shadowPos[1] -= Vector2.UnitX / 2;
				shadowPos[2] += Vector2.UnitY / 2;
				shadowPos[3] -= Vector2.UnitY / 2;

				if (shadowPos[0].X > 4)
					returning = true;
			}
			else
			{
				shadowPos[0] -= Vector2.UnitX / 2;
				shadowPos[1] += Vector2.UnitX / 2;
				shadowPos[2] -= Vector2.UnitY / 2;
				shadowPos[3] += Vector2.UnitY / 2;

				if (shadowPos[0].X < 0)
					returning = false;
			}

			return base.PreDraw(spriteBatch, screenPos, drawColor);
		}*/

		// Heal player and spawn dust on death
		public override void OnKill()
		{
			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(NPC.Center - new Vector2(15, 15), 30, 30, DustID.Firework_Red);
				dust.velocity = Vector2.Normalize(NPC.Center - dust.position) * Main.rand.NextFloat(3f) * -1f;
				dust.noGravity = true;
			}

			if (!Main.player[(int)TargetPlayer].moonLeech) // Don't heal the player if Moon Leech is active, as that prevents healing
			{
				Projectile.NewProjectile(NPC.GetSource_Death(), NPC.position, Vector2.Zero, ProjectileID.VampireHeal, 0, 0f, (int)TargetPlayer, TargetPlayer, 5);
			}
		}
	}
}