using Terraria.ModLoader.Config;
using System.ComponentModel;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System.Collections.Generic;
using System.Linq;
using Terratweaks.Items;

namespace Terratweaks
{
	public enum DummySetting
	{
		Off,
		Limited,
		On
	}

	public enum SentryAccSetting
	{
		Off,
		Limited,
		On
	}

	public enum DamageVarianceSetting
	{
		Off,
		Limited,
		On
	}

	public enum CoinTheftSetting
	{
		Off,
		Limited,
		On
	}

	public enum SturdyLarvaeSetting
	{
		Off,
		Env,
		Peace,
		Bee
	}

	public enum ConsumableBossSummonsSetting
	{
		Off,
		ForceConsumable,
		ForceNonConsumable
	}

	public class TerratweaksConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		private static ItemDefinition OldAutoFishingItem = new();

		// Extra tweak categories
		[Header("Categories")]

		public CraftableUncraftables craftableUncraftables = new();

		public CalTweaks calamitweaks = new();

		public ThorTweaks thoritweaks = new();

		public AlchTweaks alchemitweaks = new();

		public EmodeTweaks eternitweaks = new();

		// Block & biome tweaks
		[Header("BlockBiomeTweaks")]

		[DefaultValue(true)]
		public bool BetterCrackedBricks { get; set; }

		[DefaultValue(true)]
		public bool BombableMeteorite { get; set; }

		[DefaultValue(false)]
		public bool OverrideGraveyardRequirements { get; set; }

		[Increment(1)]
		[Range(1, 18)]
		[DefaultValue(4)]
		[Slider]
		public int GraveyardVisuals { get; set; }

		[Increment(1)]
		[Range(1, 18)]
		[DefaultValue(7)]
		[Slider]
		public int GraveyardFunctionality { get; set; }

		[Increment(1)]
		[Range(1, 18)]
		[DefaultValue(9)]
		[Slider]
		public int GraveyardMax { get; set; }

		[DefaultValue(SturdyLarvaeSetting.Env)]
		public SturdyLarvaeSetting SturdyLarvae { get; set; }

		// Boss and Event Tweaks
		[Header("BossTweaks")]

		[DefaultValue(false)]
		public bool EarlyLacewing { get; set; }

		[DefaultValue(true)]
		public bool NoLingeringProbes { get; set; }

		[DefaultValue(0)]
		[Range(0, 10)]
		[Slider]
		public int NPCDeathsToCallOffInvasion { get; set; }

		[DefaultValue(0)]
		[Range(0, 10)]
		[Slider]
		public int PlayerDeathsToCallOffInvasion { get; set; }

		[DefaultValue(true)]
		public bool DeerclopsRegens { get; set; }

		[Increment(60)]
		[Range(60, 3600)]
		[DefaultValue(600)]
		public int DeerRegenAmt { get; set; }

		[DefaultValue(false)]
		public bool ForceBossContactDamage { get; set; }

		[DefaultValue(false)]
		public bool MoreOOAMedals { get; set; }

		[DefaultValue(false)]
		public bool NoBossSpawns { get; set; }

		[DefaultValue(false)]
		public bool NoEventSpawns { get; set; }

		[DefaultValue(true)]
		public bool PostEyeSandstorms { get; set; }

		[DefaultValue(1)]
		[Range(0f, 10f)]
		[Increment(0.25f)]
		public float BossesLowerSpawnRates { get; set; }

		[ReloadRequired]
		[DefaultValue(0)]
		public int TerraprismaDropRate { get; set; }

		// Buff Tweaks
		[Header("BuffTweaks")]

		[DefaultValue(true)]
		public bool DangersenseHighlightsSilt { get; set; }

