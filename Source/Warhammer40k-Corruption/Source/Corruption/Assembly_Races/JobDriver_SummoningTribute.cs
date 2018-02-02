using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_SummoningTribute : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            throw new NotImplementedException();
        }
    }
}
