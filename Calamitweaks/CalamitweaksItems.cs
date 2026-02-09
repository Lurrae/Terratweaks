using CalamityMod;
using CalamityMod.Items;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.GemTech;
using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.PlaceableTurrets;
using CalamityMod.Items.SummonItems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terratweaks.Items;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksItems : GlobalItem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override void Load()
		{
			Dictionary<object, Array> entriesToReplace = new();
			List<int> entriesToRemove = new();

			FieldInfo currentTweaksField = typeof(CalamityGlobalItem).GetField("currentTweaks", BindingFlags.NonPublic | BindingFlags.Static);
			IDictionary curTweaksDict = (IDictionary)currentTweaksField.GetValue(null);

			foreach (var key in curTweaksDict.Keys)
			{
				int itemType = (int)key;
				Item item = ContentSamples.ItemsByType[itemType];

				object tweaks = curTweaksDict[key];
				Array tweaksList = (Array)tweaks;
				int i = -1;

				// Revert all changes made to vanilla boss summoning items with the config option active
				if (Terratweaks.Calamitweaks.ConsumableCalBossSummons && ItemChanges.IsBossSummon(item, false, false))
				{
					entriesToRemove.Add(itemType);
				}

				// Revert any use time changes applied to pickaxe items
				if (Terratweaks.Calamitweaks.RevertPickSpeedBuffs && item.pick > 0)
				{
					foreach (object tweak in tweaksList)
					{
						string tweakName = tweak.GetType().Name;

						// Only do stuff for use time changes
						if (tweakName.Equals("UseTimeExactRule"))
						{
							break;
						}

						i++;
					}
				}

				// Found a tweak to remove, remove it!
				if (i > -1 && i < tweaksList.Length)
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

			// Replace all the marked arrays, if we have a config active that should make us do so
			if (Terratweaks.Calamitweaks.RevertPickSpeedBuffs)
			{
				foreach (KeyValuePair<object, Array> pair in entriesToReplace)
				{
					curTweaksDict[pair.Key] = pair.Value;
				}
			}

			// Remove any entries we wanna fully remove, like the boss summon changes
			if (Terratweaks.Calamitweaks.ConsumableCalBossSummons)
			{
				foreach (int itemID in entriesToRemove)
				{
					curTweaksDict.Remove(itemID);
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
				ItemID.Sets.KillsToBanner[ModContent.ItemType<BurrowerBanner>()] = 25;
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

		public static readonly List<int> BossSummonBlacklist = new()
		{
			ModContent.ItemType<Starcore>(),			// Designed to be a non-consumable counterpart of Titan Hearts
			ModContent.ItemType<ProfanedCore>(),		// Dropped by Profaned Guardians, so it'd be annoying to get multiples of
			ModContent.ItemType<MarkofProvidence>(),	// Dropped by Providence, so it'd be annoying to get multiples of
			ModContent.ItemType<CeremonialUrn>(),		// Designed to be a non-consumable counterpart of Ashes of Calamity
			ModContent.ItemType<NO>()					// It would be cruel to force players to farm this item with how unfair THE LORDE is
		};

		public static bool IsVanillaOrCalBossSummon(Item item, bool ignoreBlacklist = false)
		{
			// Always return false for null items, just in case
			if (item == null)
				return false;

			// Return false for any non-Calamity modded items
			if (item.ModItem != null && !item.ModItem.Mod.Name.Equals("CalamityMod"))
				return false;

			// Ignore items on the blacklist, unless told otherwise
			if (!ignoreBlacklist && BossSummonBlacklist.Contains(item.type))
				return false;

			if (item.TryGetGlobalItem(out BossSummonStuff globalItem))
			{
				var ogConsumable = globalItem.OriginalConsumableValue;
				var ogMaxStack = globalItem.OriginalMaxStackValue;

				if (ogConsumable.HasValue && ogMaxStack.HasValue)
					return ItemID.Sets.SortingPriorityBossSpawns[item.type] != -1 && ((!ogConsumable.Value && ogMaxStack.Value == 1) || (ogConsumable.Value && (item.ResearchUnlockCount == 1 || item.ResearchUnlockCount == 3)));
			}

			return ItemID.Sets.SortingPriorityBossSpawns[item.type] != -1 && ((!item.consumable && item.maxStack == 1) || (item.consumable && (item.ResearchUnlockCount == 1 || item.ResearchUnlockCount == 3)));
		}

		public override void SetDefaults(Item item)
		{
			// Add a tooltip specifying that certain items were modified by Terratweaks while the corresponding configs are active
			#region StatsModifiedBy stuff
			bool itemIsModified = false;

			if (Terratweaks.Calamitweaks.AsgardsValorBuff && (item.type == ModContent.ItemType<OrnateShield>() || item.type == ModContent.ItemType<AsgardsValor>() || item.type == ModContent.ItemType<AsgardianAegis>()))
			{
				itemIsModified = true;

				if (item.type == ModContent.ItemType<AsgardsValor>())
				{
					item.defense += 4;
				}
				else if (item.type == ModContent.ItemType<AsgardianAegis>())
				{
					item.defense += 6;
				}
			}

			if (Terratweaks.Calamitweaks.DeificAmuletBuff && (item.type == ModContent.ItemType<DeificAmulet>() || item.type == ModContent.ItemType<RampartofDeities>()))
				itemIsModified = true;

			if (Terratweaks.Calamitweaks.DRBuffs && DRItems.Contains(item.type))
				itemIsModified = true;

			if (Terratweaks.Calamitweaks.SummonerAccBuffs && (item.type == ModContent.ItemType<StarTaintedGenerator>() || item.type == ModContent.ItemType<Nucleogenesis>()))
				itemIsModified = true;

			// In the case of modified boss summons, if the item's from Calamity we also need to set its consumability and max stack size
			if (Terratweaks.Calamitweaks.ConsumableCalBossSummons && IsVanillaOrCalBossSummon(item))
			{
				itemIsModified = true;

				// Since we already filtered out all non-Calamity items, checking if the item has a ModItem attached guarantees it's a Calamity summon
				if (item.ModItem != null)
				{
					item.maxStack = Item.CommonMaxStack; // 9999
					item.consumable = true;
				}
			}

			if (itemIsModified && !Terratweaks.ClientConfig.HideItemModifiedTips)
				item.StatsModifiedBy.Add(Mod);
			#endregion
		}

		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			if (Terratweaks.Calamitweaks.OnionMasterMode)
			{
				if (item.type == ItemID.MoonLordBossBag)
				{
					// Add a drop rule for if the player is in Master Mode and doesn't already have the slot
					// Calamity already handles dropping the Onion in non-Master worlds, so we don't need to worry about that
					itemLoot.AddIf((info) => !info.player.Calamity().extraAccessoryML && Main.masterMode, ModContent.ItemType<CelestialOnion>());
				}
			}
		}

		public override void UpdateEquip(Item item, Player player)
		{
			if (Terratweaks.Calamitweaks.DRBuffs)
			{
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
				// Revert nerf to Ornate Shield
				if (item.type == ModContent.ItemType<OrnateShield>())
				{
					player.buffImmune[BuffID.Chilled] = true;
					player.buffImmune[BuffID.Frostburn] = true;
					player.buffImmune[BuffID.Frostburn2] = true;
					player.buffImmune[BuffID.Frozen] = true;
				}

				// Apply effects of new components
				if (item.type == ModContent.ItemType<AsgardsValor>() || item.type == ModContent.ItemType<AsgardianAegis>())
				{
					// Ankh Shield effect
					player.noKnockback = true;
					player.fireWalk = true;
					player.buffImmune[BuffID.Bleeding] = true;
					player.buffImmune[BuffID.BrokenArmor] = true;
					player.buffImmune[BuffID.Confused] = true;
					player.buffImmune[BuffID.Cursed] = true;
					player.buffImmune[BuffID.Darkness] = true;
					player.buffImmune[BuffID.Poisoned] = true;
					player.buffImmune[BuffID.Silenced] = true;
					player.buffImmune[BuffID.Slow] = true;
					player.buffImmune[BuffID.Stoned] = true;
					player.buffImmune[BuffID.Weak] = true;
					player.buffImmune[BuffID.WindPushed] = true;

					// All-Encompassing Ankh Shield adds Chromatic Cloak effect
					// Ornate Shield's buff already covers the Chilled and Frozen immunity that would otherwise need to be added too
					if (Terratweaks.Config.AllEncompassingAnkhShield && !player.controlDownHold)
						player.shimmerImmune = true;

					// Ornate Shield effect
					player.buffImmune[BuffID.Chilled] = true;
					player.buffImmune[BuffID.Frostburn] = true;
					player.buffImmune[BuffID.Frostburn2] = true;
					player.buffImmune[BuffID.Frozen] = true;

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

			// Vanilla/Calamity boss summons - Remove the added line about being non-consumable
			if (Terratweaks.Calamitweaks.ConsumableCalBossSummons && IsVanillaOrCalBossSummon(item))
			{
				// Vanilla items have a \n added to an existing line, so we have to remove that instead of deleting the entire line
				if (item.ModItem == null)
				{
					if (tooltips.Any(t => t.Text.Contains("\nNot consumable")))
					{
						TooltipLine line = tooltips.First(t => t.Text.Contains("\nNot consumable"));
						line.Text = line.Text.Replace("\nNot consumable", "");
					}
				}
				// Calamity items have a whole new tooltip line that we can remove instead
				else
				{
					if (tooltips.Any(t => t.Text.Contains("Not consumable")))
					{
						TooltipLine line = tooltips.First(t => t.Text.Contains("Not consumable"));
						tooltips.Remove(line);
					}
				}
			}

			if (Terratweaks.Calamitweaks.DRBuffs)
			{
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
				if (item.type == ModContent.ItemType<TheSponge>() && tooltips.Any(t => t.Text.Contains("damage reduction")))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains("damage reduction"));
					line.Text = line.Text.Replace("10% damage reduction", "30% damage reduction");
				}
			}

			if (Terratweaks.Calamitweaks.SummonerAccBuffs)
			{
				// Star-Tainted Generator - Increased minion slots granted from +2 to +3
				if (item.type == ModContent.ItemType<StarTaintedGenerator>() && tooltips.Any(t => t.Text.Contains(Language.GetTextValueWith("CommonItemTooltip.IncreasesMaxMinionsBy", 2))))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains(Language.GetTextValueWith("CommonItemTooltip.IncreasesMaxMinionsBy", 2)));
					line.Text = line.Text.Replace(Language.GetTextValueWith("CommonItemTooltip.IncreasesMaxMinionsBy", 2), Language.GetTextValueWith("CommonItemTooltip.IncreasesMaxMinionsBy", 3));
				}

				// Nucleogenesis - Increased minion slots granted from +4 to +6
				if (item.type == ModContent.ItemType<Nucleogenesis>() && tooltips.Any(t => t.Text.Contains(Language.GetTextValueWith("CommonItemTooltip.IncreasesMaxMinionsBy", 4))))
				{
					TooltipLine line = tooltips.First(t => t.Text.Contains(Language.GetTextValueWith("CommonItemTooltip.IncreasesMaxMinionsBy", 4)));
					line.Text = line.Text.Replace(Language.GetTextValueWith("CommonItemTooltip.IncreasesMaxMinionsBy", 4), Language.GetTextValueWith("CommonItemTooltip.IncreasesMaxMinionsBy", 6));
				}
			}

			if (Terratweaks.Calamitweaks.DeificAmuletBuff && !PlayerInput.Triggers.Current.SmartCursor && tooltips.Any(t => t.Name.Equals("Tooltip0")))
			{
				// Deific Amulet and Rampart of Deities - Now inherit Charm of Myths effect
				if (item.type == ModContent.ItemType<DeificAmulet>() || item.type == ModContent.ItemType<RampartofDeities>())
				{
					TooltipLine line = tooltips.First(t => t.Name.Equals("Tooltip0"));
					tooltips.Insert(tooltips.IndexOf(line) + 1, new TooltipLine(Mod, "MythsEffect", Language.GetTextValue("ItemTooltip.CharmofMyths")));
				}
			}

			if (Terratweaks.Calamitweaks.AsgardsValorBuff && tooltips.Any(t => t.Name.Equals("Tooltip0")))
			{
				// Ornate Shield - Now grants immunity to Chilled, Frozen, Frostburn, and Frostbite
				if (item.type == ModContent.ItemType<OrnateShield>())
				{
					TooltipLine line = tooltips.FindLast(t => t.Name.Contains("Tooltip"));
					tooltips.Insert(tooltips.IndexOf(line) + 1, new TooltipLine(Mod, "ColdImmunity", Language.GetTextValue("Mods.Terratweaks.Common.OrnateShieldTip")));
				}

				// Asgard's Valor and Asgardian Aegis - Now inherit Shield of the Ocean and Deep Diver's effects
				if (item.type == ModContent.ItemType<AsgardsValor>() || item.type == ModContent.ItemType<AsgardianAegis>())
				{
					TooltipLine line = tooltips.FindLast(t => t.Name.Contains("Tooltip"));
					tooltips.Insert(tooltips.IndexOf(line) + 1, new TooltipLine(Mod, "MoreShields", Language.GetTextValue("Mods.Terratweaks.Common.AsgardTip")));
					
					// Chromatic Cloak tooltip
					if (Terratweaks.Config.AllEncompassingAnkhShield)
					{
						tooltips.Insert(tooltips.IndexOf(line) + 2, new TooltipLine(Mod, "ShimmerTip", Language.GetTextValue("ItemTooltip.ShimmerCloak")));
					}
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