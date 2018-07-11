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
                    if (SermonUtility.TryGetSermonWatchPosition(lordJob.altar, pawn, out result, out chair))
                    {
                        if (chair != null)
                        {
                            Job attendJobChair = new Job(C_JobDefOf.AttendSermon, lordJob.Preacher, lordJob.altar, chair);
                            attendJobChair.locomotionUrgency = LocomotionUrgency.Jog;
                            return attendJobChair;
                        }
                        Job attendJob = new Job(C_JobDefOf.AttendSermon, lordJob.Preacher, lordJob.altar, result);
                        attendJob.locomotionUrgency = LocomotionUrgency.Jog;
                        return attendJob;
                    }
                }
            }
            return null;
        }
    }
}
