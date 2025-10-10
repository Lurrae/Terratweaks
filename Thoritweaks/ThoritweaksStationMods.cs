using MagicStorage.Stations;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using ThoriumMod.Tiles;

namespace Terratweaks.Thoritweaks
{
	// Actually add the new crafting stations to the respective tiles, otherwise they won't count as the new stations!
	[JITWhenModsEnabled("ThoriumMod", "MagicStorage")]
	public class ThoritweaksStationMods : GlobalTile
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("ThoriumMod") && ModLoader.HasMod("MagicStorage");

		public override int[] AdjTiles(int type)
		{
			List<int> newAdjTiles = base.AdjTiles(type).ToList();

			if (!Terratweaks.Calamitweaks.CombinedStationSupport)
				return newAdjTiles.ToArray();

			if (type == ModContent.TileType<CombinedStations1Tile>() ||
				type == ModContent.TileType<CombinedStations2Tile>() ||
				type == ModContent.TileType<CombinedStations3Tile>() ||
				type == ModContent.TileType<CombinedStations4Tile>())
			{
				newAdjTiles.AddRange([ ModContent.TileType<ThoriumAnvil>(), ModContent.TileType<ArcaneArmorFabricator>() ]);
			}
			
			if (type == ModContent.TileType<CombinedStations3Tile>() || type == ModContent.TileType<CombinedStations4Tile>())
			{
				newAdjTiles.AddRange([ ModContent.TileType<SoulForge>() ]);
			}

			return newAdjTiles.ToArray();
		}
	}
}