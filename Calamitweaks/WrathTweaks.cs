using NoxusBoss.Core.World.GameScenes.RiftEclipse;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod", "NoxusBoss")]
	public class WrathTweaks : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod") && ModLoader.HasMod("NoxusBoss");

		private static readonly MethodInfo _isRiftEclipseSceneActive = typeof(RiftEclipseScene).GetMethod("IsSceneEffectActive", BindingFlags.Instance | BindingFlags.Public);

		public override void Load()
		{
			MonoModHooks.Add(_isRiftEclipseSceneActive, DisableRiftMusic);
		}

		private bool DisableRiftMusic(Func<RiftEclipseScene, Player, bool> orig, RiftEclipseScene self, Player player)
		{
			if (Terratweaks.Calamitweaks.NoSilentRift)
			{
				return false; // Should prevent rift music from playing
			}

			return orig(self, player);
		}

		// Add a recipe for the Rift Eclipse music box, which can only be crafted if the config option is enabled
		// The recipe still exists regardless so that the config option can be toggled without requiring a mod reload
		public override void AddRecipes()
		{
			Recipe.Create(ModContent.Find<ModItem>("NoxusBoss", "RiftEclipseMusicBox").Type)
				.AddIngredient(ModContent.Find<ModItem>("NoxusBoss", "RiftEclipseBlizzardMusicBox").Type)
				.AddIngredient(ModContent.Find<ModItem>("NoxusBoss", "RiftEclipseFogMusicBox").Type)
				.AddTile(TileID.TinkerersWorkbench)
				.AddCondition(new Condition("Mods.Terratweaks.Conditions.RiftEclipseConfigActive", () => Terratweaks.Calamitweaks.NoSilentRift))
				.Register();
		}
	}
}