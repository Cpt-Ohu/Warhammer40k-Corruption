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
    public class Dialog_LaunchProductionSitePods : Window
    {
        ProductionSite ProductionSite;

        private List<Thing> things = new List<Thing>();

        public Dictionary<ResourceEntry, int> Resources = new Dictionary<ResourceEntry, int>();

        private List<TransferableOneWay> transferables = new List<TransferableOneWay>();

        public TransferableOneWayWidget transferableWidget;

        public Dialog_LaunchProductionSitePods(ProductionSite site)
        {
            this.ProductionSite = site;
            foreach (var stock in site.Stock)
            {
                this.AddToTransferables(stock);
            }
        }

        private void AddToTransferables(ResourceEntry entry)
        {
            Thing thing = ThingMaker.MakeThing(entry.Def, entry.stuff);
            thing.stackCount = entry.Count;
            this.things.Add(thing);
            TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching<TransferableOneWay>(thing, this.transferables, TransferAsOneMode.PodsOrCaravanPacking);
            if (transferableOneWay == null)
            {
                transferableOneWay = new TransferableOneWay();
                this.transferables.Add(transferableOneWay);
            }
            transferableOneWay.things.Add(thing);
        }

        public Vector2 ScrollPos = Vector2.zero;

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(0f, 0f, inRect.width, 64f);
            Rect stockRect = new Rect(0f, titleRect.yMax + 4f,inRect.width, 300f);
            GUI.BeginGroup(stockRect);
            Rect outRect = new Rect(0f, 0f, stockRect.width, stockRect.height);
            Rect viewRect = new Rect(0f, 0f, stockRect.width, this.ProductionSite.Stock.Count * Text.LineHeight);
            GUI.BeginScrollView(outRect, this.ScrollPos, viewRect);

            GUI.EndScrollView();
            foreach (var stock in this.Resources)
            {
                //this.DrawStockRow()
            }
            GUI.EndGroup();
        }

        public override void Close(bool doCloseSound = true)
        {
            this.things.ForEach(x => x.Destroy());
            base.Close(doCloseSound);
        }
    }
}
