using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terratweaks.Buffs;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks.Items
{
	// Anything that's done specifically for cross-mod compatibility can be done with this GlobalItem
	public class ModCompatChanges : GlobalItem
	{
		public override bool CanUseItem(Item item, Player player)
		{
			// Check if Calamity is enabled since Calamity already locks sandstorms progression-wise
			bool progressionLockedSandstorms = GetInstance<TerratweaksConfig>().PostEyeSandstorms && !ModLoader.HasMod("CalamityMod");

			if (ModLoader.TryGetMod("Fargowiltas", out Mod _) && TryFind("Fargowiltas", "ForbiddenScarab", out ModItem scarab))
			{
				if (item.type == scarab.Type && progressionLockedSandstorms)
				{
					if (!NPC.downedBoss1)
					{
						Color color = new(175, 75, 255);
						Main.NewText(Language.GetTextValue("Mods.Terratweaks.Common.SandstormFail"), color);
						return false;
					}
					return !Sandstorm.Happening;
				}
			}

			return base.CanUseItem(item, player);
		}
	}

	// Any tooltips that are affected by mod configs goes here
	public class TooltipChanges : GlobalItem
	{
		public override bool InstancePerEntity => true;
		public int hitsDone = 0;

		public static readonly Dictionary<int, Func<Player, bool>> PermBuffBools = new()
		{
			{ ItemID.DemonHeart, (Player p) => p.extraAccessory },
			{ ItemID.CombatBook, (Player p) => NPC.combatBookWasUsed },
			{ ItemID.ArtisanLoaf, (Player p) => p.ateArtisanBread },
			{ ItemID.TorchGodsFavor, (Player p) => p.unlockedBiomeTorches },
			{ ItemID.AegisCrystal, (Player p) => p.usedAegisCrystal },
			{ ItemID.AegisFruit, (Player p) => p.usedAegisFruit },
			{ ItemID.ArcaneCrystal, (Player p) => p.usedArcaneCrystal },
			{ ItemID.Ambrosia, (Player p) => p.usedAmbrosia },
			{ ItemID.GummyWorm, (Player p) => p.usedGummyWorm },
			{ ItemID.GalaxyPearl, (Player p) => p.usedGalaxyPearl },
			{ ItemID.CombatBookVolumeTwo, (Player p) => NPC.combatBookVolumeTwoWasUsed },
			{ ItemID.PeddlersSatchel, (Player p) => NPC.peddlersSatchelWasUsed },
			{ ItemID.MinecartPowerup, (Player p) => p.unlockedSuperCart }
		};

		public static readonly Dictionary<int, Func<Player, Vector2>> MultiPermBuffs = new()
		{
			{ ItemID.LifeCrystal, (Player p) => new Vector2(p.ConsumedLifeCrystals, 15) },
			{ ItemID.ManaCrystal, (Player p) => new Vector2(p.ConsumedManaCrystals, 9) },
			{ ItemID.LifeFruit, (Player p) => new Vector2(p.ConsumedLifeFruit, 20) }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_Hardmode = new()
		{
			{ ItemID.BoneHelm, () => GetInstance<TerratweaksConfig>().expertAccBuffs.BoneHelm }
		};

		// TODO: Add configs for items that scale at other progression points too
		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_QS = new()
		{
			{ ItemID.RoyalGel, () => GetInstance<TerratweaksConfig>().expertAccBuffs.RoyalGel }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_Mechs = new() { };

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_Plant = new()
		{
			{ ItemID.HiveBackpack, () => GetInstance<TerratweaksConfig>().expertAccBuffs.HivePack }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_ML = new() { };

		public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (GetInstance<TerratweaksConfig>().NoDamageVariance == DamageVarianceSetting.Limited)
				modifiers.DamageVariationScale *= 0;

			if (GetInstance<TerratweaksConfig_Client>().NoRandomCrit)
			{
				hitsDone += player.GetWeaponCrit(item);

				if (hitsDone >= 100)
				{
					modifiers.SetCrit();
					hitsDone = 0;

					if (GetInstance<TerratweaksConfig>().CritsBypassDefense)
						modifiers.DefenseEffectiveness *= 0;
				}
				else
					modifiers.DisableCrit();
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			TerratweaksConfig_Client clientConfig = GetInstance<TerratweaksConfig_Client>();
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();
			
			int idx = -1;
			int velIdx = -1;

			if (clientConfig.StatsInTip)
			{
				foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && (t.Name == "Knockback" || t.Name == "Speed")))
				{
					if (tooltip.Name == "Knockback")
						idx = tooltips.IndexOf(tooltip);

					for (int i = 6; i < 22; i++) // LegacyTooltip.6 is "insanely fast speed", LegacyTooltip.22 is "insane knockback"
					{
						string text = Language.GetTextValue($"LegacyTooltip.{i}");

						if (tooltip.Text == text)
							tooltip.Text = Language.GetTextValue($"Mods.Terratweaks.LegacyTooltip.{i}", i < 14 ? item.useAnimation : item.knockBack);
					}
				}

				if (item.shoot > -1 && item.shootSpeed > 0 && idx != -1)
				{
					float realShootSpeed = item.shootSpeed * (ContentSamples.ProjectilesByType[item.shoot].extraUpdates + 1);
					tooltips.Insert(idx + 1, new TooltipLine(Mod, "Velocity", Language.GetTextValue("Mods.Terratweaks.LegacyTooltip.V", realShootSpeed)));
					velIdx = idx + 1;
				}
			}

			if (clientConfig.EstimatedDPS)
			{
				idx = -1;

				foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && (t.Name.Equals("Knockback") || t.Name.Equals("Speed") || t.Name.Contains("Tooltip") || t.Name.Contains("Power"))))
				{
					idx = tooltips.IndexOf(tooltip);
				}

				if (velIdx != -1 && velIdx > idx)
					idx = velIdx;

				if (Main.LocalPlayer.accDreamCatcher && idx != -1)
				{
					float dps = (float)Math.Round(item.damage * ((float)item.useAnimation / item.useTime) * (60.0f / item.useAnimation), 2);
					TooltipLine dpsLine = new(Mod, "EstDps", Language.GetTextValue("Mods.Terratweaks.Common.EstDPS", dps));

					if (item.damage > 0 && !item.accessory && !item.IsACoin)
					{
						tooltips.Insert(idx + 1, dpsLine);
					}
				}
			}

			if (clientConfig.NoRandomCrit)
			{
				foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Name == "CritChance"))
					tooltip.Text = Language.GetTextValue("Mods.Terratweaks.LegacyTooltip.C", Main.LocalPlayer.GetWeaponCrit(item));
			}

			if (config.SIRework && !ModLoader.HasMod("CalamityMod"))
			{
				if (item.type == ItemID.EmpressFlightBooster)
				{
					List<TooltipLine> removeList = new();

					foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Name.Contains("Tooltip")))
					{
						if (tooltip.Name == "Tooltip0")
							tooltip.Text = Language.GetTextValue("Mods.Terratweaks.Common.ReworkedSITip");
						else
							removeList.Add(tooltip);
					}

					foreach (TooltipLine tooltip in removeList)
						tooltips.Remove(tooltip);
				}
			}

			if (config.UmbrellaHatRework)
			{
				if (item.type == ItemID.UmbrellaHat)
				{
					idx = -1;

					foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria"))
					{
						if (tooltip.Name == "Equipable")
							idx = tooltips.IndexOf(tooltip);
					}

					if (idx != -1)
					{
						TooltipLine line = new(Mod, "UmbrellaHatTip", Language.GetTextValue("Mods.Terratweaks.Common.ReworkedUmbrellaHatTip"));
						tooltips.Insert(idx + 1, line);
					}
				}
			}

			if (config.DeerWeaponsRework)
			{
				if (item.type == ItemID.LucyTheAxe)
				{
					idx = -1;

					foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria"))
					{
						if (tooltip.Name == "Tooltip0")
							idx = tooltips.IndexOf(tooltip);
					}

					if (idx != -1)
					{
						tooltips[idx] = new(Mod, "Tooltip0", Language.GetTextValue("Mods.Terratweaks.Common.ReworkedLucyTip", 15, 10, 25));
					}
				}
			}

			if (config.DyeTraderShopExpansion && Terratweaks.DyeItemsSoldByTrader.Contains(item.type))
			{
				idx = -1;

				foreach (TooltipLine line in tooltips)
				{
					if (line.Name == "ItemName" || line.Name == "Material" || line.Name == "Tooltip0")
						idx = tooltips.IndexOf(line);
				}

				if (idx > -1)
					tooltips.Insert(idx + 1, new TooltipLine(Mod, "SoldByDyeTrader", Language.GetTextValue("Mods.Terratweaks.Common.SoldByDyeTrader")) { OverrideColor = Color.SlateBlue });
			}

			// Find the last tooltip line that describes the item's effects (i.e, no flavor text)
			idx = -1;

			foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Name.Contains("Tooltip")))
			{
				if (!tooltip.Text.StartsWith("'"))
					idx = tooltips.IndexOf(tooltip);
			}

			// Add a line to the DPS Meter
			if (clientConfig.EstimatedDPS && idx != -1)
			{
				if (item.type == ItemID.DPSMeter || item.type == ItemID.GoblinTech)
				{
					tooltips.Insert(idx + 1, new TooltipLine(Mod, "DpsMeterExtraTip", Language.GetTextValue("Mods.Terratweaks.Common.DpsMeterExtraTip")));
				}
			}

			// Add a line to consumed permanent buffs (doesn't support modded ones by default, requires mod call)
			if (clientConfig.PermBuffTips && idx != -1)
			{
				// Permanent buffs that can be consumed multiple times use special logic, listing the number consumed instead of if they have/haven't been consumed
				if (MultiPermBuffs.ContainsKey(item.type) && MultiPermBuffs.TryGetValue(item.type, out Func<Player, Vector2> values))
				{
					int numConsumed = (int)values(Main.LocalPlayer).X;
					int max = (int)values(Main.LocalPlayer).Y;

					TooltipLine line = new(Mod, "PermBuffTip", Language.GetTextValue("Mods.Terratweaks.Common.AmtConsumed", item.Name, numConsumed, max))
					{
						OverrideColor = new Color(138, 138, 138)
					};
					tooltips.Insert(idx + 1, line);
				}
				// List if an item has been consumed for permanent buffs
				else
				{
					if (PermBuffBools.ContainsKey(item.type) && PermBuffBools.TryGetValue(item.type, out Func<Player, bool> hasUsedItem))
					{
						if (hasUsedItem(Main.LocalPlayer))
						{
							TooltipLine line = new(Mod, "PermBuffTip", Language.GetTextValue("Mods.Terratweaks.Common.PermBuffTip", item.Name))
							{
								OverrideColor = new Color(138, 138, 138)
							};
							tooltips.Insert(idx + 1, line);
						}
					}
				}
			}

			// Add a line to summon weapons explaining the cooldown
			if (config.ManaFreeSummoner && idx != -1 && item.CountsAsClass(DamageClass.Summon) && ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj))
			{
				if (proj.minion || proj.sentry)
				{
					TooltipLine line = new(Mod, "SummonCooldown", Language.GetTextValue("Mods.Terratweaks.Common.SummonCooldown"))
					{
						OverrideColor = ItemRarity.GetColor(ItemRarityID.LightRed)
					};
					tooltips.Insert(idx + 1, line);
				}
			}

			// Add/replace some lines for accessories with removed diminishing returns
			if (config.NoDiminishingReturns && idx != -1)
			{
				// Replace tooltip0 and tooltip1 on most accessories
				foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Name.Contains("Tooltip")))
				{
					// Tooltip0 - Only edited by Avenger and Destroyer Emblem
					if (tooltip.Name.Equals("Tooltip0"))
					{
						if (item.type == ItemID.AvengerEmblem || item.type == ItemID.DestroyerEmblem) // 15% increased damage
							tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedDamage", 15);
					}
					// Tooltip1 - Edited by literally everything except Avenger Emblem, Stalker's Quiver, and Arcane Flower
					else if (tooltip.Name.Equals("Tooltip1"))
					{
						if (item.type == ItemID.LightningBoots || item.type == ItemID.FrostsparkBoots || item.type == ItemID.TerrasparkBoots) // 15% increased movement speed
							tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedMovementSpeed", 15);
						else if (item.type == ItemID.DestroyerEmblem) // 10% increased crit chance
							tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedCritChance", 10);
						else if (item.type == ItemID.ReconScope) // 20% increased ranged damage and crit chance
							tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedRangedDamageCritChance", 20);
					}
				}

				// Add lines for Stalker's Quiver and Arcane Flower; doesn't matter if we do this before or after,
				// I'm just doing it after for organization purposes
				if (item.type == ItemID.StalkersQuiver || item.type == ItemID.ArcaneFlower)
				{
					TooltipLine line = new(Mod, "Tooltip1", "");

					if (item.type == ItemID.StalkersQuiver)
						line.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedRangedDamageCritChance", 10);
					else
						line.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedMagicDamageCritChance", 10);

					tooltips.Insert(idx + 1, line);
				}
			}

			// Add an extra line (or multiple) for Expert accessories that need to scale with progression
			if (idx != -1)
			{
				int numLines = 0;

				// Count the number of lines that need to be displayed while the player is holding shift
				if (ExpertItemsThatScale_Hardmode.ContainsKey(item.type) && ExpertItemsThatScale_Hardmode.TryGetValue(item.type, out Func<bool> configEnabled) && configEnabled())
					numLines++;
				if (ExpertItemsThatScale_QS.ContainsKey(item.type) && ExpertItemsThatScale_QS.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;
				if (ExpertItemsThatScale_Mechs.ContainsKey(item.type) && ExpertItemsThatScale_Mechs.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;
				if (ExpertItemsThatScale_Plant.ContainsKey(item.type) && ExpertItemsThatScale_Plant.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;
				if (ExpertItemsThatScale_ML.ContainsKey(item.type) && ExpertItemsThatScale_ML.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;

				bool addedMoonlord = false;
				bool addedPlant = false;
				bool addedMechs = false;
				bool addedQS = false;
				bool addedHM = false;

				// Player is holding shift, display lines for each relevant boss
				if (ItemSlot.ShiftInUse)
				{
					for (int i = 1; i <= numLines; i++)
					{
						TooltipLine line = new(Mod, $"ProgressionStatBoost{i - 1}", "")
						{
							OverrideColor = ItemRarity.GetColor(ItemRarityID.LightRed)
						};

						if (!addedMoonlord && ExpertItemsThatScale_ML.TryGetValue(item.type, out configEnabled) && configEnabled())
						{
							line.Text = Language.GetTextValue("Mods.Terratweaks.Common.StrongerPostML");
							addedMoonlord = true;

							if (NPC.downedMoonlord)
								line.OverrideColor = ItemRarity.GetColor(ItemRarityID.Lime);

							tooltips.Insert(idx + 1, line);
							continue;
						}

						if (!addedPlant && ExpertItemsThatScale_Plant.TryGetValue(item.type, out configEnabled) && configEnabled())
						{
							line.Text = Language.GetTextValue("Mods.Terratweaks.Common.StrongerPostPlant");
							addedPlant = true;

							if (NPC.downedPlantBoss)
								line.OverrideColor = ItemRarity.GetColor(ItemRarityID.Lime);

							tooltips.Insert(idx + 1, line);
							continue;
						}

						if (!addedMechs && ExpertItemsThatScale_Mechs.TryGetValue(item.type, out configEnabled) && configEnabled())
						{
							line.Text = Language.GetTextValue("Mods.Terratweaks.Common.StrongerPostMechs");
							addedMechs = true;

							if (DownedMechBossAll())
								line.OverrideColor = ItemRarity.GetColor(ItemRarityID.Lime);

							tooltips.Insert(idx + 1, line);
							continue;
						}

						if (!addedQS && ExpertItemsThatScale_QS.TryGetValue(item.type, out configEnabled) && configEnabled())
						{
							line.Text = Language.GetTextValue("Mods.Terratweaks.Common.StrongerPostQS");
							addedQS = true;

							if (NPC.downedQueenSlime)
								line.OverrideColor = ItemRarity.GetColor(ItemRarityID.Lime);

							tooltips.Insert(idx + 1, line);
							continue;
						}

						if (!addedHM && ExpertItemsThatScale_Hardmode.TryGetValue(item.type, out configEnabled) && configEnabled())
						{
							line.Text = Language.GetTextValue("Mods.Terratweaks.Common.StrongerInHM");
							addedHM = true;

							if (Main.hardMode)
								line.OverrideColor = ItemRarity.GetColor(ItemRarityID.Lime);

							tooltips.Insert(idx + 1, line);
							continue;
						}
					}
				}
				// Player is not holding shift, display a tooltip telling them to hold shift
				// Only displays for items that have at least one line to display
				else if (numLines > 0)
				{
					TooltipLine line = new(Mod, "ProgressionStatBoost0", Language.GetTextValue("Mods.Terratweaks.Common.StrongerHoldShift"))
					{
						OverrideColor = ItemRarity.GetColor(ItemRarityID.LightRed)
					};

					tooltips.Insert(idx + 1, line);
				}
			}
		}

		static bool DownedMechBossAll()
		{
			return NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
		}
	}

	// Any changes to crates, boss treasure bags, or other grab bag items go here
	[JITWhenModsEnabled("CalamityMod")]
	public class GrabBagChanges : GlobalItem
	{
		public override bool InstancePerEntity => true;

		private static readonly List<int> pyramidAccs = new() { ItemID.SandstorminaBottle, ItemID.FlyingCarpet };

		// Oasis Crates drop random pyramid loot
		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			// Update Oasis/Mirage Crate loot tables
			if (GetInstance<TerratweaksConfig>().OasisCrateBuff && (item.type == ItemID.OasisCrate || item.type == ItemID.OasisCrateHard))
			{
				var rules = itemLoot.Get();
				var mainRule = (AlwaysAtleastOneSuccessDropRule)rules.FirstOrDefault(x => x is AlwaysAtleastOneSuccessDropRule);
				var rule = (OneFromOptionsNotScaledWithLuckDropRule)mainRule.rules.FirstOrDefault(x => x is OneFromOptionsNotScaledWithLuckDropRule r && r.dropIds.Contains(ItemID.SandBoots));
				var drops = rule?.dropIds ?? Array.Empty<int>();
				List<int> vanillaItems = drops.ToList();

				// Has a chance to drop one of the Sandstone Chest items, or one of the accessories from the pyramid (but never drops both at once)
				// There is a 1/10 chance to drop a pyramid item, but if that fails it's guaranteed to drop a regular crate item instead, making those much more common
				var pharaohsCurse = new CommonDrop(ItemID.PharaohsMask, 1);
				pharaohsCurse.OnSuccess(new CommonDrop(ItemID.PharaohsRobe, 1)); // This is done to make the Pharaoh's Robe drop alongside the mask

				OneFromRulesRule newRule = new(1,
					new OneFromRulesRule(10,
						new IItemDropRule[]
						{
							new OneFromOptionsNotScaledWithLuckDropRule(1, 1, pyramidAccs.ToArray()),
							pharaohsCurse
						}
					).OnFailedRoll(new OneFromOptionsNotScaledWithLuckDropRule(1, 1, vanillaItems.ToArray()))
				);

				// Update the main rule to be our new one
				var newMainRuleList = mainRule.rules.ToList();
				newMainRuleList.Remove(rule);
				newMainRuleList.Add(newRule);

				var newMainRule = mainRule;
				newMainRule.rules = newMainRuleList.ToArray();

				itemLoot.Remove(mainRule);
				itemLoot.Add(newMainRule);
			}

			if (GetInstance<TerratweaksConfig>().calamitweaks.RevertTerraprisma && ModLoader.HasMod("CalamityMod") && item.type == ItemID.FairyQueenBossBag)
			{
				foreach (IItemDropRule rule in itemLoot.Get(false))
				{
					HandleCalamityEoLChanges(rule);
				}
			}
		}

		void HandleCalamityEoLChanges(IItemDropRule rule)
		{
			if (rule is CalamityMod.DropHelper.AllOptionsAtOnceWithPityDropRule pityRule)
			{
				CalamityMod.WeightedItemStack stackToRemove = new();
				bool foundTerraprisma = false;

				foreach (CalamityMod.WeightedItemStack stack in pityRule.stacks)
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
					List<CalamityMod.WeightedItemStack> stacksList = pityRule.stacks.ToList();
					stacksList.Remove(stackToRemove);
					pityRule.stacks = stacksList.ToArray();
				}
			}
		}
	}

	// This GlobalItem is used for any accessory reworks, like making the DD2 accessories stack
	public class AccChanges : GlobalItem
	{
		public static readonly List<int> DD2Accs = new()
		{
			ItemID.ApprenticeScarf,
			ItemID.HuntressBuckler,
			ItemID.MonkBelt,
			ItemID.SquireShield
		};

		public override void SetDefaults(Item item)
		{
			var config = GetInstance<TerratweaksConfig>();

			if (item.type == ItemID.UmbrellaHat && config.UmbrellaHatRework)
			{
				item.vanity = false;
				item.accessory = true;
				item.headSlot = -1;
			}
		}

		public override void UpdateVanity(Item item, Player player)
		{
			var config = GetInstance<TerratweaksConfig>();

			if (item.type == ItemID.UmbrellaHat && config.UmbrellaHatRework)
			{
				player.GetModPlayer<InputPlayer>().umbrellaHatVanity = true;
			}
		}

		public override void UpdateAccessory(Item item, Player player, bool hideVisual)
		{
			var config = GetInstance<TerratweaksConfig>();
			SentryAccSetting dd2AccsStack = config.StackableDD2Accs;

			if (item.type == ItemID.UmbrellaHat && config.UmbrellaHatRework)
			{
				player.noFallDmg = true;
				player.GetModPlayer<InputPlayer>().umbrellaHat = true;
				player.GetModPlayer<InputPlayer>().umbrellaHatVanity = !hideVisual;
			}
			
			if (dd2AccsStack == SentryAccSetting.Limited)
			{
				if (item.type == ItemID.ApprenticeScarf || item.type == ItemID.SquireShield)
					player.dd2Accessory = true;

				if (item.type == ItemID.MonkBelt || item.type == ItemID.HuntressBuckler)
				{
					player.dd2Accessory = false;
					player.GetModPlayer<TerratweaksPlayer>().dd2Accessory2 = true;
				}
			}

			if (DD2Accs.Contains(item.type) && dd2AccsStack == SentryAccSetting.On)
			{
				player.dd2Accessory = false;
				player.maxTurrets += 1;
				player.GetDamage(DamageClass.Summon) += 0.1f;
			}

			if (config.NoDiminishingReturns)
			{
				// Boots need to give +15% movement speed, but they currently only give +8%- so increase it by 7%!
				if (item.type == ItemID.LightningBoots || item.type == ItemID.FrostsparkBoots || item.type == ItemID.TerrasparkBoots)
					player.moveSpeed += 0.07f;
				// Avenger Emblem needs to give 3% more damage to get up to +15%
				else if (item.type == ItemID.AvengerEmblem)
					player.GetDamage(DamageClass.Generic) += 0.03f;
				// Destroyer Emblem needs 5% more damage and 2% more crit chance
				else if (item.type == ItemID.DestroyerEmblem)
				{
					player.GetDamage(DamageClass.Generic) += 0.05f;
					player.GetCritChance(DamageClass.Generic) += 2;
				}
				// Recon Scope and Stalker's Quiver both need 10% more ranged damage and crit
				else if (item.type == ItemID.ReconScope || item.type == ItemID.StalkersQuiver)
				{
					player.GetDamage(DamageClass.Ranged) += 0.1f;
					player.GetCritChance(DamageClass.Ranged) += 10;
				}
				// Arcane Flower needs 10% more magic damage and crit
				else if (item.type == ItemID.ArcaneFlower)
				{
					player.GetDamage(DamageClass.Magic) += 0.1f;
					player.GetCritChance(DamageClass.Magic) += 10;
				}
			}

			if (item.type == ItemID.EmpressFlightBooster && config.SIRework && !ModLoader.HasMod("CalamityMod"))
			{
				// Disable vanilla SI effects
				player.empressBrooch = false;
				player.moveSpeed -= 0.1f;

				// New effects: +25% wing flight time, +10% jump speed, +10% acceleration
				player.wingTimeMax = (int)Math.Round(player.wingTimeMax * 1.25f);
				player.jumpSpeedBoost += 0.5f;
				player.runAcceleration *= 1.1f;
			}
		}
	}

	// Contains all item changes, like tools or weapons
	public class ItemChanges : GlobalItem
	{
		public static readonly List<int> IgnoredSummonWeapons = new();

		public override void SetDefaults(Item item)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			if (config.OreUnification)
			{
				switch (item.type)
				{
					#region Copper/Tin
					case ItemID.CopperShortsword:
						item.CloneDefaults(ItemID.TinShortsword);
						break;
					case ItemID.CopperBroadsword:
						item.CloneDefaults(ItemID.TinBroadsword);
						break;
					case ItemID.CopperBow:
						item.CloneDefaults(ItemID.TinBow);
						break;
					case ItemID.AmethystStaff:
						item.CloneDefaults(ItemID.TopazStaff);
						break;
					case ItemID.CopperHelmet:
						item.CloneDefaults(ItemID.TinHelmet);
						item.headSlot = ArmorIDs.Head.CopperHelmet;
						break;
					case ItemID.CopperChainmail:
						item.CloneDefaults(ItemID.TinChainmail);
						item.bodySlot = ArmorIDs.Body.CopperChainmail;
						break;
					case ItemID.CopperGreaves:
						item.CloneDefaults(ItemID.TinGreaves);
						item.legSlot = ArmorIDs.Legs.CopperGreaves;
						break;
					case ItemID.CopperAxe:
						item.CloneDefaults(ItemID.TinAxe);
						break;
					case ItemID.CopperHammer:
						item.CloneDefaults(ItemID.TinHammer);
						item.hammer = 40;
						break;
					case ItemID.TinHammer:
						item.hammer = 40;
						break;
					case ItemID.CopperPickaxe:
						item.CloneDefaults(ItemID.TinPickaxe);
						break;
					#endregion
					#region Iron/Lead
					case ItemID.IronShortsword:
						item.CloneDefaults(ItemID.LeadShortsword);
						break;
					case ItemID.IronBroadsword:
						item.CloneDefaults(ItemID.LeadBroadsword);
						break;
					case ItemID.IronBow:
						item.CloneDefaults(ItemID.LeadBow);
						break;
					case ItemID.IronHelmet:
						item.CloneDefaults(ItemID.LeadHelmet);
						item.headSlot = ArmorIDs.Head.IronHelmet;
						break;
					case ItemID.AncientIronHelmet:
						item.CloneDefaults(ItemID.LeadHelmet);
						item.headSlot = ArmorIDs.Head.AncientIronHelmet;
						break;
					case ItemID.IronChainmail:
						item.CloneDefaults(ItemID.LeadChainmail);
						item.bodySlot = ArmorIDs.Body.IronChainmail;
						break;
					case ItemID.IronGreaves:
						item.CloneDefaults(ItemID.LeadGreaves);
						item.legSlot = ArmorIDs.Legs.IronGreaves;
						break;
					case ItemID.IronAxe:
						item.CloneDefaults(ItemID.LeadAxe);
						break;
					case ItemID.IronHammer:
						item.CloneDefaults(ItemID.LeadHammer);
						item.hammer = 45;
						break;
					case ItemID.LeadHammer:
						item.hammer = 45;
						break;
					case ItemID.IronPickaxe:
						item.CloneDefaults(ItemID.LeadPickaxe);
						item.pick = 45;
						break;
					case ItemID.LeadPickaxe:
						item.pick = 45;
						break;
					#endregion
					#region Silver/Tungsten
					case ItemID.SilverShortsword:
						item.CloneDefaults(ItemID.TungstenShortsword);
						break;
					case ItemID.SilverBroadsword:
						item.CloneDefaults(ItemID.TungstenBroadsword);
						break;
					case ItemID.SilverBow:
						item.CloneDefaults(ItemID.TungstenBow);
						break;
					case ItemID.SapphireStaff:
						item.CloneDefaults(ItemID.EmeraldStaff);
						break;
					case ItemID.SilverHelmet:
						item.CloneDefaults(ItemID.TungstenHelmet);
						item.headSlot = ArmorIDs.Head.SilverHelmet;
						break;
					case ItemID.SilverChainmail:
						item.CloneDefaults(ItemID.TungstenChainmail);
						item.bodySlot = ArmorIDs.Body.SilverChainmail;
						break;
					case ItemID.SilverGreaves:
						item.CloneDefaults(ItemID.TungstenGreaves);
						item.legSlot = ArmorIDs.Legs.SilverGreaves;
						break;
					case ItemID.SilverAxe:
						item.CloneDefaults(ItemID.TungstenAxe);
						break;
					case ItemID.SilverHammer:
						item.CloneDefaults(ItemID.TungstenHammer);
						break;
					case ItemID.SilverPickaxe:
						item.CloneDefaults(ItemID.TungstenPickaxe);
						break;
					#endregion
					#region Gold/Platinum
					case ItemID.GoldShortsword:
						item.CloneDefaults(ItemID.PlatinumShortsword);
						break;
					case ItemID.GoldBroadsword:
						item.CloneDefaults(ItemID.PlatinumBroadsword);
						break;
					case ItemID.GoldBow:
						item.CloneDefaults(ItemID.PlatinumBow);
						break;
					case ItemID.RubyStaff:
						item.CloneDefaults(ItemID.DiamondStaff);
						break;
					case ItemID.GoldHelmet:
						item.CloneDefaults(ItemID.PlatinumHelmet);
						item.headSlot = ArmorIDs.Head.GoldHelmet;
						break;
					case ItemID.AncientGoldHelmet:
						item.CloneDefaults(ItemID.PlatinumHelmet);
						item.headSlot = ArmorIDs.Head.AncientGoldHelmet;
						break;
					case ItemID.GoldChainmail:
						item.CloneDefaults(ItemID.PlatinumChainmail);
						item.bodySlot = ArmorIDs.Body.GoldChainmail;
						break;
					case ItemID.GoldGreaves:
						item.CloneDefaults(ItemID.PlatinumGreaves);
						item.legSlot = ArmorIDs.Legs.GoldGreaves;
						break;
					case ItemID.GoldAxe:
						item.CloneDefaults(ItemID.PlatinumAxe);
						break;
					case ItemID.GoldHammer:
						item.CloneDefaults(ItemID.PlatinumHammer);
						item.hammer = 60;
						break;
					case ItemID.PlatinumHammer:
						item.hammer = 60;
						break;
					case ItemID.GoldPickaxe:
						item.CloneDefaults(ItemID.PlatinumPickaxe);
						item.pick = 60;
						break;
					case ItemID.PlatinumPickaxe:
						item.pick = 60;
						break;
					#endregion
					case ItemID.CactusPickaxe:
						item.tileBoost = -1;
						break;
					case ItemID.ReaverShark:
						item.pick = 60;
						break;
				}
			}

			if (config.ManaFreeSummoner && !IgnoredSummonWeapons.Contains(item.type) && item.CountsAsClass(DamageClass.Summon) && ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj))
			{
				if (proj.minion || proj.sentry)
				{
					item.mana = 0;
				}
			}

			if (config.DeerWeaponsRework && item.type == ItemID.PewMaticHorn)
			{
				item.useTime = item.useAnimation = 10;
			}
		}

		public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();
			Player player = Main.LocalPlayer;

			// Don't do anything if we aren't in a world
			if (Main.gameMenu)
				return;

			if (config.ManaFreeSummoner && !IgnoredSummonWeapons.Contains(item.type) && item.CountsAsClass(DamageClass.Summon) && ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj))
			{
				if (proj.minion || proj.sentry)
				{
					if (player.GetModPlayer<TerratweaksPlayer>().summonsDisabled)
					{
						int buffIdx = player.FindBuffIndex(BuffType<SummonsDisabled>());
						
						if (buffIdx > -1)
						{
							// Draw an X over disabled items, in much the same way as healing potions
							Texture2D value = TextureAssets.InventoryBack.Value;
							Texture2D texture = TextureAssets.Cd.Value;
							float inventoryScale = Main.inventoryScale;

							Color color = Color.White;
							if (drawColor != Color.Transparent)
							{
								color = drawColor;
							}

							Vector2 vector = value.Size() * inventoryScale;
							Vector2 originalPosition = position - (vector / 2f);

							Vector2 position2 = originalPosition + value.Size() * inventoryScale / 2f - texture.Size() * inventoryScale / 2f;
							Color color3 = item.GetAlpha(color) * (player.buffTime[buffIdx] / (float)Conversions.ToFrames(3));
							spriteBatch.Draw(texture, position2, null, color3, 0f, default, 1.0f, SpriteEffects.None, 0f);
						}
					}
				}
			}
		}

		public override bool CanUseItem(Item item, Player player)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			if (config.ManaFreeSummoner && !IgnoredSummonWeapons.Contains(item.type) && item.CountsAsClass(DamageClass.Summon))
			{
				Projectile proj = ContentSamples.ProjectilesByType[item.shoot];

				if (proj.minion || proj.sentry)
				{
					if (player.GetModPlayer<TerratweaksPlayer>().summonsDisabled)
					{
						return false;
					}
				}
			}

			return base.CanUseItem(item, player);
		}

		public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			if (config.ManaFreeSummoner && !IgnoredSummonWeapons.Contains(item.type) && item.CountsAsClass(DamageClass.Summon))
			{
				Projectile proj = ContentSamples.ProjectilesByType[item.shoot];

				if (proj.minion || proj.sentry)
				{
					if (/*player.GetModPlayer<CombatPlayer>().IsInCombat()*/ Main.CurrentFrameFlags.AnyActiveBossNPC || AnyBossExists(true))
					{
						player.AddBuff(BuffType<SummonsDisabled>(), Conversions.ToFrames(3));
					}
				}
			}

			return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
		}

		private static bool AnyBossExists(bool ignorePillars = false)
		{
			bool flag = false;

			foreach (NPC npc in Main.npc)
			{
				if (!npc.active || (!npc.boss && !NPCID.Sets.DangerThatPreventsOtherDangers[npc.type]))
					continue;

				// Don't count pillars if told not to
				if (ignorePillars && (npc.type == NPCID.LunarTowerNebula || npc.type == NPCID.LunarTowerSolar || npc.type == NPCID.LunarTowerVortex || npc.type == NPCID.LunarTowerStardust))
					continue;

				flag = true;
				break;
			}

			return flag;
		}
	}

	// Contains all armor changes, usually in the form of new set bonuses or updated effects
	public class ArmorChanges : GlobalItem
	{
		public static readonly List<int> CobaltHeads = new()
		{
			ItemID.CobaltHat,
			ItemID.CobaltHelmet,
			ItemID.CobaltMask
		};

		public static readonly List<int> MythrilHeads = new()
		{
			ItemID.MythrilHat,
			ItemID.MythrilHelmet,
			ItemID.MythrilHood
		};

		public static readonly List<int> AdamantiteHeads = new()
		{
			ItemID.AdamantiteHeadgear,
			ItemID.AdamantiteHelmet,
			ItemID.AdamantiteMask
		};

		public override void UpdateEquip(Item item, Player player)
		{
			ArmorReworks armorToggles = GetInstance<TerratweaksConfig>().armorBonuses;

			switch (item.type)
			{
				//Spider armor
				case ItemID.SpiderMask:
				case ItemID.SpiderGreaves:
					if (armorToggles.Spider)
						player.GetDamage(DamageClass.Summon) += 0.02f;
					break;
				case ItemID.SpiderBreastplate:
					if (armorToggles.Spider)
					{
						player.GetDamage(DamageClass.Summon) += 0.02f;
						player.maxTurrets += 1;
					}
					break;

				//Spooky armor
				case ItemID.SpookyHelmet:
				case ItemID.SpookyBreastplate:
				case ItemID.SpookyLeggings:
					if (armorToggles.Spooky)
						player.GetDamage(DamageClass.Summon) += 0.06f;
					break;
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			ArmorReworks armorToggles = GetInstance<TerratweaksConfig>().armorBonuses;

			foreach (TooltipLine tooltip in tooltips)
			{
				if (tooltip.Name == "Tooltip1")
				{
					switch (item.type)
					{
						case ItemID.SpiderMask:
							if (armorToggles.Spider)
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 7);
							break;
						case ItemID.SpiderBreastplate:
							if (armorToggles.Spider)
							{
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 7);
								tooltip.Text += "\n" + Language.GetTextValue("CommonItemTooltip.IncreasesMaxSentriesBy", 1);
							}
							break;
						case ItemID.SpiderGreaves:
							if (armorToggles.Spider)
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 8);
							break;
						case ItemID.SpookyHelmet:
						case ItemID.SpookyBreastplate:
						case ItemID.SpookyLeggings:
							if (armorToggles.Spooky)
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 17);
							break;
					}
				}
			}
		}

		// Since vanilla doesn't necessarily have booleans for every vanilla set or anything, we have to define the set bonuses ourselves
		public override string IsArmorSet(Item head, Item body, Item legs)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();
			ArmorReworks armorToggles = config.armorBonuses;

			if (config.OreUnification)
			{
				if ((head.type == ItemID.IronHelmet || head.type == ItemID.AncientIronHelmet) && body.type == ItemID.IronChainmail && legs.type == ItemID.IronGreaves)
				{
					return "Iron";
				}
				if ((head.type == ItemID.GoldHelmet || head.type == ItemID.AncientGoldHelmet) && body.type == ItemID.GoldChainmail && legs.type == ItemID.GoldGreaves)
				{
					return "Gold";
				}
			}

			if (head.type == ItemID.SpiderMask && body.type == ItemID.SpiderBreastplate && legs.type == ItemID.SpiderGreaves && armorToggles.Spider)
			{
				return "Spider";
			}

			if (CobaltHeads.Contains(head.type) && body.type == ItemID.CobaltBreastplate && legs.type == ItemID.CobaltLeggings && armorToggles.Cobalt)
			{
				return "Cobalt";
			}
			if (MythrilHeads.Contains(head.type) && body.type == ItemID.MythrilChainmail && legs.type == ItemID.MythrilGreaves && armorToggles.Mythril)
			{
				return "Mythril";
			}
			if (AdamantiteHeads.Contains(head.type) && body.type == ItemID.AdamantiteBreastplate && legs.type == ItemID.AdamantiteLeggings && armorToggles.Adamantite)
			{
				return "Adamantite";
			}

			if (head.type == ItemID.SpookyHelmet && body.type == ItemID.SpookyBreastplate && legs.type == ItemID.SpookyLeggings && armorToggles.Spooky)
			{
				return "Spooky";
			}

			return base.IsArmorSet(head, body, legs);
		}

		// And now, the fun part: Actually activating all the new set bonuses! (and disabling the original bonuses)
		public override void UpdateArmorSet(Player player, string set)
		{
			int headType = player.armor[0].type;
			TerratweaksPlayer tPlr = player.GetModPlayer<TerratweaksPlayer>();

			switch (set)
			{
				case "Iron":
					player.setBonus = Language.GetTextValue("ArmorSetBonus.MetalTier2");
					player.statDefense++; // Increase player's defense by 1 more than in vanilla
					break;
				case "Gold":
					player.setBonus = Language.GetTextValue("ArmorSetBonus.Platinum");
					player.statDefense++; // Increase player's defense by 1 more than in vanilla
					break;
				case "Spider":
					player.setBonus = Language.GetTextValue("Mods.Terratweaks.SetBonus.Spider");
					player.spikedBoots = 2;
					player.buffImmune[BuffID.Webbed] = true;
					tPlr.spiderWeb = true;

					// This is done to remove vanilla's bonus, since without this the player would have 12% more summon damage than they should
					player.GetDamage(DamageClass.Summon) -= 0.12f;
					break;

				case "Cobalt":
					player.setBonus = Language.GetTextValue("Mods.Terratweaks.SetBonus.Cobalt");
					tPlr.cobaltDefense = true;

					// Negate vanilla bonuses
					if (headType == ItemID.CobaltHelmet)
						player.GetAttackSpeed(DamageClass.Melee) -= 0.15f;
					else if (headType == ItemID.CobaltMask)
						player.ammoCost80 = false;
					else if (headType == ItemID.CobaltHat)
						player.manaCost += 0.14f;
					break;
				case "Mythril":
					player.setBonus = Language.GetTextValue("Mods.Terratweaks.SetBonus.Mythril");
					tPlr.mythrilFire = true;

					// Negate vanilla bonuses
					if (headType == ItemID.MythrilHelmet)
						player.GetCritChance(DamageClass.Melee) -= 10;
					else if (headType == ItemID.MythrilHat)
						player.ammoCost80 = false;
					else if (headType == ItemID.MythrilHood)
						player.manaCost += 0.17f;
					break;
				case "Adamantite":
					player.setBonus = Language.GetTextValue("Mods.Terratweaks.SetBonus.Adamantite");
					tPlr.adamHearts = true;

					// Negate vanilla bonuses
					if (headType == ItemID.AdamantiteHelmet)
					{
						player.moveSpeed -= 0.2f;
						player.GetAttackSpeed(DamageClass.Melee) -= 0.2f;
					}
					else if (headType == ItemID.AdamantiteMask)
						player.ammoCost75 = false;
					else if (headType == ItemID.AdamantiteHeadgear)
						player.manaCost += 0.19f;
					break;

				case "Spooky":
					player.setBonus = Language.GetTextValue("Mods.Terratweaks.SetBonus.Spooky");
					tPlr.spookyShots = true;

					// Negate vanilla bonuses
					player.GetDamage(DamageClass.Summon) -= 0.25f;
					break;
			}
		}
	}

	public class RadiantInsigniaCutscene : GlobalItem
	{
		public override bool InstancePerEntity => true;
		public bool inCutscene = false;
		float speed = 1;

		public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
		{
			Player plr = Main.player[item.playerIndexTheItemIsReservedFor];
			CutscenePlayer cPlr = plr.GetModPlayer<CutscenePlayer>();

			if (item.type == ItemID.EmpressFlightBooster && cPlr.inCutscene)
			{
				if (item.shimmerTime < 0.9f)
				{
					item.shimmerTime += 0.015f;
					gravity = 0;
					item.velocity = Vector2.Zero;
					if (plr.ownedLargeGems == 0)
						item.position.Y -= speed;
					else
						item.position.X += speed * cPlr.direction;

					if (item.shimmerTime > 0.2f)
					{
						if (speed > 0.1)
							speed -= 0.06f;

						if (item.shimmerTime > 0.45f)
							speed = 0;
					}
				}
				else
				{
					Item.ShimmerEffect(item.Center);
					var prefixOld = item.prefix;
					item.ChangeItemType(ItemType<RadiantInsignia>());
					item.prefix = prefixOld;
					item.shimmered = true;
					cPlr.inCutscene = false;
					CombatText.NewText(item.getRect(), Color.HotPink, "The Soaring Insignia has regained its might!");
					SoundEngine.PlaySound(SoundID.AchievementComplete, item.position);
				}
			}
		}
	}

	public class ShimmerTransmutationHandler : GlobalItem
	{
		public static readonly Dictionary<string, List<int>> ShimmerableBossDrops = new()
		{
			{ "KingSlime", new List<int> { ItemID.SlimeGun, ItemID.SlimeHook } },
			{ "NinjaSet", new List<int> { ItemID.NinjaHood, ItemID.NinjaShirt, ItemID.NinjaPants } },
			{ "QueenBee", new List<int> { ItemID.BeeKeeper, ItemID.BeesKnees, ItemID.BeeGun } },
			{ "BeeSet", new List<int> { ItemID.BeeHat, ItemID.BeeShirt, ItemID.BeePants } },
			{ "Deerclops", new List<int> { ItemID.LucyTheAxe, ItemID.PewMaticHorn, ItemID.WeatherPain, ItemID.HoundiusShootius } },
			{ "CrystalAssassinSet", new List<int> { ItemID.CrystalNinjaHelmet, ItemID.CrystalNinjaChestplate, ItemID.CrystalNinjaLeggings } },
			{ "Plantera", new List<int> { ItemID.Seedler, ItemID.FlowerPow, ItemID.VenusMagnum, ItemID.GrenadeLauncher, ItemID.NettleBurst, ItemID.LeafBlower, ItemID.WaspGun } },
			{ "Golem", new List<int> { ItemID.GolemFist, ItemID.PossessedHatchet, ItemID.Stynger, ItemID.HeatRay, ItemID.StaffofEarth, ItemID.EyeoftheGolem, ItemID.SunStone } },
			{ "DukeFishron", new List<int> { ItemID.Flairon, ItemID.Tsunami, ItemID.RazorbladeTyphoon, ItemID.BubbleGun, ItemID.TempestStaff } },
			{ "EmpressOfLight", new List<int> { ItemID.PiercingStarlight, ItemID.FairyQueenRangedItem, ItemID.FairyQueenMagicItem, ItemID.RainbowWhip } },
			{ "Betsy", new List<int> { ItemID.DD2SquireBetsySword, ItemID.DD2BetsyBow, ItemID.ApprenticeStaffT3, ItemID.MonkStaffT3 } },
			{ "MoonLord", new List<int> { ItemID.Meowmere, ItemID.StarWrath, ItemID.Terrarian, ItemID.SDMG, ItemID.Celeb2, ItemID.LastPrism, ItemID.LunarFlareBook, ItemID.RainbowCrystalStaff, ItemID.MoonlordTurretStaff } }
		};

		public override void SetStaticDefaults()
		{
			var config = GetInstance<TerratweaksConfig>().craftableUncraftables;

			if (config.Moss)
			{
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.RedMoss, ItemID.BrownMoss, ItemID.GreenMoss, ItemID.BlueMoss, ItemID.PurpleMoss });
				AddShimmerTransmutation_Chain(new List<int> { ItemID.LavaMoss, ItemID.XenonMoss, ItemID.KryptonMoss, ItemID.ArgonMoss, ItemID.VioletMoss, ItemID.RainbowMoss });
			}

			if (config.Gravestones)
			{
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.Tombstone, ItemID.GraveMarker, ItemID.CrossGraveMarker, ItemID.Headstone, ItemID.Gravestone, ItemID.Obelisk });
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.RichGravestone1, ItemID.RichGravestone2, ItemID.RichGravestone3, ItemID.RichGravestone4, ItemID.RichGravestone5 });
			}

			if (config.Trophies)
			{
				AddTrophyRecipes();
			}

			if (config.ShimmerBottomlessAndSponges)
			{
				ItemID.Sets.ShimmerTransformToItem[ItemID.BottomlessLavaBucket] = ItemID.LavaAbsorbantSponge;
				ItemID.Sets.ShimmerTransformToItem[ItemID.LavaAbsorbantSponge] = ItemID.BottomlessLavaBucket;

				ItemID.Sets.ShimmerTransformToItem[ItemID.BottomlessHoneyBucket] = ItemID.HoneyAbsorbantSponge;
				ItemID.Sets.ShimmerTransformToItem[ItemID.HoneyAbsorbantSponge] = ItemID.BottomlessHoneyBucket;
			}

			if (config.PrehardUnobtainables)
			{
				ItemID.Sets.ShimmerTransformToItem[ItemID.HelFire] = ItemID.Cascade;
				ItemID.Sets.ShimmerTransformToItem[ItemID.ZapinatorOrange] = ItemID.ZapinatorGray;
			}

			if (config.ShimmerBossDrops)
			{
				foreach (KeyValuePair<string, List<int>> pair in ShimmerableBossDrops)
				{
					if (pair.Value.Count > 0)
						AddShimmerTransmutation_Cycle(pair.Value);
				}
			}
		}

		static void AddTrophyRecipes()
		{
			TrophyRecipes_Vanilla();

			if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
				TrophyRecipes_Calamity(calamity);
		}

		static void TrophyRecipes_Vanilla()
		{
			ItemID.Sets.ShimmerTransformToItem[ItemID.RetinazerTrophy] = ItemID.SpazmatismTrophy;
			ItemID.Sets.ShimmerTransformToItem[ItemID.SpazmatismTrophy] = ItemID.RetinazerTrophy;
		}

		static void TrophyRecipes_Calamity(Mod calamity)
		{
			if (calamity.TryFind("AnahitaTrophy", out ModItem anahita) &&
				calamity.TryFind("LeviathanTrophy", out ModItem levi))
			{
				ItemID.Sets.ShimmerTransformToItem[anahita.Type] = levi.Type;
				ItemID.Sets.ShimmerTransformToItem[levi.Type] = anahita.Type;
			}

			if (calamity.TryFind("ApolloTrophy", out ModItem apollo) &&
				calamity.TryFind("ArtemisTrophy", out ModItem artemis) &&
				calamity.TryFind("ThanatosTrophy", out ModItem thanatos) &&
				calamity.TryFind("AresTrophy", out ModItem ares))
			{
				AddShimmerTransmutation_Cycle(new List<int> { artemis.Type, apollo.Type, thanatos.Type, ares.Type });
			}

			if (calamity.TryFind("CalamitasCloneTrophy", out ModItem calClone) &&
				calamity.TryFind("CataclysmTrophy", out ModItem cataclysm) &&
				calamity.TryFind("CatastropheTrophy", out ModItem catastrophe))
			{
				AddShimmerTransmutation_Cycle(new List<int> { calClone.Type, cataclysm.Type, catastrophe.Type });
			}

			if (calamity.TryFind("SupremeCalamitasTrophy", out ModItem sCal) &&
				calamity.TryFind("SupremeCataclysmTrophy", out ModItem sCataclysm) &&
				calamity.TryFind("SupremeCatastropheTrophy", out ModItem sCatastrophe))
			{
				AddShimmerTransmutation_Cycle(new List<int> { sCal.Type, sCataclysm.Type, sCatastrophe.Type });
			}
		}

		static void AddShimmerTransmutation_Cycle(List<int> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				int item = items[i];
				int result = i == items.Count - 1 ? items[0] : items[i + 1];

				ItemID.Sets.ShimmerTransformToItem[item] = result;
			}
		}

		static void AddShimmerTransmutation_Chain(List<int> items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				int item = items[i];
				int result = i == items.Count - 1 ? -1 : items[i + 1];

				if (result > -1)
					ItemID.Sets.ShimmerTransformToItem[item] = result;
			}
		}
	}
}
