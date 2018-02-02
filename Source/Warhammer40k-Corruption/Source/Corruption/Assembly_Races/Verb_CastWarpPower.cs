using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using AlienRace;
using Verse.Sound;
using System.Reflection;

namespace Corruption
{
    public class Verb_CastWarpPower : Verb_LaunchProjectile
    {
        public VerbProperties_WarpPower warpverbprops
        {
            get
            {
                return (VerbProperties_WarpPower)this.verbProps;
            }
        }

        public ProjectileDef_WarpPower warpProjectileDef
        {
            get
            {
                return this.warpverbprops.defaultProjectile as ProjectileDef_WarpPower;
            }
        } 

        public List<LocalTargetInfo> TargetsAoE = new List<LocalTargetInfo>();

        public Need_Soul soul
        {
            get
            {
                return this.CasterPawn.needs.TryGetNeed<Need_Soul>();
            }
            set
            {

            }
        }
        
        
        public CompPsyker psycomp
        {
            get
            {
                return this.CasterPawn.TryGetComp<CompPsyker>();
            }
        }

        private void UpdateTargets()
        {
            this.TargetsAoE.Clear();
            if (this.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
            {
                if (warpverbprops.AoETargetClass == null)
                {
                    Log.Error("Tried to Cast AoE-Psyker Power without defining a target class");
                }

                List<Thing> targets = new List<Thing>();
                if (warpProjectileDef != null && warpProjectileDef.IsHealer)
                {
                    this.TargetsAoE.Add(new LocalTargetInfo(this.currentTarget.Cell));
                    targets = this.caster.Map.listerThings.AllThings.Where(x => (x.Position.InHorDistOf(caster.Position, this.warpverbprops.range)) && (x.GetType() == this.warpverbprops.AoETargetClass) && !x.Faction.HostileTo(Faction.OfPlayer)).ToList<Thing>();
                }
                else if ((this.warpverbprops.AoETargetClass == typeof(Plant)) || (this.warpverbprops.AoETargetClass == typeof(Building)))
                {
                    targets.Clear();
                    targets = this.caster.Map.listerThings.AllThings.Where(x => (x.Position.InHorDistOf(caster.Position, this.warpverbprops.range)) && (x.GetType() == this.warpverbprops.AoETargetClass)).ToList<Thing>();
                    foreach (Thing targ in targets)
                    {
                        LocalTargetInfo tinfo = new LocalTargetInfo(targ);
                        TargetsAoE.Add(tinfo);                        
                    }
                    return;
                }
                else
                {
                    targets.Clear();
                    targets = this.caster.Map.listerThings.AllThings.Where(x => (x.Position.InHorDistOf(caster.Position, this.warpverbprops.range)) && (x.GetType() == this.warpverbprops.AoETargetClass) && x.Faction.HostileTo(Faction.OfPlayer)).ToList<Thing>();
                }
                
                foreach (Thing targ in targets)
                {
                    TargetInfo tinfo = new TargetInfo(targ);
                    if (this.warpverbprops.targetParams.CanTarget(tinfo))
                    {
                        TargetsAoE.Add(new LocalTargetInfo(targ));
                    }
                }
            }
            else
            {
                this.TargetsAoE.Clear();
                this.TargetsAoE.Add(this.currentTarget);
            }
        }

        protected override bool TryCastShot()
        {
            this.TargetsAoE.Clear();
            UpdateTargets();
            int burstshots = this.ShotsPerBurst;
            if (this.warpverbprops.PsykerPowerCategory != PsykerPowerTargetCategory.TargetAoE && this.TargetsAoE.Count > 1)
            {
                this.TargetsAoE.RemoveRange(0, TargetsAoE.Count - 1);
            }
            for (int i = 0; i < TargetsAoE.Count; i++)
            {
                for (int j = 0; j < burstshots; j++)
                {
                    ShootLine shootLine;
                    bool flag = base.TryFindShootLineFromTo(this.caster.Position, TargetsAoE[i], out shootLine);
                    if (this.verbProps.stopBurstWithoutLos && !flag)
                    {
                        return false;
                    }
                    Vector3 drawPos = this.caster.DrawPos;
                    Projectile projectile = (Projectile)GenSpawn.Spawn(this.verbProps.defaultProjectile, shootLine.Source, this.caster.Map);
                    projectile.FreeIntercept = (this.canFreeInterceptNow && !projectile.def.projectile.flyOverhead);
                    ShotReport shotReport = ShotReport.HitReportFor(this.caster, this, TargetsAoE[i]);
                    if (!this.warpverbprops.AlwaysHits)
                    {
                        if (Rand.Value > shotReport.ChanceToNotGoWild_IgnoringPosture)
                        {
                            if (DebugViewSettings.drawShooting)
                            {
                                MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToWild", -1f);
                            }
                            shootLine.ChangeDestToMissWild();
                            if (TargetsAoE[i].HasThing)
                            {
                                projectile.ThingToNeverIntercept = TargetsAoE[i].Thing;
                            }
                            if (!projectile.def.projectile.flyOverhead)
                            {
                                projectile.InterceptWalls = true;
                            }
                            projectile.Launch(this.caster, drawPos, shootLine.Dest, this.ownerEquipment);
                            return true;
                        }
                        if (Rand.Value > shotReport.ChanceToNotHitCover)
                        {                            
                            if (DebugViewSettings.drawShooting)
                            {
                                MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToCover", -1f);
                            }
                            if (TargetsAoE[i].Thing != null && TargetsAoE[i].Thing.def.category == ThingCategory.Pawn)
                            {
                                Thing randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
                                if (!projectile.def.projectile.flyOverhead)
                                {
                                    projectile.InterceptWalls = true;
                                }
                                projectile.Launch(this.caster, drawPos, randomCoverToMissInto, this.ownerEquipment);
                                return true;
                            }
                        }
                    }
                    if (DebugViewSettings.drawShooting)
                    {
                        MoteMaker.ThrowText(this.caster.DrawPos, this.caster.Map, "ToHit", -1f);
                    }
                    if (!projectile.def.projectile.flyOverhead)
                    {
                        projectile.InterceptWalls = (!TargetsAoE[i].HasThing || TargetsAoE[i].Thing.def.Fillage == FillCategory.Full);
                    }
                    if (TargetsAoE[i].Thing != null)
                    {

                        if (this.warpverbprops.DrawProjectileOnTarget)
                        {
                            Projectile_WarpPower wprojectile = projectile as Projectile_WarpPower;
                            if (wprojectile != null)
                            {
                                wprojectile.selectedTarget = TargetsAoE[i].Thing;
                                wprojectile.Caster = this.CasterPawn;
                                wprojectile.Launch(this.caster, drawPos, TargetsAoE[i]);
                            }
                        }
                        else
                        {
                            projectile.Launch(this.caster, drawPos, TargetsAoE[i]);
                        }
                    }
                    else
                    {
                        if (this.warpverbprops.DrawProjectileOnTarget)
                        {
                            Projectile_WarpPower wprojectile = projectile as Projectile_WarpPower;
                            wprojectile.targetVec = shootLine.Dest.ToVector3();
                            wprojectile.Launch(this.caster, drawPos, TargetsAoE[i]);
                        }
                        projectile.Launch(this.caster, drawPos, shootLine.Dest);
                    }

                }

                psycomp.TicksToCast = this.warpverbprops.TicksToRecharge;
                psycomp.TicksToCastMax = this.warpverbprops.TicksToRecharge;
            }
            this.burstShotsLeft = 0;
            if (soul != null)
            {
                soul.GainNeed(0.01f * (-warpverbprops.CorruptionFactor));
            }
        //    PsykerUtility.PsykerShockEvents(psycomp, psycomp.curPower.PowerLevel);
            return true;
        }

        protected override int ShotsPerBurst
        {
            get
            {
                return this.verbProps.burstShotCount;
            }
        }
        
        public override void WarmupComplete()
        {
            this.burstShotsLeft = this.ShotsPerBurst;
            this.state = VerbState.Bursting;
            this.TryCastNextBurstShot();
        }
    }
}
