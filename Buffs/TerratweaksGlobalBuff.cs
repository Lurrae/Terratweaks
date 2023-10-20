using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Terratweaks.Buffs
{
	public class BuffChanges : GlobalBuff
	{
		public override void SetStaticDefaults()
		{
			if (GetInstance<TerratweaksConfig>().ChesterRework)
			{
				Main.vanityPet[BuffID.ChesterPet] = false;
				Main.lightPet[BuffID.ChesterPet] = true;
			}

			if (GetInstance<TerratweaksConfig>().NoExpertDebuffTimes)
			{
				for (int i = 0; i < BuffID.Sets.LongerExpertDebuff.Length; i++)
					BuffID.Sets.LongerExpertDebuff[i] = false;
			}
		}
	}
}