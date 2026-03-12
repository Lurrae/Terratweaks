using CalamityMod;
using CalamityMod.Items.Accessories;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.SunkenSea;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksNPCs : GlobalNPC
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override void SetBestiary(NPC npc, BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			if (Terratweaks.Calamitweaks.EzCalBanners)
			{
				if (npc.type == ModContent.NPCType<GiantClam>())
				{
					if (bestiaryEntry.UIInfoProvider is CommonEnemyUICollectionInfoProvider uiInfo)
					{
						FieldInfo fieldInfo = typeof(CommonEnemyUICollectionInfoProvider).GetField("_killCountNeededToFullyUnlock", BindingFlags.NonPublic | BindingFlags.Instance);
						fieldInfo.SetValue(uiInfo, 1);
					}
				}
			}
		}

		public override void SetDefaults(NPC npc)
		{
			if (Terratweaks.Calamitweaks.NoDefenseDamage)
			{
				CalamityGlobalNPC calNpc = npc.Calamity();
				calNpc.canBreakPlayerDefense = false;
			}
		}

		public override void OnKill(NPC npc)
		{
			if (npc.type == ModContent.NPCType<Providence>() && Terratweaks.Calamitweaks.EnragedProvDropsSoulCrystal != ProfanedSoulCrystalSetting.Off && Main.netMode == NetmodeID.SinglePlayer)
			{
				Providence prov = npc.ModNPC<Providence>();
				bool enraged = prov.hasBeenGivenFullPower;
				
				if (Terratweaks.Calamitweaks.EnragedProvDropsSoulCrystal == ProfanedSoulCrystalSetting.EnragedPlusArtifact)
				{
					int psa = ModContent.ItemType<ProfanedSoulArtifact>();
					enraged = prov.hasBeenGivenFullPower && (Main.LocalPlayer.HasItemInInventoryOrOpenVoidBag(psa) || Main.LocalPlayer.armor.Any(i => i.type == psa));
				}

				if (enraged)
					Main.NewText(Language.GetTextValue("Mods.CalamityMod.Status.Progression.ProfanedBossText4"), Color.DarkOrange);
			}
		}

		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			if (npc.type == ModContent.NPCType<Providence>() && Terratweaks.Calamitweaks.EnragedProvDropsSoulCrystal != ProfanedSoulCrystalSetting.Off)
			{
				string conditionText = Language.GetTextValue("Mods.Terratweaks.Conditions.EnragedProvidence");

				if (Terratweaks.Calamitweaks.EnragedProvDropsSoulCrystal == ProfanedSoulCrystalSetting.EnragedPlusArtifact)
					conditionText = Language.GetTextValue("Mods.Terratweaks.Conditions.EnragedPlusArtifact");

				npcLoot.DefineConditionalDropSet(DropHelper.If(info =>
				{
					Providence prov = info.npc.ModNPC<Providence>();
					int psa = ModContent.ItemType<ProfanedSoulArtifact>();
					if (Terratweaks.Calamitweaks.EnragedProvDropsSoulCrystal == ProfanedSoulCrystalSetting.EnragedPlusArtifact)
						return prov.hasBeenGivenFullPower && (info.player.HasItemInInventoryOrOpenVoidBag(psa) || info.player.armor.Any(i => i.type == psa));
					return prov.hasBeenGivenFullPower;
				}, () => Main.expertMode, conditionText)).Add(ModContent.ItemType<ProfanedSoulCrystal>());
			}
		}
	}
}