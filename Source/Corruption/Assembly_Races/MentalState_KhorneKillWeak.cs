using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class MentalState_KhorneKillWeak : MentalState
    {
        private bool HasChosenVictim = false;

        private Pawn Victim;

        private List<Pawn> prisoners = new List<Pawn>();

        public override void MentalStateTick()
        {
            base.MentalStateTick();

            if (Victim != null && Victim.Dead)
            {
                this.RecoverFromState();
            }
        }

        public override void PostStart(string reason)
        {
            prisoners = this.pawn.Map.mapPawns.PrisonersOfColonySpawned;
            if (!HasChosenVictim && prisoners != null)
            {
              //  Log.Message("Getting Victim");
                prisoners.TryRandomElement(out Victim);
                if (pawn.CanReserve(Victim, 1))
                {
                    HasChosenVictim = true;
         //           Log.Message("Chosen Victim :" + Victim.ToString());
                    Job job = new Job(JobDefOf.Goto, Victim.Position);
                    Job job2 = new Job(JobDefOf.AttackStatic, Victim);
                    this.pawn.QueueJob(job);
                    this.pawn.QueueJob(job2);
                    this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    Victim.QueueJob(new Job(JobDefOf.FleeAndCower));
                    Victim.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
        }

        public override RandomSocialMode SocialModeMax()
        {
            return RandomSocialMode.Off;
        }

        public override bool ForceHostileTo(Thing t)
        {
            Pawn prisoner = t as Pawn;

            if (prisoner != null && prisoner.IsPrisoner)
            {
                return true;
            }
            return false;
        }

    }
}
