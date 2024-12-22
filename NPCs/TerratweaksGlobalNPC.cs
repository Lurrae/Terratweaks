using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terratweaks.Items;
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

			if (GetInstance<TerratweaksConfig>().SmartNymphs)
			{
				NPCID.Sets.DontDoHardmodeScaling[NPCID.LostGirl] = true;
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
			var config = GetInstance<TerratweaksConfig>();

			// Handle forcing vanilla boss contact damage
			// ModNPC check makes this only affect vanilla bosses
			if (npc.boss && npc.ModNPC == null && config.ForceBossContactDamage)
			{
				// Force the boss to deal damage at all times
				// TODO: Is this the best way to do this?
				npc.damage = npc.defDamage;
			}

			// Ignore everything below if the config option to disable enemy invulnerability is not set
			if (!config.NoEnemyInvulnerability)
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

		public static readonly List<int> ignoreNoContactDmg = new();

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			bool shouldNotDoContactDamage = false;

			if (npc.aiStyle == NPCAIStyleID.Caster)
				shouldNotDoContactDamage = true;
			
			if (npcTypesThatShouldNotDoContactDamage.Contains(npc.type))
				shouldNotDoContactDamage = true;

			if (shouldNotDoContactDamage && !ignoreNoContactDmg.Contains(npc.type) && GetInstance<TerratweaksConfig>().NoCasterContactDamage)
				return false;

			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}
	}

	// Any changes that occur when an NPC is killed should be handled here
	public class DeathEffectHandler : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public static int townNpcDeathsThisInvasion = 0;

		public override void AI(NPC npc)
		{
			// Reset death counter if an invasion is not active
			if (Main.invasionType == InvasionID.None)
				townNpcDeathsThisInvasion = 0;
		}

		public override void OnKill(NPC npc)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			// Killed daytime EoL
			if (npc.type == NPCID.HallowBoss && Main.dayTime)
			{
				if (config.SIRework) // Transform the Soaring Insignia into the Radiant Insignia if the player has one
				{
					foreach (Player plr in Main.player)
					{
						if (plr.dead || plr.ghost || !plr.active)
							continue;

						Item insignia = plr.inventory.FirstOrDefault(i => i.type == ItemID.EmpressFlightBooster);
						int aInsigType = -1;

						if (ModLoader.TryGetMod("CalamityMod", out Mod cal) && cal.TryFind("AscendantInsignia", out ModItem aInsig))
						{
							if (config.calamitweaks.RadiantInsigniaUpgradesFromAscendant)
							{
								aInsigType = aInsig.Type;
								insignia = plr.inventory.FirstOrDefault(i => i.type == aInsigType);
							}
						}

						if (insignia != default(Item)) // Make sure the player has a Soaring Insignia in their inventory to begin with
						{
							var oldPrefix = insignia.prefix;
							plr.DropItem(new EntitySource_Loot(npc), plr.position, ref insignia);
							RadiantInsigniaCutscene.AscendantInsigniaType = aInsigType;
							plr.GetModPlayer<CutscenePlayer>().inCutscene = true;
						}
						else // Player didn't have one in their inventory... Maybe they have one equipped?
						{
							insignia = plr.armor.FirstOrDefault(i => i.type == ItemID.EmpressFlightBooster);

							if (ModLoader.HasMod("CalamityMod") && config.calamitweaks.RadiantInsigniaUpgradesFromAscendant && aInsigType > -1)
							{
								insignia = plr.armor.FirstOrDefault(i => i.type == aInsigType);
							}

							if (insignia != default(Item))
							{
								var oldPrefix = insignia.prefix;
								plr.DropItem(new EntitySource_Loot(npc), plr.position, ref insignia);
								RadiantInsigniaCutscene.AscendantInsigniaType = aInsigType;
								CutscenePlayer cPlr = plr.GetModPlayer<CutscenePlayer>();
								cPlr.inCutscene = true;
								cPlr.direction = plr.direction;
							}
						}
					}
				}
			}

			// Town NPC died- increment invasion death counter and call off the invasion if necessary
			if (npc.townNPC && config.NPCDeathsToCallOffInvasion > 0 && Main.invasionType != InvasionID.None)
			{
				townNpcDeathsThisInvasion++;

				if (townNpcDeathsThisInvasion >= config.NPCDeathsToCallOffInvasion)
				{
					Main.invasionType = InvasionID.None;
					Color color = new(175, 75, 255);
					Main.NewText(Language.GetTextValue("Mods.Terratweaks.Common.InvasionFailNPC"), color);
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
				if (config.TerraprismaDropRate > 0 && (!ModLoader.HasMod("CalamityMod") || config.calamitweaks.RevertTerraprisma))
				{
					var nightEol = new TerratweaksDropConditions.NightEoL();
					npcLoot.Add(new ItemDropWithConditionRule(ItemID.EmpressBlade, 100, 1, 1, nightEol, config.TerraprismaDropRate));
				}
			}

			// All enemies from the celestial pillars need to have an option to drop fragments once Moon Lord has been downed
			if (config.PillarEnemiesDropFragments)
			{
				int fragmentID = -1;

				switch (npc.type)
				{
					case NPCID.SolarCorite:
					case NPCID.SolarCrawltipedeTail:
					case NPCID.SolarDrakomire:
					case NPCID.SolarDrakomireRider:
					case NPCID.SolarSolenian:
					case NPCID.SolarSpearman:
					case NPCID.SolarSroller:
						fragmentID = ItemID.FragmentSolar;
						break;
					case NPCID.VortexHornet:
					case NPCID.VortexHornetQueen:
					case NPCID.VortexLarva:
					case NPCID.VortexRifleman:
					case NPCID.VortexSoldier:
						fragmentID = ItemID.FragmentVortex;
						break;
					case NPCID.NebulaBeast:
					case NPCID.NebulaBrain:
					case NPCID.NebulaHeadcrab:
					case NPCID.NebulaSoldier:
						fragmentID = ItemID.FragmentNebula;
						break;
					case NPCID.StardustCellSmall:
					case NPCID.StardustJellyfishBig:
					case NPCID.StardustSoldier:
					case NPCID.StardustSpiderBig:
					case NPCID.StardustWormHead:
						fragmentID = ItemID.FragmentStardust;
						break;
				}

				if (fragmentID > -1)
				{
					npcLoot.Add(new ItemDropWithConditionRule(fragmentID, 4, 1, 1, new TerratweaksDropConditions.DownedMoonlord()));
				}
			}
		}

		private static void HandleCalamityEoLChanges(IItemDropRule rule)
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
		public static readonly Dictionary<int, int> SellableWeapons = new()
		{
			{ ItemID.AleThrowingGlove, NPCID.Demolitionist },
			{ ItemID.DyeTradersScimitar, NPCID.DyeTrader },
			{ ItemID.PainterPaintballGun, NPCID.Painter },
			{ ItemID.StylistKilLaKillScissorsIWish, NPCID.Stylist },
			{ ItemID.CombatWrench, NPCID.Mechanic },
			{ ItemID.TaxCollectorsStickOfDoom, NPCID.Clothier },
			{ ItemID.PrincessWeapon, NPCID.Princess }
		};

		public static readonly Dictionary<int, List<Condition>> SellableWeaponConditions = new()
		{
			{ ItemID.AleThrowingGlove, new List<Condition>() { Condition.NpcIsPresent(NPCID.DD2Bartender) } },
			{ ItemID.TaxCollectorsStickOfDoom, new List<Condition>() { Condition.NpcIsPresent(NPCID.TaxCollector) } },
			{ ItemID.PrincessWeapon, new List<Condition>() { Condition.DownedPlantera } }
		};

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
						shop.InsertAfter(itemType, itemType, [ Condition.RemixWorld, Condition.DownedMoonLord ]);
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

			if (config.TownNPCsSellWeapons)
			{
				// Make sure the NPC whose shop we're editing actually has a weapon to sell
				if (SellableWeapons.Any(pair => pair.Value == shop.NpcType))
				{
					// Foreach loop to ensure that if an NPC needs to sell multiple weapons, they can do so without issue
					foreach (KeyValuePair<int, int> pair in SellableWeapons.Where(p => p.Value == shop.NpcType))
					{
						int weaponType = pair.Key;
						List<Condition> conditions = new() { Condition.HappyEnoughToSellPylons }; // Ensures all NPCs must be happy to sell weapons, unless it's remix seed

						// Some NPCs will have additional conditions- like the Demolitionist and Clothier requiring the NPC whose weapon they're selling,
						// and the Princess requiring Plantera to be defeated- which will be handled by this code
						if (SellableWeaponConditions.TryGetValue(weaponType, out List<Condition> bonusConditions))
						{
							conditions.AddRange(bonusConditions);
						}

						// Finally, add the item to the shop with whatever conditions we're given!
						shop.Add(weaponType, conditions.ToArray());
					}
				}
			}
		}
	}

	// Handles all of the spawning behavior changes done to make non-biome Mimics harder to discern from real chests
	public class MimicChanges : GlobalNPC
	{
		private static bool IsAMimic(NPC npc)
		{
			return npc.active && npc.aiStyle == NPCAIStyleID.Mimic && npc.TypeName.Contains("Mimic");
		}

		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			// This should affect any modded NPCs that use mimic AI, such as Thorium's Lihzahrd Mimics
			if (GetInstance<TerratweaksConfig>().SmartMimics && IsAMimic(npc))
			{
				Vector2 snapPos = new((int)Math.Floor(npc.position.X / 16), (int)Math.Floor(npc.position.Y / 16));
				if (!TileObject.CanPlace((int)snapPos.X, (int)snapPos.Y, 21, 0, 1, out _, true))
				{
					if (FindClosestMimicSpawnPosition(snapPos) != null)
						snapPos = FindClosestMimicSpawnPosition(snapPos).Value;
				}

				npc.Bottom = snapPos.ToWorldCoordinates(npc.width / 2, 32 - npc.height) + new Vector2(-4, 8);
			}
		}

		private static Vector2? FindClosestMimicSpawnPosition(Vector2 originalPosition)
		{
			// This code was taken from the following StackOverflow answer: https://stackoverflow.com/a/3706260
			// It was modified slightly to work in this context of course

			// (dx, dy) is a vector - direction in which we move right now
			int dx = 1;
			int dy = 0;
			// length of current segment
			int segment_length = 1;

			// current position (x, y) and how much of current segment we passed
			int x = (int)originalPosition.X;
			int y = (int)originalPosition.Y;
			int segment_passed = 0;
			for (int n = 0; n < 256; ++n)
			{
				// make a step, add 'direction' vector (dx, dy) to current position (x, y)
				x += dx;
				y += dy;
				++segment_passed;

				if (segment_passed == segment_length)
				{
					// done with current segment
					segment_passed = 0;

					// 'rotate' directions
					int buffer = dx;
					dx = -dy;
					dy = buffer;

					// increase segment length if necessary
					if (dy == 0)
					{
						++segment_length;
					}
				}

				if (TileObject.CanPlace(x, y, 21, 0, 1, out _, true))
				{
					return new Vector2(x, y);
				}
			}

			return null;
		}

		public override bool PreAI(NPC npc)
		{
			if (GetInstance<TerratweaksConfig>().SmartMimics && IsAMimic(npc))
			{
				npc.canDisplayBuffs = npc.ai[0] != 0;

				if (npc.ai[0] == 0)
				{
					npc.dontTakeDamage = true;
				}
				else
				{
					npc.dontTakeDamage = false;
				}
			}
			
			return base.PreAI(npc);
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			if (GetInstance<TerratweaksConfig>().SmartMimics && IsAMimic(npc))
			{
				if (npc.ai[0] == 0 && Main.LocalPlayer.HasBuff(BuffID.Spelunker))
				{
					if (drawColor.R < 200)
						drawColor.R = 200;

					if (drawColor.G < 170)
						drawColor.G = 170;

					if (Main.rand.NextBool(60))
					{
						Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.TreasureSparkle, Alpha: 150, Scale: 0.3f);
						dust.fadeIn = 1f;
						dust.velocity *= 0.1f;
						dust.noLight = true;
					}
				}
			}
		}
	}

	// Handles all of the AI changes to Lost Girls to make them more difficult to discern from real bound NPCs
	public class NymphChanges : GlobalNPC
	{
		public override void SetDefaults(NPC npc)
		{
			if (GetInstance<TerratweaksConfig>().SmartNymphs && npc.type == NPCID.LostGirl)
			{
				npc.friendly = true;
			}
		}

		public override bool PreAI(NPC npc)
		{
			// Stop vanilla Lost Girl AI from running, as we're going to be using custom behavior and don't want vanilla stuff to interfere
			if (GetInstance<TerratweaksConfig>().SmartNymphs && npc.type == NPCID.LostGirl)
			{
				return false;
			}

			return base.PreAI(npc);
		}

		public override bool? CanChat(NPC npc)
		{
			if (GetInstance<TerratweaksConfig>().SmartNymphs && npc.type == NPCID.LostGirl)
				return true;

			return base.CanChat(npc);
		}

		public override void PostAI(NPC npc)
		{
			if (GetInstance<TerratweaksConfig>().SmartNymphs && npc.type == NPCID.LostGirl)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					for (int i = 0; i < Main.maxPlayers; i++)
					{
						Player player = Main.player[i];
						if (player.active && player.talkNPC == npc.whoAmI)
						{
							npc.AI_000_TransformBoundNPC(i, NPCID.Nymph);
							player.SetTalkNPC(-1);
						}
					}
				}
			}
		}
	}

	public class SpawnRateScaling : GlobalNPC
	{
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			bool anyBosses = false;

			for (int i = 0; i < 200; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && (npc.boss || NPCID.Sets.DangerThatPreventsOtherDangers[npc.type]))
				{
					if (npc.type == NPCID.LunarTowerSolar ||
						npc.type == NPCID.LunarTowerVortex ||
						npc.type == NPCID.LunarTowerNebula ||
						npc.type == NPCID.LunarTowerStardust)
					{
						continue;
					}

					anyBosses = true;
				}
			}

			if (!anyBosses)
				return;

			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			if (config.BossesLowerSpawnRates > 0)
				spawnRate = (int)Math.Round(spawnRate * (1 / config.BossesLowerSpawnRates));

			maxSpawns = (int)Math.Round(maxSpawns * config.BossesLowerSpawnRates);
		}
	}
}