using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.ProductionSites
{
    public class UpgradeBuildProgress : IExposable
    {
        public ProductionSiteUpgradeDef Def;

        public float ConstructionProgress;

        public UpgradeBuildProgress(ProductionSiteUpgradeDef def, float progress)
        {
            this.Def = def;
            this.ConstructionProgress = progress;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<float>(ref this.ConstructionProgress, "ConstructionProgress");
            Scribe_Defs.Look<ProductionSiteUpgradeDef>(ref this.Def, "Upgrade");
        }
    }
}
