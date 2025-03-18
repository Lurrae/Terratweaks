using CalamityMod;
using CalamityMod.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terratweaks.Calamitweaks
{
	[JITWhenModsEnabled("CalamityMod")]
	public class CalamitweaksProjs : GlobalProjectile
	{
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("CalamityMod");

		public override bool InstancePerEntity => true;
		public int OwnerEoL = -1;

		public override void SetDefaults(Projectile proj)
		{
			if (Terratweaks.Calamitweaks.NoDefenseDamage)
			{
				CalamityGlobalProjectile calProj = proj.Calamity();
				calProj.DealsDefenseDamage = false;
			}
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source)
		{
			if (Terratweaks.Calamitweaks.EnragedEoLInstakills)
			{
				if (source is EntitySource_Parent parentSource)
				{
					if (parentSource.Entity is NPC npc && npc.type == NPCID.HallowBoss)
					{
						OwnerEoL = npc.whoAmI;
					}
				}
			}
		}
	}
}