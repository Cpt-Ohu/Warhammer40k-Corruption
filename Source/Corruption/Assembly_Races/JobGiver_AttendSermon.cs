using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace Corruption
{
    public class JobGiver_AttendSermon : JobGiver_Wander
    {
        private Pawn preacher;

        public JobGiver_AttendSermon(Pawn preacher)
        {
            this.preacher = preacher;
        }

        protected override IntVec3 GetExactWanderDest(Pawn pawn)
        {
            IntVec3 result;
            if (!SermonUtility.TryFindRandomCellInSermonArea(preacher, pawn, out result))
            {
                return IntVec3.Invalid;
            }
            return result;
        }

        protected override IntVec3 GetWanderRoot(Pawn pawn)
        {
            throw new NotImplementedException();
        }
    }
}
