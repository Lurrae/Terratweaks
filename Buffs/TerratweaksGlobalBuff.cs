using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
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

			// Allow the Sugar Rush buff to have infinite duration
			if (Terratweaks.Config.InfiniteCakeSlice)
			{
				BuffID.Sets.TimeLeftDoesNotDecrease[BuffID.SugarRush] = true;
				Main.buffNoTimeDisplay[BuffID.SugarRush] = true;
			}

			// Allow all station buffs to persist through death, just like flasks do
			if (Terratweaks.Config.PersistentStationBuffs)
			{
				for (int i = 0; i < TerratweaksContentSets.StationBuff.Length; i++)
				{
					Main.persistentBuff[i] |= TerratweaksContentSets.StationBuff[i]; // If it's a station buff, make it persistent, otherwise don't change it
				}
			}
		}

		public override void Update(int type, Player player, ref int buffIndex)
		{
			TerratweaksPlayer tPlr = player.GetModPlayer<TerratweaksPlayer>();

			// While the player has the Cerebral Mindtrick buff and is wearing a buffed Brain of Confusion, increase summon damage and crit chance by an extra 5%
			if (type == BuffID.BrainOfConfusionBuff && Terratweaks.Config.WormBrain)
			{
				if (tPlr.IsBuffedBrainEquipped())
				{
					player.GetCritChance(DamageClass.Generic) += 5;
					player.GetDamage(DamageClass.Summon) += 0.05f;
				}
			}

			// While in werebeaver form, tick down the debuff's duration an extra time each frame if it's a hot or cold debuff
			if (tPlr.werebeaver && (TerratweaksContentSets.HotDebuff[type] || TerratweaksContentSets.ColdDebuff[type]))
			{
				player.buffTime[buffIndex]--;

				// Clear the debuff if it runs out of time
				if (player.buffTime[buffIndex] <= 0)
				{
					player.DelBuff(buffIndex);
					buffIndex--;
				}
			}

			// Provide chilled/frozen immunity while under the effects of the Warmth Potion
			if (type == BuffID.Warmth && Terratweaks.Config.WarmthGivesColdImmunity)
			{
				player.buffImmune[BuffID.Chilled] = true;
				player.buffImmune[BuffID.Frozen] = true;
			}
		}

        public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare)
        {
			if (type == BuffID.Warmth && Terratweaks.Config.WarmthGivesColdImmunity)
			{
				tip += Language.GetTextValue("Mods.Terratweaks.Common.WarmthBuffExtraTip");
			}
		}
	}
}