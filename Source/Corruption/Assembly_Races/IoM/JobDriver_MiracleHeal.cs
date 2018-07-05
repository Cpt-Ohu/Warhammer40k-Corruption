using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class JobDriver_MiracleHeal : JobDriver_CastPsykerPowerVerb
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil getInRangeToil = Toils_Goto.Goto(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return getInRangeToil;
            yield return Toils_Combat.CastVerb(TargetIndex.A, false);
            compPsyker.IsActive = true;
            this.AddFinishAction(() =>
            {
                if (compPsyker.IsActive)
                {
                    PsykerUtility.PsykerShockEvents(compPsyker);
                }
                compPsyker.ShotFired = true;
            });
        }
    }
}
