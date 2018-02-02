using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_Pray : JobDriver
    {
        private TargetIndex AltarIndex = TargetIndex.A;
        private TargetIndex OccupyChairIndex = TargetIndex.B;

        public override void ExposeData()
        {
            base.ExposeData();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.EndOnDespawnedOrNull(AltarIndex, JobCondition.Incompletable);
            yield return Toils_Reserve.Reserve(AltarIndex, this.CurJob.def.joyMaxParticipants);
            yield return Toils_Reserve.Reserve(OccupyChairIndex, 1);
            Toil gotoAltarToil;
            if (this.TargetB.HasThing)
            {
                gotoAltarToil = Toils_Goto.GotoThing(OccupyChairIndex, PathEndMode.OnCell);
            }
            else
            {
                gotoAltarToil = Toils_Goto.GotoCell(OccupyChairIndex, PathEndMode.OnCell);
            }         
            yield return gotoAltarToil;
            var altarToil = new Toil();
            altarToil.defaultCompleteMode = ToilCompleteMode.Delay;
            altarToil.defaultDuration = this.CurJob.def.joyDuration;
            altarToil.AddPreTickAction(() =>
            {
                this.pawn.Drawer.rotator.FaceCell(this.TargetA.Cell);
                this.pawn.GainComfortFromCellIfPossible();
                CorruptionUtilities.PrayerTickCheckEnd(this.pawn);
            });
            yield return altarToil;
            this.AddFinishAction(() =>
            {
                if (this.TargetB.HasThing)
                {
                    Find.Reservations.Release(this.CurJob.targetB.Thing, pawn);
                }
                else
                {
                    Find.Reservations.Release(this.CurJob.targetB.Cell, this.pawn);
                }
            });
        }
    }
}
