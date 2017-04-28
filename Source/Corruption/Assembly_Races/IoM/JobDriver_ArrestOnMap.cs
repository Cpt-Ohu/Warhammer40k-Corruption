﻿using Corruption.Ships;
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
                return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected ShipBase Transporter
        {
            get
            {
                return (ShipBase)base.CurJob.GetTarget(TargetIndex.B).Thing;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
           // this.FailOnDestroyedOrNull(TargetIndex.B);

                yield return Toils_Reserve.Reserve(TargetIndex.A, 1);

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
