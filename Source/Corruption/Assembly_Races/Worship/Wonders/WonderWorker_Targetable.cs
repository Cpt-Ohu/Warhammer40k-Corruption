using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_Targetable : WonderWorker
    {
        protected TargetInfo target;

        protected virtual TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,                
                validator = ((TargetInfo x) => BaseTargetValidator(x.Thing))
            };
        }

        protected void StartTargeting(int worshipPoints)
        {
            if (this.target == null)
            {
                return;
            }
            if (this.target != null && !this.GetTargetingParameters().CanTarget(this.target))
            {
                return;
            }
            TryDoEffectOnTarget(worshipPoints);
        }

        protected virtual void TryDoEffectOnTarget(int worshipPoints)
        {

        }

        public bool BaseTargetValidator(Thing t)
        {
            return true;
        }

        public override bool TryExecuteWonder(int worshipPoints)
        {
            Find.Targeter.BeginTargeting(this.GetTargetingParameters(), delegate (LocalTargetInfo t)
            {
                this.target = t.ToTargetInfo(Find.CurrentMap);
                this.StartTargeting(worshipPoints);
            }, null, null, ChaosGodsUtilities.GetPatronIcon(this.Def.DefaultGod));
            return true;
        }
    }
}
