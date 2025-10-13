using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terratweaks.Items;

namespace Terratweaks.NPCs
{
	// A GlobalNPC made to handle any custom debuffs inflicted upon an enemy
	public class DebuffEffectHandler : GlobalNPC
	{
		// Set debuff immunities if the corresponding config options are active
		// The config check makes it possible to disable these changes should another mod have issues with them
		public override void SetDefaults(NPC npc)
		{
			// Spiders and bosses are immune to the slowing effects of Webbed
			// Also checks if the enemy is already immune, such as from another mod (that's what the "|=" is for, it keeps existing true values intact)
			if (Terratweaks.Config.SpiderSetBonus)
				npc.buffImmune[BuffID.Webbed] |= npc.NPCCanStickToWalls() || npc.boss;

			// Any non-organic enemies are immune to Bleeding
			// If another mod makes them immune to Bleeding, that applies too (that's what the "|=" is for, it keeps existing true values intact)
			// Note that "IsProbablyOrganic()" says 'Probably' for a reason; modded hit sounds are not accounted for,
			// so any enemies that use a modded hit sound will automatically not be immune unless the mod they come from makes them immune by default
			if (Terratweaks.Config.PaperCuts)
				npc.buffImmune[BuffID.Bleeding] |= !IsProbablyOrganic(npc);
		}

		// Any enemy with 
		private static readonly List<SoundStyle> InorganicHitSounds = new()
		{
			SoundID.NPCHit2,	// Skeleton
			SoundID.NPCHit4,	// Mech
			SoundID.NPCHit5,	// Pixie
			SoundID.NPCHit11,	// Snowman
			SoundID.NPCHit30,	// Angry Nimbus
			SoundID.NPCHit34,	// Deadly Sphere
			SoundID.NPCHit36,	// Dungeon Spirit
			SoundID.NPCHit41,	// Granite Golem
			SoundID.NPCHit42,	// Martian Drone
			SoundID.NPCHit43,	// Bubble Shield
			SoundID.NPCHit49,	// Reaper
			SoundID.NPCHit52,	// Shadowflame Apparition
			SoundID.NPCHit53,	// Martian Tesla Turret
			SoundID.NPCHit54	// Wraith
		};

		private static readonly List<int> InorganicAIStyles = new()
		{
			// Note: All vanilla enemies with Spell AI and Spore AI are considered "Projectile NPCs",
			// but I'm adding them here to account for modded enemies that don't properly set ProjectileNPC[Type]
			NPCAIStyleID.Spell,
			NPCAIStyleID.Spore,

			// Honestly idk why Shadowflame Apparitions and all of the Cultist's NPC-based projectiles aren't considered projectile NPCs,
			// maybe cuz they have more health?
			NPCAIStyleID.AncientDoom,
			NPCAIStyleID.AncientLight,
			NPCAIStyleID.AncientVision
		};

