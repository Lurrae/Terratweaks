using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Vanity;
using CalamityMod.Items.Fishing;
using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using EfficientNohits.Items;
using FargowiltasSouls.Content.Buffs;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Content.Buffs.Souls;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs;
using ThoriumMod.Items.BardItems;
using ThoriumMod.Items.BossBoreanStrider;
using ThoriumMod.Items.BossBuriedChampion;
using ThoriumMod.Items.BossFallenBeholder;
using ThoriumMod.Items.BossForgottenOne;
using ThoriumMod.Items.BossGraniteEnergyStorm;
using ThoriumMod.Items.BossLich;
using ThoriumMod.Items.BossMini;
using ThoriumMod.Items.BossQueenJellyfish;
using ThoriumMod.Items.BossStarScouter;
using ThoriumMod.Items.BossTheGrandThunderBird;
using ThoriumMod.Items.BossViscount;
using ThoriumMod.Items.Cultist;
using ThoriumMod.Items.DD;
using ThoriumMod.Items.Depths;
using ThoriumMod.Items.HealerItems;
using ThoriumMod.Items.NPCItems;

namespace Terratweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamityModSupport : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override void SetStaticDefaults()
		{
			// Shimmerable boss drops
			// Pre-Hardmode
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<SaharaSlicers>()] = new List<int> { ModContent.ItemType<SaharaSlicers>(), ModContent.ItemType<Barinade>(), ModContent.ItemType<SandstreamScepter>(), ModContent.ItemType<BrittleStarStaff>(), ModContent.ItemType<ScourgeoftheDesert>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<MycelialClaws>()] = new List<int> { ModContent.ItemType<MycelialClaws>(), ModContent.ItemType<InfestedClawmerang>(), ModContent.ItemType<Fungicide>(), ModContent.ItemType<HyphaeRod>(), ModContent.ItemType<PuffShroom>(), ModContent.ItemType<Mycoroot>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<PerfectDark>()] = new List<int> { ModContent.ItemType<PerfectDark>(), ModContent.ItemType<Shadethrower>(), ModContent.ItemType<ShaderainStaff>(), ModContent.ItemType<DankStaff>() }; // Rot Balls are excluded due to being consumable, and you can craft them anyways
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<VeinBurster>()] = new List<int> { ModContent.ItemType<VeinBurster>(), ModContent.ItemType<SausageMaker>(), ModContent.ItemType<Aorta>(), ModContent.ItemType<Eviscerator>(), ModContent.ItemType<BloodBath>(), ModContent.ItemType<FleshOfInfidelity>() }; // Tooth Balls are excluded due to being consumable, and you can craft them anyways
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<OverloadedBlaster>()] = new List<int> { ModContent.ItemType<OverloadedBlaster>(), ModContent.ItemType<AbyssalTome>(), ModContent.ItemType<EldritchTome>(), ModContent.ItemType<CorroslimeStaff>(), ModContent.ItemType<CrimslimeStaff>() };

			// Hardmode
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Avalanche>()] = new List<int> { ModContent.ItemType<Avalanche>(), ModContent.ItemType<HoarfrostBow>(), ModContent.ItemType<SnowstormStaff>(), ModContent.ItemType<Icebreaker>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<SubmarineShocker>()] = new List<int> { ModContent.ItemType<SubmarineShocker>(), ModContent.ItemType<Barinautical>(), ModContent.ItemType<Downpour>(), ModContent.ItemType<DeepseaStaff>(), ModContent.ItemType<ScourgeoftheSeas>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Brimlance>()] = new List<int> { ModContent.ItemType<Brimlance>(), ModContent.ItemType<SeethingDischarge>(), ModContent.ItemType<DormantBrimseeker>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Oblivion>()] = new List<int> { ModContent.ItemType<Oblivion>(), ModContent.ItemType<Animosity>(), ModContent.ItemType<LashesofChaos>(), ModContent.ItemType<EntropysVigil>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Greentide>()] = new List<int> { ModContent.ItemType<Greentide>(), ModContent.ItemType<Leviatitan>(), ModContent.ItemType<Atlantis>(), ModContent.ItemType<AnahitasArpeggio>(), ModContent.ItemType<BrackishFlask>(), ModContent.ItemType<PearlofEnthrallment>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Nebulash>()] = new List<int> { ModContent.ItemType<Nebulash>(), ModContent.ItemType<AuroraBlazer>(), ModContent.ItemType<AlulaAustralis>(), ModContent.ItemType<BorealisBomber>(), ModContent.ItemType<AuroradicalThrow>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Virulence>()] = new List<int> { ModContent.ItemType<Virulence>(), ModContent.ItemType<DiseasedPike>(), ModContent.ItemType<Pandemic>(), ModContent.ItemType<Malevolence>(), ModContent.ItemType<PestilentDefiler>(), ModContent.ItemType<TheHive>(), ModContent.ItemType<BlightSpewer>(), ModContent.ItemType<PlagueStaff>(), ModContent.ItemType<FuelCellBundle>(), ModContent.ItemType<InfectedRemote>(), ModContent.ItemType<TheSyringe>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<UltimusCleaver>()] = new List<int> { ModContent.ItemType<UltimusCleaver>(), ModContent.ItemType<RealmRavager>(), ModContent.ItemType<Hematemesis>(), ModContent.ItemType<SpikecragStaff>(), ModContent.ItemType<CraniumSmasher>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<TheMicrowave>()] = new List<int> { ModContent.ItemType<TheMicrowave>(), ModContent.ItemType<StarSputter>(), ModContent.ItemType<StarShower>(), ModContent.ItemType<StarspawnHelixStaff>(), ModContent.ItemType<RegulusRiot>() };

			// Post-Moonlord
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<GildedProboscis>()] = new List<int> { ModContent.ItemType<GildedProboscis>(), ModContent.ItemType<GoldenEagle>(), ModContent.ItemType<RougeSlash>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<HolyCollider>()] = new List<int> { ModContent.ItemType<HolyCollider>(), ModContent.ItemType<BurningRevelation>(), ModContent.ItemType<TelluricGlare>(), ModContent.ItemType<BlissfulBombardier>(), ModContent.ItemType<PurgeGuzzler>(), ModContent.ItemType<DazzlingStabberStaff>(), ModContent.ItemType<MoltenAmputator>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<TheStorm>()] = new List<int> { ModContent.ItemType<TheStorm>(), ModContent.ItemType<StormDragoon>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<MirrorBlade>()] = new List<int> { ModContent.ItemType<MirrorBlade>(), ModContent.ItemType<VoidConcentrationStaff>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<AncientGodSlayerHelm>()] = new List<int> { ModContent.ItemType<AncientGodSlayerHelm>(), ModContent.ItemType<AncientGodSlayerChestplate>(), ModContent.ItemType<AncientGodSlayerLeggings>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Cosmilamp>()] = new List<int> { ModContent.ItemType<Cosmilamp>(), ModContent.ItemType<CosmicKunai>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<TerrorBlade>()] = new List<int> { ModContent.ItemType<TerrorBlade>(), ModContent.ItemType<BansheeHook>(), ModContent.ItemType<DaemonsFlame>(), ModContent.ItemType<FatesReveal>(), ModContent.ItemType<GhastlyVisage>(), ModContent.ItemType<EtherealSubjugator>(), ModContent.ItemType<GhoulishGouger>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<InsidiousImpaler>()] = new List<int> { ModContent.ItemType<InsidiousImpaler>(), ModContent.ItemType<FetidEmesis>(), ModContent.ItemType<SepticSkewer>(), ModContent.ItemType<VitriolicViper>(), ModContent.ItemType<CadaverousCarrion>(), ModContent.ItemType<MutatedTruffle>(), ModContent.ItemType<ToxicantTwister>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Excelsus>()] = new List<int> { ModContent.ItemType<Excelsus>(), ModContent.ItemType<TheObliterator>(), ModContent.ItemType<Deathwind>(), ModContent.ItemType<DeathhailStaff>(), ModContent.ItemType<StaffoftheMechworm>(), ModContent.ItemType<Eradicator>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<TheBurningSky>()] = new List<int> { ModContent.ItemType<TheBurningSky>(), ModContent.ItemType<DragonRage>(), ModContent.ItemType<DragonsBreath>(), ModContent.ItemType<ChickenCannon>(), ModContent.ItemType<PhoenixFlameBarrage>(), ModContent.ItemType<YharonsKindleStaff>(), ModContent.ItemType<Wrathwing>(), ModContent.ItemType<TheFinalDawn>() };
			// Exo Mechs are skipped because the weapons are dropped at a 100% chance based on which mech was killed last, rather than a random chance to get each one
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Violence>()] = new List<int> { ModContent.ItemType<Violence>(), ModContent.ItemType<Condemnation>(), ModContent.ItemType<Vehemence>(), ModContent.ItemType<Heresy>(), ModContent.ItemType<Perdition>(), ModContent.ItemType<Vigilance>(), ModContent.ItemType<Sacrifice>() };

			// Max life consumables
			//Mod.Call("AddPermConsumable", ModContent.ItemType<BloodOrange>(), (Player p) => p.Calamity().bOrange);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<MiracleFruit>(), (Player p) => p.Calamity().mFruit);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<Elderberry>(), (Player p) => p.Calamity().eBerry);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<Dragonfruit>(), (Player p) => p.Calamity().dFruit);

			// Max mana consumables
			TerratweaksContentSets.MultiUsePermBuffs[ModContent.ItemType<EnchantedStarfish>()] = (Player p) => new Vector2(p.ConsumedManaCrystals, 9);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<CometShard>(), (Player p) => p.Calamity().cShard);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<EtherealCore>(), (Player p) => p.Calamity().eCore);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<PhantomHeart>(), (Player p) => p.Calamity().pHeart);

			// Rage/Adrenaline upgrades
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<MushroomPlasmaRoot>()] = (Player p) => p.Calamity().rageBoostOne;
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<InfernalBlood>()] = (Player p) => p.Calamity().rageBoostTwo;
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<RedLightningContainer>()] = (Player p) => p.Calamity().rageBoostThree;
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<ElectrolyteGelPack>()] = (Player p) => p.Calamity().adrenalineBoostOne;
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<StarlightFuelCell>()] = (Player p) => p.Calamity().adrenalineBoostTwo;
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<Ectoheart>()] = (Player p) => p.Calamity().adrenalineBoostThree;

			// Accessory slots
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<CelestialOnion>()] = (Player p) => p.Calamity().extraAccessoryML;

			// Hot debuffs
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<BanishingFire>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<BrimstoneFlames>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<Dragonfire>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<GodSlayerInferno>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<HolyFlames>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<VulnerabilityHex>()] = true;

			// Cold debuffs
			TerratweaksContentSets.ColdDebuff[ModContent.BuffType<GlacialState>()] = true;
			TerratweaksContentSets.ColdDebuff[ModContent.BuffType<Nightwither>()] = true;
			TerratweaksContentSets.ColdDebuff[ModContent.BuffType<TemporalSadness>()] = true;
		}

		public override void PostSetupContent()
		{
			// Town NPC weapon drops
			Mod.Call("AddSellableWeapon", ModContent.ItemType<ClothiersWrath>(), NPCID.Clothier, new List<Condition>() { Condition.Hardmode });

			// Custom biome conditions
			Mod.Call("AddBiomeConditions", new List<Condition> { CalamityConditions.InCrag, CalamityConditions.InAstral, CalamityConditions.InSulph, CalamityConditions.InSunken });
		}
	}

	[JITWhenModsEnabled("ThoriumMod")]
	public class ThoriumModSupport : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("ThoriumMod");

		public override void SetStaticDefaults()
		{
			// Shimmerable boss drops
			// Pre-Hardmode
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<ThunderTalon>()] = new List<int> { ModContent.ItemType<ThunderTalon>(), ModContent.ItemType<TalonBurst>(), ModContent.ItemType<StormHatchlingStaff>(), ModContent.ItemType<Didgeridoo>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<GiantGlowstick>()] = new List<int> { ModContent.ItemType<GiantGlowstick>(), ModContent.ItemType<SparkingJellyBall>(), ModContent.ItemType<BuccaneerBlunderBuss>(), ModContent.ItemType<JellyPondWand>(), ModContent.ItemType<ConchShell>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<BatWing>()] = new List<int> { ModContent.ItemType<BatWing>(), ModContent.ItemType<GuanoGunner>(), ModContent.ItemType<VampireScepter>(), ModContent.ItemType<ViscountCane>(), ModContent.ItemType<BatScythe>(), ModContent.ItemType<SonarCannon>() }; // Dracula Fangs are excluded due to being consumable
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<DarkMageStaff>()] = new List<int> { ModContent.ItemType<DarkMageStaff>(), ModContent.ItemType<DarkTome>(), ModContent.ItemType<TabooWand>() }; // Arcane Anelaces are excluded due to being consumable
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<EnergyStormPartisan>()] = new List<int> { ModContent.ItemType<EnergyStormPartisan>(), ModContent.ItemType<EnergyStormBolter>(), ModContent.ItemType<EnergyProjector>(), ModContent.ItemType<BoulderProbeStaff>(), ModContent.ItemType<ShockAbsorber>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<ChampionSwiftBlade>()] = new List<int> { ModContent.ItemType<ChampionSwiftBlade>(), ModContent.ItemType<ChampionsTrifectaShot>(), ModContent.ItemType<ChampionBomberStaff>(), ModContent.ItemType<ChampionsGodHand>(), ModContent.ItemType<ChampionsRebuttal>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<StarTrail>()] = new List<int> { ModContent.ItemType<StarTrail>(), ModContent.ItemType<HitScanner>(), ModContent.ItemType<ParticleWhip>(), ModContent.ItemType<DistressCaller>(), ModContent.ItemType<StarRod>(), ModContent.ItemType<Roboboe>(), ModContent.ItemType<GaussFlinger>() };

			// Hardmode
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<TheJuggernaut>()] = new List<int> { ModContent.ItemType<TheJuggernaut>(), ModContent.ItemType<ShipsHelm>(), ModContent.ItemType<HandCannon>(), ModContent.ItemType<DutchmansAvarice>(), ModContent.ItemType<TwentyFourCaratTuba>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<Glacier>()] = new List<int> { ModContent.ItemType<Glacier>(), ModContent.ItemType<GlacialSting>(), ModContent.ItemType<FreezeRay>(), ModContent.ItemType<BoreanFangStaff>(), ModContent.ItemType<TheCryoFang>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<HellishHalberd>()] = new List<int> { ModContent.ItemType<HellishHalberd>(), ModContent.ItemType<Obliterator>(), ModContent.ItemType<PyroclastStaff>(), ModContent.ItemType<BeholderStaff>(), ModContent.ItemType<HellRoller>(), ModContent.ItemType<BeholderGaze>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<OgreSnotGun>()] = new List<int> { ModContent.ItemType<OgreSnotGun>(), ModContent.ItemType<OgreSandal>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<WitherStaff>()] = new List<int> { ModContent.ItemType<WitherStaff>(), ModContent.ItemType<SoulRender>(), ModContent.ItemType<CadaverCornet>(), ModContent.ItemType<SoulCleaver>() }; // Soul Bombs are excluded due to being consumable
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<MantisShrimpPunch>()] = new List<int> { ModContent.ItemType<MantisShrimpPunch>(), ModContent.ItemType<TrenchSpitter>(), ModContent.ItemType<OldGodsVision>(), ModContent.ItemType<TheIncubator>(), ModContent.ItemType<SirensLyre>() }; // Whispering armor is excluded since normally the full set drops at once, and there's no way to replicate that with Shimmer
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<LivewireCrasher>()] = new List<int> { ModContent.ItemType<LivewireCrasher>(), ModContent.ItemType<MolecularStabilizer>(), ModContent.ItemType<Turntable>(), ModContent.ItemType<TheTriangle>() };
			TerratweaksContentSets.ShimmerableBossDrops[ModContent.ItemType<AncientFlame>()] = new List<int> { ModContent.ItemType<AncientFlame>(), ModContent.ItemType<AncientFrost>(), ModContent.ItemType<AncientSpark>() };

			// Max inspiration consumables
			TerratweaksContentSets.MultiUsePermBuffs[ModContent.ItemType<InspirationFragment>()] = (Player p) => new Vector2(Math.Clamp(p.GetModPlayer<ThoriumPlayer>().bardResourceMax - 10, 0, 10), 10);
			TerratweaksContentSets.MultiUsePermBuffs[ModContent.ItemType<InspirationShard>()] = (Player p) => new Vector2(Math.Clamp(p.GetModPlayer<ThoriumPlayer>().bardResourceMax - 20, 0, 10), 10);
			TerratweaksContentSets.MultiUsePermBuffs[ModContent.ItemType<InspirationCrystalNew>()] = (Player p) => new Vector2(Math.Clamp(p.GetModPlayer<ThoriumPlayer>().bardResourceMax - 30, 0, 10), 10);
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<InspirationGem>()] = (Player p) => p.GetModPlayer<ThoriumPlayer>().consumedInspirationGem;

			// Move speed upgrades
			TerratweaksContentSets.MultiUsePermBuffs[ModContent.ItemType<CrystalWave>()] = (Player p) => new Vector2(p.GetModPlayer<ThoriumPlayer>().consumedCrystalWaveCount, 5);
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<AstralWave>()] = (Player p) => p.GetModPlayer<ThoriumPlayer>().consumedAstralWave;

			// Hot debuffs
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<IncandescentSparkDebuff>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<NapalmDebuff>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<SchmelzeDebuff>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<Singed>()] = true;

			// Cold debuffs
			TerratweaksContentSets.ColdDebuff[ModContent.BuffType<Freezing>()] = true;
		}

		public override void PostSetupContent()
		{
			// Custom biome conditions
			Mod.Call("AddBiomeCondition", ThoriumConditions.NotInBeachSnowDesert);
		}
	}

	[JITWhenModsEnabled("EfficientNohits")]
	public class NycroModSupport : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("EfficientNohits");

		public override void SetStaticDefaults()
		{
			// Permanent consumables
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<MaxLifeCrystal>()] = (Player p) => p.ConsumedLifeCrystals == 15;
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<MaxManaCrystal>()] = (Player p) => p.ConsumedManaCrystals == 9;
			TerratweaksContentSets.SingleUsePermBuffs[ModContent.ItemType<MaxLifeFruit>()] = (Player p) => p.ConsumedLifeFruit == 20;
		}
	}

	[JITWhenModsEnabled("FargowiltasSouls")]
	public class FargoSoulsSupport : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("FargowiltasSouls");

		public override void SetStaticDefaults()
		{
			// Hot debuffs
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<FlamesoftheUniverseBuff>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<ShadowflameBuff>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<TwinsInstallBuff>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<HellFireBuff>()] = true;
			TerratweaksContentSets.HotDebuff[ModContent.BuffType<SolarFlareBuff>()] = true;

			// Cold debuffs
			TerratweaksContentSets.ColdDebuff[ModContent.BuffType<HypothermiaBuff>()] = true;
		}
	}
}