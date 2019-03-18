using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Corruption.Domination;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public class Dialog_ExpandSiteLevel : Dialog_BuildOnWorldMap
    {
        private ProductionSiteLevelDef newLevelDef;
        private ProductionSiteLevelDef previousLevelDef;

        private ProductionSite site;

        private ResourceProductionComp production;

        public Dialog_ExpandSiteLevel(ProductionSite site, ResourceProductionComp productionComp, ProductionSiteLevelDef newLevelDef, ProductionSiteLevelDef previousLevelDef)
        {
            this.site = site;
            this.production = productionComp;
            this.newLevelDef = newLevelDef;
            this.DefToBuild = newLevelDef;
            this.previousLevelDef = previousLevelDef;
        }

        protected override IEnumerable<ThingDefCountClass> Cost
        {
            get
            {
                return this.newLevelDef.Cost.AsEnumerable();
            }
        }

        protected override IWorldObjectBuilder Builder => this.site;

        protected override void Build()
        {
            this.production.SetLevel(this.newLevelDef);
        }

        protected override float DrawInfo(float curY, float width)
        {
            Text.Anchor = TextAnchor.UpperLeft;
            Rect descRect = new Rect(0f, curY, width, Text.LineHeight * 3f);
            Text.Font = GameFont.Tiny;
            Widgets.TextArea(descRect, this.DefToBuild.description, true);
            Rect effectRect = new Rect(0f, descRect.yMax + 2f, width, Text.LineHeight);
            string oldYield = "BaseProduction".Translate(this.newLevelDef.BaseProduction);
            Widgets.Label(effectRect, oldYield);
            Text.Anchor = TextAnchor.UpperRight;
            string oldSpeed = "BaseProductionCycle".Translate(this.newLevelDef.DaysProductionCycle);
            Widgets.Label(effectRect, oldSpeed);
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.color = ExpansionColor;
            Rect newEffectRect = new Rect(0f , effectRect.yMax + 2f, effectRect.width, Text.LineHeight);
            Widgets.Label(newEffectRect, "+" + (this.newLevelDef.BaseProduction - this.previousLevelDef.BaseProduction).ToString());

            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(newEffectRect, "-" + (this.newLevelDef.DaysProductionCycle - this.previousLevelDef.DaysProductionCycle).ToString());
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            return newEffectRect.yMax;
        }

        protected override void DrawCostInfo(Rect costRect)
        {
            float curX = 0f;
            foreach (var costItem in this.Cost)
            {
                Rect thingRect = new Rect(curX, costRect.y, 24, 24);
                TooltipHandler.TipRegion(thingRect, string.Concat(costItem.count, "x ", costItem.thingDef.label));
                if (!this.site.HasResource(costItem.thingDef, costItem.count))
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
