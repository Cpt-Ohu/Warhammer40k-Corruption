using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompProperties_StartWithHediff : CompProperties
    {
        public List<HediffDef> StartsWithHediffs;
        
        public CompProperties_StartWithHediff()
        {
            this.compClass = typeof(CompStartWithHediff);
        }
    }
}
