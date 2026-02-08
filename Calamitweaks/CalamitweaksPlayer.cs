using CalamityMod;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
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

		//public float aquaticBoostMounted = 0f;

		public override void PostUpdateBuffs()
		{
			if (Player.HasBuff(BuffID.IceBarrier) && Terratweaks.Calamitweaks.DRBuffs)
				Player.endurance += 0.1f;
		}

		public override void PostUpdateEquips()
		{
			CalamityPlayer calPlr = Player.Calamity();

			if (calPlr.GemTechState.IsPinkGemActive && Terratweaks.Calamitweaks.DRBuffs)
			{
				Player.endurance += 0.13f; // Increase DR granted from +12% to +25%
			}

			// Smoothly convert existing mounted/non-mounted boosts between each other
			// Without this, the boost would reset upon mounting/un-mounting
			// No longer needed as of Calamity version 2.1!
			/*if (calPlr.aquaticEmblem && Terratweaks.Calamitweaks.AquaticEmblemBuff)
			{
				if (Player.mount.Active)
				{
					if (aquaticBoostMounted > calPlr.aquaticBoost)
					{
						aquaticBoostMounted = calPlr.aquaticBoost;
						calPlr.aquaticBoost = calPlr.aquaticBoostMax;
					}
				}
				else
				{
					if (aquaticBoostMounted < calPlr.aquaticBoost)
					{
						calPlr.aquaticBoost = aquaticBoostMounted;
						aquaticBoostMounted = calPlr.aquaticBoostMax;
					}
				}
			}*/
		}

		/*public override void PostUpdateMiscEffects()
		{
			CalamityPlayer calPlr = Player.Calamity();

			if (!Terratweaks.Calamitweaks.AquaticEmblemBuff)
				return;

			// Allow Aquatic Emblem to work with mounts because it is SO dumb that it automatically negates the effects as soon as you mount
			// This code is literally just the original Aquatic Emblem code but without the stupid mount checks
			if (calPlr.aquaticEmblem && Player.mount.Active)
			{
				if (Player.IsUnderwater() && Player.wet && !Player.lavaWet && !Player.honeyWet)
				{
					if (aquaticBoostMounted > 0f)
					{
						aquaticBoostMounted -= 2f;

						if (aquaticBoostMounted <= 0f)
						{
							aquaticBoostMounted = 0f;

							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								NetMessage.SendData(MessageID.PlayerStealth, number: Player.whoAmI);
							}
						}
					}
				}
				else
				{
					aquaticBoostMounted += 2f;

					if (aquaticBoostMounted > calPlr.aquaticBoostMax)
					{
						aquaticBoostMounted = calPlr.aquaticBoostMax;
					}
				}

				Player.statDefense += (int)((1f - aquaticBoostMounted * 0.0001f) * 50f);
				Player.moveSpeed -= (1f - aquaticBoostMounted * 0.0001f) * 0.1f;
			}
			else
			{
				aquaticBoostMounted = calPlr.aquaticBoostMax;
			}
		}*/

		// Force enraged (daytime) EoL to insta-kill if the config setting is enabled
		/*public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
		{
			if (Terratweaks.Calamitweaks.EnragedEoLInstakills && npc.type == NPCID.HallowBoss && npc.Calamity().CurrentlyEnraged)
			{
				modifiers.FinalDamage.Base = 9999;
				modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) => info.Dodgeable = false; // Ensures you can't dodge this attack
			}
		}

		public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
		{
			if (Terratweaks.Calamitweaks.EnragedEoLInstakills)
			{
				CalamitweaksProjs cProj = proj.GetGlobalProjectile<CalamitweaksProjs>();

				if (cProj.OwnerEoL > -1)
				{
					NPC npc = Main.npc[cProj.OwnerEoL];

					if (npc != null && npc.active && npc.Calamity() != null)
					{
						if (npc.type == NPCID.HallowBoss && npc.Calamity().CurrentlyEnraged)
						{
							modifiers.FinalDamage.Base = Main.masterMode ? 59994 : Main.expertMode ? 39996 : 19998;
							modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) => info.Dodgeable = false; // Ensures you can't dodge this attack
						}
					}
				}
			}
		}*/
	}
}