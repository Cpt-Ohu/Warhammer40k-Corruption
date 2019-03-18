using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class IncidentWorker_NurgleBreath : IncidentWorker_DiseaseHuman
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.ShuffleDiseaseDef();
            if( base.TryExecuteWorker(parms))
            {
                CFind.WorshipTracker.PlayerWorshipProgressLookup[PatronDefOf.Nurgle] += 1000;
                return true;
            }
            return false;
        }

        private void ShuffleDiseaseDef()
        {
            List<IncidentDef> diseases = DefDatabase<IncidentDef>.AllDefsListForReading.FindAll(x => x.category == IncidentCategoryDefOf.DiseaseHuman);
            this.def.diseaseIncident = diseases.RandomElement().diseaseIncident;
        }
    }
}
