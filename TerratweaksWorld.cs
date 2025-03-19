using System.Linq;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ModLoader;

namespace Terratweaks
{
	public class TerratweaksWorld : ModSystem
	{
		public override void PostUpdateWorld()
		{
			// Block sandstorms from happening if Eye of Cthulhu has not been defeated and the config setting is enabled
			if (Terratweaks.Config.PostEyeSandstorms && !NPC.downedBoss1)
			{
				Sandstorm.Happening = false;
			}
		}

		public override void ResetNearbyTileEffects()
		{
			foreach (Player plr in Main.player.Where(p => p.active))
			{
				plr.GetModPlayer<InputPlayer>().inGravGlobeRange = false;
			}
		}
	}
}