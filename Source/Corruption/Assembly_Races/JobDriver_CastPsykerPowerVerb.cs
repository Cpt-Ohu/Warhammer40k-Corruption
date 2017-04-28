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
        private CompPsyker compPsyker
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

        protected override IEnumerable<Toil> MakeNewToils()
        {
            
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            Toil getInRangeToil = Toils_Combat.GotoCastPosition(TargetIndex.A, false);
            yield return getInRangeToil;
            Verb_CastWarpPower verb = pawn.CurJob.verbToUse as Verb_CastWarpPower;

            Find.Targeter.targetingVerb = verb;
            yield return Toils_Combat.CastVerb(TargetIndex.A, false);
            compPsyker.IsActive = true;
            this.AddFinishAction(() =>
            {
             //   Log.Message("FinishACtion");
                if (compPsyker.IsActive)
                {
                    PsykerUtility.PsykerShockEvents(compPsyker, compPsyker.curPower.PowerLevel);
                }
                compPsyker.ShotFired = true;
            });
        }
    }
}
