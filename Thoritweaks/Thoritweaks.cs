using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Reflection;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod.Items.BossTheGrandThunderBird;
using ThoriumMod.Items.Lodestone;
using ThoriumMod.Items.MeleeItems;
using ThoriumMod.Items.NPCItems;
using ThoriumMod.Items.Terrarium;
using ThoriumMod.Items.Thorium;
using ThoriumMod.Items.Valadium;

namespace Terratweaks.Thoritweaks
{
	[JITWhenModsEnabled("ThoriumMod")]
	public class Thoritweaks : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("ThoriumMod");

		public override void Load()
		{
			ThorTweaks thoritweaks = ModContent.GetInstance<TerratweaksConfig>().thoritweaks;

			// Add more swords to Zenith profiles
			if (thoritweaks.ZenithRecipeOverhaul)
			{
				FieldInfo _zenithProfiles = typeof(FinalFractalHelper).GetField("_fractalProfiles", BindingFlags.Static | BindingFlags.NonPublic);
				Dictionary<int, FinalFractalHelper.FinalFractalProfile> profiles = (Dictionary<int, FinalFractalHelper.FinalFractalProfile>)_zenithProfiles.GetValue(null);
				AddThoriumZenithProfiles(profiles);
			}
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
	}
}