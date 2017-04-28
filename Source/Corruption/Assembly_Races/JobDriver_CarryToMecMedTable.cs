using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_CarryToMecMedTable : JobDriver
    {
        private const TargetIndex TakeeInd = TargetIndex.A;

        private const TargetIndex DropPodInd = TargetIndex.B;

        protected Pawn Takee
        {
            get
            {
                return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Building_MechanicusMedTable DropPod
        {
            get
            {
                return (Building_MechanicusMedTable)base.CurJob.GetTarget(TargetIndex.B).Thing;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => this.DropPod.GetInnerContainer().Count > 0).FailOn(() => !this.Takee.Downed).FailOn(() => !this.pawn.CanReach(TargetA, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 500;
            toil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            yield return toil;
            yield return new Toil
            {
                initAction = delegate
                {
                    this.DropPod.TryAcceptThing(this.Takee, true);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
    }
}
