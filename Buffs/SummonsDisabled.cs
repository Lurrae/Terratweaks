using Terraria;
using Terraria.ModLoader;

namespace Terratweaks.Buffs
{
	public class SummonsDisabled : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// See localization files for name and tooltip
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<TerratweaksPlayer>().summonsDisabled = true;
		}
	}
}