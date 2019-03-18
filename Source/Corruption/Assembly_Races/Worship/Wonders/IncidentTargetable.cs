using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class IncidentTargetable : IncidentWorker
    {
        private Thing target;

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Find.Targeter.BeginTargeting(this.GetTargetingParameters(), delegate (LocalTargetInfo t)
            {
                this.target = t.Thing;
                this.StartTargeting();
            }, null, null, null);
            return true;
        }

        protected virtual TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,                
                validator = ((TargetInfo x) => BaseTargetValidator(x.Thing))
            };
        }

        protected void StartTargeting()
        {
            if (this.target == null)
            {
                return;
            }
            if (this.target != null && !this.GetTargetingParameters().CanTarget(this.target))
            {
                return;
            }
            TryDoEffectOnTarget();
        }

        protected virtual void TryDoEffectOnTarget()
        {

        }

        public bool BaseTargetValidator(Thing t)
        {
            return true;
        }


    }
}
