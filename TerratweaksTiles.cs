using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks
{
	public class TerratweaksTiles : GlobalTile
	{
		public override bool? IsTileDangerous(int i, int j, int type, Player player)
		{
			TerratweaksConfig config = GetInstance<TerratweaksConfig>();

			if (config.DangersenseHighlightsSilt && (type == TileID.Silt || type == TileID.Slush))
			{
				return true;
			}

			if (config.DangersenseIgnoresThinIce && type == TileID.BreakableIce)
			{
				return false;
			}

			return base.IsTileDangerous(i, j, type, player);
		}
	}
}