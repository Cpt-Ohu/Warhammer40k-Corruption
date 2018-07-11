using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class ResourcePackEntry : IExposable
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

        public void ExposeData()
        {
            Scribe_Defs.Look(ref Def, "Def");
            Scribe_Values.Look(ref Count, "Count", 0, true);
            Scribe_Values.Look(ref AvgQualityCategory, "AvgQuality", QualityCategory.Normal);
        }
    }
}
