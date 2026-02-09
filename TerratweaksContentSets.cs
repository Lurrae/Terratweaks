using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks
{
	[ReinitializeDuringResizeArrays] // Allows this class to utilize modded content. Probably not strictly necessary due to Terratweaks' nature but-
	public static class TerratweaksContentSets
	{
		// I'm putting the names and description for every custom named set in their own variables for convenience
		// Note that each set's key will be appended with "Terratweaks/", so they'll actually appear as "Terratweaks/<key>"
		#region ItemID.Sets
		const string SingleUsePermBuffs_Key = "SingleUsePermBuffs";
		const string SingleUsePermBuffs_Desc = "Tracks the variable associated with a given permanent boost item, like the Vital Crystal or Artisan's Loaf";

		public static Func<Player, bool>[] SingleUsePermBuffs = ItemID.Sets.Factory.CreateNamedSet(SingleUsePermBuffs_Key)
			.Description(SingleUsePermBuffs_Desc)
			.RegisterCustomSet<Func<Player, bool>>(null,
			ItemID.DemonHeart, (Player p) => p.extraAccessory,
			ItemID.CombatBook, (Player p) => NPC.combatBookWasUsed,
			ItemID.ArtisanLoaf, (Player p) => p.ateArtisanBread,
			ItemID.TorchGodsFavor, (Player p) => p.unlockedBiomeTorches,
			ItemID.AegisCrystal, (Player p) => p.usedAegisCrystal,
			ItemID.AegisFruit, (Player p) => p.usedAegisFruit,
			ItemID.ArcaneCrystal, (Player p) => p.usedArcaneCrystal,
			ItemID.Ambrosia, (Player p) => p.usedAmbrosia,
			ItemID.GummyWorm, (Player p) => p.usedGummyWorm,
			ItemID.GalaxyPearl, (Player p) => p.usedGalaxyPearl,
			ItemID.CombatBookVolumeTwo, (Player p) => NPC.combatBookVolumeTwoWasUsed,
			ItemID.PeddlersSatchel, (Player p) => NPC.peddlersSatchelWasUsed,
			ItemID.MinecartPowerup, (Player p) => p.unlockedSuperCart
			);

		const string MultiUsePermBuffs_Key = "MultiUsePermBuffs";
		const string MultiUsePermBuffs_Desc = "Tracks the variable associated with a given multi-use permanent boost item, like Life Crystals or Mana Crystals";

		public static Func<Player, Vector2>[] MultiUsePermBuffs = ItemID.Sets.Factory.CreateNamedSet(MultiUsePermBuffs_Key)
			.Description(MultiUsePermBuffs_Desc)
			.RegisterCustomSet<Func<Player, Vector2>>(null,
			ItemID.LifeCrystal, (Player p) => new Vector2(p.ConsumedLifeCrystals, 15),
			ItemID.ManaCrystal, (Player p) => new Vector2(p.ConsumedManaCrystals, 9),
			ItemID.LifeFruit, (Player p) => new Vector2(p.ConsumedLifeFruit, 20)
			);

		const string SpecialSummonWeapons_Key = "SpecialSummonWeapons";
		const string SpecialSummonWeapons_Desc = "If set to true for an item type, prevents the effects of the summon weapon rework (removes mana cost, applies 5-second cooldown during boss fights) from applying";

		public static bool[] SpecialSummonWeapons = ItemID.Sets.Factory.CreateNamedSet(SpecialSummonWeapons_Key)
			.Description(SpecialSummonWeapons_Desc)
			.RegisterBoolSet(false);

		const string ShimmerableBossDrops_Key = "ShimmerableBossDrops";
		const string ShimmerableBossDrops_Desc = "Stores a list of shimmer transmutations for an item that should be enabled with the Shimmerable Boss Drops config option";

		public static List<int>[] ShimmerableBossDrops = ItemID.Sets.Factory.CreateNamedSet(ShimmerableBossDrops_Key)
			.Description(ShimmerableBossDrops_Desc)
			.RegisterCustomSet<List<int>>(null,
			// Note: Technically the "key" item (i.e, Slime Gun, Ninja Hood, etc.) is not actually needed at all, it just makes the item set work
			// Obviously each item does need to be unique though, otherwise the existing shimmer cycle will get replaced
			ItemID.SlimeGun, new List<int> { ItemID.SlimeGun, ItemID.SlimeHook },
			ItemID.NinjaHood, new List<int> { ItemID.NinjaHood, ItemID.NinjaShirt, ItemID.NinjaPants },
			ItemID.ApprenticeScarf, new List<int> { ItemID.ApprenticeScarf, ItemID.SquireShield },
			ItemID.DD2PetGato, new List<int> { ItemID.DD2PetGato, ItemID.DD2PetDragon, ItemID.DD2PetGhost },
			ItemID.BeeKeeper, new List<int> { ItemID.BeeKeeper, ItemID.BeesKnees, ItemID.BeeGun },
			ItemID.BeeHat, new List<int> { ItemID.BeeHat, ItemID.BeeShirt, ItemID.BeePants },
			ItemID.LucyTheAxe, new List<int> { ItemID.LucyTheAxe, ItemID.PewMaticHorn, ItemID.WeatherPain, ItemID.HoundiusShootius },
			ItemID.ShadowFlameKnife, new List<int> { ItemID.ShadowFlameKnife, ItemID.ShadowFlameBow, ItemID.ShadowFlameHexDoll },
			ItemID.CrystalNinjaHelmet, new List<int> { ItemID.CrystalNinjaHelmet, ItemID.CrystalNinjaChestplate, ItemID.CrystalNinjaLeggings },
			ItemID.HuntressBuckler, new List<int> { ItemID.HuntressBuckler, ItemID.MonkBelt },
			ItemID.DD2SquireDemonSword, new List<int> { ItemID.DD2SquireDemonSword, ItemID.MonkStaffT1, ItemID.MonkStaffT2, ItemID.DD2PhoenixBow, ItemID.BookStaff },
			ItemID.Seedler, new List<int> { ItemID.Seedler, ItemID.FlowerPow, ItemID.VenusMagnum, ItemID.GrenadeLauncher, ItemID.NettleBurst, ItemID.LeafBlower, ItemID.WaspGun },
			ItemID.StakeLauncher, new List<int> { ItemID.StakeLauncher, ItemID.NecromanticScroll, ItemID.SpookyHook, ItemID.SpookyTwig, ItemID.CursedSapling },
			ItemID.TheHorsemansBlade, new List<int> { ItemID.TheHorsemansBlade, ItemID.CandyCornRifle, ItemID.JackOLanternLauncher, ItemID.BatScepter, ItemID.RavenStaff, ItemID.ScytheWhip, ItemID.BlackFairyDust, ItemID.SpiderEgg },
			ItemID.ChristmasTreeSword, new List<int> { ItemID.ChristmasTreeSword, ItemID.Razorpine, ItemID.FestiveWings, ItemID.ChristmasHook },
			ItemID.ElfMelter, new List<int> { ItemID.ElfMelter, ItemID.ChainGun },
			ItemID.NorthPole, new List<int> { ItemID.NorthPole, ItemID.SnowmanCannon, ItemID.BlizzardStaff, ItemID.BabyGrinchMischiefWhistle, ItemID.ReindeerBells },
			ItemID.GolemFist, new List<int> { ItemID.GolemFist, ItemID.PossessedHatchet, ItemID.Stynger, ItemID.HeatRay, ItemID.StaffofEarth, ItemID.EyeoftheGolem, ItemID.SunStone },
			ItemID.Flairon, new List<int> { ItemID.Flairon, ItemID.Tsunami, ItemID.RazorbladeTyphoon, ItemID.BubbleGun, ItemID.TempestStaff },
			ItemID.PiercingStarlight, new List<int> { ItemID.PiercingStarlight, ItemID.FairyQueenRangedItem, ItemID.FairyQueenMagicItem, ItemID.RainbowWhip },
			ItemID.DD2SquireBetsySword, new List<int> { ItemID.DD2SquireBetsySword, ItemID.DD2BetsyBow, ItemID.ApprenticeStaffT3, ItemID.MonkStaffT3 },
			ItemID.InfluxWaver, new List<int> { ItemID.InfluxWaver, ItemID.Xenopopper, ItemID.ElectrosphereLauncher, ItemID.LaserMachinegun, ItemID.XenoStaff, ItemID.CosmicCarKey },
			ItemID.Meowmere, new List<int> { ItemID.Meowmere, ItemID.StarWrath, ItemID.Terrarian, ItemID.SDMG, ItemID.Celeb2, ItemID.LastPrism, ItemID.LunarFlareBook, ItemID.RainbowCrystalStaff, ItemID.MoonlordTurretStaff }
			);

		const string ShimmerableEnemySets_Key = "ShimmerableEnemySets";
		const string ShimmerableEnemySets_Desc = "Stores a list of shimmer transmutations for an item that should be enabled with the Shimmerable Enemy-Dropped Armor/Vanity config option";

		public static List<int>[] ShimmerableEnemySets = ItemID.Sets.Factory.CreateNamedSet(ShimmerableEnemySets_Key)
			.Description(ShimmerableEnemySets_Desc)
			.RegisterCustomSet<List<int>>(null,
			// Note: As with ShimmerableBossDrops, the "key" item isn't actually used, but does have to be unique
			ItemID.MummyMask, new List<int> { ItemID.MummyMask, ItemID.MummyShirt, ItemID.MummyPants },
			ItemID.LamiaHat, new List<int> { ItemID.LamiaHat, ItemID.LamiaShirt, ItemID.LamiaPants },
			ItemID.ScarecrowHat, new List<int> { ItemID.ScarecrowHat, ItemID.ScarecrowShirt, ItemID.ScarecrowPants },
			ItemID.ElfHat, new List<int> { ItemID.ElfHat, ItemID.ElfShirt, ItemID.ElfPants },
			ItemID.MartianUniformHelmet, new List<int> { ItemID.MartianUniformHelmet, ItemID.MartianUniformTorso, ItemID.MartianUniformPants },
			ItemID.MartianCostumeMask, new List<int> { ItemID.MartianCostumeMask, ItemID.MartianCostumeShirt, ItemID.MartianCostumePants },
			ItemID.PedguinHat, new List<int> { ItemID.PedguinHat, ItemID.PedguinShirt, ItemID.PedguinPants },
			ItemID.RainHat, new List<int> { ItemID.RainHat, ItemID.RainCoat },
			ItemID.EskimoCoat, new List<int> { ItemID.EskimoCoat, ItemID.EskimoCoat, ItemID.EskimoPants }
			);

		const string NoBiomeBlacklist_Key = "NoBiomeBlacklist";
		const string NoBiomeBlacklist_Desc = "If set to true for an item type, the item won't have its biome conditions removed from shops with the config option that disables shop biome conditions";

		public static bool[] NoBiomeBlacklist = ItemID.Sets.Factory.CreateNamedSet(NoBiomeBlacklist_Key)
			.Description(NoBiomeBlacklist_Desc)
			.RegisterBoolSet(false);

		const string OnFireImmunityItem_Key = "OnFireImmunityItem";
		const string OnFireImmunityItem_Desc = "If set to true for an item type, the item will have an extra line added to its tooltip indicating that it provides immunity to On Fire!";

		public static bool[] OnFireImmunityItem = ItemID.Sets.Factory.CreateNamedSet(OnFireImmunityItem_Key)
			.Description(OnFireImmunityItem_Desc)
			.RegisterBoolSet(false, ItemID.ObsidianSkull, ItemID.LavaSkull, ItemID.MoltenCharm, ItemID.ObsidianSkullRose, ItemID.MoltenSkullRose, ItemID.ObsidianHorseshoe, ItemID.ObsidianShield, ItemID.ObsidianWaterWalkingBoots, ItemID.LavaWaders, ItemID.TerrasparkBoots);
		#endregion

		#region NPCID.Sets
		const string DefensiveEnemyProperties_Key = "DefensiveEnemyProperties";
		const string DefensiveEnemyProperties_Desc = "Stores the properties of defensive enemies, like Granite Elemental and Granite Golem";

		public static Tuple<float, float, Func<NPC, bool>>[] DefensiveEnemyProperties = NPCID.Sets.Factory.CreateNamedSet(DefensiveEnemyProperties_Key)
			.Description(DefensiveEnemyProperties_Desc)
			.RegisterCustomSet<Tuple<float, float, Func<NPC, bool>>>(null,
			NPCID.GraniteFlyer, new Tuple<float, float, Func<NPC, bool>>(0.25f, 1.1f, (NPC npc) => npc.ai[0] == -1),
			NPCID.GraniteGolem, new Tuple<float, float, Func<NPC, bool>>(0.25f, 0.05f, (NPC npc) => npc.ai[2] < 0f)
			);

		const string RetalitoryEnemyProperties_Key = "RetalitoryEnemyProperties";
		const string RetalitoryEnemyProperties_Desc = "Stores the properties of enemies that have a revenge-damage state, like all jellyfish";

		public static Tuple<float, Func<NPC, bool>>[] RetalitoryEnemyProperties = NPCID.Sets.Factory.CreateNamedSet(RetalitoryEnemyProperties_Key)
			.Description(RetalitoryEnemyProperties_Desc)
			.RegisterCustomSet<Tuple<float, Func<NPC, bool>>>(null,
			NPCID.BlueJellyfish, new Tuple<float, Func<NPC, bool>>(2.0f, (NPC npc) => npc.wet && npc.ai[1] == 1f)
			);

		const string ProjectileAttacker_Key = "ProjectileAttacker";
		const string ProjectileAttacker_Desc = "If set to true for an NPC type, contact damage will be disabled with the corresponding config option";

		public static bool[] ProjectileAttacker = NPCID.Sets.Factory.CreateNamedSet(ProjectileAttacker_Key)
			.Description(ProjectileAttacker_Desc)
			.RegisterBoolSet(false,
			// Hornets
			NPCID.Hornet,
			NPCID.HornetFatty,
			NPCID.HornetHoney,
			NPCID.HornetLeafy,
			NPCID.HornetSpikey,
			NPCID.HornetStingy,
			NPCID.MossHornet,

			// Archers
			NPCID.CultistArcherBlue,
			NPCID.CultistArcherWhite,
			NPCID.ElfArcher,
			NPCID.GoblinArcher,
			NPCID.PirateCrossbower,
			NPCID.SkeletonArcher,

			// Gun users
			NPCID.PirateDeadeye,
			NPCID.RayGunner,
			NPCID.ScutlixRider,
			NPCID.SnowBalla,
			NPCID.SnowmanGangsta,
			NPCID.SkeletonCommando,
			NPCID.SkeletonSniper,
			NPCID.TacticalSkeleton,
			NPCID.VortexRifleman,

			// Salamanders
			NPCID.Salamander,
			NPCID.Salamander2,
			NPCID.Salamander3,
			NPCID.Salamander4,
			NPCID.Salamander5,
			NPCID.Salamander6,
			NPCID.Salamander7,
			NPCID.Salamander8,
			NPCID.Salamander9,

			// Misc.
			NPCID.AngryNimbus,
			NPCID.Eyezor,
			NPCID.Gastropod,
			NPCID.IcyMerman,
			NPCID.MartianTurret,
			NPCID.Probe
			);

		const string IgnoresRoomSize_Key = "IgnoresRoomSize";
		const string IgnoresRoomSize_Desc = "If set to true for an NPC type, they won't be affected by room size even when that config option's enabled";

		public static bool[] IgnoresRoomSize = NPCID.Sets.Factory.CreateNamedSet(IgnoresRoomSize_Key)
			.Description(IgnoresRoomSize_Desc)
			.RegisterBoolSet(false, NPCID.Princess);
		#endregion

		#region BuffID.Sets
		const string HotDebuff_Key = "HotDebuff";
		const string HotDebuff_Desc = "If set to true for a buff type, allows the debuff to interact with the Lucy rework's Werebeaver form";

		public static bool[] HotDebuff = BuffID.Sets.Factory.CreateNamedSet(HotDebuff_Key)
			.Description(HotDebuff_Desc)
			.RegisterBoolSet(false, BuffID.OnFire, BuffID.CursedInferno, BuffID.ShadowFlame, BuffID.Daybreak, BuffID.OnFire3);

		const string ColdDebuff_Key = "ColdDebuff";
		const string ColdDebuff_Desc = "If set to true for a buff type, allows the debuff to interact with the Lucy rework's Werebeaver form";

		public static bool[] ColdDebuff = BuffID.Sets.Factory.CreateNamedSet(ColdDebuff_Key)
			.Description(ColdDebuff_Desc)
			.RegisterBoolSet(false, BuffID.Frostburn, BuffID.Frozen, BuffID.Frostburn2);

		const string StationBuff_Key = "StationBuff";
		const string StationBuff_Desc = "If set to true for a buff type, makes the buff persist through death with the Persistent Station Buffs config option";

		public static bool[] StationBuff = BuffID.Sets.Factory.CreateNamedSet(StationBuff_Key)
			.Description(StationBuff_Desc)
			.RegisterBoolSet(false, BuffID.AmmoBox, BuffID.Bewitched, BuffID.Clairvoyance, BuffID.Sharpened, BuffID.WarTable, BuffID.SugarRush);
		#endregion
	}
}