using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class CompProperties_MissionRelevant : CompProperties
    {
        public MissionDef MissionDef;

        public List<ThingSetMakerDef> ThingSetMakerDefs = new List<ThingSetMakerDef>();

        public List<ThingDef> FixedThingDefs = new List<ThingDef>();

        public int MaxThingCount = 1;

        public float MaxMarketValue = 1000;
        public FloatRange MarketValueRange = new FloatRange(100, 1000);
        public TechLevel MaxTechLevel = TechLevel.Industrial;

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (this.compClass == null) this.compClass = typeof(CompMissionRelevant);
        }
    }
}
