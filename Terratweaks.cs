global using TepigCore;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks
{
	public enum PacketType
	{
		SyncInferno = 0
	}

	public class Terratweaks : Mod
	{
		bool playerHasChesterSafeOpened = false;

		public static ModKeybind InfernoToggleKeybind { get; private set; }
		public static readonly List<int> DyeItemsSoldByTrader = new();

		public override void Load()
		{
			InfernoToggleKeybind = KeybindLoader.RegisterKeybind(this, "InfernoToggle", "I");
			On_Main.DrawInfernoRings += On_Main_DrawInfernoRings;
			On_Main.TryInteractingWithMoneyTrough += On_Main_TryInteractingWithMoneyTrough;
			On_Player.HandleBeingInChestRange += On_Player_HandleBeingInChestRange;
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