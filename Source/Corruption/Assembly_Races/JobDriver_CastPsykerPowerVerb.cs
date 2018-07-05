using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_CastPsykerPowerVerb : JobDriver
    {
        public CompPsyker compPsyker
        {
            get
            {
                return this.pawn.TryGetComp<CompPsyker>();
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {            
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            Toil getInRangeToil = Toils_Combat.GotoCastPosition(TargetIndex.A, false);
            yield return getInRangeToil;
            Verb_CastWarpPower verb = pawn.CurJob.verbToUse as Verb_CastWarpPower;
            Toil castToil = Toils_Combat.CastVerb(TargetIndex.A, false);
             
            yield return castToil;
            compPsyker.IsActive = true;
            this.AddFinishAction(() =>
            {
             //   Log.Message("FinishACtion");
                if (compPsyker.IsActive)
                {
                    PsykerUtility.PsykerShockEvents(compPsyker);
                }
                compPsyker.ShotFired = true;
                compPsyker.IsActive = false;
            });
        }
    }
}
