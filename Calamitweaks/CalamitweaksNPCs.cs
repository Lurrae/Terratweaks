using CalamityMod;
using CalamityMod.Items.Placeables;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.World;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
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
	}
}