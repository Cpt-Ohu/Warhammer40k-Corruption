using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Corruption.Domination;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public class Dialog_BuildUpgrade : Dialog_BuildOnWorldMap
    {
        private ProductionSiteUpgradeDef _upgrade;

        private ProductionSite _site;

        private ResourceProductionComp _production;

        public Dialog_BuildUpgrade(ProductionSite site, ResourceProductionComp productionComp, ProductionSiteUpgradeDef upgradeDef)
        {
            this.DefToBuild = upgradeDef;
            this._site = site;
            this._production = productionComp;
            this._upgrade = upgradeDef;
        }


        protected override IEnumerable<ThingDefCountClass> Cost
        {
            get
            {
                return this._upgrade.Cost.AsEnumerable();
            }
        }

        protected override IWorldObjectBuilder Builder => this._site;

        protected override void Build()
        {
            this._production.Upgrades.Add(new UpgradeBuildProgress(this._upgrade, 0f));
        }

        protected override float DrawInfo(float curY, float width)
        {
            Rect descRect = new Rect(0f, curY, width, Text.LineHeight * 3f);
            Text.Font = GameFont.Tiny;
            Widgets.TextArea(descRect, this.DefToBuild.description, true);
            Rect effectRect = new Rect(0f, descRect.yMax, width, Text.LineHeight * 2.1f);
            GUI.color = ExpansionColor;
            Text.Anchor = TextAnchor.UpperLeft;
            if (_upgrade.SpeedBoost > 0f)
            {
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(effectRect, "ProductionSpeed".Translate());

                Text.Anchor = TextAnchor.LowerLeft;
                Widgets.Label(effectRect, string.Concat((_upgrade.SpeedBoost - 1f) * 100, "%"));
            }
            
            if (_upgrade.YieldBoost > 0f)
            {
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(effectRect, "YieldBoost".Translate());
                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(effectRect, string.Concat((_upgrade.YieldBoost - 1f) * 100, "%"));
            }
        
            if (_upgrade.extraProduct != null)
            {
                Text.Anchor = TextAnchor.UpperRight;
                Widgets.Label(effectRect, "ExtraProduct".Translate());
                Text.Anchor = TextAnchor.LowerRight;
                Widgets.Label(effectRect, _upgrade.extraProduct.thingDef?.label);
            }
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.color = Color.white;
            return effectRect.yMax;


        }

        protected override void DrawCostInfo(Rect costRect)
        {
            float curX = 0f;
            foreach (var costItem in this.Cost)
            {
                Rect thingRect = new Rect(curX, costRect.y, 24, 24);
                TooltipHandler.TipRegion(thingRect, string.Concat(costItem.count, "x ", costItem.thingDef.label));
                if (!this._site.HasResource(costItem.thingDef, costItem.count))
                {
                    this.CanBuild = false;
                    GenUI.DrawTextureWithMaterial(thingRect, costItem.thingDef.uiIcon, TexUI.GrayscaleGUI, default(Rect));
                }
                else
                {
                    GUI.DrawTexture(thingRect, costItem.thingDef.uiIcon);
                }
                curX = thingRect.xMax + 2;
            }
        }
    }
}
