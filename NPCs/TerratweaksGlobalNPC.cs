using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks.NPCs
{
	// A GlobalNPC made to handle any custom debuffs inflicted upon an enemy
	public class DebuffEffectHandler : GlobalNPC
	{
		public override void SetDefaults(NPC npc)
		{
			npc.buffImmune[BuffID.Webbed] = npc.NPCCanStickToWalls() || npc.boss;
		}

		public override void AI(NPC npc)
		{
			Player player = Main.player[Main.myPlayer];
			TerratweaksPlayer tPlr = player.GetModPlayer<TerratweaksPlayer>();

			if (tPlr.spiderWeb && npc.damage > 0 && npc.Distance(Main.player[Main.myPlayer].Center) <= 64 && !npc.buffImmune[BuffID.Webbed])
			{
				npc.position -= npc.velocity * 0.5f;
			}
		}
	}

	// Handles modifying the Target Dummy "ghost" npc to allow minions and homing weapons to target it, if that config setting is enabled
	public class DummyTargetHandler : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.TargetDummy;

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (GetInstance<TerratweaksConfig>().DummyFix == DummySetting.Limited)
			{
				if (ProjectileID.Sets.IsAWhip[projectile.type])
				{
					npc.immortal = false;
				}
			}
		}

		public override void AI(NPC npc)
		{
			switch (GetInstance<TerratweaksConfig>().DummyFix)
			{
				case DummySetting.Off:
					npc.immortal = true;
					break;
				case DummySetting.Limited:
					npc.immortal = !(Main.player[Main.myPlayer].HasMinionAttackTargetNPC && Main.player[Main.myPlayer].MinionAttackTargetNPC == npc.whoAmI);
					break;
				case DummySetting.On:
					npc.immortal = false;
					break;
			}
			
			npc.life = npc.lifeMax;
		}
	}

	// Handles Deerclops' regeneration effect if no living players are within 1k blocks of him
	public class DeerclopsRegenHandler : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Deerclops;

		int cooldown = 0;
		public override void AI(NPC npc)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			if (!config.DeerclopsRegens) // Do nothing if Deerclops shouldn't heal
				return;

			int playersNearby = 0;

			foreach (Player player in Main.player)
			{
				// Ignore dead players and hardcore ghosts, as well as players who don't actually exist
				if (!player.active || player.dead || player.ghost)
					continue;

				if (player.Center.Distance(npc.Center) < Conversions.ToPixels(1000))
					playersNearby++;
			}

			// If no players nearby and at less than max HP, heal Deerclops' HP
			// Only displays a number every 60 ticks, or 1 second
			if (playersNearby == 0 && npc.life < npc.lifeMax)
			{
				int healFactor = config.DeerRegenAmt / 60;
				npc.life += healFactor;
				
				cooldown--;
				if (cooldown <= 0)
				{
					cooldown = 60;
					npc.HealEffect(config.DeerRegenAmt);
				}
			}
		}
	}

	// Any changes that occur when an NPC is killed should be handled here
	public class DeathEffectHandler : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public override void OnKill(NPC npc)
		{
			// Killed daytime EoL
			if (npc.type == NPCID.HallowBoss && Main.dayTime)
			{
				TerratweaksConfig config = GetInstance<TerratweaksConfig>();

				if (config.SIRework) // Transform the Soaring Insignia into the Radiant Insignia if the player has one
				{
					foreach (Player plr in Main.player)
					{
						if (plr.dead || plr.ghost || !plr.active)
							continue;

						Item insignia = plr.inventory.FirstOrDefault(i => i.type == ItemID.EmpressFlightBooster);

						if (insignia != default(Item)) // Make sure the player has a Soaring Insignia in their inventory to begin with
						{
							var oldPrefix = insignia.prefix;
							plr.DropItem(new EntitySource_Loot(npc), plr.position, ref insignia);
							plr.GetModPlayer<CutscenePlayer>().inCutscene = true;
						}
						else // Player didn't have one in their inventory... Maybe they have one equipped?
						{
							insignia = plr.armor.FirstOrDefault(i => i.type == ItemID.EmpressFlightBooster);

							if (insignia != default(Item))
							{
								var oldPrefix = insignia.prefix;
								plr.DropItem(new EntitySource_Loot(npc), plr.position, ref insignia);
								CutscenePlayer cPlr = plr.GetModPlayer<CutscenePlayer>();
								cPlr.inCutscene = true;
								cPlr.direction = plr.direction;
							}
						}
					}
				}
			}
		}
	}

	// Modify shops here
	public class ShopHandler : GlobalNPC
	{
		public override void ModifyShop(NPCShop shop)
		{
			if (shop.NpcType == NPCID.DyeTrader)
			{
				foreach (KeyValuePair<int, Item> pair in ContentSamples.ItemsByType)
				{
					int type = pair.Key;
					Item item = pair.Value;

					if (shop.Entries.Any(e => e.Item.type == type)) // Ignore any items already sold by the Dye Trader
						continue;

					bool isDyeIngredient = false;

					// Should hopefully handle modded items that are used for dyes!
					// Also ignores items that are dyes themselves, since we don't need to iterate over recipes if we already know this one's getting added
					if (item.dye == 0)
					{
						foreach (Recipe recipe in Main.recipe.Where(r => r.ContainsIngredient(item.type)))
						{
							// Don't add items that are used in a recipe containing bottled water or another dye
							// This should prevent most items that aren't used exclusively for a dye, like Crystal Shards or Luminite Bars, from being sold by the Dye Trader
							if (recipe.ContainsIngredient(ItemID.BottledWater) || recipe.requiredItem.Any(i => i.dye > 0))
								continue;

							if (recipe.createItem.dye > 0)
							{
								isDyeIngredient = true;
								break;
							}
						}
					}

					if (item.dye > 0 || isDyeIngredient)
					{
						Condition configEnabled = new("Mods.Terratweaks.Conditions.DyeConfigActive", () => GetInstance<TerratweaksConfig>().DyeTraderShopExpansion);
						Condition itemInInv = new("Mods.Terratweaks.Conditions.InPlayerInv", () =>
							Main.LocalPlayer.inventory.Any(i => i.type == type) ||
							Main.LocalPlayer.dye.Any(i => i.type == type) ||
							Main.LocalPlayer.miscDyes.Any(i => i.type == type) ||
							Main.LocalPlayer.bank.item.Any(i => i.type == type) ||
							Main.LocalPlayer.bank2.item.Any(i => i.type == type) ||
							Main.LocalPlayer.bank3.item.Any(i => i.type == type) ||
							Main.LocalPlayer.bank4.item.Any(i => i.type == type));
						shop.Add(type, configEnabled, itemInInv);
						Terratweaks.DyeItemsSoldByTrader.Add(type);
					}
				}
			}
		}
	}
}