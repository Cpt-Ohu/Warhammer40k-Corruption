using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class ResourceEntry : IExposable
    {
        public ThingDef Def;

        public ThingDef stuff;

        public int Count;

        public QualityCategory AvgQualityCategory;

        public float MarketValue
        {
            get
            {
                StatRequest request = StatRequest.For(this.Def, this.stuff, AvgQualityCategory);
                StatWorker worker = new StatWorker_MarketValue();
                float baseValue = worker.GetValue(request, false);
                return baseValue * Count;
            }
        }

        public ResourceEntry()
        {
        }

        public ResourceEntry(ThingDef thingDef, ThingDef stuff, int initialAmount)
        {
            this.Def = thingDef;
            this.stuff = stuff;
            this.Count = initialAmount;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref Def, "Def");
            Scribe_Values.Look(ref Count, "Count", 0, true);
            Scribe_Values.Look(ref AvgQualityCategory, "AvgQuality", QualityCategory.Normal);
        }

        public static void InsertOrUpdate(ref List<ResourceEntry> list, ThingDef thingDef, ThingDef stuff, int amount)
        {
            ResourceEntry entry = list.FirstOrDefault(x => x.Def == thingDef);
            if (entry != null)
            {
                entry.Count += amount;
            }
            else
            {
                list.Add(new ResourceEntry(thingDef, stuff, amount));
            }
        }
    }
}
