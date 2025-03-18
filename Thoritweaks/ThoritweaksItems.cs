using System;
using System.Reflection;
using Terraria.ModLoader;
using ThoriumMod.Items.Consumable;

namespace Terratweaks.Thoritweaks
{
	[JITWhenModsEnabled("ThoriumMod")]
	public class ThoritweaksItems : GlobalItem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("ThoriumMod");

		private static readonly PropertyInfo _eatCookFoodOOC = typeof(CookFoodItem).GetProperty("CanBeConsumedOutOfCombat", BindingFlags.Public | BindingFlags.Instance);

		public override void Load()
		{
			MonoModHooks.Add(_eatCookFoodOOC.GetGetMethod(), AllowEatingCookFoodInCombat);
		}

		private static bool AllowEatingCookFoodInCombat(Func<CookFoodItem, bool> orig, CookFoodItem self)
		{
			if (Terratweaks.Thoritweaks.EatCooksFoodInCombat)
			{
				return true;
			}

			return orig(self);
		}
	}
}