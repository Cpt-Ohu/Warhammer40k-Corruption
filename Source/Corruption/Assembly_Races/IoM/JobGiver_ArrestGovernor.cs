using Corruption.DefOfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.IoM
{
    public class JobGiver_ArrestGovernor : ThinkNode_JobGiver
    {
        private Pawn Governor
        {
            get
            {
                Pawn pawn = CFind.StoryTracker.PlanetaryGovernor;
                if (pawn != null)
                {
                    return pawn;
                }
                else
                {
                    return null;
                }
            }
        }

        private Lord lordArrest
        {
            get
            {
                if (Governor != null && Governor.Map != null)
                {
                    return Governor.Map.lordManager.lords.FirstOrDefault(x => x.LordJob.GetType() == typeof(LordJob_ArrestGovernor));
                }
                return null;
            }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (this.Governor != null && this.lordArrest != null && !pawn.Map.reservationManager.IsReservedByAnyoneOf(this.Governor, pawn.Faction))
            {
                LordJob_ArrestGovernor lordJob = (LordJob_ArrestGovernor)lordArrest.LordJob;
                Job job = new Job(C_JobDefOf.ArrestGovernor, this.Governor, lordJob.ship);
                job.count = 1;
                return job;
            }
            return null;
        }
        
    }
}
