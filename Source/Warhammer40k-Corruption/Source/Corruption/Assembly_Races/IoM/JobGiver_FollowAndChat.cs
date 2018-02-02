using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;

namespace Corruption.IoM
{
    public class JobGiver_FollowAndChat : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Pawn colonist = pawn.Map.mapPawns.FreeColonistsSpawned.Where((Pawn p) => p.Awake()).RandomElement();
            if(!IoM_StoryUtilities.PawnInPrivateQuarters(colonist) && Find.TickManager.TicksGame % 2500 < 180)
            {
                 return this.FollowJob(colonist);                
            }
            return null;
        }

        protected virtual Job FollowJob(Pawn colonist)
        {
            Job job = new Job(DefOfs.C_JobDefOf.FollowAndChat, colonist, 1200);
            return job;
        }
    }

    public class JobGiver_FollowAndCorrupt : JobGiver_FollowAndChat
    {
        protected override Job FollowJob(Pawn colonist)
        {
            Job job = new Job(DefOfs.C_JobDefOf.FollowAndCorrupt, colonist, 1200);
            return job;
        }
    }
    public class JobGiver_FollowAndPraise : JobGiver_FollowAndChat
    {
        protected override Job FollowJob(Pawn colonist)
        {
            Job job = new Job(DefOfs.C_JobDefOf.FollowAndPraise, colonist, 1200);
            return job;
        }
    }
    public class JobGiver_FollowAndConvertTau : JobGiver_FollowAndChat
    {
        protected override Job FollowJob(Pawn colonist)
        {
            Job job = new Job(DefOfs.C_JobDefOf.FollowAndConvertTau, colonist, 1200);
            return job;
        }
    }
    public class JobGiver_FollowAndInvestigate : JobGiver_FollowAndChat
    {
        protected override Job FollowJob(Pawn colonist)
        {
            Job job = new Job(DefOfs.C_JobDefOf.FollowAndInvestigate, colonist, 1200);
            return job;
        }
    }
    
}
