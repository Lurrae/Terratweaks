using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.Items.Placeables.PlaceableTurrets;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksItems : GlobalItem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override void SetStaticDefaults()
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// Reduce banner kill counts for some enemies
			if (calamitweaks.EzCalBanners)
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
			if (calamitweaks.CraftableHostileTurrets)
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

		public override void UpdateEquip(Item item, Player player)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.DRBuffs)
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

			if (calamitweaks.SummonerAccBuffs)
			{
				if (item.type == ModContent.ItemType<StarTaintedGenerator>())
					player.maxMinions++; // STG's components together give +3, so STG itself should give +3

				if (item.type == ModContent.ItemType<Nucleogenesis>())
					player.maxMinions += 2; // STG + Statis' Curse together give +6, so Nucleogenesis should do the same
			}

			if (calamitweaks.DeificAmuletBuff)
			{
				if (item.type == ModContent.ItemType<DeificAmulet>() || item.type == ModContent.ItemType<RampartofDeities>())
				{
					// Charm of Myths effect
					player.lifeRegen += 1;
					player.pStone = true;
				}
			}

			if (calamitweaks.AsgardsValorBuff)
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
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.RevertGraveyards)
			{
				// Tombstones - Only requires 7 to create a graveyard now
				if (item.type == ItemID.Tombstone || (item.type >= ItemID.GraveMarker && item.type <= ItemID.Obelisk) || (item.type >= ItemID.RichGravestone1 && item.type <= ItemID.RichGravestone5))
				{
					if (tooltips.Any(t => t.Text.Contains("20 of any tombstone")))
					{
						TooltipLine line = tooltips.First(t => t.Text.Contains("20 of any tombstone"));
						line.Text = line.Text.Replace("20 of any tombstone", "7 of any tombstone");
					}
				}
			}

			if (calamitweaks.DRBuffs)
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

			if (calamitweaks.SummonerAccBuffs)
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

			if (calamitweaks.DeificAmuletBuff)
			{
				// Deific Amulet and Rampart of Deities - Now inherit Charm of Myths effect
				if (item.type == ModContent.ItemType<DeificAmulet>() || item.type == ModContent.ItemType<RampartofDeities>())
				{
					TooltipLine line = tooltips.First(t => t.Name.Equals("Tooltip0"));
					tooltips.Insert(tooltips.IndexOf(line) + 1, new TooltipLine(Mod, "MythsEffect", "Provides life regeneration and reduces the cooldown of healing potions by 25%"));
				}
			}

			if (calamitweaks.AsgardsValorBuff)
			{
				// Asgard's Valor and Asgardian Aegis - Now inherit Shield of the Ocean and Deep Diver's effects
				if (item.type == ModContent.ItemType<AsgardsValor>() || item.type == ModContent.ItemType<AsgardianAegis>())
				{
					TooltipLine line = tooltips.FindLast(t => t.Name.Contains("Tooltip"));
					tooltips.Insert(tooltips.IndexOf(line) + 1, new TooltipLine(Mod, "MoreShields", "Increases defense by 5 and provides +10% movement speed and +1 HP/s life regeneration when submerged in a liquid\nProvides greatly improved water mobility"));
				}
			}
		}
	}
}