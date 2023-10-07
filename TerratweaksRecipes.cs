using System.Collections.Generic;
using Terraria;
using Terraria.ID;
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

		public override void AddRecipes()
		{
			if (GetInstance<TerratweaksConfig>().CraftablePlanterBoxes)
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
		}
	}
}