using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.ProductionSites
{
    public class ProductionGenOption
    {
        public ThingDef thingDef;
        public float yieldFactor;
        public List<ThingDefCount> RequiredResources = new List<ThingDefCount>();
    }
}
