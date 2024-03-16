global using TepigCore;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terratweaks
{
	public enum PacketType
	{
		SyncInferno = 0
	}

	public static class TerratweaksDropConditions
	{
		public class NightEoL : IItemDropRuleCondition, IProvideItemConditionDescription
		{
			public bool CanDrop(DropAttemptInfo info)
			{
				return !info.npc.AI_120_HallowBoss_IsGenuinelyEnraged();
			}

			public bool CanShowItemDropInUI()
			{
				return true;
			}

			public string GetConditionDescription()
			{
				return Language.GetTextValue("Mods.Terratweaks.Conditions.NightEoL");
			}
		}
	}

	public class Terratweaks : Mod
	{
		bool playerHasChesterSafeOpened = false;

		public static ModKeybind InfernoToggleKeybind { get; private set; }
		public static ModKeybind RulerToggleKeybind { get; private set; }
		public static ModKeybind MechRulerToggleKeybind { get; private set; }
		public static readonly List<int> DyeItemsSoldByTrader = new();

		public override void Load()
		{
			InfernoToggleKeybind = KeybindLoader.RegisterKeybind(this, "InfernoToggle", "I");
			RulerToggleKeybind = KeybindLoader.RegisterKeybind(this, "RulerToggle", "NumPad1");
			MechRulerToggleKeybind = KeybindLoader.RegisterKeybind(this, "MechRulerToggle", "NumPad2");
			
			On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
			On_Main.TryInteractingWithMoneyTrough += On_Main_TryInteractingWithMoneyTrough;
			On_Player.HandleBeingInChestRange += On_Player_HandleBeingInChestRange;
			On_Player.UpdateJumpHeight += On_Player_UpdateJumpHeight;
			On_NPC.CountKillForBannersAndDropThem += On_NPC_CountKillForBannersAndDropThem;
			On_Main.DamageVar_float_int_float += On_Main_DamageVar_float_int_float;
		}

		private int On_Main_DamageVar_float_int_float(On_Main.orig_DamageVar_float_int_float orig, float dmg, int percent, float luck)
		{
			if (ModContent.GetInstance<TerratweaksConfig>().NoDamageVariance == DamageVarianceSetting.On)
			{
				return (int)Math.Round(dmg);
			}
			else
			{
				return orig(dmg, percent, luck);
			}
		}

		private void On_Player_UpdateJumpHeight(On_Player.orig_UpdateJumpHeight orig, Player self)
		{
			orig(self);

			if (self.GetModPlayer<TerratweaksPlayer>().radiantInsignia) // Mimic SI's effect
			{
				self.jumpSpeedBoost += 1.8f;
			}
		}

		public override void HandlePacket(BinaryReader reader, int fromWho)
		{
			PacketType type = (PacketType)reader.ReadByte();

			if (type == PacketType.SyncInferno)
			{
				if (Main.netMode == NetmodeID.Server) // Server needs to send out packets to all other players
				{
					int playerIdxToIgnore = fromWho;
					bool value = reader.ReadBoolean();

					ModPacket packet = GetPacket();
					packet.Write((byte)PacketType.SyncInferno);
					packet.Write(value);
					packet.Write(playerIdxToIgnore);
					packet.Send(ignoreClient: fromWho);
				}
				else // Multiplayer client needs to update the inferno visuals for the player they just received data from
				{
					int senderWhoAmI = reader.ReadInt32();
					bool value = reader.ReadBoolean();
					Main.player[senderWhoAmI].GetModPlayer<InputPlayer>().showInferno = value;
				}
			}
		}

		public override void Unload()
		{
			InfernoToggleKeybind = null;
			RulerToggleKeybind = null;
			MechRulerToggleKeybind = null;
		}

		private void On_NPC_CountKillForBannersAndDropThem(On_NPC.orig_CountKillForBannersAndDropThem orig, NPC self)
		{
			if (ModContent.GetInstance<TerratweaksConfig>().BannersDontSpamChat)
			{
				// Code adapted from the original method, just modified to use CombatText instead of printing to chat
				int num = Item.NPCtoBanner(self.BannerID());
				if (num > 0 && !self.ExcludedFromDeathTally())
				{
					NPC.killCount[num]++;
					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendData(MessageID.NPCKillCountDeathTally, -1, -1, null, num, 0f, 0f, 0f, 0, 0, 0);
					}
					int num2 = ItemID.Sets.KillsToBanner[Item.BannerToItem(num)];
					if (NPC.killCount[num] % num2 == 0 && num > 0)
					{
						int num3 = Item.BannerToNPC(num);
						//int num4 = self.lastInteraction;
						Player player = Main.player[self.lastInteraction];
						if (!player.active || player.dead)
						{
							player = Main.player[self.FindClosestPlayer()];
						}
						string message = Language.GetTextValue("Game.EnemiesDefeatedAnnouncement", NPC.killCount[num], Lang.GetNPCName(num3));
						if (player.whoAmI >= 0 && player.whoAmI < 255)
						{
							message = Language.GetTextValue("Game.EnemiesDefeatedByAnnouncement", player.name, NPC.killCount[num], Lang.GetNPCName(num3));
						}

						Rectangle rect = new((int)player.position.X, (int)player.position.Y + 192, 16, 16);
						Color color = new(250, 250, 0);

						if (Main.netMode == NetmodeID.SinglePlayer)
						{
							CombatText.NewText(rect, color, message, true);
						}
						else if (Main.netMode == NetmodeID.Server)
						{
							foreach (Player plr in Main.player.Where(p => p.active))
							{
								CombatText.NewText(rect, color, message, true);
							}
						}
						int num5 = Item.BannerToItem(num);
						Vector2 position = self.position;
						if (player.whoAmI >= 0 && player.whoAmI < 255)
						{
							position = player.position;
						}
						Item.NewItem(self.GetSource_Loot(), (int)position.X, (int)position.Y, self.width, self.height, num5, 1, false, 0, false, false);
					}
				}
			}
			// Just run the original method if the config is disabled, no need to do anything special
			else
			{
				orig(self);
			}
		}

		private void On_Main_DrawInfernoRings(On_Main.orig_DrawInfernoRings orig, Main self)
		{
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];

				if (player.active && !player.outOfRange && !player.dead)
				{
					if (!player.GetModPlayer<InputPlayer>().showInferno)
						player.inferno = false;
				}
			}

			orig(self);
		}

		private void On_Player_HandleBeingInChestRange(On_Player.orig_HandleBeingInChestRange orig, Player self)
		{
			bool chesterRework = ModContent.GetInstance<TerratweaksConfig>().ChesterRework;
			if (!chesterRework) // No need to run any special code if the rework is disabled, other than setting the chester safe bool to false
			{
				orig(self);
				playerHasChesterSafeOpened = false;
				return;
			}

			if (playerHasChesterSafeOpened)
				self.chest = -2;

			orig(self);

			if (playerHasChesterSafeOpened)
			{
				if (self.chest == -1)
					playerHasChesterSafeOpened = false;
				else
					self.chest = -3;
			}
		}

		private int On_Main_TryInteractingWithMoneyTrough(On_Main.orig_TryInteractingWithMoneyTrough orig, Projectile proj)
		{
			Player player = Main.LocalPlayer;
			bool chesterRework = ModContent.GetInstance<TerratweaksConfig>().ChesterRework;

			// Make sure the game closes the safe when needed
			// This SHOULD only trigger on safe Chester
			if (proj.type == ProjectileID.ChesterPet && Main.mouseRight && Main.mouseRightRelease && playerHasChesterSafeOpened)
			{
				Main.LocalPlayer.chest = -2;
				playerHasChesterSafeOpened = false;
			}

			int originalReturn = orig(proj);

			// Check if the player's currently opened chest is Chester, and if so, set the player's opened chest to the safe instead of the piggy bank
			if (chesterRework && proj.type == ProjectileID.ChesterPet
				&& player.chestX == (int)(proj.Center.X / 16) && player.chestY == (int)(proj.Center.Y / 16)
				&& player.chest == -2)
			{
				player.chest = -3;
				playerHasChesterSafeOpened = true;
				Recipe.FindRecipes();
			}

			return originalReturn;
		}

		public override object Call(params object[] args)
		{
			if (args[0] is string content)
			{
				switch (content)
				{
					case "IsSentryKillingEnabled":
						return ModContent.GetInstance<TerratweaksConfig>().KillSentries;
				}
			}

			return true;
		}
	}
}