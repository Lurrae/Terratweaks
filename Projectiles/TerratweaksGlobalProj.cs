using Terraria;
using Terraria.Audio;
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

			// Chester needs to check if he should access the player's safe instead of their piggy bank
			if (GetInstance<TerratweaksConfig>().ChesterRework && player.chest == -2)
			{
				player.OpenChest((int)(projectile.position.X / 16), (int)(projectile.position.Y / 16), -3);
			}
		}

		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
		{
			var clientConfig = GetInstance<TerratweaksConfig_Client>();

			if (clientConfig.NoDamageVariance)
				modifiers.DamageVariationScale *= 0;

			if (clientConfig.NoRandomCrit && sourceItem != null)
			{
				var globalItem = sourceItem.GetGlobalItem<TooltipChanges>();
				globalItem.hitsDone += projectile.CritChance;

				if (globalItem.hitsDone >= 100)
				{
					modifiers.SetCrit();
					globalItem.hitsDone = 0;
				}
				else
					modifiers.DisableCrit();
			}
		}
	}
}