using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class MentalState_PurgeHeretic : MentalState_KillSinglePawn
    {
        protected override Pawn ChooseVictim()
        {
            IEnumerable<Pawn> colonists = this.pawn.Map.mapPawns.AllPawnsSpawned.Where(x => !x.NonHumanlikeOrWildMan() && CompSoul.GetPawnSoul(x).Corrupted);
            //if (colonists.Count() > 0)
            //{
                return colonists.RandomElement();
            //}
        }

        protected override void VictimReaction()
        {
            TraverseMode mode = TraverseMode.PassAllDestroyableThings;
            IntVec3 spot;
            RCellFinder.TryFindBestExitSpot(pawn, out spot, mode);
            Victim.jobs.jobQueue.EnqueueLast(new Job(JobDefOf.Flee, spot));
            Victim.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

    }
}
