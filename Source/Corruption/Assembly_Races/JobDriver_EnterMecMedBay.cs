using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_EnterMecMedBay : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 500;
            toil.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            yield return toil;
            yield return new Toil
            {
                initAction = delegate
                {
                    Pawn actor = this.GetActor();
                    Building_MechanicusMedTable pod = (Building_MechanicusMedTable)actor.CurJob.targetA.Thing;
                    Action action = delegate
                    {
                        actor.DeSpawn();
                        pod.TryAcceptThing(actor, true);
                    };
                    if (!pod.def.building.isPlayerEjectable)
                    {
                        int freeColonistsSpawnedOrInPlayerEjectablePodsCount = actor.Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount;
                        if (freeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                        {
                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate().AdjustedFor(actor), action, false, null));
                        }
                        else
                        {
                            action();
                        }
                    }
                    else
                    {
                        action();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
    }
}

