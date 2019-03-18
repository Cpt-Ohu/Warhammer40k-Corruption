using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.ProductionSites
{
    public class AgriProductionComp : ResourceProductionComp
    {
        private float curTemperature
        {
            get
            {
                return (Find.World.tileTemperatures.GetOutdoorTemp(this.parent.Tile));
            }
        }

        protected virtual bool IgnoreGrowingSeason
        {
            get
            {
                return false;
            }
        }

        private float TemperatureInfluence
        {
            get
            {
                return (float)((Math.Exp(0.035 * curTemperature) - 1) * (2.3 - Math.Exp(0.017 * curTemperature)));
            }
        }

        internal override void Production(bool ignoreRestrictions = false)
        {
            if (curTemperature > -2f || this.IgnoreGrowingSeason || ignoreRestrictions)
            {
                base.Production(ignoreRestrictions);
            }
        }

        protected override float ProductionSpeedModifier
        {
            get
            {
                return this.TemperatureInfluence;
            }
        }
    }
}
