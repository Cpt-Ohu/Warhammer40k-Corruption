using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_CastPsykerPowerSelf : JobDriver
    {
        private CompPsyker compPsyker
        {
            get
            {
                return this.pawn.TryGetComp<CompPsyker>();
            }
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Combat.CastVerb(TargetIndex.A, true);
            compPsyker.IsActive = true;

            this.AddFinishAction(() =>
            {
                if (compPsyker.IsActive)
                {
                    PsykerUtility.PsykerShockEvents(compPsyker);
                }
                compPsyker.IsActive = false;
                compPsyker.ShotFired = true;
            });
        }
    }
}
