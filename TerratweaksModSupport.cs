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
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
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
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModLoader.HasMod("CalamityMod");
		}

		public override void PostSetupContent()
		{
			// Max life consumables
			//Mod.Call("AddPermConsumable", ModContent.ItemType<BloodOrange>(), (Player p) => p.Calamity().bOrange);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<MiracleFruit>(), (Player p) => p.Calamity().mFruit);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<Elderberry>(), (Player p) => p.Calamity().eBerry);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<Dragonfruit>(), (Player p) => p.Calamity().dFruit);

			// Max mana consumables
			Mod.Call("AddPermConsumable", ModContent.ItemType<EnchantedStarfish>(), (Player p) => new Vector2(p.ConsumedManaCrystals, 9));
			//Mod.Call("AddPermConsumable", ModContent.ItemType<CometShard>(), (Player p) => p.Calamity().cShard);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<EtherealCore>(), (Player p) => p.Calamity().eCore);
			//Mod.Call("AddPermConsumable", ModContent.ItemType<PhantomHeart>(), (Player p) => p.Calamity().pHeart);

			// Rage/Adrenaline upgrades
			Mod.Call("AddPermConsumable", ModContent.ItemType<MushroomPlasmaRoot>(), (Player p) => p.Calamity().rageBoostOne);
			Mod.Call("AddPermConsumable", ModContent.ItemType<InfernalBlood>(), (Player p) => p.Calamity().rageBoostTwo);
			Mod.Call("AddPermConsumable", ModContent.ItemType<RedLightningContainer>(), (Player p) => p.Calamity().rageBoostThree);
			Mod.Call("AddPermConsumable", ModContent.ItemType<ElectrolyteGelPack>(), (Player p) => p.Calamity().adrenalineBoostOne);
			Mod.Call("AddPermConsumable", ModContent.ItemType<StarlightFuelCell>(), (Player p) => p.Calamity().adrenalineBoostTwo);
			Mod.Call("AddPermConsumable", ModContent.ItemType<Ectoheart>(), (Player p) => p.Calamity().adrenalineBoostThree);

			// Accessory slots
			Mod.Call("AddPermConsumable", ModContent.ItemType<CelestialOnion>(), (Player p) => p.Calamity().extraAccessoryML);

			// Hot debuffs
			Mod.Call("AddHotDebuff", ModContent.BuffType<BanishingFire>());
			Mod.Call("AddHotDebuff", ModContent.BuffType<BrimstoneFlames>());
			Mod.Call("AddHotDebuff", ModContent.BuffType<Dragonfire>());
			Mod.Call("AddHotDebuff", ModContent.BuffType<GodSlayerInferno>());
			Mod.Call("AddHotDebuff", ModContent.BuffType<HolyFlames>());
			Mod.Call("AddHotDebuff", ModContent.BuffType<VulnerabilityHex>());

			// Cold debuffs
			Mod.Call("AddColdDebuff", ModContent.BuffType<GlacialState>());
			Mod.Call("AddColdDebuff", ModContent.BuffType<Nightwither>());
			Mod.Call("AddColdDebuff", ModContent.BuffType<TemporalSadness>());

			// Shimmerable boss drops
			// Pre-Hardmode
			Mod.Call("AddShimmerableBossDrop", "DesertScourge", new List<int> { ModContent.ItemType<SaharaSlicers>(), ModContent.ItemType<Barinade>(), ModContent.ItemType<SandstreamScepter>(), ModContent.ItemType<BrittleStarStaff>(), ModContent.ItemType<ScourgeoftheDesert>() });
			Mod.Call("AddShimmerableBossDrop", "Crabulon", new List<int> { ModContent.ItemType<MycelialClaws>(), ModContent.ItemType<InfestedClawmerang>(), ModContent.ItemType<Fungicide>(), ModContent.ItemType<HyphaeRod>(), ModContent.ItemType<PuffShroom>(), ModContent.ItemType<Mycoroot>() });
			Mod.Call("AddShimmerableBossDrop", "HiveMind", new List<int> { ModContent.ItemType<PerfectDark>(), ModContent.ItemType<Shadethrower>(), ModContent.ItemType<ShaderainStaff>(), ModContent.ItemType<DankStaff>() }); // Rot Balls are excluded due to being consumable, and you can craft them anyways
			Mod.Call("AddShimmerableBossDrop", "Perforators", new List<int> { ModContent.ItemType<VeinBurster>(), ModContent.ItemType<SausageMaker>(), ModContent.ItemType<Aorta>(), ModContent.ItemType<Eviscerator>(), ModContent.ItemType<BloodBath>(), ModContent.ItemType<FleshOfInfidelity>() }); // Tooth Balls are excluded due to being consumable, and you can craft them anyways
			Mod.Call("AddShimmerableBossDrop", "SlimeGod", new List<int> { ModContent.ItemType<OverloadedBlaster>(), ModContent.ItemType<AbyssalTome>(), ModContent.ItemType<EldritchTome>(), ModContent.ItemType<CorroslimeStaff>(), ModContent.ItemType<CrimslimeStaff>() });
			
			// Hardmode
			Mod.Call("AddShimmerableBossDrop", "Cryogen", new List<int> { ModContent.ItemType<Avalanche>(), ModContent.ItemType<HoarfrostBow>(), ModContent.ItemType<SnowstormStaff>(), ModContent.ItemType<Icebreaker>() });
			Mod.Call("AddShimmerableBossDrop", "AquaticScourge", new List<int> { ModContent.ItemType<SubmarineShocker>(), ModContent.ItemType<Barinautical>(), ModContent.ItemType<Downpour>(), ModContent.ItemType<DeepseaStaff>(), ModContent.ItemType<ScourgeoftheSeas>() });
			Mod.Call("AddShimmerableBossDrop", "BrimstoneElemental", new List<int> { ModContent.ItemType<Brimlance>(), ModContent.ItemType<SeethingDischarge>(), ModContent.ItemType<DormantBrimseeker>() });
			Mod.Call("AddShimmerableBossDrop", "CalClone", new List<int> { ModContent.ItemType<Oblivion>(), ModContent.ItemType<Animosity>(), ModContent.ItemType<LashesofChaos>(), ModContent.ItemType<EntropysVigil>() });
			Mod.Call("AddShimmerableBossDrop", "Leviathan", new List<int> { ModContent.ItemType<Greentide>(), ModContent.ItemType<Leviatitan>(), ModContent.ItemType<Atlantis>(), ModContent.ItemType<AnahitasArpeggio>(), ModContent.ItemType<BrackishFlask>(), ModContent.ItemType<PearlofEnthrallment>() });
			Mod.Call("AddShimmerableBossDrop", "AstrumAureus", new List<int> { ModContent.ItemType<Nebulash>(), ModContent.ItemType<AuroraBlazer>(), ModContent.ItemType<AlulaAustralis>(), ModContent.ItemType<BorealisBomber>(), ModContent.ItemType<AuroradicalThrow>() });
			Mod.Call("AddShimmerableBossDrop", "PlaguebringerGoliath", new List<int> { ModContent.ItemType<Virulence>(), ModContent.ItemType<DiseasedPike>(), ModContent.ItemType<Pandemic>(), ModContent.ItemType<Malevolence>(), ModContent.ItemType<PestilentDefiler>(), ModContent.ItemType<TheHive>(), ModContent.ItemType<BlightSpewer>(), ModContent.ItemType<PlagueStaff>(), ModContent.ItemType<FuelCellBundle>(), ModContent.ItemType<InfectedRemote>(), ModContent.ItemType<TheSyringe>() });
			Mod.Call("AddShimmerableBossDrop", "Ravager", new List<int> { ModContent.ItemType<UltimusCleaver>(), ModContent.ItemType<RealmRavager>(), ModContent.ItemType<Hematemesis>(), ModContent.ItemType<SpikecragStaff>(), ModContent.ItemType<CraniumSmasher>() });
			Mod.Call("AddShimmerableBossDrop", "AstrumDeus", new List<int> { ModContent.ItemType<TheMicrowave>(), ModContent.ItemType<StarSputter>(), ModContent.ItemType<StarShower>(), ModContent.ItemType<StarspawnHelixStaff>(), ModContent.ItemType<RegulusRiot>() });

			// Post-Moonlord
			Mod.Call("AddShimmerableBossDrop", "Dragonfolly", new List<int> { ModContent.ItemType<GildedProboscis>(), ModContent.ItemType<GoldenEagle>(), ModContent.ItemType<RougeSlash>() });
			Mod.Call("AddShimmerableBossDrop", "Providence", new List<int> { ModContent.ItemType<HolyCollider>(), ModContent.ItemType<BurningRevelation>(), ModContent.ItemType<TelluricGlare>(), ModContent.ItemType<BlissfulBombardier>(), ModContent.ItemType<PurgeGuzzler>(), ModContent.ItemType<DazzlingStabberStaff>(), ModContent.ItemType<MoltenAmputator>() });
			Mod.Call("AddShimmerableBossDrop", "StormWeaver", new List<int> { ModContent.ItemType<TheStorm>(), ModContent.ItemType<StormDragoon>() });
			Mod.Call("AddShimmerableBossDrop", "CeaselessVoid", new List<int> { ModContent.ItemType<MirrorBlade>(), ModContent.ItemType<VoidConcentrationStaff>() });
			Mod.Call("AddShimmerableBossDrop", "AncientGodSlayerSet", new List<int> { ModContent.ItemType<AncientGodSlayerHelm>(), ModContent.ItemType<AncientGodSlayerChestplate>(), ModContent.ItemType<AncientGodSlayerLeggings>() });
			Mod.Call("AddShimmerableBossDrop", "Signus", new List<int> { ModContent.ItemType<Cosmilamp>(), ModContent.ItemType<CosmicKunai>() });
			Mod.Call("AddShimmerableBossDrop", "Polterghast", new List<int> { ModContent.ItemType<TerrorBlade>(), ModContent.ItemType<BansheeHook>(), ModContent.ItemType<DaemonsFlame>(), ModContent.ItemType<FatesReveal>(), ModContent.ItemType<GhastlyVisage>(), ModContent.ItemType<EtherealSubjugator>(), ModContent.ItemType<GhoulishGouger>() });
			Mod.Call("AddShimmerableBossDrop", "OldDuke", new List<int> { ModContent.ItemType<InsidiousImpaler>(), ModContent.ItemType<FetidEmesis>(), ModContent.ItemType<SepticSkewer>(), ModContent.ItemType<VitriolicViper>(), ModContent.ItemType<CadaverousCarrion>(), ModContent.ItemType<MutatedTruffle>(), ModContent.ItemType<ToxicantTwister>() });
			Mod.Call("AddShimmerableBossDrop", "DevourerofGods", new List<int> { ModContent.ItemType<Excelsus>(), ModContent.ItemType<TheObliterator>(), ModContent.ItemType<Deathwind>(), ModContent.ItemType<DeathhailStaff>(), ModContent.ItemType<StaffoftheMechworm>(), ModContent.ItemType<Eradicator>() });
			Mod.Call("AddShimmerableBossDrop", "Yharon", new List<int> { ModContent.ItemType<TheBurningSky>(), ModContent.ItemType<DragonRage>(), ModContent.ItemType<DragonsBreath>(), ModContent.ItemType<ChickenCannon>(), ModContent.ItemType<PhoenixFlameBarrage>(), ModContent.ItemType<YharonsKindleStaff>(), ModContent.ItemType<Wrathwing>(), ModContent.ItemType<TheFinalDawn>() });
			// Exo Mechs are skipped because the weapons are dropped at a 100% chance based on which mech was killed last, rather than a random chance to get each one
			Mod.Call("AddShimmerableBossDrop", "SCal", new List<int> { ModContent.ItemType<Violence>(), ModContent.ItemType<Condemnation>(), ModContent.ItemType<Vehemence>(), ModContent.ItemType<Heresy>(), ModContent.ItemType<Perdition>(), ModContent.ItemType<Vigilance>(), ModContent.ItemType<Sacrifice>() });
		}
	}

	[JITWhenModsEnabled("ThoriumMod")]
	public class ThoriumModSupport : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModLoader.HasMod("ThoriumMod");
		}

		public override void PostSetupContent()
		{
			// Max inspiration consumables
			Mod.Call("AddPermConsumable", ModContent.ItemType<InspirationFragment>(), (Player p) => new Vector2(Math.Clamp(p.GetModPlayer<ThoriumPlayer>().bardResourceMax - 10, 0, 10), 10));
			Mod.Call("AddPermConsumable", ModContent.ItemType<InspirationShard>(), (Player p) => new Vector2(Math.Clamp(p.GetModPlayer<ThoriumPlayer>().bardResourceMax - 20, 0, 10), 10));
			Mod.Call("AddPermConsumable", ModContent.ItemType<InspirationCrystalNew>(), (Player p) => new Vector2(Math.Clamp(p.GetModPlayer<ThoriumPlayer>().bardResourceMax - 30, 0, 10), 10));
			Mod.Call("AddPermConsumable", ModContent.ItemType<InspirationGem>(), (Player p) => p.GetModPlayer<ThoriumPlayer>().consumedInspirationGem);

			// Move speed upgrades
			Mod.Call("AddPermConsumable", ModContent.ItemType<CrystalWave>(), (Player p) => new Vector2(p.GetModPlayer<ThoriumPlayer>().consumedCrystalWaveCount, 5));
			Mod.Call("AddPermConsumable", ModContent.ItemType<AstralWave>(), (Player p) => p.GetModPlayer<ThoriumPlayer>().consumedAstralWave);

			// Hot debuffs
			Mod.Call("AddHotDebuff", ModContent.BuffType<IncandescentSparkDebuff>());
			Mod.Call("AddHotDebuff", ModContent.BuffType<NapalmDebuff>());
			Mod.Call("AddHotDebuff", ModContent.BuffType<SchmelzeDebuff>());
			Mod.Call("AddHotDebuff", ModContent.BuffType<Singed>());

			// Cold debuffs
			Mod.Call("AddColdDebuff", ModContent.BuffType<Freezing>());

			// Shimmerable boss drops
			// Pre-Hardmode
			Mod.Call("AddShimmerableBossDrop", "GrandThunderbird", new List<int> { ModContent.ItemType<ThunderTalon>(), ModContent.ItemType<TalonBurst>(), ModContent.ItemType<StormHatchlingStaff>(), ModContent.ItemType<Didgeridoo>() });
			Mod.Call("AddShimmerableBossDrop", "QueenJellyfish", new List<int> { ModContent.ItemType<GiantGlowstick>(), ModContent.ItemType<SparkingJellyBall>(), ModContent.ItemType<BuccaneerBlunderBuss>(), ModContent.ItemType<JellyPondWand>(), ModContent.ItemType<ConchShell>() });
			Mod.Call("AddShimmerableBossDrop", "Viscount", new List<int> { ModContent.ItemType<BatWing>(), ModContent.ItemType<GuanoGunner>(), ModContent.ItemType<VampireScepter>(), ModContent.ItemType<ViscountCane>(), ModContent.ItemType<BatScythe>(), ModContent.ItemType<SonarCannon>() }); // Dracula Fangs are excluded due to being consumable
			Mod.Call("AddShimmerableBossDrop", "DarkMage", new List<int> { ModContent.ItemType<DarkMageStaff>(), ModContent.ItemType<DarkTome>(), ModContent.ItemType<TabooWand>() }); // Arcane Anelaces are excluded due to being consumable
			Mod.Call("AddShimmerableBossDrop", "GraniteEnergyStorm", new List<int> { ModContent.ItemType<EnergyStormPartisan>(), ModContent.ItemType<EnergyStormBolter>(), ModContent.ItemType<EnergyProjector>(), ModContent.ItemType<BoulderProbeStaff>(), ModContent.ItemType<ShockAbsorber>() });
			Mod.Call("AddShimmerableBossDrop", "BuriedChampion", new List<int> { ModContent.ItemType<ChampionSwiftBlade>(), ModContent.ItemType<ChampionsTrifectaShot>(), ModContent.ItemType<ChampionBomberStaff>(), ModContent.ItemType<ChampionsGodHand>(), ModContent.ItemType<ChampionsRebuttal>() });
			Mod.Call("AddShimmerableBossDrop", "StarScouter", new List<int> { ModContent.ItemType<StarTrail>(), ModContent.ItemType<HitScanner>(), ModContent.ItemType<ParticleWhip>(), ModContent.ItemType<DistressCaller>(), ModContent.ItemType<StarRod>(), ModContent.ItemType<Roboboe>(), ModContent.ItemType<GaussFlinger>() });

			// Hardmode
			Mod.Call("AddShimmerableBossDrop", "FlyingDutchman", new List<int> { ModContent.ItemType<TheJuggernaut>(), ModContent.ItemType<ShipsHelm>(), ModContent.ItemType<HandCannon>(), ModContent.ItemType<DutchmansAvarice>(), ModContent.ItemType<TwentyFourCaratTuba>() });
			Mod.Call("AddShimmerableBossDrop", "BoreanStrider", new List<int> { ModContent.ItemType<Glacier>(), ModContent.ItemType<GlacialSting>(), ModContent.ItemType<FreezeRay>(), ModContent.ItemType<BoreanFangStaff>(), ModContent.ItemType<TheCryoFang>() });
			Mod.Call("AddShimmerableBossDrop", "FallenBeholder", new List<int> { ModContent.ItemType<HellishHalberd>(), ModContent.ItemType<Obliterator>(), ModContent.ItemType<PyroclastStaff>(), ModContent.ItemType<BeholderStaff>(), ModContent.ItemType<HellRoller>(), ModContent.ItemType<BeholderGaze>() });
			Mod.Call("AddShimmerableBossDrop", "Ogre", new List<int> { ModContent.ItemType<OgreSnotGun>(), ModContent.ItemType<OgreSandal>() });
			Mod.Call("AddShimmerableBossDrop", "Lich", new List<int> { ModContent.ItemType<WitherStaff>(), ModContent.ItemType<SoulRender>(), ModContent.ItemType<CadaverCornet>(), ModContent.ItemType<SoulCleaver>() }); // Soul Bombs are excluded due to being consumable
			Mod.Call("AddShimmerableBossDrop", "ForgottenOne", new List<int> { ModContent.ItemType<MantisShrimpPunch>(), ModContent.ItemType<TrenchSpitter>(), ModContent.ItemType<OldGodsVision>(), ModContent.ItemType<TheIncubator>(), ModContent.ItemType<SirensLyre>() }); // Whispering armor is excluded since normally the full set drops at once, and there's no way to replicate that with Shimmer
			Mod.Call("AddShimmerableBossDrop", "MartianSaucer", new List<int> { ModContent.ItemType<LivewireCrasher>(), ModContent.ItemType<MolecularStabilizer>(), ModContent.ItemType<Turntable>(), ModContent.ItemType<TheTriangle>() });
			Mod.Call("AddShimmerableBossDrop", "LunaticCultist", new List<int> { ModContent.ItemType<AncientFlame>(), ModContent.ItemType<AncientFrost>(), ModContent.ItemType<AncientSpark>() });
		}
	}

	[JITWhenModsEnabled("EfficientNohits")]
	public class NycroModSupport : ModSystem
	{
		public override bool IsLoadingEnabled(Mod mod)
		{
			return ModLoader.HasMod("EfficientNohits");
		}

		public override void PostSetupContent()
		{
			Mod.Call("AddPermConsumable", ModContent.ItemType<MaxLifeCrystal>(), (Player p) => p.ConsumedLifeCrystals == 15);
			Mod.Call("AddPermConsumable", ModContent.ItemType<MaxManaCrystal>(), (Player p) => p.ConsumedManaCrystals == 9);
			Mod.Call("AddPermConsumable", ModContent.ItemType<MaxLifeFruit>(), (Player p) => p.ConsumedLifeFruit == 20);
		}
	}
}