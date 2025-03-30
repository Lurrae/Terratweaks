using Microsoft.CodeAnalysis.CSharp.Syntax;
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
using Terratweaks.Tiles;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks.Items
{
	// Handles adding items to Item.StatsModifiedBy(Mod) so they show an "item edited by Terratweaks" tooltip
	public class ModifiedStatsTip : GlobalItem
	{
		public static readonly List<int> DiminishingReturnItems = new()
		{
			ItemID.DestroyerEmblem,
			ItemID.AvengerEmblem,
			ItemID.LightningBoots,
			ItemID.FrostsparkBoots,
			ItemID.TerrasparkBoots,
			ItemID.ReconScope,
			ItemID.StalkersQuiver,
			ItemID.ArcaneFlower
		};

		public static readonly List<int> EarlyOreTools = new()
		{
			ItemID.CopperShortsword,
			ItemID.CopperBroadsword,
			ItemID.CopperBow,
			ItemID.AmethystStaff,
			ItemID.CopperHelmet,
			ItemID.CopperChainmail,
			ItemID.CopperGreaves,
			ItemID.CopperAxe,
			ItemID.CopperHammer,
			ItemID.TinHammer,
			ItemID.CopperPickaxe,
			ItemID.IronShortsword,
			ItemID.IronBroadsword,
			ItemID.IronBow,
			ItemID.IronHelmet,
			ItemID.AncientIronHelmet,
			ItemID.IronChainmail,
			ItemID.IronGreaves,
			ItemID.IronAxe,
			ItemID.IronHammer,
			ItemID.LeadHammer,
			ItemID.IronPickaxe,
			ItemID.LeadPickaxe,
			ItemID.SilverShortsword,
			ItemID.SilverBroadsword,
			ItemID.SilverBow,
			ItemID.SapphireStaff,
			ItemID.SilverHelmet,
			ItemID.SilverChainmail,
			ItemID.SilverGreaves,
			ItemID.SilverAxe,
			ItemID.SilverHammer,
			ItemID.SilverPickaxe,
			ItemID.GoldShortsword,
			ItemID.GoldBroadsword,
			ItemID.GoldBow,
			ItemID.RubyStaff,
			ItemID.GoldHelmet,
			ItemID.AncientGoldHelmet,
			ItemID.GoldChainmail,
			ItemID.GoldGreaves,
			ItemID.GoldAxe,
			ItemID.GoldHammer,
			ItemID.PlatinumHammer,
			ItemID.GoldPickaxe,
			ItemID.PlatinumPickaxe,
			ItemID.CactusPickaxe,
			ItemID.ReaverShark
		};

		public override void SetDefaults(Item item)
		{
			bool itemIsModified = false;

			#region Main Config
			if (Terratweaks.Config.ChesterRework && item.type == ItemID.ChesterPetItem)
				itemIsModified = true;

			if (Terratweaks.Config.DeerWeaponsRework && (item.type == ItemID.LucyTheAxe || item.type == ItemID.PewMaticHorn || item.type == ItemID.WeatherPain || item.type == ItemID.HoundiusShootius))
				itemIsModified = true;

			if (Terratweaks.Config.ManaFreeSummoner && item.CountsAsClass(DamageClass.Summon) && ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj) && (proj.minion || proj.sentry))
				itemIsModified = true;

			if (Terratweaks.Config.NoDiminishingReturns && DiminishingReturnItems.Contains(item.type))
				itemIsModified = true;

			if (Terratweaks.Config.OreUnification && EarlyOreTools.Contains(item.type))
				itemIsModified = true;

			if (Terratweaks.Config.ReaverSharkTweaks && item.type == ItemID.ReaverShark)
				itemIsModified = true;

			if (Terratweaks.Config.SIRework && item.type == ItemID.EmpressFlightBooster && !ModLoader.HasMod("CalamityMod"))
				itemIsModified = true;

			if (Terratweaks.Config.StackableDD2Accs != SentryAccSetting.Off && item.type >= ItemID.ApprenticeScarf && item.type <= ItemID.MonkBelt)
				itemIsModified = true;

			if (Terratweaks.Config.UmbrellaHatRework && item.type == ItemID.UmbrellaHat)
				itemIsModified = true;

			if (Terratweaks.Config.PlaceableGravGlobe && item.type == ItemID.GravityGlobe)
				itemIsModified = true;

			if (Terratweaks.Config.FrostHydraBuff && item.type == ItemID.StaffoftheFrostHydra)
				itemIsModified = true;
			#endregion

			#region Expert Accessory & Armor Tweaks
			if ((Terratweaks.Config.RoyalGel && item.type == ItemID.RoyalGel) ||
				(Terratweaks.Config.HivePack && item.type == ItemID.HiveBackpack) ||
				(Terratweaks.Config.BoneHelm && item.type == ItemID.BoneHelm) ||
				(Terratweaks.Config.BoneGlove && item.type == ItemID.BoneGlove) ||
				(Terratweaks.Config.EyeShield && item.type == ItemID.EoCShield) ||
				(Terratweaks.Config.WormBrain && (item.type == ItemID.WormScarf || item.type == ItemID.BrainOfConfusion)))
				itemIsModified = true;

			if ((Terratweaks.Config.SpiderSetBonus && item.type >= ItemID.SpiderMask && item.type <= ItemID.SpiderGreaves) ||
				(Terratweaks.Config.CobaltSetBonus && item.type >= ItemID.CobaltHat && item.type <= ItemID.CobaltLeggings) ||
				(Terratweaks.Config.MythrilSetBonus && item.type >= ItemID.MythrilHood && item.type <= ItemID.MythrilGreaves) ||
				(Terratweaks.Config.AdamantiteSetBonus && item.type >= ItemID.AdamantiteHeadgear && item.type <= ItemID.AdamantiteLeggings) ||
				(Terratweaks.Config.SpookySetBonus && item.type >= ItemID.SpookyHelmet && item.type <= ItemID.SpookyLeggings) ||
				(Terratweaks.Config.ConvertMonkArmor && item.type >= ItemID.MonkBrows && item.type <= ItemID.MonkPants) ||
				(Terratweaks.Config.ConvertMonkArmor && item.type >= ItemID.MonkAltHead && item.type <= ItemID.MonkAltPants) ||
				(Terratweaks.Config.StardustArmorBuff && item.type >= ItemID.StardustHelmet && item.type <= ItemID.StardustLeggings))
				itemIsModified = true;
			#endregion

			if (itemIsModified)
				item.StatsModifiedBy.Add(Mod);
		}
	}

	// Anything that's done specifically for cross-mod compatibility can be done with this GlobalItem
	public class ModCompatChanges : GlobalItem
	{
		public override bool CanUseItem(Item item, Player player)
		{
			// Check if Calamity is enabled since Calamity already locks sandstorms progression-wise
			bool progressionLockedSandstorms = Terratweaks.Config.PostEyeSandstorms && !ModLoader.HasMod("CalamityMod");

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
			{ ItemID.BoneHelm, () => Terratweaks.Config.BoneHelm }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_QS = new()
		{
			{ ItemID.RoyalGel, () => Terratweaks.Config.RoyalGel }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_MechEye = new()
		{
			{ ItemID.EoCShield, () => Terratweaks.Config.EyeShield }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_MechWorm = new()
		{
			{ ItemID.WormScarf, () => Terratweaks.Config.WormBrain },
			{ ItemID.BrainOfConfusion, () => Terratweaks.Config.WormBrain }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_MechSkull = new()
		{
			{ ItemID.BoneGlove, () => Terratweaks.Config.BoneGlove }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_Mechs = new() { };

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_Plant = new()
		{
			{ ItemID.HiveBackpack, () => Terratweaks.Config.HivePack }
		};

		static readonly Dictionary<int, Func<bool>> ExpertItemsThatScale_ML = new() { };

		public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Terratweaks.Config.NoDamageVariance == DamageVarianceSetting.Limited)
				modifiers.DamageVariationScale *= 0;

			if (Terratweaks.ClientConfig.NoRandomCrit)
			{
				hitsDone += player.GetWeaponCrit(item);

				if (hitsDone >= 100)
				{
					modifiers.SetCrit();
					hitsDone = 0;

					if (Terratweaks.Config.CritsBypassDefense)
						modifiers.DefenseEffectiveness *= 0;
				}
				else
					modifiers.DisableCrit();
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			int idx = -1;
			int velIdx = -1;

			if (Terratweaks.ClientConfig.StatsInTip)
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
					realShootSpeed = (float)Math.Round(realShootSpeed, 2); // Round velocity to 2 digits
					tooltips.Insert(idx + 1, new TooltipLine(Mod, "Velocity", Language.GetTextValue("Mods.Terratweaks.LegacyTooltip.V", realShootSpeed)));
					velIdx = idx + 1;
				}
			}

			// Grammar corrections option only applies when playing with English localization,
			// since I don't know enough about other languages to even know if the incorrect grammar from vanilla
			// still applies in other languages, let alone how best to correct it
			if (Terratweaks.ClientConfig.GrammarCorrections && Language.ActiveCulture.Name == "en-US" && item.Name.Contains("The") && item.prefix > 0)
			{
				idx = tooltips.IndexOf(tooltips.First(t => t.Name == "ItemName"));
				TooltipLine name = tooltips[idx];
				
				// Split apart the item name into individual words
				List<string> splitName = name.Text.Split(' ').ToList();

				// Find and remove the word "The", and then insert it back at the very start
				if (splitName.Remove("The"))
				{
					splitName.Insert(0, "The");
				}

				// Re-assemble the item's name
				name.Text = string.Join(" ", splitName);
			}

			if (Terratweaks.ClientConfig.WingStatsInTip)
			{
				idx = -1;

				// Place the wing stats directly after the "Allows flight and slow fall" text
				// Using GetTextValue ensures that this should find the tooltip even in other languages
				foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Text.Equals(Language.GetTextValue("CommonItemTooltip.FlightAndSlowfall"))))
				{
					idx = tooltips.IndexOf(tooltip);
				}

				if (idx != -1 && item.wingSlot > 0)
				{
					// Ignore vanilla wings if Calamity is enabled, and ignore modded wings from Calamity Mod
					if ((item.ModItem == null && !ModLoader.HasMod("CalamityMod")) || (item.ModItem != null && item.ModItem.Mod.Name != "CalamityMod"))
					{
						WingStats wingStats = ArmorIDs.Wing.Sets.Stats[item.wingSlot];
						float flyTime = (float)Math.Round(Conversions.ToSeconds(wingStats.FlyTime), 2);
						float horiSpdMult = wingStats.AccRunSpeedOverride < 0 ? Main.LocalPlayer.accRunSpeed : wingStats.AccRunSpeedOverride;
						string maxHoriSpd = $"{Math.Round(horiSpdMult * (216000 / 42240), 2)} mph";
						float horiAccel = wingStats.AccRunAccelerationMult * 100;

						if (wingStats.HasDownHoverStats)
						{
							float hoverHoriSpdMult = wingStats.DownHoverSpeedOverride < 0 ? Main.LocalPlayer.accRunSpeed : wingStats.DownHoverSpeedOverride;
							string hoverHoriSpd = $"{Math.Round(hoverHoriSpdMult * (216000 / 42240), 2)} mph";
							float hoverHoriAccel = wingStats.DownHoverAccelerationMult * 100;

							TooltipLine wingStatsTip = new(Mod, "WingStats", Language.GetTextValue("Mods.Terratweaks.Common.WingStats_Hover", flyTime, maxHoriSpd, horiAccel, hoverHoriSpd, hoverHoriAccel));

							tooltips.Insert(idx + 1, wingStatsTip);
						}
						else
						{
							TooltipLine wingStatsTip = new(Mod, "WingStats", Language.GetTextValue("Mods.Terratweaks.Common.WingStats", flyTime, maxHoriSpd, horiAccel));

							tooltips.Insert(idx + 1, wingStatsTip);
						}
					}
				}
			}

			if (Terratweaks.ClientConfig.EstimatedDPS)
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

			if (Terratweaks.ClientConfig.NoRandomCrit)
			{
				foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Name == "CritChance"))
					tooltip.Text = Language.GetTextValue("Mods.Terratweaks.LegacyTooltip.C", Main.LocalPlayer.GetWeaponCrit(item));
			}

			if (Terratweaks.Config.SIRework && !ModLoader.HasMod("CalamityMod"))
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

			if (Terratweaks.Config.UmbrellaHatRework)
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

			if (Terratweaks.Config.DeerWeaponsRework)
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

			if (Terratweaks.Config.DyeTraderShopExpansion && Terratweaks.DyeItemsSoldByTrader.Contains(item.type))
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
			if (Terratweaks.ClientConfig.EstimatedDPS && idx != -1)
			{
				if (item.type == ItemID.DPSMeter || item.type == ItemID.GoblinTech)
				{
					tooltips.Insert(idx + 1, new TooltipLine(Mod, "DpsMeterExtraTip", Language.GetTextValue("Mods.Terratweaks.Common.DpsMeterExtraTip")));
				}
			}

			// Add a line to the Gravity Globe
			if (Terratweaks.Config.PlaceableGravGlobe && idx != -1)
			{
				if (item.type == ItemID.GravityGlobe)
				{
					tooltips.Insert(idx + 1, new TooltipLine(Mod, "GravGlobeExtraTip", Language.GetTextValue("Mods.Terratweaks.Common.GravGlobeExtraTip")));
				}
			}

			// Add a line to consumed permanent buffs (doesn't support modded ones by default, requires mod call)
			if (Terratweaks.ClientConfig.PermBuffTips && idx != -1)
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
			if (Terratweaks.Config.ManaFreeSummoner && idx != -1 && item.CountsAsClass(DamageClass.Summon) && ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj))
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
			if (Terratweaks.Config.NoDiminishingReturns && idx != -1)
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
				if (ExpertItemsThatScale_MechSkull.ContainsKey(item.type) && ExpertItemsThatScale_MechSkull.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;
				if (ExpertItemsThatScale_MechEye.ContainsKey(item.type) && ExpertItemsThatScale_MechEye.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;
				if (ExpertItemsThatScale_MechWorm.ContainsKey(item.type) && ExpertItemsThatScale_MechWorm.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;
				if (ExpertItemsThatScale_Plant.ContainsKey(item.type) && ExpertItemsThatScale_Plant.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;
				if (ExpertItemsThatScale_ML.ContainsKey(item.type) && ExpertItemsThatScale_ML.TryGetValue(item.type, out configEnabled) && configEnabled())
					numLines++;

				bool addedMoonlord = false;
				bool addedPlant = false;
				bool addedMechs = false;
				bool addedMechSkull = false;
				bool addedMechEye = false;
				bool addedMechWorm = false;
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

						if (!addedMechSkull && ExpertItemsThatScale_MechSkull.TryGetValue(item.type, out configEnabled) && configEnabled())
						{
							line.Text = Language.GetTextValue("Mods.Terratweaks.Common.StrongerPostMechSkull");
							addedMechSkull = true;

							if (NPC.downedMechBoss3)
								line.OverrideColor = ItemRarity.GetColor(ItemRarityID.Lime);

							tooltips.Insert(idx + 1, line);
							continue;
						}

						if (!addedMechEye && ExpertItemsThatScale_MechEye.TryGetValue(item.type, out configEnabled) && configEnabled())
						{
							line.Text = Language.GetTextValue("Mods.Terratweaks.Common.StrongerPostMechEye");
							addedMechEye = true;

							if (NPC.downedMechBoss2)
								line.OverrideColor = ItemRarity.GetColor(ItemRarityID.Lime);

							tooltips.Insert(idx + 1, line);
							continue;
						}

						if (!addedMechWorm && ExpertItemsThatScale_MechWorm.TryGetValue(item.type, out configEnabled) && configEnabled())
						{
							line.Text = Language.GetTextValue("Mods.Terratweaks.Common.StrongerPostMechWorm");
							addedMechWorm = true;

							if (NPC.downedMechBoss1)
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

		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			// Update Oasis/Mirage Crate loot tables
			if (Terratweaks.Config.OasisCrateBuff && (item.type == ItemID.OasisCrate || item.type == ItemID.OasisCrateHard))
			{
				var rules = itemLoot.Get();
				var mainRule = (AlwaysAtleastOneSuccessDropRule)rules.FirstOrDefault(x => x is AlwaysAtleastOneSuccessDropRule);
				var rule = (CommonDropNotScalingWithLuck)mainRule.rules.FirstOrDefault(x => x is CommonDropNotScalingWithLuck r && r.itemId == ItemID.SandstorminaBottle);

				// In vanilla, there are three options for pyramid loot: Sandstorm in a Bottle, Flying Carpet, and a full Pharaoh vanity set
				var sandBottle = new CommonDrop(ItemID.SandstorminaBottle, 1);
				var carpet = new CommonDrop(ItemID.FlyingCarpet, 1);
				var pharaohsCurse = new CommonDrop(ItemID.PharaohsMask, 1);
				pharaohsCurse.OnSuccess(new CommonDrop(ItemID.PharaohsRobe, 1)); // This is done to make the Pharaoh's Robe drop alongside the mask

				// Check for Calamity Mod; if it's installed, replace the Pharaoh set with an Amber Hook
				if (ModLoader.HasMod("CalamityMod"))
				{
					pharaohsCurse = new CommonDrop(ItemID.AmberHook, 1);
				}

				// Combine all of these into one rule- with the odds being based on the config option
				// By default, it keeps the same 1/35 chance as vanilla
				var combinedRule = new OneFromRulesRule(Terratweaks.Config.OasisCrateOdds, new IItemDropRule[] { sandBottle, carpet, pharaohsCurse });
				
				// Replace the vanilla rule with our new one
				var idx = mainRule.rules.ToList().IndexOf(rule);
				mainRule.rules[idx] = combinedRule;
			}

			// Sky/Azure Crates don't drop important items until Skeletron has been defeated in For the Worthy
			if (Terratweaks.Config.NerfSkyCrates && (item.type == ItemID.FloatingIslandFishingCrate || item.type == ItemID.FloatingIslandFishingCrateHard))
			{
				var rules = itemLoot.Get();
				var mainRule = (AlwaysAtleastOneSuccessDropRule)rules.FirstOrDefault(x => x is AlwaysAtleastOneSuccessDropRule);
				
				// Make sure the main rule still exists, in case a mod exists that completely reworks crate drops
				if (mainRule != null)
				{
					var skyChestLoot = (OneFromOptionsNotScaledWithLuckDropRule)mainRule.rules.FirstOrDefault(x => x is OneFromOptionsNotScaledWithLuckDropRule op && op.dropIds.Contains(ItemID.Starfury));
					var wingsLoot = (CommonDropNotScalingWithLuck)mainRule.rules.FirstOrDefault(x => x is CommonDropNotScalingWithLuck cd && cd.itemId == ItemID.CreativeWings);

					// Only mess with the rules if we could find the original ones
					if (skyChestLoot != null || wingsLoot != null)
					{
						var newRules = mainRule.rules.ToList();

						if (skyChestLoot != default(OneFromOptionsNotScaledWithLuckDropRule))
						{
							int idx = newRules.IndexOf(skyChestLoot);

							var newSkyChestLoot = new LeadingConditionRule(new TerratweaksDropConditions.PostSkeletronOnFtw());
							newSkyChestLoot.OnSuccess(skyChestLoot);
							newRules[idx] = newSkyChestLoot;
						}

						if (wingsLoot != default(CommonDropNotScalingWithLuck))
						{
							int idx = newRules.IndexOf(wingsLoot);

							var newWingsLoot = new LeadingConditionRule(new TerratweaksDropConditions.PostSkeletronOnFtw()).OnSuccess(wingsLoot);
							newRules[idx] = newWingsLoot;
						}

						mainRule.rules = newRules.ToArray();
					}
				}
			}

			// Jungle boss bags drop two items instead of one
			if (Terratweaks.Config.JungleBossBags && (item.type == ItemID.QueenBeeBossBag || item.type == ItemID.PlanteraBossBag || item.type == ItemID.GolemBossBag))
			{
				var rules = itemLoot.Get();

				switch (item.type)
				{
					case ItemID.QueenBeeBossBag:
						var originalBeeRule = (OneFromOptionsNotScaledWithLuckDropRule)rules.FirstOrDefault(x => x is OneFromOptionsNotScaledWithLuckDropRule r && r.dropIds.Contains(ItemID.BeeKeeper));

						// Do nothing if we couldn't find a matching rule
						if (originalBeeRule is default(OneFromOptionsNotScaledWithLuckDropRule))
							break;

						var originalBeeDrops = originalBeeRule?.dropIds ?? Array.Empty<int>();
						
						// Don't replace the original rule unless we successfully found the original rule
						// This means that if a mod messes with Queen Bee's drops other than just adding new items, it shouldn't break anything
						if (originalBeeDrops.Length > 0)
						{
							var newBeeRule = new FewFromOptionsNotScaledWithLuckDropRule(2, 1, 1, originalBeeDrops);
							itemLoot.Remove(originalBeeRule);
							itemLoot.Add(newBeeRule);
						}
						break;
					case ItemID.PlanteraBossBag:
						var originalPlantRule = (OneFromRulesRule)rules.FirstOrDefault(x => x is OneFromRulesRule r && r.options.Any(c => c is CommonDrop cd && cd.itemId == ItemID.Seedler));
						
						// Do nothing if we couldn't find a matching rule
						if (originalPlantRule is default(OneFromRulesRule))
							break;

						var originalPlantDrops = originalPlantRule?.options ?? Array.Empty<IItemDropRule>();

						// Don't replace the original rule unless we successfully found the original rule
						// This means that if a mod messes with Plantera's drops other than just adding new items, it shouldn't break anything
						if (originalPlantDrops.Length > 0)
						{
							var newPlantRule = new FewFromRulesRule(2, 1, originalPlantDrops);
							itemLoot.Remove(originalPlantRule);
							itemLoot.Add(newPlantRule);
						}
						break;
					case ItemID.GolemBossBag:
						var originalGolemRule = (OneFromRulesRule)rules.FirstOrDefault(x => x is OneFromRulesRule r && r.options.Any(c => c is CommonDrop cd && cd.itemId == ItemID.SunStone));

						// Do nothing if we couldn't find a matching rule
						if (originalGolemRule is default(OneFromRulesRule))
							break;
						
						var originalGolemDrops = originalGolemRule?.options ?? Array.Empty<IItemDropRule>();

						// Don't replace the original rule unless we successfully found the original rule
						// This means that if a mod messes with Golem's drops other than just adding new items, it shouldn't break anything
						if (originalGolemDrops.Length > 0)
						{
							var newGolemRule = new FewFromRulesRule(2, 1, originalGolemDrops);
							itemLoot.Remove(originalGolemRule);
							itemLoot.Add(newGolemRule);
						}
						break;
				}
			}

			if (Terratweaks.Config.CultistGravGlobe && item.type == ItemID.MoonLordBossBag)
			{
				// Remove the Gravity Globe from Moon Lord's treasure bag, as Cultist drops it instead
				itemLoot.RemoveWhere(r => r is CommonDrop cd && cd.itemId == ItemID.GravityGlobe);
			}

			if (Terratweaks.Calamitweaks.RevertTerraprisma && ModLoader.HasMod("CalamityMod") && item.type == ItemID.FairyQueenBossBag)
			{
				foreach (IItemDropRule rule in itemLoot.Get(false))
				{
					HandleCalamityEoLChanges(rule);
				}
			}
		}

		static void HandleCalamityEoLChanges(IItemDropRule rule)
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
			if (item.type == ItemID.UmbrellaHat && Terratweaks.Config.UmbrellaHatRework)
			{
				item.vanity = false;
				item.accessory = true;
				item.headSlot = -1;
			}
		}

		public override void UpdateVanity(Item item, Player player)
		{
			if (item.type == ItemID.UmbrellaHat && Terratweaks.Config.UmbrellaHatRework)
			{
				player.GetModPlayer<InputPlayer>().umbrellaHatVanity = true;
			}
		}

		public override void UpdateAccessory(Item item, Player player, bool hideVisual)
		{
			SentryAccSetting dd2AccsStack = Terratweaks.Config.StackableDD2Accs;

			if (item.type == ItemID.UmbrellaHat && Terratweaks.Config.UmbrellaHatRework)
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

			if (Terratweaks.Config.NoDiminishingReturns)
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

			if (item.type == ItemID.EmpressFlightBooster && Terratweaks.Config.SIRework && !ModLoader.HasMod("CalamityMod"))
			{
				// Disable vanilla SI effects
				player.empressBrooch = false;
				player.moveSpeed -= 0.1f;

				// New effects: +25% wing flight time, +10% jump speed, +10% acceleration
				player.wingTimeMax = (int)Math.Round(player.wingTimeMax * 1.25f);
				player.jumpSpeedBoost += 0.5f;
				player.runAcceleration *= 1.1f;
			}

			// Buff the DR provided by the Worm Scarf and increase max HP if the Destroyer has been defeated with the config active
			// Also sets the variable for the buffed Worm Scarf so it can spawn friendly probes
			if (item.type == ItemID.WormScarf && Terratweaks.Config.WormBrain && NPC.downedMechBoss1)
			{
				player.endurance += 0.04f;
				player.statLifeMax2 += 20;
				player.GetModPlayer<TerratweaksPlayer>().buffedWormScarf = item;
			}

			// Sets the variable for the buffed Brain of Confusion so it can do all its effects elsewhere
			if (item.type == ItemID.BrainOfConfusion && Terratweaks.Config.WormBrain && NPC.downedMechBoss1)
			{
				player.GetModPlayer<TerratweaksPlayer>().buffedBrainOfConfusion = item;
			}
		}
	}

	// Contains all item changes, like tools or weapons
	public class ItemChanges : GlobalItem
	{
		public static readonly List<int> IgnoredSummonWeapons = new();

		public override void SetDefaults(Item item)
		{
			if (Terratweaks.Config.OreUnification)
			{
				switch (item.type)
				{
					#region Copper/Tin
					case ItemID.CopperShortsword:
						item.CloneDefaults(ItemID.TinShortsword);
						item.shoot = ProjectileID.CopperShortswordStab;
						break;
					case ItemID.CopperBroadsword:
						item.CloneDefaults(ItemID.TinBroadsword);
						break;
					case ItemID.CopperBow:
						item.CloneDefaults(ItemID.TinBow);
						break;
					case ItemID.AmethystStaff:
						item.CloneDefaults(ItemID.TopazStaff);
						item.shoot = ProjectileID.AmethystBolt;
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
						item.shoot = ProjectileID.IronShortswordStab;
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
						item.shoot = ProjectileID.SilverShortswordStab;
						break;
					case ItemID.SilverBroadsword:
						item.CloneDefaults(ItemID.TungstenBroadsword);
						break;
					case ItemID.SilverBow:
						item.CloneDefaults(ItemID.TungstenBow);
						break;
					case ItemID.SapphireStaff:
						item.CloneDefaults(ItemID.EmeraldStaff);
						item.shoot = ProjectileID.SapphireBolt;
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
						item.shoot = ProjectileID.GoldShortswordStab;
						break;
					case ItemID.GoldBroadsword:
						item.CloneDefaults(ItemID.PlatinumBroadsword);
						break;
					case ItemID.GoldBow:
						item.CloneDefaults(ItemID.PlatinumBow);
						break;
					case ItemID.RubyStaff:
						item.CloneDefaults(ItemID.DiamondStaff);
						item.shoot = ProjectileID.RubyBolt;
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

			if (Terratweaks.Config.ReaverSharkTweaks && item.type == ItemID.ReaverShark)
			{
				item.pick = 100;
			}

			if (Terratweaks.Config.ManaFreeSummoner && !IgnoredSummonWeapons.Contains(item.type) && item.CountsAsClass(DamageClass.Summon) && ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj))
			{
				if (proj.minion || proj.sentry)
				{
					item.mana = 0;
				}
			}

			if (Terratweaks.Config.DeerWeaponsRework && item.type == ItemID.PewMaticHorn)
			{
				item.useTime = item.useAnimation = 10;
			}

			if (Terratweaks.Config.FrostHydraBuff && item.type == ItemID.StaffoftheFrostHydra)
			{
				item.damage = (int)Math.Floor(item.damage * FROST_HYDRA_DMG_NERF);
			}
		}

		public static readonly float FROST_HYDRA_DMG_NERF = 0.75f; // -25% damage

		public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Player player = Main.LocalPlayer;

			// Don't do anything if we aren't in a world
			if (Main.gameMenu)
				return;

			if (Terratweaks.Config.ManaFreeSummoner && !IgnoredSummonWeapons.Contains(item.type) && item.CountsAsClass(DamageClass.Summon) && ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out Projectile proj))
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
			if (Terratweaks.Config.ManaFreeSummoner && !IgnoredSummonWeapons.Contains(item.type) && item.CountsAsClass(DamageClass.Summon))
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
			Projectile proj = ContentSamples.ProjectilesByType[item.shoot];

			// Disable minions if one is summoned during a boss fight
			if (Terratweaks.Config.ManaFreeSummoner && !IgnoredSummonWeapons.Contains(item.type) && item.CountsAsClass(DamageClass.Summon))
			{
				if (proj.minion || proj.sentry)
				{
					if (/*player.GetModPlayer<CombatPlayer>().IsInCombat()*/ Main.CurrentFrameFlags.AnyActiveBossNPC || AnyBossExists(true))
					{
						player.AddBuff(BuffType<SummonsDisabled>(), Conversions.ToFrames(3));
					}
				}
			}

			// Chance to cause a paper cut when throwing a Paper Airplane,
			// unless the player is immune to Bleeding or invulnerable due to Journey Mode godmode
			if (Terratweaks.Config.PaperCuts && proj.aiStyle == ProjAIStyleID.PaperPlane && !player.buffImmune[BuffID.Bleeding] && !player.creativeGodMode)
			{
				if (Main.rand.NextBool(10)) // 10% chance to cause a paper cut
				{
					Player.HurtInfo info = new()
					{
						Damage = 1, // Paper cut deals only 1 damage, so it's just a minor nuisance if anything
						// Custom damage source means it should provide a custom message: "[player] couldn't handle a paper cut"
						DamageSource = PlayerDeathReason.ByCustomReason(Language.GetTextValue("Mods.Terratweaks.PlayerDeathReason.PaperCut", player.name)),
						Dodgeable = false // No dodging the paper cuts! (which is probably for the better, seeing as it's 1 damage and could probably be abused to avoid stronger attacks...)
					};

					player.Hurt(info);
					player.AddBuff(BuffID.Bleeding, Conversions.ToFrames(4)); // 4 seconds of Bleeding
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
			switch (item.type)
			{
				//Spider armor
				case ItemID.SpiderMask:
				case ItemID.SpiderGreaves:
					if (Terratweaks.Config.SpiderSetBonus)
						player.GetDamage(DamageClass.Summon) += 0.02f;
					break;
				case ItemID.SpiderBreastplate:
					if (Terratweaks.Config.SpiderSetBonus)
					{
						player.GetDamage(DamageClass.Summon) += 0.02f;
						player.maxTurrets += 1;
					}
					break;

				// Monk armor
				case ItemID.MonkBrows:
					if (Terratweaks.Config.ConvertMonkArmor)
					{
						player.GetAttackSpeed(DamageClass.Melee) -= 0.2f; // Remove vanilla melee speed increase
						player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.2f; // Increase whip speed by 20%
					}
					break;
				case ItemID.MonkShirt:
					if (Terratweaks.Config.ConvertMonkArmor)
					{
						player.GetDamage(DamageClass.Melee) -= 0.2f; // Remove vanilla melee damage increase
					}
					break;
				case ItemID.MonkPants:
					if (Terratweaks.Config.ConvertMonkArmor)
					{
						player.GetCritChance(DamageClass.Melee) -= 15; // Remove vanilla melee crit increase
						player.whipRangeMultiplier += 0.15f; // Increase whip range by 15%
					}
					break;

				//Spooky armor
				case ItemID.SpookyHelmet:
				case ItemID.SpookyBreastplate:
				case ItemID.SpookyLeggings:
					if (Terratweaks.Config.SpookySetBonus)
						player.GetDamage(DamageClass.Summon) += 0.06f;
					break;

				// Shinobi Infiltrator armor
				case ItemID.MonkAltHead:
					if (Terratweaks.Config.ConvertMonkArmor)
					{
						player.GetDamage(DamageClass.Melee) -= 0.2f; // Remove vanilla melee damage increase
					}
					break;
				case ItemID.MonkAltShirt:
					if (Terratweaks.Config.ConvertMonkArmor)
					{
						player.GetAttackSpeed(DamageClass.Melee) -= 0.2f; // Remove vanilla melee speed increase
						player.GetCritChance(DamageClass.Melee) -= 5; // Remove vanilla melee crit increase
						player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.2f; // Increase whip speed by 20%
						player.whipRangeMultiplier += 0.15f; // Increase whip range by 15%
					}
					break;
				case ItemID.MonkAltPants:
					if (Terratweaks.Config.ConvertMonkArmor)
					{
						player.GetCritChance(DamageClass.Melee) -= 20; // Remove vanilla melee crit increase
						player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.15f; // Increase whip speed by 15%
						player.whipRangeMultiplier += 0.2f; // Increase whip range by 20%
					}
					break;
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			foreach (TooltipLine tooltip in tooltips)
			{
				if (tooltip.Name == "Tooltip0")
				{
					switch (item.type)
					{
						case ItemID.MonkBrows:
							if (Terratweaks.Config.ConvertMonkArmor)
								tooltip.Text = $"{Language.GetTextValue("CommonItemTooltip.IncreasesMaxSentriesBy", 1)} and {Language.GetTextValue("Mods.Terratweaks.CommonItemTooltip.WhipSpeed", 20)}";
							break;
						case ItemID.MonkShirt:
							if (Terratweaks.Config.ConvertMonkArmor)
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 20);
							break;
						case ItemID.MonkAltHead:
							if (Terratweaks.Config.ConvertMonkArmor)
								tooltip.Text = $"{Language.GetTextValue("CommonItemTooltip.IncreasesMaxSentriesBy", 2)} and {Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 20)}";
							break;
						case ItemID.MonkAltShirt:
							if (Terratweaks.Config.ConvertMonkArmor)
								tooltip.Text = $"{Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 20)} and whip speed";
							break;
						case ItemID.MonkAltPants:
							if (Terratweaks.Config.ConvertMonkArmor)
							{
								tooltip.Text = $"{Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 20)} and whip range";
								tooltip.Text += "\n" + Language.GetTextValue("Mods.Terratweaks.CommonItemTooltip.WhipSpeed", 15);
							}
							break;
					}
				}

				if (tooltip.Name == "Tooltip1")
				{
					switch (item.type)
					{
						case ItemID.SpiderMask:
							if (Terratweaks.Config.SpiderSetBonus)
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 7);
							break;
						case ItemID.SpiderBreastplate:
							if (Terratweaks.Config.SpiderSetBonus)
							{
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 7);
								tooltip.Text += "\n" + Language.GetTextValue("CommonItemTooltip.IncreasesMaxSentriesBy", 1);
							}
							break;
						case ItemID.SpiderGreaves:
							if (Terratweaks.Config.SpiderSetBonus)
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 8);
							break;
						case ItemID.MonkPants:
							if (Terratweaks.Config.ConvertMonkArmor)
								tooltip.Text = $"{Language.GetTextValue("CommonItemTooltip.IncreasesWhipRangeByPercent", 15)} and {Language.GetTextValue("CommonItemTooltip.PercentIncreasedMovementSpeed", 20)}";
							break;
						case ItemID.SpookyHelmet:
						case ItemID.SpookyBreastplate:
						case ItemID.SpookyLeggings:
							if (Terratweaks.Config.SpookySetBonus)
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 17);
							break;
						case ItemID.MonkAltShirt:
							if (Terratweaks.Config.ConvertMonkArmor)
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.IncreasesWhipRangeByPercent", 15);
							break;
					}
				}
			}
		}

		// Since vanilla doesn't necessarily have booleans for every vanilla set or anything, we have to define the set bonuses ourselves
		public override string IsArmorSet(Item head, Item body, Item legs)
		{
			if (Terratweaks.Config.OreUnification)
			{
				if ((head.type == ItemID.IronHelmet || head.type == ItemID.AncientIronHelmet) && body.type == ItemID.IronChainmail && legs.type == ItemID.IronGreaves)
					return "Iron";

				if ((head.type == ItemID.GoldHelmet || head.type == ItemID.AncientGoldHelmet) && body.type == ItemID.GoldChainmail && legs.type == ItemID.GoldGreaves)
					return "Gold";
			}

			if (head.type == ItemID.SpiderMask && body.type == ItemID.SpiderBreastplate && legs.type == ItemID.SpiderGreaves && Terratweaks.Config.SpiderSetBonus)
				return "Spider";

			if (CobaltHeads.Contains(head.type) && body.type == ItemID.CobaltBreastplate && legs.type == ItemID.CobaltLeggings && Terratweaks.Config.CobaltSetBonus)
				return "Cobalt";

			if (MythrilHeads.Contains(head.type) && body.type == ItemID.MythrilChainmail && legs.type == ItemID.MythrilGreaves && Terratweaks.Config.MythrilSetBonus)
				return "Mythril";

			if (AdamantiteHeads.Contains(head.type) && body.type == ItemID.AdamantiteBreastplate && legs.type == ItemID.AdamantiteLeggings && Terratweaks.Config.AdamantiteSetBonus)
				return "Adamantite";

			if (head.type == ItemID.SpookyHelmet && body.type == ItemID.SpookyBreastplate && legs.type == ItemID.SpookyLeggings && Terratweaks.Config.SpookySetBonus)
				return "Spooky";

			if (head.type == ItemID.StardustHelmet && body.type == ItemID.StardustBreastplate && legs.type == ItemID.StardustLeggings && Terratweaks.Config.StardustArmorBuff)
				return "Stardust";

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
				case "Stardust":
					player.setBonus += "\n" + Language.GetTextValue("Mods.Terratweaks.CommonItemTooltip.WhipSpeedRangeDiff",35,20);

					// Increase whip speed and range
					player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.35f;
					player.whipRangeMultiplier += 0.2f;
					break;
			}
		}
	}

	public class RadiantInsigniaCutscene : GlobalItem
	{
		public override bool InstancePerEntity => true;
		public static int AscendantInsigniaType = -1;
		public bool inCutscene = false;
		float speed = 1;

		public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
		{
			Player plr = Main.player[item.playerIndexTheItemIsReservedFor];
			CutscenePlayer cPlr = plr.GetModPlayer<CutscenePlayer>();

			int targetItemType = AscendantInsigniaType > -1 ? AscendantInsigniaType : ItemID.EmpressFlightBooster;

			if (item.type == targetItemType && cPlr.inCutscene)
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
					var nameOld = item.Name;
					item.ChangeItemType(ItemType<RadiantInsignia>());
					item.prefix = prefixOld;
					item.shimmered = true;
					cPlr.inCutscene = false;
					CombatText.NewText(item.getRect(), Color.HotPink, $"The {nameOld} has regained its full might!");
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
			var enabledRecipes = Terratweaks.Config.craftableUncraftables;

			if (enabledRecipes.Moss)
			{
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.RedMoss, ItemID.BrownMoss, ItemID.GreenMoss, ItemID.BlueMoss, ItemID.PurpleMoss });
				AddShimmerTransmutation_Chain(new List<int> { ItemID.LavaMoss, ItemID.XenonMoss, ItemID.KryptonMoss, ItemID.ArgonMoss, ItemID.VioletMoss, ItemID.RainbowMoss });
			}

			if (enabledRecipes.Gravestones)
			{
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.Tombstone, ItemID.GraveMarker, ItemID.CrossGraveMarker, ItemID.Headstone, ItemID.Gravestone, ItemID.Obelisk });
				AddShimmerTransmutation_Cycle(new List<int> { ItemID.RichGravestone1, ItemID.RichGravestone2, ItemID.RichGravestone3, ItemID.RichGravestone4, ItemID.RichGravestone5 });
			}

			if (enabledRecipes.Trophies)
			{
				AddTrophyRecipes();
			}

			if (enabledRecipes.ShimmerBottomlessAndSponges)
			{
				ItemID.Sets.ShimmerTransformToItem[ItemID.BottomlessLavaBucket] = ItemID.LavaAbsorbantSponge;
				ItemID.Sets.ShimmerTransformToItem[ItemID.LavaAbsorbantSponge] = ItemID.BottomlessLavaBucket;

				ItemID.Sets.ShimmerTransformToItem[ItemID.BottomlessHoneyBucket] = ItemID.HoneyAbsorbantSponge;
				ItemID.Sets.ShimmerTransformToItem[ItemID.HoneyAbsorbantSponge] = ItemID.BottomlessHoneyBucket;
			}

			if (enabledRecipes.PrehardUnobtainables)
			{
				ItemID.Sets.ShimmerTransformToItem[ItemID.HelFire] = ItemID.Cascade;
				ItemID.Sets.ShimmerTransformToItem[ItemID.ZapinatorOrange] = ItemID.ZapinatorGray;
			}

			if (enabledRecipes.ShimmerBossDrops)
			{
				foreach (KeyValuePair<string, List<int>> pair in ShimmerableBossDrops)
				{
					if (pair.Value.Count > 0)
						AddShimmerTransmutation_Cycle(pair.Value);
				}
			}

			if (enabledRecipes.ShimmerBlackLens)
			{
				ItemID.Sets.ShimmerTransformToItem[ItemID.BlackLens] = ItemID.Lens;
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

	public class ReforgeHandler : GlobalItem
	{
		public static readonly Dictionary<int, float> vanillaPrefixValues = new()
		{
			{ PrefixID.Large, 1.2544f },
			{ PrefixID.Massive, 1.3924f },
			{ PrefixID.Dangerous, 1.3147f },
			{ PrefixID.Savage, 1.7716f },
			{ PrefixID.Sharp, 1.3225f },
			{ PrefixID.Pointy, 1.21f },
			{ PrefixID.Tiny, 0.6724f },
			{ PrefixID.Terrible, 0.3951f },
			{ PrefixID.Small, 0.81f },
			{ PrefixID.Dull, 0.7225f },
			{ PrefixID.Unhappy, 0.5314f },
			{ PrefixID.Bulky, 1.1662f },
			{ PrefixID.Shameful, 0.6273f },
			{ PrefixID.Heavy, 1.0712f },
			{ PrefixID.Light, 1.0712f },
			{ PrefixID.Sighted, 1.3596f },
			{ PrefixID.Rapid, 1.6002f },
			{ PrefixID.Hasty, 1.6002f },
			{ PrefixID.Intimidating, 1.4581f },
			{ PrefixID.Deadly, 1.7538f },
			{ PrefixID.Staunch, 1.6002f },
			{ PrefixID.Awful, 0.474f },
			{ PrefixID.Lethargic, 0.5852f },
			{ PrefixID.Awkward, 0.5184f },
			{ PrefixID.Powerful, 1.1145f },
			{ PrefixID.Mystic, 1.6002f },
			{ PrefixID.Adept, 1.3225f },
			{ PrefixID.Masterful, 1.9283f },
			{ PrefixID.Inept, 0.81f },
			{ PrefixID.Ignorant, 0.5184f },
			{ PrefixID.Deranged, 0.6561f },
			{ PrefixID.Intense, 0.8742f },
			{ PrefixID.Taboo, 1.1859f },
			{ PrefixID.Celestial, 1.435f },
			{ PrefixID.Furious, 1.1194f },
			{ PrefixID.Keen, 1.1236f },
			{ PrefixID.Superior, 1.6451f },
			{ PrefixID.Forceful, 1.3225f },
			{ PrefixID.Broken, 0.3136f },
			{ PrefixID.Damaged, 0.7225f },
			{ PrefixID.Shoddy, 0.5852f },
			{ PrefixID.Quick, 1.21f },
			{ PrefixID.Deadly2, 1.4641f },
			{ PrefixID.Agile, 1.3596f },
			{ PrefixID.Nimble, 1.1025f },
			{ PrefixID.Murderous, 1.4454f },
			{ PrefixID.Slow, 0.7225f },
			{ PrefixID.Sluggish, 0.64f },
			{ PrefixID.Lazy, 0.8464f },
			{ PrefixID.Annoying, 0.4624f },
			{ PrefixID.Nasty, 1.1687f },
			{ PrefixID.Manic, 1.1859f },
			{ PrefixID.Hurtful, 1.21f },
			{ PrefixID.Strong, 1.3225f },
			{ PrefixID.Unpleasant, 1.4581f },
			{ PrefixID.Weak, 0.64f },
			{ PrefixID.Ruthless, 1.1278f },
			{ PrefixID.Frenzying, 0.9555f },
			{ PrefixID.Godly, 2.1163f },
			{ PrefixID.Demonic, 1.6002f },
			{ PrefixID.Zealous, 1.21f },
			{ PrefixID.Hard, 1.1025f },
			{ PrefixID.Guarding, 1.21f },
			{ PrefixID.Armored, 1.3225f },
			{ PrefixID.Warding, 1.44f },
			{ PrefixID.Arcane, 1.3225f },
			{ PrefixID.Precise, 1.21f },
			{ PrefixID.Lucky, 1.44f },
			{ PrefixID.Jagged, 1.1025f },
			{ PrefixID.Spiked, 1.21f },
			{ PrefixID.Angry, 1.3225f },
			{ PrefixID.Menacing, 1.44f },
			{ PrefixID.Brisk, 1.1025f },
			{ PrefixID.Fleeting, 1.21f },
			{ PrefixID.Hasty2, 1.3225f },
			{ PrefixID.Quick2, 1.44f },
			{ PrefixID.Wild, 1.1025f },
			{ PrefixID.Rash, 1.21f },
			{ PrefixID.Intrepid, 1.3225f },
			{ PrefixID.Violent, 1.44f },
			{ PrefixID.Legendary, 3.0985f },
			{ PrefixID.Unreal, 3.0985f },
			{ PrefixID.Mythical, 3.0985f },
			{ PrefixID.Legendary2, 3.0985f }
		};

		static int OldPrefix = 0;

		public override void PreReforge(Item item)
		{
			OldPrefix = item.prefix;
		}

		public override bool AllowPrefix(Item item, int pre)
		{
			// Don't force rerolls with Calamity Mod since it reworks reforging anyways
			// TODO: Can we access Calamity's config from here? We should check if their rework is enabled,
			//		 since it can be toggled in their config - Lurrae
			if (!ModLoader.HasMod("CalamityMod") && Terratweaks.Config.BetterHappiness && Main.InReforgeMenu && OldPrefix > 0)
			{
				float currentPrefixValue;
				float targetPrefixValue;

				if (vanillaPrefixValues.TryGetValue(OldPrefix, out float value))
				{
					currentPrefixValue = value;
				}
				else
				{
					PrefixLoader.GetPrefix(OldPrefix).ModifyValue(ref value);
					currentPrefixValue = value;
				}

				if (vanillaPrefixValues.TryGetValue(pre, out value))
				{
					targetPrefixValue = value;
				}
				else
				{
					PrefixLoader.GetPrefix(pre).ModifyValue(ref value);
					targetPrefixValue = value;
				}

				// Don't affect chances if tinkerer is unhappy (PriceAdjustment > 1.0)
				if (Main.LocalPlayer.currentShoppingSettings.PriceAdjustment < 1.0f)
				{
					var chance = 1.0f - Main.LocalPlayer.currentShoppingSettings.PriceAdjustment;

					if (targetPrefixValue < currentPrefixValue && Main.rand.NextFloat() <= chance * 3)
					{
						// Force a reroll because this would've been a worse prefix
						return false;
					}
				}
			}

			return base.AllowPrefix(item, pre);
		}
	}

	public class PlaceableHandler : GlobalItem
	{
		public override void SetDefaults(Item item)
		{
			if (item.type == ItemID.GravityGlobe && Terratweaks.Config.PlaceableGravGlobe)
			{
				item.createTile = TileType<GravGlobePlaced>();
				item.placeStyle = 0;
				item.useStyle = ItemUseStyleID.Swing;
				item.useTime = 10;
				item.useAnimation = 15;
				item.consumable = true;
			}
		}
	}
}