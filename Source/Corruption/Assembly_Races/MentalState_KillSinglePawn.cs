using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class MentalState_KillSinglePawn : MentalState
    {
        public Pawn Victim;

        protected bool HasChosenVictim = false;
        
        protected virtual Pawn ChooseVictim()
        {
            return this.pawn.Map.mapPawns.AllPawns.RandomElement();
        }
        
        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override void MentalStateTick()
        {
            base.MentalStateTick();
            if (this.Victim != null && this.Victim.Dead)
            {
                this.RecoverFromState();
            }
        }

        public override void PostStart(string reason)
        {
            if (!HasChosenVictim)
            {
                this.Victim = this.ChooseVictim();
                if (pawn.CanReserve(Victim, 1, 1, null, true))
                {
                    HasChosenVictim = true;
                    Job job = new Job(JobDefOf.Goto, Victim.Position);
                    Job job2 = new Job(JobDefOf.AttackStatic, Victim);
                    this.pawn.jobs.jobQueue.EnqueueLast(job);
                    this.pawn.jobs.jobQueue.EnqueueLast(job2);
                    this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    this.VictimReaction();
                }
            }
        }

        protected virtual void VictimReaction()
        {
            Victim.jobs.jobQueue.EnqueueLast(new Job(JobDefOf.FleeAndCower));
            Victim.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

    }
}
