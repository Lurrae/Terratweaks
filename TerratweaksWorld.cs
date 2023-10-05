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
			if (ModContent.GetInstance<TerratweaksConfig>().PostEyeSandstorms && !NPC.downedBoss1)
			{
				Sandstorm.Happening = false;
			}
		}
	}
}