		// TODO: Figure out a better system for accounting for modded enemies,
		//		 particularly ones with custom hit sounds like many of Calamity's enemies and bosses
		private static bool IsProbablyOrganic(NPC npc)
		{
			// Ignore projectile NPCs, as well as any enemies not flagged with ProjectileNPC that should still be counted
			if (NPCID.Sets.ProjectileNPC[npc.type] || InorganicAIStyles.Contains(npc.aiStyle))
				return false;

			// Ignore enemies with an inorganic hit sound
			// This unfortunately will ignore any enemy with a modded hit sound, but unless I wanna add
			// a mod call or something there ain't much I can do about it
			if (npc.HitSound.HasValue && InorganicHitSounds.Contains(npc.HitSound.Value))
				return false;

			return true;
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

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			// TODO: It would probably be ideal to not do anything if another mod already makes Bleeding do something,
			//		 but for now just checking that the paper cut config is active works fine; if another mod makes Bleeding have an effect,
			//		 paper cuts can just be disabled to resolve the conflicts
			if (Terratweaks.Config.PaperCuts)
			{
				if (npc.HasBuff(BuffID.Bleeding))
				{
					// Prevent natural regen if the enemy is regenerating somehow
					if (npc.lifeRegen > 0)
						npc.lifeRegen = 0;

					npc.lifeRegen -= 2; // Loses 1 HP per second, the same amount Poisoned deals to players
										// (a third of what it deals to enemies, and half of what On Fire! deals to both enemies and players)
				}
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			// TODO: Ditto the todo in UpdateLifeRegen()
			//		 tl;dr: Would prefer to check if another mod adds functionality to Bleeding instead of just checking the config,
			//		 but for now this is fine
			if (Terratweaks.Config.PaperCuts)
			{
				if (npc.HasBuff(BuffID.Bleeding) && Main.rand.NextBool(30))
				{
					Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood);
					Dust dust2 = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood);
					dust2.velocity.Y += 0.5f;
					dust.velocity *= 0.25f;
				}
			}
		}
	}

	// Handles any changes to bestiary kill counts
	public class BestiaryEditHandler : GlobalNPC
	{
		public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			if (!Terratweaks.Config.BetterBestiary)
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
			if (Terratweaks.Config.DummyFix == DummySetting.Limited)
			{
				if (ProjectileID.Sets.IsAWhip[projectile.type])
				{
					npc.immortal = false;
				}
			}
		}

		public override void AI(NPC npc)
		{
			switch (Terratweaks.Config.DummyFix)
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
			if (!Terratweaks.Config.DeerclopsRegens) // Do nothing if Deerclops shouldn't heal
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
				int healFactor = Terratweaks.Config.DeerRegenAmt / 60;
				npc.life += healFactor;
				
				cooldown--;
				if (cooldown <= 0)
				{
					cooldown = 60;
					npc.HealEffect(Terratweaks.Config.DeerRegenAmt);
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
			if (Terratweaks.Config.NoExpertScaling)
			{
				for (int i = 0; i < NPCID.Sets.NeedsExpertScaling.Length; i++)
				{
					NPCID.Sets.DontDoHardmodeScaling[i] = true;
				}
			}

			if (Terratweaks.Config.SmartNymphs)
			{
				NPCID.Sets.DontDoHardmodeScaling[NPCID.LostGirl] = true;
			}
		}

		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			baseKBResist = npc.knockBackResist;
			baseDamageMult = npc.takenDamageMultiplier;
		}

		public override void PostAI(NPC npc)
		{
			// Handle forcing vanilla boss contact damage
			// ModNPC check makes this only affect vanilla bosses
			if (npc.boss && npc.ModNPC == null && Terratweaks.Config.ForceBossContactDamage)
			{
				// Force the boss to deal damage at all times
				// TODO: Is this the best way to do this?
				npc.damage = npc.defDamage;
			}

			// Ignore everything below if the config option to disable enemy invulnerability is not set
			if (!Terratweaks.Config.NoEnemyInvulnerability)
				return;

			// Do not try to remove invulnerability from bosses (bosses should NEVER have any of these AIs, but you never know)
			if (npc.boss)
				return;

			// Granite enemies have their invulnerability disabled and damage reduction applied
			if (TerratweaksContentSets.DefensiveEnemyProperties[npc.type] != null)
			{
				Tuple<float, float, Func<NPC, bool>> drData = TerratweaksContentSets.DefensiveEnemyProperties[npc.type];
				npc.dontTakeDamage = false;

				if (drData.Item3(npc))
				{
					npc.takenDamageMultiplier = drData.Item1;
					npc.knockBackResist = drData.Item2;
				}
				else
				{
					npc.takenDamageMultiplier = baseDamageMult;
					npc.knockBackResist = baseKBResist;
				}
			}
			// Remove invulnerability from jellyfish. The increased retaliation damage on melee hits needs to be handled elsewhere
			else if (TerratweaksContentSets.RetalitoryEnemyProperties[npc.type] != null)
			{
				Tuple<float, Func<NPC, bool>> retData = TerratweaksContentSets.RetalitoryEnemyProperties[npc.type];
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

					npc.damage = (retData.Item2(npc) || npc.velocity.Length() > damagingVelocity) ? npc.defDamage : 0;
				}
			}
		}

		// Call TakeDamageFromJellyfish manually because for SOME reason, vanilla checks npc.dontTakeDamage instead of npc.ai[1] == 1
		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (TerratweaksContentSets.RetalitoryEnemyProperties[npc.type] != null)
			{
				Tuple<float, Func<NPC, bool>> retData = TerratweaksContentSets.RetalitoryEnemyProperties[npc.type];

				if (Terratweaks.Config.NoEnemyInvulnerability && retData.Item2(npc))
				{
					player.TakeDamageFromJellyfish(npc.whoAmI);
				}
			}
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (TerratweaksContentSets.RetalitoryEnemyProperties[npc.type] != null)
			{
				Tuple<float, Func<NPC, bool>> retData = TerratweaksContentSets.RetalitoryEnemyProperties[npc.type];

				// TODO: Should I make a custom set for this or something...? Or see if an existing mod does so?
				bool isHurtingProjectile = projectile.aiStyle == ProjAIStyleID.Spear || projectile.aiStyle == ProjAIStyleID.ShortSword || projectile.aiStyle == ProjAIStyleID.HeldProjectile || projectile.aiStyle == ProjAIStyleID.SleepyOctopod || ProjectileID.Sets.IsAWhip[projectile.type] || ProjectileID.Sets.AllowsContactDamageFromJellyfish[projectile.type];

				if (Terratweaks.Config.NoEnemyInvulnerability && retData.Item2(npc) && isHurtingProjectile)
				{
					Main.player[projectile.owner].TakeDamageFromJellyfish(npc.whoAmI);
				}
			}
		}

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			bool shouldNotDoContactDamage = false;

			if (npc.aiStyle == NPCAIStyleID.Caster)
				shouldNotDoContactDamage = true;
			
			if (TerratweaksContentSets.ProjectileAttacker[npc.type])
				shouldNotDoContactDamage = true;

			if (shouldNotDoContactDamage && Terratweaks.Config.NoCasterContactDamage)
				return false;

			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}

		public override bool CanBeHitByNPC(NPC npc, NPC attacker)
		{
			if (Terratweaks.Config.BoundNPCsImmune && npc.friendly && npc.aiStyle == NPCAIStyleID.FaceClosestPlayer)
			{
				return false;
			}

			return base.CanBeHitByNPC(npc, attacker);
		}

		public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
		{
			if (Terratweaks.Config.BoundNPCsImmune && npc.friendly && projectile.hostile && npc.aiStyle == NPCAIStyleID.FaceClosestPlayer)
			{
				return false;
			}

			return base.CanBeHitByProjectile(npc, projectile);
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

			// If this NPC is a probe, and there is currently no Destroyer Head NPC in the world (meaning The Destroyer is dead) kill the probe
			if (Terratweaks.Config.NoLingeringProbes && npc.type == NPCID.Probe)
			{
				// "The Destroyer is dead"
				// "We know"
				// "Who killed him"
				// "We don't know"
				// "I will find him... I will capture him... And NOBODY will die again!"
				if (!NPC.AnyNPCs(NPCID.TheDestroyer) && Main.netMode != NetmodeID.MultiplayerClient)
				{
					// ...ok nevermind I guess somebody will die again (this probe, to be precise)
					npc.StrikeInstantKill();
				}
			}
		}

		public override void OnKill(NPC npc)
		{
			// Killed daytime EoL
			if (npc.type == NPCID.HallowBoss && Main.dayTime)
			{
				if (Terratweaks.Config.SIRework) // Transform the Soaring Insignia into the Radiant Insignia if the player has one
				{
					foreach (Player plr in Main.player)
					{
						if (plr.dead || plr.ghost || !plr.active)
							continue;

						Item insignia = plr.inventory.FirstOrDefault(i => i.type == ItemID.EmpressFlightBooster);
						int aInsigType = -1;

						if (ModLoader.TryGetMod("CalamityMod", out Mod cal) && cal.TryFind("AscendantInsignia", out ModItem aInsig))
						{
							if (Terratweaks.Config.calamitweaks.RadiantInsigniaUpgradesFromAscendant)
							{
								aInsigType = aInsig.Type;
								insignia = plr.inventory.FirstOrDefault(i => i.type == aInsigType);
							}
						}

						if (insignia != default(Item)) // Make sure the player has a Soaring Insignia in their inventory to begin with
						{
							var oldPrefix = insignia.prefix;
							plr.TryDroppingSingleItem(new EntitySource_Loot(npc), insignia);
							RadiantInsigniaCutscene.AscendantInsigniaType = aInsigType;
							plr.GetModPlayer<CutscenePlayer>().inCutscene = true;
						}
						else // Player didn't have one in their inventory... Maybe they have one equipped?
						{
							insignia = plr.armor.FirstOrDefault(i => i.type == ItemID.EmpressFlightBooster);

							if (ModLoader.HasMod("CalamityMod") && Terratweaks.Config.calamitweaks.RadiantInsigniaUpgradesFromAscendant && aInsigType > -1)
							{
								insignia = plr.armor.FirstOrDefault(i => i.type == aInsigType);
							}

							if (insignia != default(Item))
							{
								var oldPrefix = insignia.prefix;
								plr.TryDroppingSingleItem(new EntitySource_Loot(npc), insignia);
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
			if (npc.townNPC && Terratweaks.Config.NPCDeathsToCallOffInvasion > 0 && Main.invasionType != InvasionID.None)
			{
				townNpcDeathsThisInvasion++;

				if (townNpcDeathsThisInvasion >= Terratweaks.Config.NPCDeathsToCallOffInvasion)
				{
					Main.invasionType = InvasionID.None;
					Color color = new(175, 75, 255);
					Main.NewText(Language.GetTextValue("Mods.Terratweaks.Common.InvasionFailNPC"), color);
				}
			}

			// Gold critter died- drop a random amount of gold within the configured range
			// This is intentionally not affected by the multipliers that normally increase enemy coin drop values
			// (which is also why I can't simply use npc.value)
			if (NPCID.Sets.GoldCrittersCollection.Contains(npc.type) && Terratweaks.Config.GoldCrittersDropGold)
			{
				int minGold = Terratweaks.Config.GoldCritterMinValue;
				int maxGold = Terratweaks.Config.GoldCritterMaxValue;

				int droppedGold = Main.rand.Next(minGold, maxGold + 1); // + 1 is needed since max is exclusive normally

				if (droppedGold > 0)
				{
					// Include a 50% chance to drop some silver coins instead of a gold one
					if (Main.rand.NextBool())
					{
						droppedGold -= 1; // Drop one less gold
						int droppedSilver = Main.rand.Next(1, 20) * 5; // Will drop 5-95 silver in a random multiple of 5

						// Drop the specified amount of silver
						Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ItemID.SilverCoin, droppedSilver);

						// Don't drop gold if the critter would've only dropped one gold and that one gold was converted into silver
						if (droppedGold <= 0)
							return;
					}

					// Drop the specified amount of gold, if at least 1 gold still needs to be dropped
					Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ItemID.GoldCoin, droppedGold);
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
				if (Terratweaks.Config.TerraprismaDropRate > 0 && (!ModLoader.HasMod("CalamityMod") || Terratweaks.Config.calamitweaks.RevertTerraprisma))
				{
					var nightEol = new TerratweaksDropConditions.NightEoL();
					npcLoot.Add(new ItemDropWithConditionRule(ItemID.EmpressBlade, 100, 1, 1, nightEol, Terratweaks.Config.TerraprismaDropRate));
				}
			}

			// All enemies from the celestial pillars need to have an option to drop fragments once Moon Lord has been downed
			if (Terratweaks.Config.PillarEnemiesDropFragments)
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

			// Make Lunatic Cultist drop the Gravity Globe for all players in Expert+
			// Note that this will apply even if a mod like Thorium is present that makes Cultist drop a treasure bag
			if (Terratweaks.Config.CultistGravGlobe && npc.type == NPCID.CultistBoss)
			{
				npcLoot.Add(ItemDropRule.BossBag(ItemID.GravityGlobe));
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

		public static readonly List<Condition> BiomeConditions = new()
		{
			// Sky, surface, and underground
			Condition.InSkyHeight,
			Condition.InSpace,
			Condition.InShoppingZoneForest,
			Condition.InOverworldHeight,
			Condition.InBelowSurface,
			Condition.InDirtLayerHeight,
			Condition.InRockLayerHeight,

			// Cave biomes
			Condition.InGlowshroom,
			Condition.InGranite,
			Condition.InMarble,
			Condition.InGemCave,
			Condition.InUnderworld,
			Condition.InUnderworldHeight,
			Condition.NotInUnderworld,

			// Surface biomes
			Condition.InSnow,
			Condition.InDesert,
			Condition.InJungle,
			Condition.InBeach,
			Condition.InCorrupt,
			Condition.InCrimson,
			Condition.InEvilBiome,
			Condition.NotInEvilBiome,
			Condition.InHallow,
			Condition.NotInHallow,

			// Underground biomes
			Condition.InUndergroundDesert,

			// Structures/man-made biomes
			Condition.InHive,
			Condition.InGraveyard,
			Condition.NotInGraveyard,
			Condition.InDungeon,
			Condition.InLihzhardTemple,
			Condition.InMeteor,

			// Special biomes
			Condition.InTowerSolar,
			Condition.InTowerVortex,
			Condition.InTowerNebula,
			Condition.InTowerStardust,
			Condition.InAether
		};

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
						Condition configEnabled = new("Mods.Terratweaks.Conditions.DyeConfigActive", () => Terratweaks.Config.DyeTraderShopExpansion);
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
				if (Terratweaks.Config.SoilSolutionsPreML)
				{
					shop.ActiveEntries.First(i => i.Item.type == ItemID.DirtSolution).Disable();
					shop.InsertAfter(ItemID.DirtSolution, ItemID.DirtSolution, Condition.NotRemixWorld);
					shop.ActiveEntries.First(i => i.Item.type == ItemID.SandSolution).Disable();
					shop.InsertAfter(ItemID.SandSolution, ItemID.SandSolution, Condition.NotRemixWorld);
					shop.ActiveEntries.First(i => i.Item.type == ItemID.SnowSolution).Disable();
					shop.InsertAfter(ItemID.SnowSolution, ItemID.SnowSolution, Condition.NotRemixWorld);
				}

				if (Terratweaks.Config.SolutionsOnGFB)
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
				if (Terratweaks.Config.SolutionsOnGFB)
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

				if (Terratweaks.Config.NPCsSellMinecarts)
				{
					shop.Add(ItemID.ShroomMinecart);
				}
			}

			if (shop.NpcType == NPCID.Dryad && Terratweaks.Config.NPCsSellMinecarts)
			{
				shop.Add(ItemID.SunflowerMinecart, Condition.HappyWindyDay);
				shop.Add(ItemID.LadybugMinecart, Condition.HappyWindyDay);
				shop.Add(ItemID.BeeMinecart, Condition.DownedQueenBee);
			}

			if (shop.NpcType == NPCID.Merchant && Terratweaks.Config.NPCsSellMinecarts)
			{
				shop.Add(ItemID.DesertMinecart, Condition.InDesert);
			}

			if (shop.NpcType == NPCID.Mechanic && Terratweaks.Config.MechanicSellsToolbox)
			{
				shop.Add(ItemID.Toolbox);
			}

			if (Terratweaks.Config.TownNPCsSellWeapons)
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

			if (Terratweaks.Config.NoBiomeRequirements)
			{
				// uuugh why does editing any sort of list have to be so annoying-
				Dictionary<NPCShop.Entry, Condition[]> newEntries = new();

				// Look through every item in the shop we're looking at
				// Since we don't check for any NPC type, this'll apply to every town NPC with a shop
				foreach (NPCShop.Entry entry in shop.Entries)
				{
					Item item = entry.Item;
					var conditions = entry.Conditions;

					// Skip all pylons- which thankfully have a very easy way to filter out
					// I totally didn't spend ages trying to figure out some hacky workarounds before realizing I could just do this :)
					if (TileID.Sets.CountsAsPylon.Contains(item.createTile))
						continue;

					// Skip any items which other mods have added to the blacklist
					if (TerratweaksContentSets.NoBiomeBlacklist[item.type])
						continue;

					// We need a list so that we can remove conditions freely (and the ToArray() and Clone() just ensures that we don't mess with the OG conditions list)
					var newConditions = ((Condition[])conditions.ToArray().Clone()).ToList();

					// Look through the list of conditions and add any conditions that check for a particular biome
					// Unfortunately, this has to be hardcoded in since there's no way to check the conditions easily
					foreach (Condition condition in conditions)
					{
						if (BiomeConditions.Contains(condition))
						{
							// Remove the condition from the new list, so that when we create our own entry later the condition won't be present
							newConditions.Remove(condition);
						}
					}

					// Once we've got an up-to-date list of conditions, we can replace the original entry with our own!
					// ...Well, not really because lists are dumb and stupid and you can't add/remove stuff from them while looping over them in a foreach-
					newEntries.Add(entry, newConditions.ToArray());
				}

				// me when "Collection was modified; enumeration operation may not execute"
				foreach (KeyValuePair<NPCShop.Entry, Condition[]> pair in newEntries)
				{
					var entry = pair.Key;
					var conditions = pair.Value;

					entry.Disable();
					shop.InsertAfter(entry, entry.Item, conditions);
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
			if (Terratweaks.Config.SmartMimics && IsAMimic(npc))
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
			if (Terratweaks.Config.SmartMimics && IsAMimic(npc))
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
			if (Terratweaks.Config.SmartMimics && IsAMimic(npc))
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
			if (Terratweaks.Config.SmartNymphs && npc.type == NPCID.LostGirl)
			{
				npc.friendly = true;
				npc.dontTakeDamageFromHostiles = true;
			}
		}

		public override bool PreAI(NPC npc)
		{
			// Stop vanilla Lost Girl AI from running, as we're going to be using custom behavior and don't want vanilla stuff to interfere
			if (Terratweaks.Config.SmartNymphs && npc.type == NPCID.LostGirl)
			{
				return false;
			}

			return base.PreAI(npc);
		}

		public override bool? CanChat(NPC npc)
		{
			if (Terratweaks.Config.SmartNymphs && npc.type == NPCID.LostGirl)
				return true;

			return base.CanChat(npc);
		}

		public override void PostAI(NPC npc)
		{
			if (Terratweaks.Config.SmartNymphs && npc.type == NPCID.LostGirl)
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

			if (Terratweaks.Config.BossesLowerSpawnRates > 0)
				spawnRate = (int)Math.Round(spawnRate * (1 / Terratweaks.Config.BossesLowerSpawnRates));

			maxSpawns = (int)Math.Round(maxSpawns * Terratweaks.Config.BossesLowerSpawnRates);
		}
	}

	public class BlockSpawns : GlobalNPC
	{
		public override void OnSpawn(NPC npc, IEntitySource source)
		{
			if (Terratweaks.Config.OldChestDungeon && npc.type == NPCID.BoundTownSlimeOld && source is EntitySource_SpawnNPC)
			{
				Tile tile = Main.tile[(int)Math.Round(npc.position.X / 16), (int)Math.Round(npc.position.Y / 16)];

				if (!Main.wallDungeon[tile.WallType])
				{
					npc.active = false;
				}
			}
		}

		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			if (Terratweaks.Config.EarlyLacewing)
			{
				if (MeetsVanillaLacewingConditions(spawnInfo) && Main.hardMode && !NPC.downedPlantBoss)
				{
					// 0.1 spawn weight basically means an early Lacewing spawn is 10x rarer than any vanilla NPC spawning
					// This makes it *approximately* a 1/11 chance, slightly rarer than the Lacewing's ~10% chance normally
					// It's close enough that I really can't be bothered to do the math to figure out a more accurate weight,
					// especially since NPC spawn code is an absolute freaking *mess* and I'm not even confident that the 10%
					// chance is actually fully accurate
					pool.Add(NPCID.EmpressButterfly, 0.1f);
				}
			}
		}

		/* Vanilla Lacewing conditions:
			* Must be between 7:30 PM - 12:00 AM in-game, unless in Don't Dig Up world
			* Only spawns in the surface Hallow biome
			* There must not be another Prismatic Lacewing somewhere in the world
			* Obviously normally Plantera must be defeated, but we're not checking that here so that we can spawn it early
		*/
		private static bool MeetsVanillaLacewingConditions(NPCSpawnInfo spawnInfo)
		{
			if ((!Main.dayTime && Main.time < 16200.0) || Main.remixWorld)
			{
				if (spawnInfo.SpawnTileY <= Main.worldSurface && spawnInfo.Player.ZoneHallow)
				{
					if (!NPC.AnyNPCs(NPCID.EmpressButterfly))
						return true;
				}
			}

			return false;
		}
	}
}