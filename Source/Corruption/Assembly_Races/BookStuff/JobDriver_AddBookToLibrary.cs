using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.BookStuff
{
    public class JobDriver_AddBookToLibrary : JobDriver_HaulToCell
    {
        private const TargetIndex Book = TargetIndex.A;

        private const TargetIndex Library = TargetIndex.B;

  //      private ReadableBooks bookint;

 //       private Bookshelf bookshelf;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.B);
            if (!base.TargetThingA.IsForbidden(this.pawn))
            {
                this.FailOnForbidden(TargetIndex.A);
            }
            yield return Toils_Reserve.Reserve(TargetIndex.B, 2);
            Toil toil = Toils_Reserve.Reserve(TargetIndex.A, 1);
            toil.AddFinishAction(delegate
            {
            });
            yield return toil;
            Toil toil2 = null;
            toil2 = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            toil2.AddFinishAction(delegate
            {
            });
            yield return toil2;
            this.pawn.CurJob.count = 1;
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            Toil toil3 = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
            toil3.AddFinishAction(delegate
            {
            });
            yield return toil3;
            Toil toil4 = Bookshelf.PlaceBookInShelf(TargetIndex.A, TargetIndex.B, this.pawn);
            yield return toil4;
            yield break;


            

        }


    }
}
