using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public class ProductionSiteUpgradeDef : Def
    {
        public float SpeedBoost = 1f;

        public float YieldBoost = 1f;

        public string IconPath = "World/WorldObjects/Expanding/Town";

        public List<ThingDefCountClass> Cost = new List<ThingDefCountClass>();

        public Vector2 DrawPos = new Vector2(0f, 0f);

        public Vector2 DrawSize = new Vector2(116f, 128f);

        public int ConstructionTimeDays = 10;

        public ProductionGenOption extraProduct;

        public Texture2D Icon;

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.Icon = ContentFinder<Texture2D>.Get(this.IconPath, true);
            });
        }

    }
}
