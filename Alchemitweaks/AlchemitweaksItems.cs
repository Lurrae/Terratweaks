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
			if (Terratweaks.Alchemitweaks.DisableCustomPotions)
			{
				// Disable all AlchLite recipes that result in a potion
				foreach (Recipe recipe in Main.recipe)
				{
					if (ModLoader.TryGetMod("AlchemistNPCLite", out Mod alchLite) && recipe.createItem.ModItem != null && recipe.createItem.ModItem.Mod == recipe.Mod && recipe.Mod == alchLite && recipe.createItem.buffType > 0)
					{
						recipe.DisableRecipe();
					}
				}
			}
		}
	}
}