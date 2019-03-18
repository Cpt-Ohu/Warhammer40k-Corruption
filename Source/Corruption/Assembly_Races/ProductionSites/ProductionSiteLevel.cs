using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public class ProductionSiteLevelDef : Def
    {
        public int Level;

        public float BaseProduction;

        public float DaysProductionCycle;

        public List<ThingDefCountClass> Cost = new List<ThingDefCountClass>();

        public List<ProductionSiteUpgradeDef> UnlocksUpgrades = new List<ProductionSiteUpgradeDef>();

        public List<ProductionSiteLevelDef> RequiredAltLevels = new List<ProductionSiteLevelDef>();

        public TechLevel RequiredTechLevel = TechLevel.Neolithic;

        public string uiIconPath = "World/WorldObjects/Icon/GenericHouse";

        public Vector2 uiIconSize = new Vector2(116f, 128f);

        public string expandingTexturePath = "World/WorldObjects/Expanding/Town";

        public Texture2D uiIcon;
        
        public Texture2D ExpandingTexture;

        public string siteTexturePath = "World/WorldObjects/Icon/ProductionSiteGeneric";

        public Texture SiteTexture;

        public int MaximumSupportedWorkers = 100;

        public override void PostLoad()
        {

            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (this.expandingTexturePath != "")
                {
                    this.ExpandingTexture = ContentFinder<Texture2D>.Get(expandingTexturePath);
                }

                if (this.uiIconPath != "")
                {
                    uiIcon = ContentFinder<Texture2D>.Get(uiIconPath);
                }

                if (this.siteTexturePath != "")
                {
                    SiteTexture = ContentFinder<Texture2D>.Get(siteTexturePath);
                }
            });
        }
    }
}
