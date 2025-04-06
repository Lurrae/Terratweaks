using CalamityFables.Content.NPCs.GeodeGrawlers;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityFables")]
	public class FablesItems : GlobalItem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityFables");

		public override void SetStaticDefaults()
		{
			// Reduce banner kill counts for some enemies
			if (Terratweaks.Calamitweaks.EzFablesBanners)
			{
				// Currently, the only enemy rare enough to warrant a reduced kill count is the Geode Crawler
				// It's not too rare, but it is rare enough that requiring 50 kills is a bit excessive
				ItemID.Sets.KillsToBanner[GeodeCrawler.BannerType] = 25;
			}
		}
	}
}