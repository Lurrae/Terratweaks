global using TepigCore;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terratweaks;

namespace TerrarmoryChanges
{
	public class TerrarmoryChanges : Mod
	{
		bool playerHasChesterSafeOpened = false;

		public override void Load()
		{
			On_Main.TryInteractingWithMoneyTrough += On_Main_TryInteractingWithMoneyTrough;
			On_Player.HandleBeingInChestRange += On_Player_HandleBeingInChestRange;
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