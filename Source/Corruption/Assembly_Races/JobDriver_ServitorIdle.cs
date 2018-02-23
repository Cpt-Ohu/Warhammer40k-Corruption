using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobGiver_ServitorIdle : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            return new Job(JobDefOf.WaitWander)
            {
                expiryInterval = 600
            };
        }
    }
}
