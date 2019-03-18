using Corruption.IoM;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public class LiveStockProductionComp : AgriProductionComp
    {
        public List<ThingDef> LiveStockDefs = new List<ThingDef>();

        public override void CompTick()
        {
            base.CompTick();
        }

        public FarmProductionComp FarmComp
        {
            get
            {
                return this.parent.GetComponent<FarmProductionComp>();
            }
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);

        }

        protected override bool IgnoreGrowingSeason
        {
            get
            {
                return this.FarmComp != null && this.FarmComp.AllowLivestockFeeding;
            }
        }

        public override void PostAdd()
        {
            base.PostAdd();
            this.GetAnimals();
        }

        private void GetAnimals()
        {
            this.LiveStockDefs = new List<ThingDef>();
            IEnumerable<ThingDef> fittingAnimals = (DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.race != null && x.race.Animal && x.race.wildness < 0.35f && x.race.foodType == FoodTypeFlags.OmnivoreAnimal));
            if (fittingAnimals.Count() > 0)
            {
                for (int i = 0; i < 1; i++)
                {
                    ThingDef randomAnimal = fittingAnimals.RandomElementByWeight((ThingDef k) => 0.420000017f - k.race.wildness);
                    this.LiveStockDefs.Add(randomAnimal);
                }
            }
        }

        protected override void CreateProducts()
        {
            foreach (ThingDef animalDef in LiveStockDefs)
            {
                int num = (int)(Yield * ((animalDef.race.litterSizeCurve == null) ? 1 : Mathf.RoundToInt(Rand.ByCurve(animalDef.race.litterSizeCurve))));
                if (num < 1)
                {
                    num = 1;
                }
                ResourceEntry.InsertOrUpdate(ref this.ProductionSite.Stock, animalDef, null, num);
            }
        }
    }
}
