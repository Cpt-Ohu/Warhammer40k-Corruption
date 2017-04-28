using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace Corruption.Tithes
{
    public class TitheEntryGlobal : IExposable, ILoadReferenceable
    {
        public int ID = -1;

        public TitheEntryGlobal()
        {
            this.requestedTitheAmount = (int)TitheUtilities.TaxCalculation(50000);
            this.titheDef = DefDatabase<TitheDef>.GetRandom();
   //         this.ID = CorruptionStoryTrackerUtilities.currentStoryTracker.GetTitheID();
        }

        public TitheEntryGlobal(TitheDef titheDef, float requestedAmount)
        {
            this.titheDef = titheDef;
            this.requestedTitheAmount = (int)requestedAmount;
            GetTitheItemdefs();
            this.ID = CorruptionStoryTrackerUtilities.currentStoryTracker.GetTitheID();
        }        

        public TitheDef titheDef;

        public List<ThingDef> thingDefs = new List<ThingDef>();

        public float requestedTitheAmount;

        public float collectedTitheAmount
        {
            get
            {
                return UpdateCollectedTithe();
            }
        }

        public float tithePercent
        {
            get
            {
                return Mathf.Clamp(collectedTitheAmount / requestedTitheAmount, 0f, 1f);
            }
        }

        public bool TitheReady
        {
            get
            {
                return this.tithePercent == 1f;
            }
        }

        public float UpdateCollectedTithe()
        {
            float num = 0;
            List<Building> list = TitheUtilities.allTitheContainers;
            for (int j = 0; j < list.Count; j++)
            {
                TitheContainer current = (TitheContainer)list[j];
                for (int k = 0; k < this.thingDefs.Count; k++)
                {
                    int items = current.GetInnerContainer().TotalStackCountOfDef(thingDefs[k]);
                    num += items * thingDefs[k].BaseMarketValue;
                }
            }

            return num;
        }
        
        private bool IsValidItem(ThingDef def)
        {
            if (this.thingDefs.Contains(def))
            {
                return true;
            }
            return false;
        }

        private void GetTitheItemdefs()
        {
            List<ThingDef> list = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.thingCategories != null && x.thingCategories.Intersect(titheDef.categoryDefs).Any());
            this.thingDefs.AddRange(list);
            this.thingDefs.AddRange(titheDef.fixedTitheThings);
            this.thingDefs.RemoveAll(x => titheDef.excludedTitheThings.Contains(x));
        }

        public void ExposeData()
        {
            Scribe_Defs.LookDef<TitheDef>(ref this.titheDef, "titheDef");
            Scribe_Values.LookValue<float>(ref this.requestedTitheAmount, "requestedTitheAmount", 1000, true);
          //  Scribe_Values.LookValue<float>(ref this.collectedTitheAmount, "collectedTitheAmount", 0, true);
            Scribe_Values.LookValue<int>(ref this.ID, "ID", 0, true);
        }

        public string GetUniqueLoadID()
        {
            return "TitheEntry_" + titheDef.label +"_" + this.ID;
        }
    }
}
