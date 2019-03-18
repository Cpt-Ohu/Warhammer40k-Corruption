using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace Corruption.ProductionSites
{
    public static class ProductionSiteMaker
    {
        public static ProductionSite MakeProductionSiteAt(int tile, Faction faction, WorldObjectDef productionSiteDef, int level, IEnumerable<WorkForce> startingWorkForce = null, IEnumerable<ProductionSiteUpgradeDef> finishedUpgrades = null)
        {
            ProductionSite productionSite = (ProductionSite)WorldObjectMaker.MakeWorldObject(productionSiteDef);
            productionSite.Tile = tile;
            productionSite.SetFaction(faction);
            if (productionSite.MainProduction == null) Log.Message("No Production at " + productionSite.def.defName);
            productionSite.MainProduction._curLevelInt = level;
            if (startingWorkForce != null)
            {
                productionSite.MainProduction.WorkForce.AddRange(startingWorkForce);
            }
            if (finishedUpgrades != null)
            {
                foreach (var upgrade in finishedUpgrades)
                {
                    productionSite.MainProduction.Upgrades.Add(new UpgradeBuildProgress(upgrade, 1f));
                }
            }
            Find.WorldObjects.Add(productionSite);
            return productionSite;
        }

        public static void TurnSettlementIntoProductionSite(SettlementBase settlement, WorldObjectDef worldObjectDef)
        {
            int tile = settlement.Tile;
            Faction faction = settlement.Faction;
            Find.WorldObjects.Remove(settlement);   
            MakeProductionSiteAt(tile, faction, worldObjectDef, 0, null);
        }
    }
}
