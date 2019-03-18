using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.ProductionSites
{
    public class ResourceProductionCity : ResourceProductionComp
    {
        private float skillFactor = 2f;

        protected override void CreateProducts()
        {
            var production = this.ProductionSite.AllProduction.Where(x => this.ProductionSite.MainProduction != x).ToList().RandomElement();
            PawnKindDef pawnKind = this.ProductionSite.Faction.RandomPawnKind();
            production.AddToWorkforce(pawnKind, this.skillFactor);
        }
    }
}
