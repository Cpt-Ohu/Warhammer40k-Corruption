using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class JobGiver_AICastPsykerPower : ThinkNode_JobGiver
    {
        public PsykerPowerDef PowerDefToCast;

        public AIPsykerPowerCategory AiCategory = AIPsykerPowerCategory.DamageDealer;

        public Thing Target;

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (CorruptionStoryTrackerUtilities.GetRandOffensivePsykerPower(pawn, out this.PowerDefToCast, out this.AiCategory))
            {
                this.ResolveTarget(pawn, this.PowerDefToCast.MainVerb.range, out this.Target, this.AiCategory);
                return this.CastingJob(pawn);
            }
            return null;
        }


        protected virtual void ResolveTarget(Pawn pawn, float range, out Thing target, AIPsykerPowerCategory aiCategory = AIPsykerPowerCategory.DamageDealer)
        {
            CorruptionStoryTrackerUtilities.AIGetPsykerTarget(pawn, aiCategory, range, out target);
        }

        protected virtual Job CastingJob(Pawn pawn)
        {
            if (this.Target != null)
            {
                return CorruptionStoryTrackerUtilities.AI_CastPsykerPowerJob(pawn, PowerDefToCast, this.Target);
            }
            return null;         
        }

    }
}
