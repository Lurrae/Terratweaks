using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.GemTech;
using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.PlaceableTurrets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksItems : GlobalItem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override void Load()
		{
			if (Terratweaks.Calamitweaks.RevertPickSpeedBuffs)
			{
				Dictionary<object, Array> entriesToReplace = new();

				FieldInfo currentTweaksField = typeof(CalamityGlobalItem).GetField("currentTweaks", BindingFlags.NonPublic | BindingFlags.Static);
				IDictionary curTweaksDict = (IDictionary)currentTweaksField.GetValue(null);

				foreach (var key in curTweaksDict.Keys)
				{
					int itemType = (int)key;
					if (ContentSamples.ItemsByType[itemType].pick > 0)
					{
						object tweaks = curTweaksDict[key];
						Array tweaksList = (Array)tweaks;
						int i = 0;

						// Find an item tweak that edits the use time of the item
						foreach (object tweak in tweaksList)
						{
							if (tweak.GetType().Name.Equals("UseTimeExactRule"))
							{
								break;
							}
							i++;
						}

						// Found a tweak to remove, remove it!
						if (i < tweaksList.Length)
						{
							// Copy the last element over the tweak we want to remove
							tweaksList.SetValue(tweaksList.GetValue(tweaksList.Length - 1), i);

							// Make a new array containing everything except the last element
							Type arrayType = tweaksList.GetType().GetElementType();
							Array newArray = Array.CreateInstance(arrayType, tweaksList.Length - 1);
							Array.Copy(tweaksList, 0, newArray, 0, newArray.Length);

							// Mark this array as needing to be replaced
							entriesToReplace.Add(key, newArray);
						}
					}
				}

				// Replace all the marked arrays
				foreach (KeyValuePair<object, Array> pair in entriesToReplace)
				{
					curTweaksDict[pair.Key] = pair.Value;
				}
			}
		}

		public override void SetStaticDefaults()
		{
			// Reduce banner kill counts for some enemies
			if (Terratweaks.Calamitweaks.EzCalBanners)
			{
				// Fearless Goldfish Warrior is relatively rare, but with a Zerg Potion active it's not too bad
				// Same for Renegade Warlock
				ItemID.Sets.KillsToBanner[ModContent.ItemType<FearlessGoldfishWarriorBanner>()] = 25;
				ItemID.Sets.KillsToBanner[ModContent.ItemType<RenegadeWarlockBanner>()] = 25;

				// Piggy, on the other hand, has 0.005x the spawn rate of a typical critter
				// With a spawn rate that low, killing even 25 of them would be nigh-impossible, even on Journey Mode!
				ItemID.Sets.KillsToBanner[ModContent.ItemType<PiggyBanner>()] = 10;

				// Androombas are extremely rare, and if you capture one with a bug net you can no longer kill it
				// As such, I think it's fair to only require 10 kills for their banner
				ItemID.Sets.KillsToBanner[ModContent.ItemType<AndroombaBanner>()] = 10;

				// In vanilla, all mini-bosses (such as Sand Elementals, Ice Golems, and Dreadnautilus) have non-default kill requirements
				// Dreadnautilus is the only mini-boss in vanilla that requires 10 kills, so to me only the hard-to-kill Abyss layer 3 & 4 minibosses are deserving of such a low requirement
				ItemID.Sets.KillsToBanner[ModContent.ItemType<CnidrionBanner>()] = 25;
				ItemID.Sets.KillsToBanner[ModContent.ItemType<EarthElementalBanner>()] = 25;
				ItemID.Sets.KillsToBanner[ModContent.ItemType<CloudElementalBanner>()] = 25;
				ItemID.Sets.KillsToBanner[ModContent.ItemType<ArmoredDiggerBanner>()] = 25;
				ItemID.Sets.KillsToBanner[ModContent.ItemType<PlaguebringerBanner>()] = 25;
				ItemID.Sets.KillsToBanner[ModContent.ItemType<ColossalSquidBanner>()] = 10;
				ItemID.Sets.KillsToBanner[ModContent.ItemType<ReaperSharkBanner>()] = 10;
				ItemID.Sets.KillsToBanner[ModContent.ItemType<EidolonWyrmJuvenileBanner>()] = 10;
			}

			// Enable Shimmer transmutations for hostile turrets, and draw a skull icon just like the Dead Man's Chest and unsafe/cursed wall blocks
			if (Terratweaks.Calamitweaks.CraftableHostileTurrets)
			{
				ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<FireTurret>()] = ModContent.ItemType<HostileFireTurret>();
				ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<IceTurret>()] = ModContent.ItemType<HostileIceTurret>();
				ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<LabTurret>()] = ModContent.ItemType<HostileLabTurret>();
				ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<LaserTurret>()] = ModContent.ItemType<HostileLaserTurret>();
				ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<OnyxTurret>()] = ModContent.ItemType<HostileOnyxTurret>();
				ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<PlagueTurret>()] = ModContent.ItemType<HostilePlagueTurret>();
				ItemID.Sets.ShimmerTransformToItem[ModContent.ItemType<WaterTurret>()] = ModContent.ItemType<HostileWaterTurret>();

				ItemID.Sets.DrawUnsafeIndicator[ModContent.ItemType<HostileFireTurret>()] = true;
				ItemID.Sets.DrawUnsafeIndicator[ModContent.ItemType<HostileIceTurret>()] = true;
				ItemID.Sets.DrawUnsafeIndicator[ModContent.ItemType<HostileLabTurret>()] = true;
				ItemID.Sets.DrawUnsafeIndicator[ModContent.ItemType<HostileLaserTurret>()] = true;
				ItemID.Sets.DrawUnsafeIndicator[ModContent.ItemType<HostileOnyxTurret>()] = true;
				ItemID.Sets.DrawUnsafeIndicator[ModContent.ItemType<HostilePlagueTurret>()] = true;
				ItemID.Sets.DrawUnsafeIndicator[ModContent.ItemType<HostileWaterTurret>()] = true;
			}
		}

		public static readonly List<int> DRItems = new()
		{
			ItemID.WormScarf,
			ModContent.ItemType<BloodyWormScarf>(),
			ModContent.ItemType<OldDukeScales>(),
			ModContent.ItemType<TheSponge>(),
			ModContent.ItemType<BloodyWormScarf>(),
			ModContent.ItemType<GemTechHeadgear>(),
			ModContent.ItemType<GemTechBodyArmor>(),
			ModContent.ItemType<GemTechSchynbaulds>()
		};

		public override void SetDefaults(Item item)
		{
			// Add a tooltip specifying that certain items were modified by Terratweaks while the corresponding configs are active
			#region StatsModifiedBy stuff
			bool itemIsModified = false;

			if (Terratweaks.Calamitweaks.AsgardsValorBuff && (item.type == ModContent.ItemType<AsgardsValor>() || item.type == ModContent.ItemType<AsgardianAegis>()))
				itemIsModified = true;

			if (Terratweaks.Calamitweaks.AquaticEmblemBuff && item.type == ModContent.ItemType<AquaticEmblem>())
				itemIsModified = true;

			if (Terratweaks.Calamitweaks.DeificAmuletBuff && (item.type == ModContent.ItemType<DeificAmulet>() || item.type == ModContent.ItemType<RampartofDeities>()))
				itemIsModified = true;

			if (Terratweaks.Calamitweaks.DRBuffs && DRItems.Contains(item.type))
				itemIsModified = true;

			if (Terratweaks.Calamitweaks.SummonerAccBuffs && (item.type == ModContent.ItemType<StarTaintedGenerator>() || item.type == ModContent.ItemType<Nucleogenesis>()))
				itemIsModified = true;

			if (itemIsModified)
				item.StatsModifiedBy.Add(Mod);
			#endregion
		}

		public override bool CanUseItem(Item item, Player player)
		{
			if (item.type == ModContent.ItemType<CelestialOnion>() && Terratweaks.Calamitweaks.OnionMasterMode)
			{
				return !player.Calamity().extraAccessoryML;
			}

			return base.CanUseItem(item, player);
		}

		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			if (Terratweaks.Calamitweaks.OnionMasterMode)
			{
				if (item.type == ItemID.MoonLordBossBag)
				{
					// Add a drop rule for if the world IS in master, as Calamity already handles cases where it ISN'T
					itemLoot.AddIf((info) => !info.player.Calamity().extraAccessoryML && Main.masterMode, ModContent.ItemType<CelestialOnion>());
				}
			}
		}

		public override void UpdateEquip(Item item, Player player)
		{
			if (Terratweaks.Calamitweaks.DRBuffs)
			{
				if (item.type == ItemID.WormScarf)
					player.endurance += 0.03f; // Raise DR from 14% to 17%

				if (item.type == ModContent.ItemType<BloodyWormScarf>())
					player.endurance += 0.07f; // Raise DR from 10% to 17%

				if (item.type == ModContent.ItemType<OldDukeScales>() && !player.GetModPlayer<OldDukeScalesPlayer>().IsTired)
					player.endurance += 0.15f; // Raise DR from 10% to 25%

				if (item.type == ModContent.ItemType<TheSponge>() && player.Calamity().SpongeShieldDurability > 0)
					player.endurance += 0.2f; // Raise DR from 10% to 30%
			}

			if (Terratweaks.Calamitweaks.SummonerAccBuffs)
			{
				if (item.type == ModContent.ItemType<StarTaintedGenerator>())
					player.maxMinions++; // STG's components together give +3, so STG itself should give +3

				if (item.type == ModContent.ItemType<Nucleogenesis>())
					player.maxMinions += 2; // STG + Statis' Curse together give +6, so Nucleogenesis should do the same
			}

			if (Terratweaks.Calamitweaks.DeificAmuletBuff)
			{
				if (item.type == ModContent.ItemType<DeificAmulet>() || item.type == ModContent.ItemType<RampartofDeities>())
				{
					// Charm of Myths effect
					player.lifeRegen += 1;
					player.pStone = true;
				}
			}

			if (Terratweaks.Calamitweaks.AsgardsValorBuff)
			{
				if (item.type == ModContent.ItemType<AsgardsValor>() || item.type == ModContent.ItemType<AsgardianAegis>())
				{
					// Shield of the Ocean effect
					if (player.wet)
					{
						player.statDefense += 5;
						player.moveSpeed += 0.1f;
						player.lifeRegen += 2;
					}

					// Deep Diver effect
					player.ignoreWater = true;
				}
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (Terratweaks.Config.OverrideGraveyardRequirements)
			{
				// Tombstones - Number required varies now
				if (item.type == ItemID.Tombstone || (item.type >= ItemID.GraveMarker && item.type <= ItemID.Obelisk) || (item.type >= ItemID.RichGravestone1 && item.type <= ItemID.RichGravestone5))
				{
					if (tooltips.Any(t => t.Text.Contains("20 of any tombstone")))
					{
						int numTombstones = Terratweaks.Config.GraveyardFunctionality;
						TooltipLine line = tooltips.First(t => t.Text.Contains("20 of any tombstone"));
						line.Text = line.Text.Replace("20 of any tombstone", numTombstones + " of any tombstone");
					}
				}
			}

			if (Terratweaks.Calamitweaks.DRBuffs)
			{
				// Frozen Turtle Shell (and upgrades) and Worm Scarf - Reverted back to their vanilla DR values
				if (tooltips.Any(t => t.Text.Contains("reduces damage")))
				{
					if (item.type == ItemID.FrozenTurtleShell || item.type == ItemID.FrozenShield)
					{
						TooltipLine line = tooltips.First(t => t.Text.Contains("reduces damage by 15%"));
						line.Text = line.Text.Replace("reduces damage by 15%", "reduces damage taken by 25%");
					}
					if (item.type == ModContent.ItemType<RampartofDeities>())
					{
						TooltipLine line = tooltips.First(t => t.Text.Contains("reduces damage taken"));
						line.Text = line.Text.Replace("reduces damage taken", "reduces damage taken by 25%");
					}
				}

				if (item.type == ItemID.WormScarf && tooltips.Any(t => t.Text.Contains("14%")))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains("14%"));
					line.Text = line.Text.Replace("14%", "17%");
				}

				// Bloody Worm Scarf - Increased DR granted from 10% to 17%
				if (item.type == ModContent.ItemType<BloodyWormScarf>() && tooltips.Any(t => t.Text.Contains("10%")))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains("10%"));
					line.Text = line.Text.Replace("10% increased damage reduction and melee damage", "17% increased damage reduction\n10% increased melee damage");
				}

				// Old Duke's Scales - Increased DR granted from 10% to 20%
				if (item.type == ModContent.ItemType<OldDukeScales>() && tooltips.Any(t => t.Text.Contains("damage reduction")))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains("damage reduction"));
					line.Text = line.Text.Replace("10% increased damage reduction", "25% increased damage reduction");
				}

				// The Sponge - Increased DR granted from 10% to 30%
				if (item.type == ModContent.ItemType<TheSponge>() && tooltips.Any(t => t.Text.Contains("reduces damage taken")))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains("reduces damage taken"));
					line.Text = line.Text.Replace("reduces damage taken by 10%", "reduces damage taken by 30%");
				}
			}

			if (Terratweaks.Calamitweaks.SummonerAccBuffs)
			{
				// Star-Tainted Generator - Increased minion slots granted from +2 to +3
				if (item.type == ModContent.ItemType<StarTaintedGenerator>() && tooltips.Any(t => t.Text.Contains("+2 max minions")))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains("+2 max minions"));
					line.Text = line.Text.Replace("+2 max minions", "+3 max minions");
				}

				// Nucleogenesis - Increased minion slots granted from +4 to +6
				if (item.type == ModContent.ItemType<Nucleogenesis>() && tooltips.Any(t => t.Text.Contains("Increases max minions by 4")))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains("Increases max minions by 4"));
					line.Text = line.Text.Replace("Increases max minions by 4", "Increases max minions by 6");
				}
			}

			if (Terratweaks.Calamitweaks.DeificAmuletBuff)
			{
				// Deific Amulet and Rampart of Deities - Now inherit Charm of Myths effect
				if (item.type == ModContent.ItemType<DeificAmulet>() || item.type == ModContent.ItemType<RampartofDeities>())
				{
					TooltipLine line = tooltips.First(t => t.Name.Equals("Tooltip0"));
					tooltips.Insert(tooltips.IndexOf(line) + 1, new TooltipLine(Mod, "MythsEffect", "Provides life regeneration and reduces the cooldown of healing potions by 25%"));
				}
			}

			if (Terratweaks.Calamitweaks.AsgardsValorBuff)
			{
				// Asgard's Valor and Asgardian Aegis - Now inherit Shield of the Ocean and Deep Diver's effects
				if (item.type == ModContent.ItemType<AsgardsValor>() || item.type == ModContent.ItemType<AsgardianAegis>())
				{
					TooltipLine line = tooltips.FindLast(t => t.Name.Contains("Tooltip"));
					tooltips.Insert(tooltips.IndexOf(line) + 1, new TooltipLine(Mod, "MoreShields", "Increases defense by 5 and provides +10% movement speed and +1 HP/s life regeneration when submerged in a liquid\nProvides greatly improved water mobility"));
				}
			}

			if (Terratweaks.Calamitweaks.OnionMasterMode)
			{
				// Celestial Onion - Remove tooltip stating that it does nothing in Master Mode
				if (item.type == ModContent.ItemType<CelestialOnion>())
				{
					TooltipLine line = tooltips.Find(t => t.Name.Equals("Tooltip1"));
					tooltips.Remove(line);
				}
			}
		}
	}
}