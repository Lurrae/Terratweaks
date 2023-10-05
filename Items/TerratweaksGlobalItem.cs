using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
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

	// Anything that is affected by mod configs goes here
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

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			TerratweaksConfig_Client clientConfig = GetInstance<TerratweaksConfig_Client>();
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			if (clientConfig.StatsInTip)
			{
				int idx = -1;

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
			SentryAccSetting dd2AccsStack = GetInstance<TerratweaksConfig>().StackableDD2Accs;

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

			if (item.type == ItemID.EmpressFlightBooster && GetInstance<TerratweaksConfig>().SIRework && !ModLoader.HasMod("CalamityMod"))
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
					if (armorToggles.SpiderRework)
						player.GetDamage(DamageClass.Summon) += 0.02f;
					break;
				case ItemID.SpiderBreastplate:
					if (armorToggles.SpiderRework)
					{
						player.GetDamage(DamageClass.Summon) += 0.02f;
						player.maxTurrets += 1;
					}
					break;

				//Spooky armor
				case ItemID.SpookyHelmet:
				case ItemID.SpookyBreastplate:
				case ItemID.SpookyLeggings:
					if (armorToggles.SpookyRework)
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
							if (armorToggles.SpiderRework)
							tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 7);
							break;
						case ItemID.SpiderBreastplate:
							if (armorToggles.SpiderRework)
							{
								tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 7);
								tooltip.Text += "\n" + Language.GetTextValue("CommonItemTooltip.IncreasesMaxSentriesBy", 1);
							}
							break;
						case ItemID.SpiderGreaves:
							if (armorToggles.SpiderRework)
							tooltip.Text = Language.GetTextValue("CommonItemTooltip.PercentIncreasedSummonDamage", 8);
							break;
						case ItemID.SpookyHelmet:
						case ItemID.SpookyBreastplate:
						case ItemID.SpookyLeggings:
							if (armorToggles.SpookyRework)
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

			if (head.type == ItemID.SpiderMask && body.type == ItemID.SpiderBreastplate && legs.type == ItemID.SpiderGreaves && armorToggles.SpiderRework)
			{
				return "Spider";
			}

			if (CobaltHeads.Contains(head.type) && body.type == ItemID.CobaltBreastplate && legs.type == ItemID.CobaltLeggings && armorToggles.CobaltRework)
			{
				return "Cobalt";
			}
			if (MythrilHeads.Contains(head.type) && body.type == ItemID.MythrilChainmail && legs.type == ItemID.MythrilGreaves && armorToggles.MythrilRework)
			{
				return "Mythril";
			}
			if (AdamantiteHeads.Contains(head.type) && body.type == ItemID.AdamantiteBreastplate && legs.type == ItemID.AdamantiteLeggings && armorToggles.AdamantiteRework)
			{
				return "Adamantite";
			}

			if (head.type == ItemID.SpookyHelmet && body.type == ItemID.SpookyBreastplate && legs.type == ItemID.SpookyLeggings && armorToggles.SpookyRework)
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
}