using Corruption.IoM;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.ProductionSites
{
    public class TransportPodsArrivalAction_AddStock : TransportPodsArrivalAction
    {
        private ProductionSite ProductionSite;

        private static List<Thing> tmpContainedThings = new List<Thing>();

        public TransportPodsArrivalAction_AddStock()
        {

        }

        public TransportPodsArrivalAction_AddStock(ProductionSite site)
        {
            this.ProductionSite = site;
        }

        public override void Arrived(List<ActiveDropPodInfo> pods, int tile)
        {
            for (int i = 0; i < pods.Count; i++)
            {
                TransportPodsArrivalAction_AddStock.tmpContainedThings.Clear();
                TransportPodsArrivalAction_AddStock.tmpContainedThings.AddRange(pods[i].innerContainer);
                for (int j = 0; j < TransportPodsArrivalAction_AddStock.tmpContainedThings.Count; j++)
                {
                    pods[i].innerContainer.Remove(TransportPodsArrivalAction_AddStock.tmpContainedThings[j]);
                    ResourceEntry.InsertOrUpdate(ref this.ProductionSite.Stock, TransportPodsArrivalAction_AddStock.tmpContainedThings[j].def, TransportPodsArrivalAction_AddStock.tmpContainedThings[j].Stuff, TransportPodsArrivalAction_AddStock.tmpContainedThings[j].stackCount);
                }
            }
            TransportPodsArrivalAction_AddStock.tmpContainedThings.Clear();
            Messages.Message("MessageTransportPodsArrivedAndAddedToProductionSite".Translate(this.ProductionSite.Name).CapitalizeFirst(), this.ProductionSite, MessageTypeDefOf.TaskCompletion, true);
        }

        public static FloatMenuAcceptanceReport CanGiveTo(IEnumerable<IThingHolder> pods, ProductionSite site)
        {
            return site != null && site.Spawned && (site.IsPlayerControlled || (CorruptionModSettings.AllowDomination && CFind.DominationTracker.PlayerAlliance.GetFactions().Contains(site.Faction)));
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompLaunchable representative, IEnumerable<IThingHolder> pods, ProductionSite site)
        {
            return TransportPodsArrivalActionUtility.GetFloatMenuOptions<TransportPodsArrivalAction_AddStock>(() => TransportPodsArrivalAction_AddStock.CanGiveTo(pods, site), () => new TransportPodsArrivalAction_AddStock(site), "SendResourcesToProductionSite".Translate(site.Name), representative, site.Tile);
        }
    }
}
