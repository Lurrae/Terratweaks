using InfernumMode.Content.BehaviorOverrides.BossAIs.SupremeCalamitas;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod", "InfernumMode")]
	public class InfernumTweaks : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod") && ModLoader.HasMod("InfernumMode");

		private static readonly MethodInfo _scalPreAi = typeof(SupremeCalamitasBehaviorOverride).GetMethod("PreAI", BindingFlags.Instance | BindingFlags.Public);

		public override void Load()
		{
			MonoModHooks.Add(_scalPreAi, UnbanGamerChair);
			On_Mount.Dismount += PreventForcedDismount;
		}

		private bool shouldPreventDismount = false;

		private void PreventForcedDismount(On_Mount.orig_Dismount orig, Mount self, Player mountedPlayer)
		{
			if (Terratweaks.Calamitweaks.AllowGamerChairInInfernumScal && shouldPreventDismount)
				return;

			orig(self, mountedPlayer);
		}

		private bool UnbanGamerChair(Func<SupremeCalamitasBehaviorOverride, NPC, bool> orig, SupremeCalamitasBehaviorOverride self, NPC npc)
		{
			shouldPreventDismount = true;
			bool returnVal = orig(self, npc);
			shouldPreventDismount = false;

			return returnVal;
		}
	}
}