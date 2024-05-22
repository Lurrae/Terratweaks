using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
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

	// Handles any changes to bestiary kill counts
	public class BestiaryEditHandler : GlobalNPC
	{
		public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			if (!GetInstance<TerratweaksConfig>().BetterBestiary)
				return;
			
			// Setting "quick unlock" makes it so that every Bestiary entry will be fully unlocked after one kill, regardless of banner requirement
			if (bestiaryEntry.UIInfoProvider is CommonEnemyUICollectionInfoProvider uiInfo)
			{
				FieldInfo killCount = typeof(CommonEnemyUICollectionInfoProvider).GetField("_quickUnlock", BindingFlags.NonPublic | BindingFlags.Instance);
				killCount.SetValue(uiInfo, true);
			}
			else if (bestiaryEntry.UIInfoProvider is SalamanderShellyDadUICollectionInfoProvider uiInfo2)
			{
				FieldInfo killCount = typeof(SalamanderShellyDadUICollectionInfoProvider).GetField("_quickUnlock", BindingFlags.NonPublic | BindingFlags.Instance);
				killCount.SetValue(uiInfo2, true);
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

	// Handle removing contact damage, Expert stat scaling, and NPC invulnerability
	public class StatChangeHandler : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private float baseKBResist = 0f;
		private float baseDamageMult = 1f;

		public override void SetStaticDefaults()
		{
			if (GetInstance<TerratweaksConfig>().NoExpertScaling)
			{
				for (int i = 0; i < NPCID.Sets.NeedsExpertScaling.Length; i++)
				{
					NPCID.Sets.DontDoHardmodeScaling[i] = true;
				}
			}
		}

		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			baseKBResist = npc.knockBackResist;
			baseDamageMult = npc.takenDamageMultiplier;
		}

		public struct DREnemy(float dmgResist, float kbResist, Func<NPC, bool> defensiveState)
		{
			public float DmgResist { get; } = dmgResist;
			public float KbResist { get; } = kbResist;
			public Func<NPC, bool> DefensiveState = defensiveState;
		}

		public static readonly Dictionary<int, DREnemy> damageResistantEnemies = new()
		{
			{ NPCID.GraniteFlyer, new DREnemy(0.25f, 1.1f, (NPC npc) => npc.ai[0] == -1) },
			{ NPCID.GraniteGolem, new DREnemy(0.25f, 0.05f, (NPC npc) => npc.ai[2] < 0f) }
		};

		public override void PostAI(NPC npc)
		{
			// Ignore this entire process if the config option is not set
			if (!GetInstance<TerratweaksConfig>().NoEnemyInvulnerability)
				return;

			// Do not try to remove invulnerability from bosses (bosses should NEVER have any of these AIs, but you never know)
			if (npc.boss)
				return;

			// Granite enemies have their invulnerability disabled and damage reduction applied
			if (npc.aiStyle == NPCAIStyleID.GraniteElemental || npc.type == NPCID.GraniteGolem)
			{
				npc.dontTakeDamage = false;

				if ((npc.aiStyle == NPCAIStyleID.GraniteElemental && npc.ai[0] == -1) || (npc.type == NPCID.GraniteGolem && npc.ai[2] < 0f))
				{
					npc.takenDamageMultiplier = 0.25f;

					if (npc.aiStyle == NPCAIStyleID.GraniteElemental)
					{
						npc.knockBackResist = 1.1f;
					}
					else
					{
						npc.knockBackResist = 0.05f;
					}
				}
				else if (npc.aiStyle == NPCAIStyleID.GraniteElemental || npc.type == NPCID.GraniteGolem)
				{
					npc.takenDamageMultiplier = baseDamageMult;
					npc.knockBackResist = baseKBResist;
				}
			}
			// Remove invulnerability from jellyfish. The increased retaliation damage on melee hits needs to be handled elsewhere
			else if (npc.aiStyle == NPCAIStyleID.Jellyfish)
			{
				npc.dontTakeDamage = false;

				// If Calamity is installed, implement their conditional contact damage
				// This is needed because just like vanilla, the Calamity devs check npc.dontTakeDamage instead of npc.ai[1] == 1
				if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
				{
					float damagingVelocity = npc.type == NPCID.GreenJellyfish ? 3.6f : 2.8f;

					if ((bool)calamity.Call("GetDifficultyActive", "revengeance"))
					{
						damagingVelocity = npc.type == NPCID.GreenJellyfish ? 7.2f : 5.6f;
					}

					npc.damage = (npc.ai[1] == 1 || npc.velocity.Length() > damagingVelocity) ? npc.defDamage : 0;
				}
			}
		}

		// Call TakeDamageFromJellyfish manually because for SOME reason, vanilla checks npc.dontTakeDamage instead of npc.ai[1] == 1
		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (NPCID.Sets.ZappingJellyfish[npc.type])
			{
				if (GetInstance<TerratweaksConfig>().NoEnemyInvulnerability && npc.wet && npc.ai[1] == 1f)
				{
					player.TakeDamageFromJellyfish(npc.whoAmI);
				}
			}
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (NPCID.Sets.ZappingJellyfish[npc.type])
			{
				bool isHurtingProjectile = projectile.aiStyle == ProjAIStyleID.Spear || projectile.aiStyle == ProjAIStyleID.ShortSword || projectile.aiStyle == ProjAIStyleID.HeldProjectile || projectile.aiStyle == ProjAIStyleID.SleepyOctopod || ProjectileID.Sets.IsAWhip[projectile.type] || ProjectileID.Sets.AllowsContactDamageFromJellyfish[projectile.type];

				if (GetInstance<TerratweaksConfig>().NoEnemyInvulnerability && npc.wet && npc.ai[1] == 1f && isHurtingProjectile)
				{
					Main.player[projectile.owner].TakeDamageFromJellyfish(npc.whoAmI);
				}
			}
		}

		public static readonly List<int> npcTypesThatShouldNotDoContactDamage =
		#region npcTypesThatShouldNotDoContactDamage
		[
			// Hornets
			NPCID.Hornet,
			NPCID.HornetFatty,
			NPCID.HornetHoney,
			NPCID.HornetLeafy,
			NPCID.HornetSpikey,
			NPCID.HornetStingy,
			NPCID.MossHornet,

			// Archers
			NPCID.CultistArcherBlue,
			NPCID.CultistArcherWhite,
			NPCID.ElfArcher,
			NPCID.GoblinArcher,
			NPCID.PirateCrossbower,
			NPCID.SkeletonArcher,

			// Gun users
			NPCID.PirateDeadeye,
			NPCID.RayGunner,
			NPCID.ScutlixRider,
			NPCID.SnowBalla,
			NPCID.SnowmanGangsta,
			NPCID.SkeletonCommando,
			NPCID.SkeletonSniper,
			NPCID.TacticalSkeleton,
			NPCID.VortexRifleman,

			// Salamanders
			NPCID.Salamander,
			NPCID.Salamander2,
			NPCID.Salamander3,
			NPCID.Salamander4,
			NPCID.Salamander5,
			NPCID.Salamander6,
			NPCID.Salamander7,
			NPCID.Salamander8,
			NPCID.Salamander9,

			// Misc.
			NPCID.AngryNimbus,
			NPCID.Eyezor,
			NPCID.Gastropod,
			NPCID.IcyMerman,
			NPCID.MartianTurret,
			NPCID.Probe
		];
		#endregion

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			bool shouldNotDoContactDamage = false;

			if (npc.aiStyle == NPCAIStyleID.Caster)
				shouldNotDoContactDamage = true;
			
			if (npcTypesThatShouldNotDoContactDamage.Contains(npc.type))
				shouldNotDoContactDamage = true;

			if (shouldNotDoContactDamage && GetInstance<TerratweaksConfig>().NoCasterContactDamage)
				return false;

			return base.CanHitPlayer(npc, target, ref cooldownSlot);
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

	// Add/modify NPC drops here
	[JITWhenModsEnabled("CalamityMod")]
	public class DropHandler : GlobalNPC
	{
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			if (npc.type == NPCID.HallowBoss) // Empress of Light
			{
				if (ModLoader.HasMod("CalamityMod")) // Remove Calamity's Terraprisma drop from EoL
				{
					foreach (IItemDropRule rule in npcLoot.Get(false))
					{
						HandleCalamityEoLChanges(rule);
					}
				}

				// If the configs allow it, add a secondary Terraprisma drop chance when EoL is not "genuinely enraged"
				if (config.TerraprismaDropRate > 0 && (!ModLoader.HasMod("CalamityMod") || config.TerraprismaCalReversion))
				{
					var nightEol = new TerratweaksDropConditions.NightEoL();
					npcLoot.Add(new ItemDropWithConditionRule(ItemID.EmpressBlade, 100, 1, 1, nightEol, config.TerraprismaDropRate));
				}
			}
		}

		void HandleCalamityEoLChanges(IItemDropRule rule)
		{
			if (rule is LeadingConditionRule lcr && lcr.ChainedRules.Count > 2) // The rule we're looking for has at least 3 items
			{
				CalamityMod.DropHelper.AllOptionsAtOnceWithPityDropRule targetRule = null;

				foreach (IItemDropRuleChainAttempt chainedRule in lcr.ChainedRules)
				{
					// The rule we're looking for is a custom type from Calamity, which is why we needed the JITWhenModsEnabled stuff
					if (chainedRule.RuleToChain is CalamityMod.DropHelper.AllOptionsAtOnceWithPityDropRule pityRule)
					{
						targetRule = pityRule;
						break;
					}
				}

				if (targetRule != null) // Found Calamity's rule, now remove Terraprisma from it
				{
					CalamityMod.WeightedItemStack stackToRemove= new();
					bool foundTerraprisma = false;

					foreach (CalamityMod.WeightedItemStack stack in targetRule.stacks)
					{
						FieldInfo stackItemID = stack.GetType().GetField("itemID", BindingFlags.NonPublic | BindingFlags.Instance);
						int itemID = (int)stackItemID.GetValue(stack);

						if (itemID == ItemID.EmpressBlade)
						{
							stackToRemove = stack;
							foundTerraprisma = true;
							break;
						}
					}

					if (foundTerraprisma)
					{
						List<CalamityMod.WeightedItemStack> stacksList = targetRule.stacks.ToList();
						stacksList.Remove(stackToRemove);
						targetRule.stacks = stacksList.ToArray();
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
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

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
						Condition configEnabled = new("Mods.Terratweaks.Conditions.DyeConfigActive", () => config.DyeTraderShopExpansion);
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

			if (shop.NpcType == NPCID.Steampunker)
			{
				if (config.SoilSolutionsPreML)
				{
					shop.ActiveEntries.First(i => i.Item.type == ItemID.DirtSolution).Disable();
					shop.InsertAfter(ItemID.DirtSolution, ItemID.DirtSolution, Condition.NotRemixWorld);
					shop.ActiveEntries.First(i => i.Item.type == ItemID.SandSolution).Disable();
					shop.InsertAfter(ItemID.SandSolution, ItemID.SandSolution, Condition.NotRemixWorld);
					shop.ActiveEntries.First(i => i.Item.type == ItemID.SnowSolution).Disable();
					shop.InsertAfter(ItemID.SnowSolution, ItemID.SnowSolution, Condition.NotRemixWorld);
				}

				if (config.SolutionsOnGFB)
				{
					List<int> itemsToAddToShop = new();

					foreach (var entry in shop.ActiveEntries)
					{
						if (entry.Conditions.Contains(Condition.NotRemixWorld) && !itemsToAddToShop.Contains(entry.Item.type))
						{
							itemsToAddToShop.Add(entry.Item.type);
						}
					}

					foreach (int itemType in itemsToAddToShop)
					{
						if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
						{
							Condition inCalEndgame = new("Mods.Terratweaks.Conditions.InCalEndgame", () => (bool)calamity.Call("GetBossDowned", "calamitas") && (bool)calamity.Call("GetBossDowned", "exomechs"));

							shop.InsertAfter(itemType, itemType, Condition.RemixWorld, inCalEndgame);
						}
						else
						{
							shop.InsertAfter(itemType, itemType, Condition.RemixWorld, Condition.DownedMoonLord);
						}
					}
				}
			}

			if (shop.NpcType == NPCID.Truffle)
			{
				if (config.SolutionsOnGFB)
				{
					List<int> itemsToAddToShop = new();

					foreach (var entry in shop.ActiveEntries)
					{
						if (entry.Conditions.Contains(Condition.NotRemixWorld))
						{
							itemsToAddToShop.Add(entry.Item.type);
						}
					}

					foreach (int itemType in itemsToAddToShop)
					{
						shop.InsertAfter(itemType, itemType, new Condition[] { Condition.RemixWorld, Condition.DownedMoonLord });
					}
				}

				if (config.NPCsSellMinecarts)
				{
					shop.Add(ItemID.ShroomMinecart);
				}
			}

			if (shop.NpcType == NPCID.Dryad)
			{
				if (config.NPCsSellMinecarts)
				{
					shop.Add(ItemID.SunflowerMinecart, Condition.HappyWindyDay);
					shop.Add(ItemID.LadybugMinecart, Condition.HappyWindyDay);
					shop.Add(ItemID.BeeMinecart, Condition.DownedQueenBee);
				}
			}

			if (shop.NpcType == NPCID.Merchant)
			{
				if (config.NPCsSellMinecarts)
				{
					shop.Add(ItemID.DesertMinecart, Condition.InDesert);
				}
			}
		}
	}
}