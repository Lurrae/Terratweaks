using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Buffs
{
	public class BuffChanges : GlobalBuff
	{
		public override void SetStaticDefaults()
		{
			if (Terratweaks.Config.ChesterRework)
			{
				Main.vanityPet[BuffID.ChesterPet] = false;
				Main.lightPet[BuffID.ChesterPet] = true;
			}

			if (Terratweaks.Config.NoExpertDebuffTimes)
			{
				for (int i = 0; i < BuffID.Sets.LongerExpertDebuff.Length; i++)
					BuffID.Sets.LongerExpertDebuff[i] = false;
			}
		}

		// While the player has the Cerebral Mindtrick buff and is wearing a buffed Brain of Confusion, increase summon damage and crit chance by an extra 5%
		public override void Update(int type, Player player, ref int buffIndex)
		{
			if (type == BuffID.BrainOfConfusionBuff && Terratweaks.Config.WormBrain)
			{
				TerratweaksPlayer tPlr = player.GetModPlayer<TerratweaksPlayer>();

				if (tPlr.IsBuffedBrainEquipped())
				{
					player.GetCritChance(DamageClass.Generic) += 5;
					player.GetDamage(DamageClass.Summon) += 0.05f;
				}
			}
		}
	}
}