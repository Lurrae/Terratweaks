using CalamityMod.Tiles.Furniture.CraftingStations;
using CatalystMod.Tiles.Furniture.CraftingStations;
using MagicStorage.Stations;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	// Actually add the new crafting stations to the respective tiles, otherwise they won't count as the new stations!
	[JITWhenModsEnabled("CalamityMod", "MagicStorage")]
	public class CalamitweaksStationMods : GlobalTile
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod") && ModLoader.HasMod("MagicStorage");

		public override int[] AdjTiles(int type)
		{
			List<int> newAdjTiles = base.AdjTiles(type).ToList();

			if (!Terratweaks.Calamitweaks.CombinedStationSupport)
				return newAdjTiles.ToArray();

			if (type == ModContent.TileType<CombinedFurnitureStations1Tile>() ||
				type == ModContent.TileType<CombinedFurnitureStations2Tile>() ||
				type == ModContent.TileType<CombinedStations4Tile>())
			{
				newAdjTiles.AddRange([ ModContent.TileType<WulfrumLabstation>(), ModContent.TileType<EutrophicShelf>(), ModContent.TileType<StaticRefiner>() ]);
			}
			
			if (type == ModContent.TileType<CombinedFurnitureStations2Tile>() || type == ModContent.TileType<CombinedStations4Tile>())
			{
				newAdjTiles.AddRange([ ModContent.TileType<AncientAltar>(), ModContent.TileType<AshenAltar>(), ModContent.TileType<MonolithAmalgam>(), ModContent.TileType<PlagueInfuser>(), ModContent.TileType<VoidCondenser>()]);
			}

			if (type == ModContent.TileType<CombinedStations4Tile>())
			{
				newAdjTiles.AddRange([ ModContent.TileType<ProfanedCrucible>(), ModContent.TileType<BotanicPlanter>(), ModContent.TileType<SilvaBasin>(), ModContent.TileType<SCalAltar>(), ModContent.TileType<SCalAltarLarge>(), ModContent.TileType<CosmicAnvil>(), ModContent.TileType<DraedonsForge>() ]);
			}

			return newAdjTiles.ToArray();
		}
	}

	[JITWhenModsEnabled("CatalystMod", "MagicStorage")]
	public class CatalystStationMods : GlobalTile
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CatalystMod") && ModLoader.HasMod("MagicStorage");

		public override int[] AdjTiles(int type)
		{
			List<int> newAdjTiles = base.AdjTiles(type).ToList();

			if (type == ModContent.TileType<CombinedStations4Tile>())
			{
				newAdjTiles.AddRange([ModContent.TileType<AstralTransmogrifier>()]);
			}

			return newAdjTiles.ToArray();
		}
	}
}