using Terraria.ModLoader;
using Terratweaks;

namespace TerrarmoryChanges
{
	public class TerrarmoryChanges : Mod
	{
		public override object Call(params object[] args)
		{
			if (args[0] is string content)
			{
				switch (content)
				{
					case "IsSentryKillingEnabled":
						return ModContent.GetInstance<TerratweaksConfig>().vanillaChanges.KillSentries;
				}
			}

			return true;
		}
	}
}