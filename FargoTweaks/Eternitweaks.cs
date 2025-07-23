using FargowiltasSouls.Core.ModPlayers;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Terratweaks.FargoTweaks
{
	[JITWhenModsEnabled("FargowiltasSouls")]
	public class Eternitweaks : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("FargowiltasSouls");

		private static readonly MethodInfo _preUpdate = typeof(EModePlayer).GetMethod("PreUpdate", BindingFlags.Instance | BindingFlags.Public);

		public override void Load()
		{
			MonoModHooks.Add(_preUpdate, DisableEnvironmentalDebuffs);
		}

		private static void DisableEnvironmentalDebuffs(Action<ModPlayer> orig, ModPlayer self)
		{
			FargoSoulsPlayer fsPlr = self.Player.GetModPlayer<FargoSoulsPlayer>();
			bool oldPureHeart = fsPlr.PureHeart;

			// Force the game to think we have the Pure Heart equipped, which disables all environmental debuffs
			// Unfortunately it does disable some non-debuff hazards, like damaging cactus and the shadow hands and sparkles spawned if Deerclops/Lifelight haven't been defeated, but this is by far the easiest way to do this unfortunately
			if (Terratweaks.Eternitweaks.NoEnvironmentalDebuffs)
			{
				fsPlr.PureHeart = true;
			}

			orig(self);

			// Restore the usual value of the pure heart accessory; we don't wanna just force it to true/false since that might mess with other code elsewhere
			// Particularly, the components of Pure Heart (like Gutted Heart) have their effects buffed in Pure Heart, and if we force that to true/false it'll affect the actual accessory, which we don't want
			if (Terratweaks.Eternitweaks.NoEnvironmentalDebuffs)
			{
				fsPlr.PureHeart = oldPureHeart;
			}
		}
	}
}