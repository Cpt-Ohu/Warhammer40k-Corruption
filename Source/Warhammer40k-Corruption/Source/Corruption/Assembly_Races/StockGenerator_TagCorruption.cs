using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Reflection;

namespace Corruption
{
    public class StockGenerator_TagCorruption : StockGenerator_Tag
    {
        public string tradeTag;

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.tradeTags != null && thingDef.techLevel <= this.maxTechLevelBuy && thingDef.tradeTags.Contains(this.tradeTag);
        }        
    }
}
