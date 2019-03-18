using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class TributeDemand : IExposable
    {
        public ThingDef ThingDef;

        public int RequestedAmount;

        public int SatisfiedAmount;

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.RequestedAmount, "RequestedAmount");
            Scribe_Values.Look<int>(ref this.SatisfiedAmount, "SatisfiedAmount");
            Scribe_Defs.Look<ThingDef>(ref this.ThingDef, "ThingDef");
        }
    }
}
