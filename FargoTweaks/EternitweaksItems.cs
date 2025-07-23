using FargowiltasSouls.Content.Items.BossBags;
using FargowiltasSouls.Content.Items.Materials;
using FargowiltasSouls.Core.ItemDropRules.Conditions;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace Terratweaks.FargoTweaks
{
	[JITWhenModsEnabled("FargowiltasSouls")]
	public class EternitweaksItems : GlobalItem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("FargowiltasSouls");

		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			if (item.type == ModContent.ItemType<MutantBag>() && Terratweaks.Eternitweaks.EternalEnergyAccessibility)
			{
				// Add an extra drop rule to the Mutant's Treasure Bag that makes it drop 5-10 Eternal Energy outside of Eternity Mode
				// Since it already has a rule for dropping energy IN emode, we only need to add one for when the game ISN'T in emode
				// Theoretically I could *probably* remove the og rule and add a "DropBasedOnEMode" rule that handles both, but this is cleaner
				itemLoot.Add(ItemDropRule.ByCondition(new NotEModeDropCondition(), ModContent.ItemType<EternalEnergy>(), 1, 5, 10));
			}
		}
	}
}