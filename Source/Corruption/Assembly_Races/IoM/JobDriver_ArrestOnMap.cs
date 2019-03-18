
using OHUShips;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class JobDriver_ArrestOnMap : JobDriver
    {
        protected Pawn Takee
        {
            get
            {
                return (Pawn)base.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected ShipBase Transporter
        {
            get
            {
                return (ShipBase)base.job.GetTarget(TargetIndex.B).Thing;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
           // this.FailOnDestroyedOrNull(TargetIndex.B);
           
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
                yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
                Toil toil = new Toil();
                toil.defaultCompleteMode = ToilCompleteMode.Delay;
                toil.defaultDuration = 60;
                toil.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
                yield return toil;
                yield return new Toil
                {
                    initAction = delegate
                    {
                        this.Transporter.TryAcceptThing(this.Takee, true);
                    },
                    defaultCompleteMode = ToilCompleteMode.Instant
                };
                yield break;
            }
        
    }
}
