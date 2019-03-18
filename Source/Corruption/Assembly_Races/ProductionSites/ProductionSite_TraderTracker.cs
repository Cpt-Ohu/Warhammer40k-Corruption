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
    public class ProductionSite_TraderTracker : Settlement_TraderTracker
    {
        private ProductionSite _productionSite;

        protected ThingOwner siteStock;

        public ProductionSite ProductionSite
        {
            get
            {
                if (this._productionSite == null)
                {
                    this._productionSite = this.settlement as ProductionSite;
                }
                return _productionSite;
            }
        }

        public ProductionSite_TraderTracker(Settlement settlement) : base(settlement)
        {


        }

        private TraderKindDef _traderKind;
        public override TraderKindDef TraderKind
        {
            get
            {
                if (this._traderKind == null)
                {
                    this._traderKind = this.ProductionSite.MainProduction.Props.TraderKind;
                }
                return this._traderKind;
            }
        }

        protected override void RegenerateStock()
        {
            base.RegenerateStock();
            foreach (var entry in this.ProductionSite.Stock)
            {
                Thing actualThing = ThingMaker.MakeThing(entry.Def, entry.stuff);
                actualThing.stackCount = entry.Count;
                this.StockListForReading.Add(actualThing);

            }
        }

        public override void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            base.GiveSoldThingToPlayer(toGive, countToGive, playerNegotiator);
            int removedCount = 0;
            while (countToGive > removedCount)
            {
                ResourceEntry entry = this.ProductionSite.Stock.FirstOrDefault(x => x.Def == toGive.def && toGive.Stuff == x.stuff && x.Count > 0);
                if (entry != null)
                {
                    removedCount += Math.Min(entry.Count, countToGive);
                    entry.Count -= removedCount;
                }
                else
                {
                    removedCount = countToGive;
                }
            }
        }
        public override void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            base.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
            ResourceEntry entry = this.ProductionSite.Stock.FirstOrDefault(x => x.Def == toGive.def && toGive.Stuff == x.stuff && x.Count > 0);
            if (entry != null)
            {
                entry.Count += countToGive;
            }
            else
            {
                var newEntry = new ResourceEntry(toGive.def, toGive.Stuff, countToGive);
            }
        }

    }
}
