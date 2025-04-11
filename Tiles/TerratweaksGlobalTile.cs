using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
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

		// Change what tiles Spelunker Potions highlight
		public override bool? IsTileSpelunkable(int i, int j, int type)
		{
			if (Terratweaks.Config.SpelunkerHighlightsHellstone && type == TileID.Hellstone)
			{
				return true;
			}

			return base.IsTileSpelunkable(i, j, type);
		}

		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			var src = new EntitySource_TileBreak(i, j);
			Vector2 projPos = new(Conversions.ToPixels(i) + 8, Conversions.ToPixels(j) + 8);

			// Pots always drop bombs in addition to their normal drops in FtW worlds
			// TODO: Can I make this work with modded pots?
			if (Terratweaks.Config.FtwBombPots && Main.getGoodWorld)
			{
				// Only the top-left corner should drop a bomb, that way we don't drop four bombs per pot
				if (type == TileID.Pots && Main.tile[i,j].TileFrameX % 36 == 0 && Main.tile[i, j].TileFrameY % 36 == 0)
				{
					// Spawn a bomb
					Projectile.NewProjectile(src, projPos, Vector2.Zero, ProjectileID.Bomb, 0, 0, Main.myPlayer);
				}
			}

			// Change the functionality of Cracked Bricks to make them more consistent
			if (!Terratweaks.Config.BetterCrackedBricks)
				return;

			if (!Main.tileCracked[type] || Main.netMode == NetmodeID.MultiplayerClient)
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

			int dungeonBrickVariant = type - TileID.CrackedBlueDungeonBrick; // Should be 0, 1, or 2
			int projType = dungeonBrickVariant + ProjectileID.BlueDungeonDebris;

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

		public override void PreShakeTree(int i, int j, TreeTypes treeType)
		{
			if (Terratweaks.Config.FtwBombTrees && Main.getGoodWorld)
			{
				// Spawn a bomb at the top of the tree, which will fall down and explode on players
				// Because this uses PreShakeTree(), any existing drops shouldn't be blocked!
				Vector2 spawnPos = new(i * 16, j * 16);
				Projectile.NewProjectile(new EntitySource_ShakeTree(i, j), spawnPos, Vector2.Zero, ProjectileID.Bomb, 0, 0, Main.myPlayer);
			}
		}

		// Override the highlight color for blocks highlighted by Spelunker or Dangersense Potions
		public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			Player plr = Main.LocalPlayer;

			// Dangersense
			if (Terratweaks.ClientConfig.OverrideDangerGlow && plr.dangerSense && TileDrawing.IsTileDangerous(i, j, plr))
			{
				Color color = Terratweaks.ClientConfig.DangerGlowColor;
				color.A = drawData.tileLight.A;
				drawData.tileLight = color;
			}

			// Spelunker
			if (Terratweaks.ClientConfig.OverrideSpelunkerGlow && plr.findTreasure && Main.IsTileSpelunkable(i, j))
			{
				Color color = Terratweaks.ClientConfig.TreasureGlowColor;
				color.A = drawData.tileLight.A;
				drawData.tileLight = color;
			}
		}
	}
}