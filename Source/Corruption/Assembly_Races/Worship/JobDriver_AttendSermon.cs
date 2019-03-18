using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_AttendSermon : JobDriver
    {

//        private TargetIndex Preacher = TargetIndex.A;
//        private TargetIndex Altar = TargetIndex.B;
        private TargetIndex Spot = TargetIndex.C;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true; //this.pawn.Reserve(this.TargetC, this.job, this.job.def.joyMaxParticipants, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(Spot, JobCondition.Incompletable);
            Toil gotoPreacher;
            if (this.TargetC.HasThing)
            {
                gotoPreacher = Toils_Goto.GotoThing(Spot, PathEndMode.OnCell);
            }
            else
            {
                gotoPreacher = Toils_Goto.GotoCell(Spot, PathEndMode.OnCell);
            }
            yield return gotoPreacher;



            var altarToil = new Toil();
            altarToil.defaultCompleteMode = ToilCompleteMode.Delay;
            altarToil.defaultDuration = this.job.def.joyDuration;
            altarToil.AddPreTickAction(() =>
            {
                this.pawn.rotationTracker.FaceCell(this.TargetB.Cell);
                this.pawn.GainComfortFromCellIfPossible();
            });
            yield return altarToil;

            this.AddFinishAction(() =>
            {
                SermonUtility.AttendSermonTickCheckEnd(this.pawn, this.TargetA.Thing as Pawn, Worship.WorshipActType.None);
                //if (this.TargetC.HasThing)
                //{
                //    this.Map.reservationManager.Release(this.job.targetC.Thing, pawn, this.job);
                //}
                //else
                //{                    
                //    this.Map.reservationManager.Release(this.job.targetC.Cell, this.pawn, this.job);
                //}
                
                
            });
        }
    }
}
