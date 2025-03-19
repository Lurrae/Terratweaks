using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Terratweaks.Tiles
{
	public class GravGlobePlaced : ModTile
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return Terratweaks.Config.PlaceableGravGlobe; // Item is only loaded in if Placeable Gravity Globe is enabled
		}

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			
			DustType = DustID.Glass; // Gravity Globe will produce glass-like dust when mined

			// TileObjectData stuff, used for placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
			TileObjectData.newTile.WaterDeath = false;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.StyleLineSkip = 2;
			TileObjectData.addTile(Type);

			// Map localization - uses the same name as the Gravity Globe item, and a purple color
			AddMapEntry(new Color(86, 16, 164), Language.GetText("ItemName.GravityGlobe"));
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			if (closer)
				return;

			foreach (Player plr in Main.player.Where(p => p.active && !p.dead && !p.ghost))
			{
				Vector2 tilePos = new(i * 16, j * 16);
				Vector2 plrPos = plr.Center;

				if (Vector2.Distance(tilePos, plrPos) < Conversions.ToPixels(Terratweaks.Config.GravGlobeRange))
				{
					plr.gravDir = Main.tile[i, j].TileFrameX >= 36 ? -1 : 1;
					plr.forcedGravity = Main.tile[i, j].TileFrameX >= 36 ? 10 : 0;
					plr.GetModPlayer<InputPlayer>().inGravGlobeRange = true;
				}
			}
		}

		// Show the Gravity Globe icon when moused over
		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ItemID.GravityGlobe;
		}

		// Allows this to be highlighted by Smart Cursor
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

		// Toggle the gravity functionality on right-click
		public override bool RightClick(int i, int j)
		{
			// Play the same sound using an equipped Gravity Globe would play
			SoundEngine.PlaySound(SoundID.Item8, new Vector2(i * 16, j * 16));

			ToggleGravity(i, j);

			// Tell the game a tile interaction has occurred
			// This ensures that items' right-click actions won't happen
			return true;
		}

		// Toggle the gravity functionality on right-click
		public override void HitWire(int i, int j)
		{
			ToggleGravity(i, j);
		}

		public static void ToggleGravity(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int topX = i - ((tile.TileFrameX % 36) / 18);
			int topY = j - ((tile.TileFrameY % 36) / 18);

			short frameAdjust = (short)(tile.TileFrameX >= 36 ? -36 : 36);

			// Shift all parts of this tile over 2 spaces on the sheet, swapping which sprite is displayed
			for (int x = topX; x < topX + 2; x++)
			{
				for (int y = topY; y < topY + 2; y++)
				{
					Main.tile[x, y].TileFrameX += frameAdjust;

					if (Wiring.running)
						Wiring.SkipWire(x, y);
				}
			}

			// Not entirely sure what this means? It was in the ExampleCampfire code tho so I assume it's important
			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendTileSquare(-1, topX, topY, TileChangeType.HoneyWater);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			// Play the sound of glass breaking
			SoundEngine.PlaySound(SoundID.Shatter, new Vector2(i * 16, j * 16));

			// Drop a prefixless Gravity Globe
			int idx = Item.NewItem(new EntitySource_TileBreak(i, j), new Rectangle(i * 16, j * 16, 32, 32), ItemID.GravityGlobe);
			Item item = Main.item[idx];
			item.prefix = 0; // Force the dropped Gravity Globe to have no prefix
		}

		// Prevent vanilla drop code from running, which applies a random prefix to the dropped Gravity Globe
		// We don't want random prefixes, but also don't want to make the Gravity Globe unable to receive prefixes
		public override bool CanDrop(int i, int j)
		{
			return false;
		}

		public override bool KillSound(int i, int j, bool fail)
		{
			return base.KillSound(i, j, fail);
		}
	}
}