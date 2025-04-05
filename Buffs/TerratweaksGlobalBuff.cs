using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Buffs
{
	public class BuffChanges : GlobalBuff
	{
		public static readonly List<int> StationBuffs = new()
		{
			BuffID.AmmoBox,
			BuffID.Bewitched,
			BuffID.Clairvoyance,
			BuffID.Sharpened,
			BuffID.WarTable,
			BuffID.SugarRush
		};

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

			// Allow the Sugar Rush buff to have infinite duration
			if (Terratweaks.Config.InfiniteCakeSlice)
			{
				BuffID.Sets.TimeLeftDoesNotDecrease[BuffID.SugarRush] = true;
				Main.buffNoTimeDisplay[BuffID.SugarRush] = true;
			}

			// Allow all station buffs to persist through death, just like flasks do
			if (Terratweaks.Config.PersistentStationBuffs)
			{
				foreach (int buffID in StationBuffs)
				{
					Main.persistentBuff[buffID] = true;
				}
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