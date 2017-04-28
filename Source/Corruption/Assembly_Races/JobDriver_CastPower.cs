using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_CastPsykerPower : JobDriver
    {
        private CompPsyker compPsyker
        {
            get
            {
                return this.pawn.TryGetComp<CompPsyker>();
            }
        }

        private bool startedIncapacitated;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref this.startedIncapacitated, "startedIncapacitated", false, false);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
            yield return Toils_Combat.CastVerb(TargetIndex.A, true);
            

            this.AddFinishAction(() =>
            {
                compPsyker.IsActive = false;
                compPsyker.ShotFired = true;
            });
            yield break;
        }
    }
}
