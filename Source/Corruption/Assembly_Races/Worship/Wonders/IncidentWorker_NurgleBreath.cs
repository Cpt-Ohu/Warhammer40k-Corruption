using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class IncidentWorker_NurgleBreath : IncidentWorker_Disease
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.ShuffleDiseaseDef();
            if( base.TryExecuteWorker(parms))
            {
                CorruptionStoryTrackerUtilities.currentStoryTracker.PlayerWorshipProgressLookup[PatronDefOf.Nurgle] += 1000;
                return true;
            }
            return false;
        }

        private void ShuffleDiseaseDef()
        {
            List<IncidentDef> diseases = DefDatabase<IncidentDef>.AllDefsListForReading.FindAll(x => x.category == IncidentCategory.Disease);
            this.def.diseaseIncident = diseases.RandomElement().diseaseIncident;
        }
    }
}
