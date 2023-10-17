using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks
{
	public class TerratweaksRecipes : ModSystem
	{
		static readonly Dictionary<int, List<int>> planterBoxRecipeIngredients = new()
		{
			//		Planter box type							Soil type			Wood type	Herb seed type (matches planter box)
			{ ItemID.BlinkrootPlanterBox,	new List<int> { ItemID.DirtBlock, ItemID.Wood,			ItemID.BlinkrootSeeds } },
			{ ItemID.DayBloomPlanterBox,	new List<int> { ItemID.DirtBlock, ItemID.Wood,			ItemID.DaybloomSeeds } },
			{ ItemID.CorruptPlanterBox,		new List<int> { ItemID.DirtBlock, ItemID.Ebonwood,		ItemID.DeathweedSeeds } },
			{ ItemID.CrimsonPlanterBox,		new List<int> { ItemID.DirtBlock, ItemID.Shadewood,		ItemID.DeathweedSeeds } },
			{ ItemID.FireBlossomPlanterBox, new List<int> { ItemID.AshBlock,  ItemID.AshWood,		ItemID.FireblossomSeeds } },
			{ ItemID.MoonglowPlanterBox,	new List<int> { ItemID.MudBlock,  ItemID.RichMahogany,	ItemID.MoonglowSeeds } },
			{ ItemID.ShiverthornPlanterBox, new List<int> { ItemID.SnowBlock, ItemID.BorealWood,	ItemID.ShiverthornSeeds } },
			{ ItemID.WaterleafPlanterBox,	new List<int> { ItemID.SandBlock, ItemID.PalmWood,		ItemID.WaterleafSeeds } }
		};

		static readonly Dictionary<int, List<int>> gemsAndGemCritters = new()
		{
			{ ItemID.Amber, new List<int> { ItemID.GemBunnyAmber, ItemID.GemSquirrelAmber } },
			{ ItemID.Amethyst, new List<int> { ItemID.GemBunnyAmethyst, ItemID.GemSquirrelAmethyst } },
			{ ItemID.Diamond, new List<int> { ItemID.GemBunnyDiamond, ItemID.GemSquirrelDiamond } },
			{ ItemID.Emerald, new List<int> { ItemID.GemBunnyEmerald, ItemID.GemSquirrelEmerald } },
			{ ItemID.Ruby, new List<int> { ItemID.GemBunnyRuby, ItemID.GemSquirrelRuby } },
			{ ItemID.Sapphire, new List<int> { ItemID.GemBunnySapphire, ItemID.GemSquirrelSapphire } },
			{ ItemID.Topaz, new List<int> { ItemID.GemBunnyTopaz, ItemID.GemSquirrelTopaz } }
		};

		public override void OnModLoad()
		{
			// Add recipes for Thorium's Marine Kelp Planter Box and custom gem critters, if it's active
			ModSupport_Thorium();
		}

		static void ModSupport_Thorium()
		{
			if (ModLoader.TryGetMod("ThoriumMod", out Mod thorium))
			{
				if (thorium.TryFind("MarineKelpPlanterBox", out ModItem marineKelpPlanterBox) &&
					thorium.TryFind("MossyMarineBlock", out ModItem mossyMarineBlock) &&
					thorium.TryFind("MarineKelpSeeds", out ModItem marineKelpSeeds))
				{
					planterBoxRecipeIngredients.Add(marineKelpPlanterBox.Type, new List<int> { mossyMarineBlock.Type, ItemID.Wood, marineKelpSeeds.Type });
				}

				if (thorium.TryFind("Aquamarine", out ModItem aquamarine) &&
					thorium.TryFind("AquamarineBunny", out ModItem aquamarineBunny) &&
					thorium.TryFind("AquamarineSquirrel", out ModItem aquamarineSquirrel))
				{
					gemsAndGemCritters.Add(aquamarine.Type, new List<int> { aquamarineBunny.Type, aquamarineSquirrel.Type });
				}

				if (thorium.TryFind("Opal", out ModItem opal) &&
					thorium.TryFind("OpalBunny", out ModItem opalBunny) &&
					thorium.TryFind("OpalSquirrel", out ModItem opalSquirrel))
				{
					gemsAndGemCritters.Add(opal.Type, new List<int> { opalBunny.Type, opalSquirrel.Type });
				}
			}
		}

		public override void AddRecipes()
		{
			var config = GetInstance<TerratweaksConfig>();
			var enabledRecipes = config.craftableUncraftables;

			if (enabledRecipes.PlanterBoxes)
			{
				foreach (KeyValuePair<int, List<int>> pair in planterBoxRecipeIngredients)
				{
					Recipe.Create(pair.Key, 20)
						.AddIngredient(pair.Value[0], 10)
						.AddIngredient(pair.Value[1], 10)
						.AddIngredient(pair.Value[2], 1)
						.Register();
				}
			}

			if (enabledRecipes.GemCritters)
				AddGemBunnyRecipes();
			
			if (enabledRecipes.DungeonFurniture)
				AddDungeonFurnitureRecipes();
			
			if (enabledRecipes.ObsidianFurniture)
				AddObsidianFurnitureRecipes();
			
			if (enabledRecipes.StructureBanners)
				AddUncraftableBannerRecipes();
		}

		public override void AddRecipeGroups()
		{
			RecipeGroup.RegisterGroup("DungeonBricks", new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Dungeon Brick", ItemID.BlueBrick, ItemID.GreenBrick, ItemID.PinkBrick));
		}

		static void AddGemBunnyRecipes()
		{
			foreach (KeyValuePair<int, List<int>> pair in gemsAndGemCritters)
			{
				int gem = pair.Key;
				int gemBunny = pair.Value[0];
				int gemSquirrel = pair.Value[1];

				Recipe.Create(gemBunny)
					.AddIngredient(ItemID.Bunny)
					.AddIngredient(gem, 5)
					.AddTile(TileID.Solidifier)
					.Register();

				Recipe.Create(gemSquirrel)
					.AddIngredient(ItemID.Squirrel)
					.AddIngredient(gem, 5)
					.AddTile(TileID.Solidifier)
					.Register();

				Recipe.Create(gemSquirrel)
					.AddIngredient(ItemID.SquirrelRed)
					.AddIngredient(gem, 5)
					.AddTile(TileID.Solidifier)
					.Register();
			}
		}

		static void AddUncraftableBannerRecipes()
		{
			// Floating Island banners
			Recipe.Create(ItemID.WorldBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.SunBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.GravityBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			// Desert pyramid banners
			Recipe.Create(ItemID.AnkhBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.SnakeBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.OmegaBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			// Dungeon banners
			Recipe.Create(ItemID.MarchingBonesBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.NecromanticSign)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.RustedCompanyStandard)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.RaggedBrotherhoodSigil)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.MoltenLegionFlag)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.DiabolicSigil)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			// Underworld banners
			Recipe.Create(ItemID.HellboundBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.HellHammerBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.HelltowerBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.LostHopesofManBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.ObsidianWatcherBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();

			Recipe.Create(ItemID.LavaEruptsBanner)
				.AddIngredient(ItemID.Silk, 10)
				.AddTile(TileID.Loom)
				.Register();
		}

		static void AddDungeonFurnitureRecipes()
		{
			DungeonFurniture_BlueBrick();
			DungeonFurniture_GreenBrick();
			DungeonFurniture_PinkBrick();

			Recipe.Create(ItemID.DungeonDoor)
				.AddRecipeGroup(RecipeGroupID.Wood, 6)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.Catacomb)
				.AddRecipeGroup("DungeonBricks", 10)
				.AddIngredient(ItemID.Bone, 5)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.HangingSkeleton)
				.AddIngredient(ItemID.Bone, 20)
				.AddCondition(Condition.InGraveyard)
				.Register();

			Recipe.Create(ItemID.WallSkeleton)
				.AddIngredient(ItemID.Bone, 20)
				.AddCondition(Condition.InGraveyard)
				.Register();

			Recipe.Create(ItemID.GothicBookcase)
				.AddRecipeGroup("DungeonBricks", 20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GothicChair)
				.AddRecipeGroup("DungeonBricks", 4)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GothicTable)
				.AddRecipeGroup("DungeonBricks", 8)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GothicWorkBench)
				.AddRecipeGroup("DungeonBricks", 10)
				.Register();

			Recipe.Create(ItemID.ChainLantern)
				.AddIngredient(ItemID.Chain, 4)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.BrassLantern)
				.AddIngredient(ItemID.CopperBar, 2)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.BrassLantern)
				.AddIngredient(ItemID.TinBar, 2)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.CagedLantern)
				.AddRecipeGroup(RecipeGroupID.IronBar, 2)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.CarriageLantern)
				.AddRecipeGroup(RecipeGroupID.IronBar, 2)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.AlchemyLantern)
				.AddIngredient(ItemID.Glass, 6)
				.AddIngredient(ItemID.JungleTorch)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.DiablostLamp)
				.AddIngredient(ItemID.Silk, 6)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.OilRagSconse)
				.AddRecipeGroup(RecipeGroupID.IronBar, 2)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();
		}

		static void DungeonFurniture_BlueBrick()
		{
			Recipe.Create(ItemID.BlueBrickPlatform, 2)
				.AddIngredient(ItemID.BlueBrick)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonBathtub)
				.AddIngredient(ItemID.BlueBrick, 14)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonBed)
				.AddIngredient(ItemID.BlueBrick, 15)
				.AddIngredient(ItemID.Silk, 5)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonBookcase)
				.AddIngredient(ItemID.BlueBrick, 20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonCandelabra)
				.AddIngredient(ItemID.BlueBrick, 5)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonCandle)
				.AddIngredient(ItemID.BlueBrick, 4)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonChair)
				.AddIngredient(ItemID.BlueBrick, 4)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonChandelier)
				.AddIngredient(ItemID.BlueBrick, 4)
				.AddIngredient(ItemID.Torch, 4)
				.AddIngredient(ItemID.Chain)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.DungeonClockBlue)
				.AddIngredient(ItemID.BlueBrick, 10)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Glass, 6)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonDoor)
				.AddIngredient(ItemID.BlueBrick, 6)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonDresser)
				.AddIngredient(ItemID.BlueBrick, 16)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonLamp)
				.AddIngredient(ItemID.Torch)
				.AddIngredient(ItemID.BlueBrick, 3)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonPiano)
				.AddIngredient(ItemID.BlueBrick, 15)
				.AddIngredient(ItemID.Bone, 4)
				.AddIngredient(ItemID.Book)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonSofa)
				.AddIngredient(ItemID.BlueBrick, 5)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonVase)
				.AddIngredient(ItemID.BlueBrick, 15)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonTable)
				.AddIngredient(ItemID.BlueBrick, 8)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.BlueDungeonWorkBench)
				.AddIngredient(ItemID.BlueBrick, 10)
				.Register();
		}

		static void DungeonFurniture_GreenBrick()
		{
			Recipe.Create(ItemID.GreenBrickPlatform, 2)
				.AddIngredient(ItemID.GreenBrick)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonBathtub)
				.AddIngredient(ItemID.GreenBrick, 14)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonBed)
				.AddIngredient(ItemID.GreenBrick, 15)
				.AddIngredient(ItemID.Silk, 5)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonBookcase)
				.AddIngredient(ItemID.GreenBrick, 20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonCandelabra)
				.AddIngredient(ItemID.GreenBrick, 5)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonCandle)
				.AddIngredient(ItemID.GreenBrick, 4)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonChair)
				.AddIngredient(ItemID.GreenBrick, 4)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonChandelier)
				.AddIngredient(ItemID.GreenBrick, 4)
				.AddIngredient(ItemID.Torch, 4)
				.AddIngredient(ItemID.Chain)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.DungeonClockGreen)
				.AddIngredient(ItemID.GreenBrick, 10)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Glass, 6)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonDoor)
				.AddIngredient(ItemID.GreenBrick, 6)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonDresser)
				.AddIngredient(ItemID.GreenBrick, 16)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonLamp)
				.AddIngredient(ItemID.Torch)
				.AddIngredient(ItemID.GreenBrick, 3)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonPiano)
				.AddIngredient(ItemID.GreenBrick, 15)
				.AddIngredient(ItemID.Bone, 4)
				.AddIngredient(ItemID.Book)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonSofa)
				.AddIngredient(ItemID.GreenBrick, 5)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonVase)
				.AddIngredient(ItemID.GreenBrick, 15)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonTable)
				.AddIngredient(ItemID.GreenBrick, 8)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.GreenDungeonWorkBench)
				.AddIngredient(ItemID.GreenBrick, 10)
				.Register();
		}

		static void DungeonFurniture_PinkBrick()
		{
			Recipe.Create(ItemID.PinkBrickPlatform, 2)
				.AddIngredient(ItemID.PinkBrick)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonBathtub)
				.AddIngredient(ItemID.PinkBrick, 14)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonBed)
				.AddIngredient(ItemID.PinkBrick, 15)
				.AddIngredient(ItemID.Silk, 5)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonBookcase)
				.AddIngredient(ItemID.PinkBrick, 20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonCandelabra)
				.AddIngredient(ItemID.PinkBrick, 5)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonCandle)
				.AddIngredient(ItemID.PinkBrick, 4)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonChair)
				.AddIngredient(ItemID.PinkBrick, 4)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonChandelier)
				.AddIngredient(ItemID.PinkBrick, 4)
				.AddIngredient(ItemID.Torch, 4)
				.AddIngredient(ItemID.Chain)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.DungeonClockPink)
				.AddIngredient(ItemID.PinkBrick, 10)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Glass, 6)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonDoor)
				.AddIngredient(ItemID.PinkBrick, 6)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonDresser)
				.AddIngredient(ItemID.PinkBrick, 16)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonLamp)
				.AddIngredient(ItemID.Torch)
				.AddIngredient(ItemID.PinkBrick, 3)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonPiano)
				.AddIngredient(ItemID.PinkBrick, 15)
				.AddIngredient(ItemID.Bone, 4)
				.AddIngredient(ItemID.Book)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonSofa)
				.AddIngredient(ItemID.PinkBrick, 5)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonVase)
				.AddIngredient(ItemID.PinkBrick, 15)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonTable)
				.AddIngredient(ItemID.PinkBrick, 8)
				.AddTile(TileID.BoneWelder)
				.Register();

			Recipe.Create(ItemID.PinkDungeonWorkBench)
				.AddIngredient(ItemID.PinkBrick, 10)
				.Register();
		}

		static void AddObsidianFurnitureRecipes()
		{
			Recipe.Create(ItemID.ObsidianBathtub)
				.AddIngredient(ItemID.Obsidian, 14)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddTile(TileID.Sawmill)
				.Register();

			Recipe.Create(ItemID.ObsidianBed)
				.AddIngredient(ItemID.Obsidian, 15)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddIngredient(ItemID.Silk, 5)
				.AddTile(TileID.Sawmill)
				.Register();

			Recipe.Create(ItemID.ObsidianBookcase)
				.AddIngredient(ItemID.Obsidian, 20)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(TileID.Sawmill)
				.Register();

			Recipe.Create(ItemID.ObsidianCandelabra)
				.AddIngredient(ItemID.Obsidian, 5)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddIngredient(ItemID.Torch, 3)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.ObsidianCandle)
				.AddIngredient(ItemID.Obsidian, 4)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddIngredient(ItemID.Torch)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.ObsidianChair)
				.AddIngredient(ItemID.Obsidian, 4)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.ObsidianChandelier)
				.AddIngredient(ItemID.Obsidian, 4)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddIngredient(ItemID.Torch, 4)
				.AddIngredient(ItemID.Chain)
				.AddTile(TileID.Anvils)
				.Register();

			Recipe.Create(ItemID.ObsidianClock)
				.AddIngredient(ItemID.Obsidian, 10)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddRecipeGroup(RecipeGroupID.IronBar, 3)
				.AddIngredient(ItemID.Glass, 6)
				.AddTile(TileID.Sawmill)
				.Register();

			Recipe.Create(ItemID.ObsidianDoor)
				.AddIngredient(ItemID.Obsidian, 6)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.ObsidianDresser)
				.AddIngredient(ItemID.Obsidian, 16)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddTile(TileID.Sawmill)
				.Register();

			Recipe.Create(ItemID.ObsidianLamp)
				.AddIngredient(ItemID.Torch)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddIngredient(ItemID.Obsidian, 3)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.ObsidianPiano)
				.AddIngredient(ItemID.Obsidian, 15)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddIngredient(ItemID.Bone, 4)
				.AddIngredient(ItemID.Book)
				.AddTile(TileID.Sawmill)
				.Register();

			Recipe.Create(ItemID.ObsidianSofa)
				.AddIngredient(ItemID.Obsidian, 5)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(TileID.Sawmill)
				.Register();

			Recipe.Create(ItemID.ObsidianVase)
				.AddIngredient(ItemID.Obsidian, 15)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.ObsidianTable)
				.AddIngredient(ItemID.Obsidian, 8)
				.AddIngredient(ItemID.Hellstone, 2)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(ItemID.ObsidianWorkBench)
				.AddIngredient(ItemID.Obsidian, 10)
				.AddIngredient(ItemID.Hellstone, 2)
				.Register();
		}
	}
}