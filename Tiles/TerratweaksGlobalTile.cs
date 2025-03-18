using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Tiles
{
	public class TerratweaksGlobalTile : GlobalTile
	{
		// Change what tiles Dangersense Potions highlight
		public override bool? IsTileDangerous(int i, int j, int type, Player player)
		{
			if (Terratweaks.Config.DangersenseHighlightsSilt && (type == TileID.Silt || type == TileID.Slush))
			{
				return true;
			}

			if (Terratweaks.Config.DangersenseIgnoresThinIce && type == TileID.BreakableIce)
			{
				return false;
			}

			return base.IsTileDangerous(i, j, type, player);
		}

		// Change the functionality of Cracked Bricks to make them more consistent
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (!Terratweaks.Config.BetterCrackedBricks)
				return;

			int tileType = Main.tile[i, j].TileType;

			if (!Main.tileCracked[tileType] || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			for (int k = 0; k < 8; k++)
			{
				int x = i;
				int y = j;

				switch (k)
				{
					case 0:
						x--;
						break;
					case 1:
						x++;
						break;
					case 2:
						y--;
						break;
					case 3:
						y++;
						break;
					case 4:
						x--;
						y--;
						break;
					case 5:
						x++;
						y--;
						break;
					case 6:
						x--;
						y++;
						break;
					case 7:
						x++;
						y++;
						break;
				}

				Tile tile = Main.tile[x, y];

				if (tile.HasTile && Main.tileCracked[tile.TileType])
				{
					Main.tile[i, j].Get<TileWallWireStateData>().HasTile = false;
					WorldGen.KillTile(x, y, noItem: true);

					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.TrySendData(MessageID.TileManipulation, number: 20, number2: x, number3: y);
					}
				}
			}

			int dungeonBrickVariant = tileType - TileID.CrackedBlueDungeonBrick; // Should be 0, 1, or 2
			int projType = dungeonBrickVariant + ProjectileID.BlueDungeonDebris;
			var src = new EntitySource_TileBreak(i, j);
			Vector2 projPos = new(Conversions.ToPixels(i) + 8, Conversions.ToPixels(j) + 8);

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Projectile.NewProjectile(src, projPos, Vector2.Zero, projType, 20, 0, Main.myPlayer);
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				Projectile proj = Projectile.NewProjectileDirect(src, projPos, Vector2.Zero, projType, 20, 0, Main.myPlayer);
				proj.netUpdate = true;
			}
		}
	}
}