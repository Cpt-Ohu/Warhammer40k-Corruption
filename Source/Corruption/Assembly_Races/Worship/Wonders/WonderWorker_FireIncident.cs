using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_FireIncident : WonderWorker_Targetable
    {
        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetLocations = true,
                canTargetPawns = true,
                validator = ((TargetInfo x) => true)
            };
        }

        protected override void TryDoEffectOnTarget(int worshipPoints)
        {
            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = this.target.Map;
            incidentParms.points = this.Def.ResolveWonderPoints(worshipPoints);
            this.Def.Incident.Worker.TryExecute(incidentParms);
        }
    }
}
