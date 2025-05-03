using System;
using System.Reflection;
using Terraria;
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

		public override void SetDefaults(Item item)
		{
			// Add a tooltip specifying that certain items were modified by Terratweaks while the corresponding configs are active
			#region StatsModifiedBy stuff
			bool itemIsModified = false;

			if (Terratweaks.Thoritweaks.EatCooksFoodInCombat && item.ModItem != null && item.ModItem is CookFoodItem)
				itemIsModified = true;

			if (itemIsModified && !Terratweaks.ClientConfig.HideItemModifiedTips)
				item.StatsModifiedBy.Add(Mod);
			#endregion
		}
	}
}