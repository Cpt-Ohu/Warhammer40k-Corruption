using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption.Missions
{
    public class CompThingRetrieval : CompMissionRelevant
    {
        protected Building_MissionCasket MissionCasket
        {
            get
            {
                return this.parent as Building_MissionCasket;
            }
        }

        public virtual void GenerateThingsToRetrieve()
        {
            foreach (var thingsetmakerDef in this.Props.ThingSetMakerDefs)
            {
                ThingSetMaker thingSetMaker = thingsetmakerDef.root;
                ThingSetMakerParams parms = new ThingSetMakerParams();
                parms.countRange = new IntRange(1, this.Props.MaxThingCount);
                parms.maxThingMarketValue = this.Props.MaxMarketValue;
                parms.totalMarketValueRange = this.Props.MarketValueRange;
                parms.techLevel = this.Props.MaxTechLevel;
                List<Thing> things = thingSetMaker.Generate(parms);
                foreach (var thing in things)
                {
                    this.MissionCasket.TryAcceptThing(thing);
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad)
                this.GenerateThingsToRetrieve();
        }..
    }
}
