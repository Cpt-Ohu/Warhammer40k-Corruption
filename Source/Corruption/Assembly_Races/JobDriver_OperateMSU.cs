using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_OperateMSU : JobDriver
    {
        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to)
            {
                Building_MechanicusMedTable building_CommsConsole = (Building_MechanicusMedTable)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                CompPowerTrader power = building_CommsConsole.GetComp<CompPowerTrader>();

                return !power.PowerOn;
            });
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn actor = this.GetActor();
                    Building_CommsConsole building_CommsConsole = (Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                    if (building_CommsConsole.CanUseCommsNow)
                    {
                        actor.jobs.curJob.commTarget.TryOpenComms(actor);
                    }
                }
            };
            yield break;
        }
    }
}
