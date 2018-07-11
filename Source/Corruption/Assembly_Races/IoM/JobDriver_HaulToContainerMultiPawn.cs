using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace Corruption.IoM
{
    public class JobDriver_HaulToContainerMultiPawn : JobDriver_HaulToContainer
    {
        public override bool TryMakePreToilReservations()
        {
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
            this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 5, 1, null);
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null) && this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 5, 1, null);
        }
    }
}
