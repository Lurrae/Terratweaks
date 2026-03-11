using MonoMod.Cil;
using Spooky.Content.Tiles.Blooms;
using System.Reflection;
using Terraria.ModLoader;

namespace Terratweaks.Spookitweaks
{
	[JITWhenModsEnabled("Spooky")]
	public class Spookitweaks : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Spooky");

		private static readonly MethodInfo _CemeteryBloom_KillMultiTile = typeof(CemeteryBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _DandelionBloom_KillMultiTile = typeof(DandelionBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _DragonfruitBloom_KillMultiTile = typeof(DragonfruitBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _FallBloom_KillMultiTile = typeof(FallBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _FossilBloom_KillMultiTile = typeof(FossilBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _SeaBloom_KillMultiTile = typeof(SeaBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _SpringBloom_KillMultiTile = typeof(SpringBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _SummerBloom_KillMultiTile = typeof(SummerBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _VegetableBloom_KillMultiTile = typeof(VegetableBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo _WinterBloom_KillMultiTile = typeof(WinterBloomPlant).GetMethod("KillMultiTile", BindingFlags.Public | BindingFlags.Instance);

		public override void Load()
		{
			MonoModHooks.Modify(_CemeteryBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_DandelionBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_DragonfruitBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_FallBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_FossilBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_SeaBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_SpringBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_SummerBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_VegetableBloom_KillMultiTile, AdjustBloomDropRates);
			MonoModHooks.Modify(_WinterBloom_KillMultiTile, AdjustBloomDropRates);
		}

		private void AdjustBloomDropRates(ILContext il)
		{
			var c = new ILCursor(il);

			if (!c.TryGotoNext(i => i.MatchCall("Terraria.Utils", "NextBool")))
			{
				Mod.Logger.Warn("Spookitweaks IL edit failed when trying to adjust bloom drop rates! Dumping IL logs...");
				MonoModHooks.DumpIL(Mod, il);
				return;
			}

			c.EmitDelegate((int original) => Terratweaks.Spookitweaks.BetterWateringGourd);
		}
	}
}