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
    public class JobDriver_UseCCC : JobDriver
    {
        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn(delegate (Toil to)
            {
                Building_CommsConsoleIG building_CommsConsole = (Building_CommsConsoleIG)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                return !building_CommsConsole.CanUseCommsNow;
            });
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn actor = GetActor();
                    Building_CommsConsoleIG building_CommsConsole = (Building_CommsConsoleIG)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                    if (building_CommsConsole.CanUseCommsNow)
                    {
                        Window_CCMBase window_subsector = new Window_CCMBase(CorruptionStoryTrackerUtilities.currentStoryTracker, actor);
                        window_subsector.soundAmbient = SoundDefOf.RadioComms_Ambience;
                        Find.WindowStack.Add(window_subsector);
                    }
                }
            };
            yield break;
        }
    }
}
