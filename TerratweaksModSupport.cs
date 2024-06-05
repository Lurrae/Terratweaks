using CalamityMod;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Fishing;
using CalamityMod.Items.PermanentBoosters;
using EfficientNohits.Items;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Buffs;
using ThoriumMod.Items.BardItems;
using ThoriumMod.Items.Depths;

namespace Terratweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamityPermConsumables : ModSystem
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
		}
	}

	[JITWhenModsEnabled("ThoriumMod")]
	public class ThoriumPermConsumables : ModSystem
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
		}
	}

	[JITWhenModsEnabled("EfficientNohits")]
	public class NycroPermConsumables : ModSystem
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