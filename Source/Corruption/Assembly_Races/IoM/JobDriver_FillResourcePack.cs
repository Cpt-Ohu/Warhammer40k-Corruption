using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class JobDriver_FillResourcePack : JobDriver_HaulToCell
    {
        private CompResourcePack resPack
        {
            get
            {
                return this.TargetA.Thing.TryGetComp<CompResourcePack>();
            }
        }

        private const TargetIndex ResourcePackInd = TargetIndex.C;
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.B);
            Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return reserveTargetA;
            Toil toilGoto = null;
            toilGoto = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return toilGoto;
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, true, false);

            yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(toilGoto, TargetIndex.B);
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);

            Toil placingToil = new Toil();
            placingToil.AddFinishAction(delegate
            {
                CompResourcePack resPack = TargetC.Thing.TryGetComp<CompResourcePack>();
                resPack.TryAddRessource(TargetA.Thing);
            });
            yield return placingToil;
            this.job.targetQueueA.Remove(this.TargetA);
            this.job.countQueue.Remove(job.countQueue[0]);
            if (!this.job.targetQueueA.NullOrEmpty()  && this.resPack.remainingCapacity > 0)
            {
                this.job.SetTarget(TargetIndex.A, this.job.targetQueueA[0]);
                this.job.count = Math.Min(this.job.countQueue[0], this.resPack.remainingCapacity);                
                this.JumpToToil(reserveTargetA);
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);          
            bool char1 = this.pawn.Reserve(this.job.GetTarget(TargetIndex.C), this.job, 1, -1, null);
            bool char2 = this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, 1, null);
            return char1 && char2;
        }

    }
}
