using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terratweaks.Buffs;
using Terratweaks.Projectiles;
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

		private static int GetPlayerCrit(Item item, Player player)
		{
			return (int)player.GetCritChance(item.DamageType) + (int)player.GetCritChance(DamageClass.Generic);
		}

		public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
		{
			var clientConfig = GetInstance<TerratweaksConfig_Client>();

			if (clientConfig.NoDamageVariance)
				modifiers.DamageVariationScale *= 0;

			if (clientConfig.NoRandomCrit)
			{
				hitsDone += item.crit + GetPlayerCrit(item, player);

				if (hitsDone >= 100)
				{
					modifiers.SetCrit();
					hitsDone = 0;
				}
				else
					modifiers.DisableCrit();
			}
		}

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

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			TerratweaksConfig_Client clientConfig = GetInstance<TerratweaksConfig_Client>();
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();
			
			int idx = -1;

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
				}
			}

			if (clientConfig.NoRandomCrit)
			{
				foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Name == "CritChance"))
					tooltip.Text = Language.GetTextValue("Mods.Terratweaks.LegacyTooltip.C", item.crit + GetPlayerCrit(item, Main.LocalPlayer));
			}

			if (config.SIRework)
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

			// Find the last tooltip line that describes the item's effects
			idx = -1;

			foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Name.Contains("Tooltip")))
			{
				if (!tooltip.Text.StartsWith("'"))
					idx = tooltips.IndexOf(tooltip);
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
				if (ExpertItemsThatScale_Hardmode.ContainsKey(item.type))
					numLines++;
				if (ExpertItemsThatScale_QS.ContainsKey(item.type))
					numLines++;
				if (ExpertItemsThatScale_Mechs.ContainsKey(item.type))
					numLines++;
				if (ExpertItemsThatScale_Plant.ContainsKey(item.type))
					numLines++;
				if (ExpertItemsThatScale_ML.ContainsKey(item.type))
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

						if (!addedMoonlord && ExpertItemsThatScale_ML.TryGetValue(item.type, out Func<bool> configEnabled) && configEnabled())
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
	public class GrabBagChanges : GlobalItem
	{
		public override bool InstancePerEntity => true;

		private static readonly List<int> pyramidAccs = new() { ItemID.SandstorminaBottle, ItemID.FlyingCarpet };

		// Oasis Crates drop random pyramid loot
		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			bool CratesHavePyramidLoot = GetInstance<TerratweaksConfig>().OasisCrateBuff;

			// Update Oasis/Mirage Crate loot tables
			if (CratesHavePyramidLoot && (item.type == ItemID.OasisCrate || item.type == ItemID.OasisCrateHard))
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

		public override void UpdateAccessory(Item item, Player player, bool hideVisual)
		{
			var config = GetInstance<TerratweaksConfig>();
			SentryAccSetting dd2AccsStack = config.StackableDD2Accs;

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
		public override void SetStaticDefaults()
		{
			if (GetInstance<TerratweaksConfig>().craftableUncraftables.Moss)
			{
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.RedMoss, ItemID.BrownMoss, ItemID.GreenMoss, ItemID.BlueMoss, ItemID.PurpleMoss });
				AddShimmerTransmutation_Chain(new List<int> { ItemID.LavaMoss, ItemID.XenonMoss, ItemID.KryptonMoss, ItemID.ArgonMoss, ItemID.VioletMoss, ItemID.RainbowMoss });
			}

			if (GetInstance<TerratweaksConfig>().craftableUncraftables.Gravestones)
			{
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.Tombstone, ItemID.GraveMarker, ItemID.CrossGraveMarker, ItemID.Headstone, ItemID.Gravestone, ItemID.Obelisk });
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.RichGravestone1, ItemID.RichGravestone2, ItemID.RichGravestone3, ItemID.RichGravestone4, ItemID.RichGravestone5 });
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
				int result = i == items.Count - 1 ? ItemID.Sets.ShimmerTransformToItem[item] : items[i + 1];

				ItemID.Sets.ShimmerTransformToItem[item] = result;
			}
		}
	}
}
