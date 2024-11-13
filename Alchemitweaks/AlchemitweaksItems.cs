using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace Terratweaks.Alchemitweaks
{
	[JITWhenModsEnabled("AlchemistNPCLite")]
	public class AlchemitweaksItems : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("AlchemistNPCLite");

		public override void PostAddRecipes()
		{
			AlchTweaks alchemitweaks = ModContent.GetInstance<TerratweaksConfig>().alchemitweaks;

			if (alchemitweaks.DisableCustomPotions)
			{
				// Disable all AlchLite recipes that result in a potion
				foreach (Recipe recipe in Main.recipe.Where(r => r.Mod == AlchemistNPCLite.AlchemistNPCLite.Instance))
				{
					if (recipe.createItem.ModItem != null && recipe.createItem.ModItem.Mod == recipe.Mod && recipe.createItem.buffType > 0)
					{
						recipe.DisableRecipe();
					}
				}
			}
		}
	}
}