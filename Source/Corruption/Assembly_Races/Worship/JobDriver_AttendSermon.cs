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
            Scribe_Values.LookValue<TargetIndex>(ref this.Spot, "Spot", TargetIndex.C);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(Spot, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(Spot, this.CurJob.def.joyMaxParticipants);
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
            altarToil.defaultDuration = this.CurJob.def.joyDuration;
            altarToil.AddPreTickAction(() =>
            {
                this.pawn.Drawer.rotator.FaceCell(this.TargetB.Cell);
                this.pawn.GainComfortFromCellIfPossible();
            });
            yield return altarToil;

            this.AddFinishAction(() =>
            {
                SermonUtility.AttendSermonTickCheckEnd(this.pawn, this.TargetA.Thing as Pawn);
                if (this.TargetC.HasThing)
                {
                    this.Map.reservationManager.Release(this.CurJob.targetC.Thing, pawn);
                }
                else
                {                    
                    this.Map.reservationManager.Release(this.CurJob.targetC.Cell, this.pawn);
                }
                
                
            });
        }
    }
}
