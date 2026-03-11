using Spooky.Content.Items.Blooms;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terratweaks.Spookitweaks
{
	[JITWhenModsEnabled("Spooky")]
	public class SpookitweaksItems : GlobalItem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Spooky");

		public override void SetDefaults(Item item)
		{
			// Add a tooltip specifying that certain items were modified by Terratweaks while the corresponding configs are active
			#region StatsModifiedBy stuff
			bool itemIsModified = false;

			if (Terratweaks.Spookitweaks.BetterWateringGourd != 7 && item.type == ModContent.ItemType<FallWaterGourd>())
				itemIsModified = true;

			if (itemIsModified && !Terratweaks.ClientConfig.HideItemModifiedTips)
				item.StatsModifiedBy.Add(Mod);
			#endregion
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			// If gourd buff is set to guarantee a bloom and seed, update the tooltip accordingly
			if (Terratweaks.Spookitweaks.BetterWateringGourd == 1 && item.type == ModContent.ItemType<FallWaterGourd>())
			{
				foreach (TooltipLine tooltip in tooltips.Where(t => t.Mod == "Terraria" && t.Name.Contains("Tooltip")))
				{
					if (tooltip.Name.Equals("Tooltip0"))
						tooltip.Text = Language.GetTextValue("Mods.Terratweaks.Common.WaterGourdGuaranteedTip");
				}
			}
		}
	}
}