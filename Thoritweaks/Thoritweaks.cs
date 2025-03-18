using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Items.BossTheGrandThunderBird;
using ThoriumMod.Items.Lodestone;
using ThoriumMod.Items.MeleeItems;
using ThoriumMod.Items.NPCItems;
using ThoriumMod.Items.Terrarium;
using ThoriumMod.Items.Thorium;
using ThoriumMod.Items.Valadium;
using ThoriumMod.Tiles;

namespace Terratweaks.Thoritweaks
{
	[JITWhenModsEnabled("ThoriumMod")]
	public class Thoritweaks : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("ThoriumMod");

		public override void Load()
		{
			// Add more swords to Zenith profiles
			if (Terratweaks.Thoritweaks.ZenithRecipeOverhaul)
			{
				FieldInfo _zenithProfiles = typeof(FinalFractalHelper).GetField("_fractalProfiles", BindingFlags.Static | BindingFlags.NonPublic);
				Dictionary<int, FinalFractalHelper.FinalFractalProfile> profiles = (Dictionary<int, FinalFractalHelper.FinalFractalProfile>)_zenithProfiles.GetValue(null);
				AddThoriumZenithProfiles(profiles);
			}

			// Enable Marine Blocks and Aquaite to be bombed in pre-Hardmode if Queen Jellyfish and an evil boss are defeated
			On_Projectile.CanExplodeTile += AllowBombingDepthsBlocks;
		}

		public static void AddThoriumZenithProfiles(Dictionary<int, FinalFractalHelper.FinalFractalProfile> profiles)
		{
			profiles.Add(ModContent.ItemType<TerrariumSaber>(), new FinalFractalHelper.FinalFractalProfile(70f, new Color(255, 255, 255)));
			profiles.Add(ModContent.ItemType<SolScorchedSlab>(), new FinalFractalHelper.FinalFractalProfile(70f, new Color(97, 53, 7)));
			profiles.Add(ModContent.ItemType<LodeStoneClaymore>(), new FinalFractalHelper.FinalFractalProfile(70f, new Color(97, 53, 7)));
			profiles.Add(ModContent.ItemType<ValadiumSlicer>(), new FinalFractalHelper.FinalFractalProfile(70f, new Color(228, 20, 245)));
			profiles.Add(ModContent.ItemType<WhirlpoolSaber>(), new FinalFractalHelper.FinalFractalProfile(70f, new Color(91, 158, 232)));
			profiles.Add(ModContent.ItemType<ThoriumBlade>(), new FinalFractalHelper.FinalFractalProfile(70f, new Color(91, 158, 232)));
			profiles.Add(ModContent.ItemType<ThunderTalon>(), new FinalFractalHelper.FinalFractalProfile(70f, new Color(255, 231, 69)));
		}

		private static readonly List<int> DepthsBlocks = new()
		{
			ModContent.TileType<LeakyMarineBlock>(),
			ModContent.TileType<LeakyMossyMarineBlock>(),
			ModContent.TileType<Aquaite>()
		};

		private bool AllowBombingDepthsBlocks(On_Projectile.orig_CanExplodeTile orig, Projectile self, int x, int y)
		{
			if (Terratweaks.Thoritweaks.BombableADBlocks)
			{
				Tile tile = Main.tile[x, y];

				if (DepthsBlocks.Contains(tile.TileType) && ((ThoriumWorld.downedQueenJellyfish && NPC.downedBoss2) || Main.hardMode))
				{
					return true;
				}
			}

			return orig(self, x, y);
		}
	}
}