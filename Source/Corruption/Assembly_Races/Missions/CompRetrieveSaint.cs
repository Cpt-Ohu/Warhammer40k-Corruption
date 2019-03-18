using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class CompRetrieveSaint: CompThingRetrieval
    {

        public override void GenerateThingsToRetrieve()
        {
            base.GenerateThingsToRetrieve();
            ThingSetMakerDef holyItems = DefDatabase<ThingSetMakerDef>.GetNamed("IoM_HolyItems");
            ThingSetMakerDef remains = DefDatabase<ThingSetMakerDef>.GetNamed("IoM_SaintlyRemains");

            foreach (var def in remains.defPackage.defs)
            {
                Thing remain = ThingMaker.MakeThing((ThingDef)def);
                this.MissionCasket.TryAcceptThing(remain);
            }

            int count = 0;
            int MaxCount = Rand.RangeInclusive(1, 2);
            while (count < MaxCount)
            {
                ThingDef thingDef = (ThingDef)holyItems.defPackage.defs.RandomElement();
                ThingDef stuff = null;
                if (thingDef.IsStuff)
                {
                    stuff = GenStuff.RandomStuffFor(thingDef);
                }

                Thing item = ThingMaker.MakeThing(thingDef, stuff);
                if (item is ThingWithComps)
                {
                    CompQuality compQuality = (item as ThingWithComps)?.GetComp<CompQuality>();
                    compQuality?.SetQuality(QualityCategory.Legendary, ArtGenerationContext.Outsider);
                }
                this.MissionCasket.TryAcceptThing(item);
                count++;
            }
        }

    }
}
