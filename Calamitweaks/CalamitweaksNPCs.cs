using CalamityMod;
using CalamityMod.Items.Placeables;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SunkenSea;
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
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.EzCalBanners)
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
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.NoDefenseDamage)
			{
				CalamityGlobalNPC calNpc = npc.Calamity();
				calNpc.canBreakPlayerDefense = false;
			}
		}

		public override void PostAI(NPC npc)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			// ModNPC check makes this only affect vanilla bosses
			if (npc.boss && npc.ModNPC == null && calamitweaks.ForceBossContactDamage)
			{
				// Force the boss to deal damage at all times
				// TODO: Is this the best way to do this?
				npc.damage = npc.defDamage;
			}
		}

		public override void ModifyShop(NPCShop shop)
		{
			CalTweaks calamitweaks = ModContent.GetInstance<TerratweaksConfig>().calamitweaks;

			if (calamitweaks.DryadSellsSeeds)
			{
				Condition inAstral = new(CalamityUtils.GetText("Condition.InAstral"), () => Main.LocalPlayer.Calamity().ZoneAstral);
				Condition inCrags = new(CalamityUtils.GetText("Condition.InCrag"), () => Main.LocalPlayer.Calamity().ZoneCalamity);

				if (shop.NpcType == NPCID.Dryad)
				{
					shop.Add(ModContent.ItemType<AstralGrassSeeds>(), inAstral);
					shop.Add(ModContent.ItemType<CinderBlossomSeeds>(), inCrags);
				}
			}
		}
	}
}