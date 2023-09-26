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
	public class DebuffGlobalNPC : GlobalNPC
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
	public class DummyGlobalNPC : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.TargetDummy;

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (GetInstance<TerratweaksConfig>().vanillaChanges.DummyFix == DummySetting.Limited)
			{
				if (ProjectileID.Sets.IsAWhip[projectile.type])
				{
					npc.immortal = false;
				}
			}
		}

		public override void AI(NPC npc)
		{
			switch (GetInstance<TerratweaksConfig>().vanillaChanges.DummyFix)
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

	// Any changes that occur when an NPC is killed should be handled here
	public class KillGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public override void OnKill(NPC npc)
		{
			// Killed daytime EoL
			if (npc.type == NPCID.HallowBoss && Main.dayTime)
			{
				TerratweaksConfig config = TerratweaksConfig.Instance;

				if (config.vanillaChanges.SIRework) // Transform the Soaring Insignia into the Radiant Insignia if the player has one
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
}