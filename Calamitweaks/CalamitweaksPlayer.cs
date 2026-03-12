using CalamityMod;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CustomOnionSlot : ModAccessorySlot
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override bool IsEnabled()
		{
			if (Terratweaks.Calamitweaks.OnionMasterMode && Main.masterMode && Player.Calamity().extraAccessoryML)
				return true;

			return false;
		}
	}

	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksPlayer : ModPlayer
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override void PreUpdate()
		{
			// Essentially removes the lifesteal cap
			// This does still display the cooldown visual briefly which is annoying, but I can't seem to do anything about it unfortunately
			if (Player.whoAmI == Main.myPlayer && Terratweaks.Calamitweaks.NoLifestealCooldown)
				Player.lifeSteal = Main.expertMode ? 70 : 80;
		}

		public override void PostUpdateEquips()
		{
			CalamityPlayer calPlr = Player.Calamity();

			if (calPlr.GemTechState.IsPinkGemActive && Terratweaks.Calamitweaks.DRBuffs)
			{
				Player.endurance += 0.13f; // Increase DR granted from +12% to +25%
			}
		}
	}
}