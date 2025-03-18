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

		public override void Load()
		{
			if (Terratweaks.Calamitweaks.ForceWormContactDamage)
			{
				short[] worms = [ NPCID.DevourerBody, NPCID.DevourerTail, NPCID.DiggerBody, NPCID.DiggerTail, NPCID.TombCrawlerBody, NPCID.TombCrawlerTail, NPCID.DuneSplicerBody, NPCID.DuneSplicerTail, NPCID.GiantWormBody, NPCID.GiantWormTail, NPCID.LeechBody, NPCID.LeechTail, NPCID.StardustWormBody, NPCID.StardustWormTail, NPCID.SeekerBody, NPCID.SeekerTail, NPCID.BoneSerpentBody, NPCID.BoneSerpentTail, NPCID.WyvernBody, NPCID.WyvernTail, NPCID.WyvernBody2, NPCID.WyvernBody3, NPCID.WyvernLegs, NPCID.CultistDragonBody1, NPCID.CultistDragonBody2, NPCID.CultistDragonBody3, NPCID.CultistDragonBody4, NPCID.CultistDragonTail, NPCID.BloodEelBody, NPCID.BloodEelTail ];
				// Remove all worm-type NPCs from the list of NPCs who don't deal contact damage
				// This list only ever contains vanilla NPCs afaik, so there's no reason to check for modded NPCs
				CalamityLists.ZeroContactDamageNPCList.RemoveAll(n => worms.Any(n2 => n2 == n));
			}
		}

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

		public override bool PreAI(NPC npc)
		{
			// First, check if the boss AI changes are even disabled
			if (Terratweaks.Calamitweaks.RevertVanillaBossAIChanges)
			{
				// Check for Revengeance, Death, or Infernum Mode being active, since those three should not be affected by this config option
				if (CalamityWorld.revenge || CalamityWorld.death || (ModLoader.TryGetMod("InfernumMod", out Mod infernum) && (bool)infernum.Call("GetInfernumActive")))
				{
					return base.PreAI(npc);
				}

				// Set EoW/Destroyer's DR increase timer so that they won't be invincible
				if ((npc.type >= NPCID.TheDestroyer && npc.type <= NPCID.TheDestroyerTail) || (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail))
				{
					CalamityGlobalNPC calNpc = npc.Calamity();
					calNpc.newAI[1] = 600f;
				}
			}

			return base.PreAI(npc);
		}

		public override void ModifyShop(NPCShop shop)
		{
			if (Terratweaks.Calamitweaks.DryadSellsSeeds)
			{
				Condition inAstral = new(CalamityUtils.GetText("Condition.InAstral"), () => Main.LocalPlayer.Calamity().ZoneAstral);
				Condition inCrags = new(CalamityUtils.GetText("Condition.InCrag"), () => Main.LocalPlayer.Calamity().ZoneCalamity);

				if (shop.NpcType == NPCID.Dryad)
				{
					shop.Add(ModContent.ItemType<AstralGrassSeeds>(), inAstral);
					shop.Add(ModContent.ItemType<CinderBlossomSeeds>(), inCrags);
				}
			}

			if (Terratweaks.Calamitweaks.NoSellingRoD)
			{
				if (shop.NpcType == NPCID.Wizard && shop.TryGetEntry(ItemID.RodofDiscord, out NPCShop.Entry entry))
				{
					entry.Disable();
				}
			}
		}
	}
}