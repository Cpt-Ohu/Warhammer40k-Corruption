using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Worship
{
    public class JobGiver_HoldSermon : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Lord lord = pawn.GetLord();
            LordJob_Sermon lordJob = lord.LordJob as LordJob_Sermon;
            if (lordJob != null)
            {
                if (lordJob.Preacher == pawn)
                {
                    return new Job(C_JobDefOf.HoldSermon, lordJob.altar, lordJob.altar.InteractionCell);
                }
                else
                {
                    IntVec3 result;
                    Building chair;
                    if (!WatchBuildingUtility.TryFindBestWatchCell(lordJob.altar, pawn, true, out result, out chair))
                    {
                        if (!WatchBuildingUtility.TryFindBestWatchCell(lordJob.altar, pawn, false, out result, out chair))
                        {
                            Log.Error("No watch cell found");
                            return null;
                        }
                    }
                    if (chair != null)
                    {
                        Job attendJobChair = new Job(C_JobDefOf.AttendSermon, lordJob.Preacher, lordJob.altar, chair);
                        attendJobChair.locomotionUrgency = LocomotionUrgency.Jog;
                        return attendJobChair;
                    }
                    Job attendJob = new Job(C_JobDefOf.AttendSermon, lordJob.Preacher, lordJob.altar);
                    attendJob.locomotionUrgency = LocomotionUrgency.Jog;
                    return attendJob;
                }
            }
            return null;
        }
    }
}
