using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_HoldSermon : JobDriver
    {
        private TargetIndex AltarIndex = TargetIndex.A;
        private TargetIndex AltarInteractionCell = TargetIndex.B;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<TargetIndex>(ref this.AltarIndex, "AltarIndex", TargetIndex.A);
            Scribe_Values.Look<TargetIndex>(ref this.AltarInteractionCell, "AltarInteractionCell", TargetIndex.B);
            Scribe_References.Look<Pawn>(ref this.pawn, "pawn", false);
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(AltarIndex, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(AltarIndex, 1);
            yield return Toils_Reserve.Reserve(AltarInteractionCell, 1);
            Toil gotoAltarToil;
                gotoAltarToil = Toils_Goto.GotoThing(AltarInteractionCell, PathEndMode.OnCell);

            yield return gotoAltarToil;

            //     Log.Message("A");
            //       if (this.CurJob == null) Log.Message("NOJob?");
            
            var altarToil = new Toil();
            altarToil.defaultCompleteMode = ToilCompleteMode.Delay;
            altarToil.defaultDuration = this.CurJob.def.joyDuration;
            altarToil.AddPreTickAction(() =>
            {
      //          if (this.pawn == null) Log.Message("No Pawn??");
      //          if (this.TargetA == null) Log.Message("NoTargetA");
                this.pawn.Drawer.rotator.FaceCell(this.TargetA.Cell);
                this.pawn.GainComfortFromCellIfPossible();
                ThrowPreacherMote(this.pawn);
            });
            yield return altarToil;

  //          Log.Message("B");
     //       if (this.pawn.jobs.curDriver == null) Log.Message("NoDriver");
  //          if (this.pawn.jobs.curJob == null) Log.Message("NoJob");
            this.AddFinishAction(() =>
            {
                if (this.TargetA.HasThing)
                {
                    this.Map.reservationManager.Release(this.CurJob.targetA.Thing, pawn);
                }
                else
                {
                    this.Map.reservationManager.Release(this.CurJob.targetA.Cell, this.pawn);
                }
                BuildingAltar altar = this.TargetA.Thing as BuildingAltar;
                altar.CalledInFlock = false;
                SermonUtility.HoldSermonTickCheckEnd(this.pawn, altar);

    //            Log.Message("C");
                
            });
        }

        protected void ThrowPreacherMote(Pawn pawn)
        {
         //   Log.Message("M1");
            MoteBubble moteBubble2 = (MoteBubble)ThingMaker.MakeThing(ThingDefOf.Mote_Speech, null);
            moteBubble2.SetupMoteBubble(ChaosGodsUtilities.TryGetPreacherIcon(pawn), pawn);
            moteBubble2.Attach(pawn);
            GenSpawn.Spawn(moteBubble2, pawn.Position, pawn.Map);
        }
    }
}
