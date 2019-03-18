using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Corruption
{
    public class Projectile_WarpPower : Projectile
    {
        public Pawn Caster;

        public Thing selectedTarget;

        public int TicksToImpact
        {
            get
            {
               return  this.ticksToImpact;
            }
        }

        public Vector3 targetVec;

        public Vector3 ProjectileDrawPos
        {
            get
            {
                if (selectedTarget != null)
                {
                    return selectedTarget.DrawPos;
                }
                else if (targetVec != null)
                {
                    return targetVec;
                }
                return this.ExactPosition;
            }
        }

        public ProjectileDef_WarpPower mpdef
        {
            get
            {
               return (ProjectileDef_WarpPower)def;
            }
        }

        public override void Draw()
        {
            if (selectedTarget != null || targetVec != null)
            {
                Vector3 vector = this.ProjectileDrawPos;
                Vector3 distance = this.destination - this.origin;
                Vector3 curpos = this.destination - this.Position.ToVector3();
   //             var num = 1 - (Mathf.Sqrt(Mathf.Pow(curpos.x, 2) + Mathf.Pow(curpos.z, 2)) / (Mathf.Sqrt(Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.z, 2))));
                float angle = 0f;
                Material mat = this.Graphic.MatSingle;
                Vector3 s = new Vector3(2.5f, 1f, 2.5f);
                Matrix4x4 matrix = default(Matrix4x4);
                vector.y = 3;
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.plane10, this.DrawPos, this.ExactRotation, this.def.DrawMatSingle, 0);
            }
            base.Comps_PostDraw();
        }

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {

                Pawn victim = hitThing as Pawn;
                if (victim != null)
                {
                    if (mpdef.IsMentalStateGiver)
                    {
                        string str = "MentalStateByPsyker".Translate(new object[]
                         {
                            victim.Name,
                         });
                        if (mpdef.InducesMentalState == MentalStateDefOf.Berserk && victim.RaceProps.intelligence < Intelligence.Humanlike)
                        {
                            if (CanOverpowerMind(this.Caster, victim))
                            {
                                victim.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, str, true);
                            }
                        }
                        else
                        {
                            if (CanOverpowerMind(this.Caster, victim))
                            {
                                victim.mindState.mentalStateHandler.TryStartMentalState(mpdef.InducesMentalState, str, true);
                            }
                        }
                    }
                    else if (mpdef.IsBuffGiver)
                    {
                        CompSoul soul = CompSoul.GetPawnSoul(victim);
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.Stun, 1f, 1f, 0, this.Caster);
                        if (soul != null)
                        {
                            if (soul.PsykerPowerLevel != PsykerPowerLevel.Omega)
                            {
                                if (mpdef.BuffDef.isBad)
                                {
                                    if (CanOverpowerMind(this.Caster, victim))
                                    {
                                        victim.health.AddHediff(mpdef.BuffDef, null, dinfo);
                                    }
                                }
                                else
                                {
                                    victim.health.AddHediff(mpdef.BuffDef, null, dinfo);
                                }
                            }
                        }
                        else
                        {
                            victim.health.AddHediff(mpdef.BuffDef);
                        }
                    }
                    else if (mpdef.IsHealer)
                    {
                        List<Hediff> list = victim.health.hediffSet.hediffs.Where(x => x.def != HediffDefOf.PsychicShock && x.def != C_HediffDefOf.DemonicPossession).ToList<Hediff>();
                        if (Rand.Range(0f, 1f) > this.mpdef.HealFailChance && victim.health.hediffSet.hediffs.Count > 0)
                        {
                            for (int i = 0; i < mpdef.HealCapacity + 1; i++)
                            {
                                Hediff hediff = list.RandomElement();
                                hediff.Heal(this.def.projectile.GetDamageAmount(1f));
                            }
                        }
                    }
                    else
                    {
                        int damageAmountBase = this.def.projectile.GetDamageAmount(1f);
                        ThingDef equipmentDef = this.equipmentDef;
                        DamageInfo dinfo = new DamageInfo(this.def.projectile.damageDef, damageAmountBase, base.ArmorPenetration, this.ExactRotation.eulerAngles.y, this.launcher, null, equipmentDef);
                        hitThing.TakeDamage(dinfo);
                    }
                }
            }
            else
            {
                //SoundDefOf..PlayOneShotOnCamera();
            }            
        }

        public bool CanOverpowerMind(Pawn caster, Thing hitThing)
        {
            if (hitThing is Pawn)
            {
                Pawn target = hitThing as Pawn;
                PsykerPowerLevel casterPower = CompSoul.GetPawnSoul(caster).PsykerPowerLevel;
                PsykerPowerLevel targetPower = CompSoul.GetPawnSoul(target).PsykerPowerLevel;
                if (casterPower >= targetPower && targetPower != PsykerPowerLevel.Omega || target.Faction == Faction.OfPlayer)
                {
                    if (!target.Faction.HostileTo(Faction.OfPlayer) && target.Faction != Faction.OfPlayer)
                    {
                        target.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -10);
                    }
                    return true;
                }
                else
                {
                    if (!target.Faction.HostileTo(Faction.OfPlayer))
                    {
                        target.Faction.TryAffectGoodwillWith(Faction.OfPlayer, -30);
                    }
                    Caster.health.AddHediff(HediffDefOf.PsychicShock);
                    return false;
                }
            }
            return false;
        }
    }
}