		[DefaultValue(true)]
		public bool DangersenseIgnoresThinIce { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool InfiniteCakeSlice { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PersistentStationBuffs { get; set; }

		[DefaultValue(true)]
		public bool SpelunkerHighlightsHellstone { get; set; }

		[DefaultValue(true)]
		public bool WarmthGivesColdImmunity { get; set; }

		// Enemy Tweaks
		[Header("EnemyTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool GoblinsDropCloth { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PillarEnemiesDropFragments { get; set; }

		[DefaultValue(true)]
		public bool NoEnemyInvulnerability { get; set; }

		[DefaultValue(false)]
		public bool NoCasterContactDamage { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EzDyeBanners { get; set; }

		[DefaultValue(false)]
		public bool SmartMimics { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool SmartNymphs { get; set; }

		[DefaultValue(DummySetting.Limited)]
		public DummySetting DummyFix { get; set; }

		// Expert Mode Accessory Buffs
		[Header("ExpertAccTweaks")]

		[DefaultValue(true)]
		public bool BoneGlove { get; set; }

		[DefaultValue(true)]
		public bool BoneHelm { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool CultistGravGlobe { get; set; }

		[DefaultValue(true)]
		public bool HivePack { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PlaceableGravGlobe { get; set; }

		[Slider]
		[Increment(5)]
		[Range(5, 200)]
		[DefaultValue(125)]
		public int GravGlobeRange { get; set; }

		[DefaultValue(true)]
		public bool RoyalGel { get; set; }

		[DefaultValue(true)]
		public bool EyeShield { get; set; }

		[DefaultValue(true)]
		public bool WormBrain { get; set; }

		// Expert Mode Tweaks
		[Header("ExpertTweaks")]

		[DefaultValue(true)]
		public bool LavalessLavaSlimes { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool JungleBossBags { get; set; }

		[ReloadRequired]
		[DefaultValue(CoinTheftSetting.Limited)]
		public CoinTheftSetting NoCoinTheft { get; set; }

		[DefaultValue(false)]
		public bool NoExpertScaling { get; set; }

		[DefaultValue(false)]
		public bool NoExpertDebuffTimes { get; set; }

		[DefaultValue(false)]
		public bool NoExpertFreezingWater { get; set; }

		// For the Worthy Tweaks
		[Header("FtwTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool NerfSkyCrates { get; set; }

		[DefaultValue(false)]
		public bool NoMobGriefing { get; set; }

		// General Item Tweaks
		[Header("ItemTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool AllEncompassingAnkhShield { get; set; }

		[DefaultValue(false)]
		public bool AutoFishing { get; set; }

		public ItemDefinition AutoFishingItem { get; set; } = new(ItemID.AnglerTackleBag);

		[Slider]
		[Increment(5)]
		[Range(0,100)]
		[DefaultValue(80)]
		public int AutoFishingBobberCount { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ChesterRework { get; set; }

		[DefaultValue(DamageVarianceSetting.Off)]
		public DamageVarianceSetting NoDamageVariance { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool CoinsBypassEncStone { get; set; }

		[DefaultValue(false)]
		public bool CritsBypassDefense { get; set; }

		[DefaultValue(true)]
		public bool DeerWeaponsRework { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool FrostHydraBuff { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool LunarWingsPreML { get; set; }

		[DefaultValue(false)]
		public bool NoDiminishingReturns { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool NonConsumableBossSummons { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool OasisCrateBuff { get; set; }

		[ReloadRequired]
		[Slider]
		[Range(1, 50)]
		[DefaultValue(35)]
		public int OasisCrateOdds { get; set; }

		[DefaultValue(true)]
		public bool ObsidianSkullOnFireImmunity { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ReaverSharkTweaks { get; set; }

		[DefaultValue(true)]
		public bool ResummonMinions { get; set; }

		[DefaultValue(true)]
		public bool KillSentries { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool SIRework { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool SpectreNeedsDunerider { get; set; }

		[DefaultValue(SentryAccSetting.Limited)]
		public SentryAccSetting StackableDD2Accs { get; set; }

		[DefaultValue(true)]
		public bool ManaFreeSummoner { get; set; }

		[Expand(false)]
		public List<NPCDefinition> BossBlacklist { get; set; } = new()
		{
			new NPCDefinition(NPCID.LunarTowerSolar),
			new NPCDefinition(NPCID.LunarTowerVortex),
			new NPCDefinition(NPCID.LunarTowerNebula),
			new NPCDefinition(NPCID.LunarTowerStardust),
			new NPCDefinition(NPCID.MartianSaucer),
			new NPCDefinition(NPCID.MartianSaucerCannon),
			new NPCDefinition(NPCID.MartianSaucerCore),
			new NPCDefinition(NPCID.MartianSaucerTurret)
		};

		[Expand(false)]
		public List<NPCDefinition> BossWhitelist { get; set; } = new()
		{
			new NPCDefinition(NPCID.EaterofWorldsHead)
		};

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ToolboxHoC { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool UmbrellaHatRework { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool OreUnification { get; set; }

		// Joke Tweaks
		[Header("JokeTweaks")]

		[DefaultValue(false)]
		public bool FtwBombPots { get; set; }

		[DefaultValue(false)]
		public bool FtwBombTrees { get; set; }

		[DefaultValue(false)]
		public bool DrunkWaldo { get; set; }

		[DefaultValue(false)]
		public bool PaperCuts { get; set; }

		// Misc. Tweaks
		[Header("MiscTweaks")]

		[DefaultValue(true)]
		public bool BannersDontSpamChat { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool BetterBestiary { get; set; }

		[DefaultValue(false)]
		public bool GoldCrittersDropGold { get; set; }

		[Slider]
		[Range(0, 10)]
		[DefaultValue(3)]
		public int GoldCritterMinValue { get; set; }

		[Slider]
		[Range(0, 10)]
		[DefaultValue(5)]
		public int GoldCritterMaxValue { get; set; }

		[DefaultValue(false)]
		public bool HarmlessFallenStars { get; set; }

		// Town NPC Tweaks
		[Header("TownNpcTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool SolutionsOnGFB { get; set; }

		[DefaultValue(true)]
		public bool DyeTraderShopExpansion { get; set; }

		[DefaultValue(false)]
		public bool HouseSizeAffectsHappiness { get; set; }

		[DefaultValue(true)]
		public bool BetterHappiness { get; set; }

		[DefaultValue(false)]
		public bool BoundNPCsImmune { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool MechanicSellsToolbox { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool NPCsSellMinecarts { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TownNPCsSellWeapons { get; set; }

		[DefaultValue(true)]
		public bool OldChestDungeon { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool NoBiomeRequirements { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool SoilSolutionsPreML { get; set; }

		// Vanilla Armorset Reworks
		[Header("ArmorTweaks")]

		[DefaultValue(true)]
		public bool AdamantiteSetBonus { get; set; }

		[DefaultValue(true)]
		public bool CobaltSetBonus { get; set; }

		[DefaultValue(true)]
		public bool ConvertMonkArmor { get; set; }

		[DefaultValue(true)]
		public bool MythrilSetBonus { get; set; }

		[DefaultValue(true)]
		public bool SpiderSetBonus { get; set; }

		[DefaultValue(true)]
		public bool SpookySetBonus { get; set; }

		[DefaultValue(true)]
		public bool StardustArmorBuff { get; set; }

		public override bool NeedsReload(ModConfig pendingConfig)
		{
			if (pendingConfig is TerratweaksConfig pendingTtConfig)
			{
				// Check all craftable uncraftable configs for forced reloads
				// TODO: This will need to be removed if/when tmod updates to check nested ReloadRequireds
				if (craftableUncraftables.GemCritters != pendingTtConfig.craftableUncraftables.GemCritters ||
					craftableUncraftables.PlanterBoxes != pendingTtConfig.craftableUncraftables.PlanterBoxes ||
					craftableUncraftables.DungeonFurniture != pendingTtConfig.craftableUncraftables.DungeonFurniture ||
					craftableUncraftables.ObsidianFurniture != pendingTtConfig.craftableUncraftables.ObsidianFurniture ||
					craftableUncraftables.StructureBanners != pendingTtConfig.craftableUncraftables.StructureBanners ||
					craftableUncraftables.Moss != pendingTtConfig.craftableUncraftables.Moss ||
					craftableUncraftables.Gravestones != pendingTtConfig.craftableUncraftables.Gravestones ||
					craftableUncraftables.Trophies != pendingTtConfig.craftableUncraftables.Trophies ||
					craftableUncraftables.ClothierVoodooDoll != pendingTtConfig.craftableUncraftables.ClothierVoodooDoll ||
					craftableUncraftables.TempleTraps != pendingTtConfig.craftableUncraftables.TempleTraps ||
					craftableUncraftables.ShimmerBottomlessAndSponges != pendingTtConfig.craftableUncraftables.ShimmerBottomlessAndSponges ||
					craftableUncraftables.TeamBlocks != pendingTtConfig.craftableUncraftables.TeamBlocks ||
					craftableUncraftables.PrehardUnobtainables != pendingTtConfig.craftableUncraftables.PrehardUnobtainables ||
					craftableUncraftables.ShimmerBossDrops != pendingTtConfig.craftableUncraftables.ShimmerBossDrops ||
					craftableUncraftables.GeyserTraps != pendingTtConfig.craftableUncraftables.GeyserTraps ||
					craftableUncraftables.ShimmerBlackLens != pendingTtConfig.craftableUncraftables.ShimmerBlackLens ||
					craftableUncraftables.EarlyEchoBlocks != pendingTtConfig.craftableUncraftables.EarlyEchoBlocks ||
					craftableUncraftables.ConvertibleMushrooms != pendingTtConfig.craftableUncraftables.ConvertibleMushrooms)
				{
					return true;
				}

				// Check all Calamitweaks configs for forced reloads
				// TODO: Ditto the above todo (remove if/when tmod fixes the issue that makes this necessary)
				if (calamitweaks.AsgardsValorBuff != pendingTtConfig.calamitweaks.AsgardsValorBuff ||
					calamitweaks.CombinedStationSupport != pendingTtConfig.calamitweaks.CombinedStationSupport ||
					calamitweaks.ConsumableCalBossSummons != pendingTtConfig.calamitweaks.ConsumableCalBossSummons ||
					calamitweaks.CraftableHostileTurrets != pendingTtConfig.calamitweaks.CraftableHostileTurrets ||
					calamitweaks.DeificAmuletBuff != pendingTtConfig.calamitweaks.DeificAmuletBuff ||
					calamitweaks.EzCalBanners != pendingTtConfig.calamitweaks.EzCalBanners ||
					calamitweaks.RevertPickSpeedBuffs != pendingTtConfig.calamitweaks.RevertPickSpeedBuffs)
				{
					return true;
				}

				// Check all Thoritweaks configs for forced reloads
				// TODO: Once again, ditto the above todos
				if (thoritweaks.CombinedStationSupport != pendingTtConfig.thoritweaks.CombinedStationSupport ||
					thoritweaks.ZenithRecipeOverhaul != pendingTtConfig.thoritweaks.ZenithRecipeOverhaul)
				{
					return true;
				}

				// Check all Alchemitweaks configs for forced reloads
				// TODO: Once again, ditto the above todos
				if (alchemitweaks.AntiCheese != pendingTtConfig.alchemitweaks.AntiCheese ||
					alchemitweaks.DisableCustomPotions != pendingTtConfig.alchemitweaks.DisableCustomPotions)
				{
					return true;
				}
			}

			return base.NeedsReload(pendingConfig);
		}

		public override void OnChanged()
		{
			if (OverrideGraveyardRequirements)
			{
				SceneMetrics.GraveyardTileMax = GraveyardMax * 4;
				SceneMetrics.GraveyardTileMin = GraveyardVisuals * 4;
				SceneMetrics.GraveyardTileThreshold = GraveyardFunctionality * 4;
			}
			else
			{
				if (ModLoader.HasMod("CalamityMod"))
				{
					SceneMetrics.GraveyardTileMax = 15 * 4;
					SceneMetrics.GraveyardTileMin = 10 * 4;
					SceneMetrics.GraveyardTileThreshold = 13 * 4;
				}
				else
				{
					SceneMetrics.GraveyardTileMax = 7 * 4;
					SceneMetrics.GraveyardTileMin = 4 * 4;
					SceneMetrics.GraveyardTileThreshold = 9 * 4;
				}
			}

			UpdateAutoFishingAccList();
			UpdateSpectreBootsUpgradeAccList();
		}

		/// <summary>
		/// Updates a provided list of accessories to include all accessories which are crafted from that item, recursively
		/// For example, passing in Spectre Boots will add Spectre, Lightning, Frostspark, and Terraspark Boots, as well as any modded upgrades to any of those boots (so long as the upgrades are still considered accessories)
		/// </summary>
		/// <param name="referenceItem">The base item to check. Any item that is crafted from this item or an upgrade of this item will be added to the list passed in as the second parameter</param>
		/// <param name="listToUpdate">The list of items to be updated. Be warned that the list will be cleared out first!</param>
		void UpdateAnAccList(int referenceItem, List<int> listToUpdate)
		{
			// Clear out the list of valid accs
			listToUpdate.Clear();

			// We only need to check recipes which craft an accessory
			List<Recipe> accessoryRecipes = Main.recipe.Where(r => !r.Disabled && r.createItem.accessory).ToList();

			List<int> checkedItems = new();
			List<int> itemsToCheck = new() { referenceItem };

			while (itemsToCheck.Count > 0)
			{
				int itemType = itemsToCheck[0];

				// Already checked this item, skip it
				if (checkedItems.Contains(itemType))
				{
					itemsToCheck.Remove(itemType);
					continue;
				}

				// Add this item to the list of valid accessories if it isn't already included
				if (!listToUpdate.Contains(itemType))
					listToUpdate.Add(itemType);

				// Look through the list of all recipes that craft accessories, and find ones which require this item
				foreach (Recipe recipe in accessoryRecipes)
				{
					if (recipe.ContainsIngredient(itemType))
					{
						itemsToCheck.Add(recipe.createItem.type);
					}
				}

				// Add it to the list of checked items so we don't check it again
				checkedItems.Add(itemType);
				itemsToCheck.Remove(itemType);
			}
		}

		void UpdateAutoFishingAccList()
		{
			int newType = AutoFishingItem.Type;

			// Non-accessory item means we don't need to equip anything specific
			if (!ContentSamples.ItemsByType[newType].accessory)
			{
				// Clear out the list of valid accs, since we want to remove it
				FishingPlayer.ValidAccessoryTypes.Clear();
			}
			// If the item required for auto-fishing has changed, we need to recalculate the list of valid accessories
			else if (OldAutoFishingItem.Type != newType || FishingPlayer.ValidAccessoryTypes.Count < 1)
			{
				UpdateAnAccList(newType, FishingPlayer.ValidAccessoryTypes);
			}

			// Once this is all said and done, update the old value with the new one
			OldAutoFishingItem = AutoFishingItem;
		}

		void UpdateSpectreBootsUpgradeAccList()
		{
			// If the config's disabled, or if there are no recipes for Spectre Boots that use Dunerider Boots, just leave the list empty
			if (!SpectreNeedsDunerider || !Main.recipe.Any(r => !r.Disabled && r.HasResult(ItemID.SpectreBoots) && r.HasIngredient(ItemID.SandBoots)))
			{
				ModifiedStatsTip.SpectreBootsUpgrades.Clear();
			}
			else
			{
				UpdateAnAccList(ItemID.SpectreBoots, ModifiedStatsTip.SpectreBootsUpgrades);
			}
		}
	}

	public class TerratweaksConfig_Client : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		// Buff Tweaks
		[Header("BuffTweaks")]

		[DefaultValue(false)]
		public bool OverrideSpelunkerGlow { get; set; }

		[ColorNoAlpha]
		[DefaultValue(typeof(Color), "255, 255, 255, 255")]
		public Color TreasureGlowColor { get; set; }

		[DefaultValue(false)]
		public bool OverrideDangerGlow { get; set; }

		[ColorNoAlpha]
		[DefaultValue(typeof(Color), "255, 255, 255, 255")]
		public Color DangerGlowColor { get; set; }

		[DefaultValue(false)]
		public bool OverrideHunterGlow { get; set; }

		[ColorNoAlpha]
		[DefaultValue(typeof(Color), "255, 255, 255, 255")]
		public Color EnemyGlowColor { get; set; }

		[ColorNoAlpha]
		[DefaultValue(typeof(Color), "255, 255, 255, 255")]
		public Color FriendlyGlowColor { get; set; }

		// Misc. Tweaks
		[Header("MiscTweaks")]

		[DefaultValue(true)]
		public bool NoRandomCrit { get; set; }

		// Tooltip Tweaks
		[Header("TooltipTweaks")]

		[DefaultValue(true)]
		public bool EstimatedDPS { get; set; }

		[DefaultValue(true)]
		public bool GrammarCorrections { get; set; }

		[DefaultValue(true)]
		public bool HammushTooltip { get; set; }

		[DefaultValue(true)]
		public bool HideFavoritedTips { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool HideItemModifiedTips { get; set; }

		[DefaultValue(false)]
		public bool HideMilestoneTips { get; set; }

		[DefaultValue(true)]
		public bool PermBuffTips { get; set; }

		[DefaultValue(true)]
		public bool StatsInTip { get; set; }

		[DefaultValue(true)]
		public bool WingStatsInTip { get; set; }

		[DefaultValue(false)]
		public bool NoOneDropLogo { get; set; }
	}

	[SeparatePage]
	public class CraftableUncraftables
	{
		// Crafting Recipes
		[Header("Crafting")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ClothierVoodooDoll { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool ConvertibleMushrooms { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool DungeonFurniture { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EarlyEchoBlocks { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool GemCritters { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool GeyserTraps { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TempleTraps { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ObsidianFurniture { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PlanterBoxes { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool StructureBanners { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TeamBlocks { get; set; }

		// Shimmer Transmutations
		[Header("Shimmer")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShimmerBlackLens { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShimmerBossDrops { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ShimmerBottomlessAndSponges { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Gravestones { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool PrehardUnobtainables { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Moss { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool Trophies { get; set; }
	}

	[SeparatePage]
	public class CalTweaks
	{
		// Accessory Tweaks
		[Header("AccTweaks")]

		[ReloadRequired]
		[DefaultValue(false)]
		public bool AsgardsValorBuff { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool DeificAmuletBuff { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EarlyGrandGelatin { get; set; }

		[DefaultValue(true)]
		public bool RadiantInsigniaUpgradesFromAscendant { get; set; }

		[DefaultValue(false)]
		public bool SummonerAccBuffs { get; set; }

		// Block and Biome Tweaks
		//[Header("BlockBiomeTweaks")]
		
		// Boss and Event Tweaks
		//[Header("BossTweaks")]

		// Calamity Fables Tweaks
		[Header("FablesTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EzFablesBanners { get; set; }

		// Enemy Tweaks
		[Header("EnemyTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool EzCalBanners { get; set; }

		[DefaultValue(true)]
		public bool NoPlantParticles { get; set; }

		[DefaultValue(true)]
		public bool NoWormParticles { get; set; }

		// Gameplay Tweaks
		[Header("GameplayTweaks")]

		[DefaultValue(false)]
		public bool DRBuffs { get; set; }

		[DefaultValue(false)]
		public bool NoDefenseDamage { get; set; }

		[DefaultValue(false)]
		public bool NoTimedDR { get; set; }

		// General Item Tweaks
		[Header("ItemTweaks")]

		[ReloadRequired]
		[DefaultValue(false)]
		public bool ConsumableCalBossSummons { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool OnionMasterMode { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool RevertPickSpeedBuffs { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ZenithRecipeOverhaul { get; set; }

		// Infernum Tweaks
		[Header("InfernumTweaks")]

		[DefaultValue(false)]
		public bool AllowGamerChairInInfernumScal { get; set; }

		// Misc. tweaks
		[Header("MiscTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool CombinedStationSupport { get; set; }

		[ReloadRequired]
		[DefaultValue(true)]
		public bool CraftableHostileTurrets { get; set; }

		// Town NPC tweaks
		[Header("TownNpcTweaks")]

		[ReloadRequired]
		[DefaultValue(false)]
		public bool NoPatreonNPCNames { get; set; }

		// Wrath of the Gods Tweaks
		[Header("WotgTweaks")]

		[DefaultValue(false)]
		public bool NoSilentRift { get; set; }
	}

	[SeparatePage]
	public class ThorTweaks
	{
		// Accessory Tweaks
		//[Header("AccTweaks")]

		// Block and Biome Tweaks
		[Header("BlockBiomeTweaks")]

		[DefaultValue(false)]
		public bool BombableADBlocks { get; set; }

		// Boss and Event Tweaks
		//[Header("BossTweaks")]

		// Enemy Tweaks
		//[Header("EnemyTweaks")]

		// Gameplay Tweaks
		//[Header("GameplayTweaks")]

		// General Item Tweaks
		[Header("ItemTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ZenithRecipeOverhaul { get; set; }

		// Misc. Tweaks
		[Header("MiscTweaks")]

		[ReloadRequired]
		[DefaultValue(true)]
		public bool CombinedStationSupport { get; set; }

		// Town NPC Tweaks
		[Header("TownNpcTweaks")]

		[DefaultValue(false)]
		public bool EatCooksFoodInCombat { get; set; }
	}

	[SeparatePage]
	public class AlchTweaks
	{
		// Accessory Tweaks
		//[Header("AccTweaks")]

		// Block and Biome Tweaks
		//[Header("BlockBiomeTweaks")]

		// Boss and Event Tweaks
		//[Header("BossTweaks")]

		// Enemy Tweaks
		//[Header("EnemyTweaks")]

		// Gameplay Tweaks
		//[Header("GameplayTweaks")]

		// General Item Tweaks
		//[Header("ItemTweaks")]

		// Misc. Tweaks
		//[Header("MiscTweaks")]

		// Town NPC Tweaks
		[Header("TownNpcTweaks")]

		[ReloadRequired]
		[DefaultValue(false)]
		public bool AntiCheese { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool DisableCustomPotions { get; set; }
	}

	[SeparatePage]
	public class EmodeTweaks
	{
		// Accessory Tweaks
		//[Header("AccTweaks")]

		// Block and Biome Tweaks
		[Header("BlockBiomeTweaks")]

		[DefaultValue(false)]
		public bool NoEnvironmentalDebuffs { get; set; }

		// Boss and Event Tweaks
		[Header("BossTweaks")]

		[DefaultValue(false)]
		public bool NoClassSealDebuffs { get; set; }

		[ReloadRequired]
		[DefaultValue(false)]
		public bool EternalEnergyAccessibility { get; set; }

		// Enemy Tweaks
		//[Header("EnemyTweaks")]

		// Gameplay Tweaks
		[Header("GameplayTweaks")]

		[DefaultValue(true)]
		public bool EmodeLightningBuff { get; set; }

		// General Item Tweaks
		//[Header("ItemTweaks")]

		// Misc. Tweaks
		//[Header("MiscTweaks")]

		// Town NPC Tweaks
		//[Header("TownNpcTweaks")]
	}
}
