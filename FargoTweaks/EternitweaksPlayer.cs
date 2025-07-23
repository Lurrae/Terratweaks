using FargowiltasSouls.Content.Buffs.Masomode;
using Terraria;
using Terraria.ModLoader;

namespace Terratweaks.FargoTweaks
{
	[JITWhenModsEnabled("FargowiltasSouls")]
	public class EternitweaksPlayer : ModPlayer
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("FargowiltasSouls");

		public override void UpdateEquips()
		{
			// Provide immunity to the four class-sealing debuffs so that any weapon can be used on Lunatic Cultist, the pillars, and Moon Lord
			if (Terratweaks.Eternitweaks.NoClassSealDebuffs)
			{
				Player.buffImmune[ModContent.BuffType<AtrophiedBuff>()] = true;
				Player.buffImmune[ModContent.BuffType<JammedBuff>()] = true;
				Player.buffImmune[ModContent.BuffType<ReverseManaFlowBuff>()] = true;
				Player.buffImmune[ModContent.BuffType<AntisocialBuff>()] = true;
				Player.buffImmune[ModContent.BuffType<NullificationCurseBuff>()] = true;
			}
		}
	}
}