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
    [StaticConstructorOnStartup]
    public class FarmProductionComp : AgriProductionComp
    {
        private static Texture2D SetProductTex = ContentFinder<Texture2D>.Get("UI/Commands/SetPlantToGrow");

        public bool AllowLivestockFeeding;

        private bool HasPastures
        {
            get
            {
                return this.Upgrades.Any(x => x.Def.defName.Equals("UpgradePastures"));
            }
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
            IEnumerable<ThingDef> rawFarmPlants = DefDatabase<ThingDef>.AllDefs.Where(x => x.plant != null && x.plant.harvestedThingDef.thingCategories.Contains(ThingCategoryDefOf.PlantFoodRaw));
            while(this.ProduceDefs.Count < 4)
            {
                ThingDef randomPlant = rawFarmPlants.RandomElementByWeight(x => (20 - x.plant.sowMinSkill));
                this.ProduceDefs.Add(randomPlant.plant.harvestedThingDef);
            }
        }

        private List<ThingDef> ProduceDefs = new List<ThingDef>();

        protected override bool IgnoreGrowingSeason
        {
            get
            {
                return this.Upgrades.Any(x => x.Def.defName.Equals("UpgradeHydroponicsPlant"));
            }
        }

        protected override void CreateProducts()
        {
            this.CreateProducts();
            foreach (var treeDef in this.ProduceDefs)
            {
                ResourceEntry.InsertOrUpdate(ref this.ProductionSite.Stock, treeDef, null, Yield);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (this.parent.Faction == Faction.OfPlayer)
            {
                Command_Action command = new Command_Action();
                command.defaultLabel = "SetFarmProducts".Translate();
                command.defaultDesc = "SetFarmProductsDesc".Translate();
                command.icon = SetProductTex;
                command.action = delegate
                {
                    this.SelectProduce();
                };
            }
        }

        private void SelectProduce()
        {
            throw new NotImplementedException();
        }
    }
}
