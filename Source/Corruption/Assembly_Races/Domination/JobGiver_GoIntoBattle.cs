using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse;

namespace Corruption.Domination
{
    public class JobGiver_GoIntoBattle : ThinkNode_JobGiver
    {
        private LocomotionUrgency locomotionUrgency = LocomotionUrgency.Jog;
        private Danger maxDanger = Danger.Deadly;
        private int jobMaxDuration = 99999;
        private IntRange WaitTicks = new IntRange(15, 40);

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GoIntoBattle jobGiver_GotoTravelDestination = (JobGiver_GoIntoBattle)base.DeepCopy(resolve);
            jobGiver_GotoTravelDestination.locomotionUrgency = this.locomotionUrgency;
            jobGiver_GotoTravelDestination.maxDanger = this.maxDanger;
            return jobGiver_GotoTravelDestination;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            IntVec3 cell = pawn.mindState.duty.focus.Cell;
            IntVec3 actualTravelDest = CellFinder.RandomClosewalkCellNear(cell, pawn.Map, 20, null);
            if (!pawn.CanReach(cell, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(pawn, this.maxDanger), false, TraverseMode.ByPawn))
            {
                Log.Message("Cannot reach");
                return null;
            }
            if (pawn.Position == cell)
            {
                Log.Message("Arrived");
                return null;
            }
            
            return new Job(JobDefOf.Goto, cell)
            {
                locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, this.locomotionUrgency),
                expiryInterval = this.jobMaxDuration
            };
        }

    }
}
