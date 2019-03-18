using Corruption.Domination;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public class Dialog_BuildProductionSite : Dialog_BuildOnWorldMap
    {
        private WorldBuilderComp builderComp;

        private WorldMapBuildableDef worldMapBuildableDef;

        private WorldObjectCompProperties_ResourceProduction mainProduction;

        public Dialog_BuildProductionSite(WorldMapBuildableDef def, WorldBuilderComp builder)
        {
            this.DefToBuild = def;
            this.mainProduction = this.worldMapBuildableDef.WorldObjectDef.comps.FirstOrDefault(x => x.compClass.IsAssignableFrom(typeof(ResourceProductionComp))) as WorldObjectCompProperties_ResourceProduction;
            if (this.mainProduction == null)
            {
                Log.Error("Tried to build ProductionSite of Def {0} with no ResourceProductionComp");
                this.Close();
            }
            this.worldMapBuildableDef = def;
            this.builderComp = builder;
        }
    

        private ProductionSiteLevelDef levelToBuild;

        private ProductionSiteLevelDef previousLevel
        {
            get
            {
                return this.mainProduction.Levels.FirstOrDefault(x => x.Level == levelToBuild.Level - 1);
            }
        }


        private ProductionSiteLevelDef nextLevel
        {
            get
            {
                return this.mainProduction.Levels.FirstOrDefault(x => x.Level == levelToBuild.Level + 1);
            }
        }

        protected override IEnumerable<ThingDefCountClass> Cost => this.worldMapBuildableDef.Cost;

        protected override IWorldObjectBuilder Builder
        {
            get
            {
                return this.builderComp;
            }
        }

        protected override void Build()
        {
            UnfinishedWorldObject constructionSite = (UnfinishedWorldObject)WorldObjectMaker.MakeWorldObject(DefOfs.C_WorldObjectDefOf.UnfinishedWorldObject);
            constructionSite.Tile = this.builderComp.parent.Tile;
            constructionSite.SetFaction(this.builderComp.parent.Faction);
            Find.WorldObjects.Add(constructionSite);
            constructionSite.StartConstruction(this.worldMapBuildableDef.ConstructionTimeDays, this.worldMapBuildableDef.WorldObjectDef);
        }

        protected override void DrawCostInfo(Rect costRect)
        {
            Rect levelSelectorRect = new Rect(0f, costRect.y, costRect.width, 30f);
            Widgets.DrawBox(levelSelectorRect, 2);
            Widgets.Label(levelSelectorRect, this.levelToBuild?.label);
            Rect prevRect = new Rect(0f, levelSelectorRect.x - 32f, 30f, 30f);
            if (Widgets.ButtonText(prevRect, "<",true,false, this.previousLevel != null ))
            {
                this.levelToBuild = this.previousLevel;
            }
            Rect nextRect = new Rect(levelSelectorRect.xMax +2f, 0f, 30f, 30f);
            if (Widgets.ButtonText(nextRect, ">", true, false, this.nextLevel != null))
            {
                this.levelToBuild = this.nextLevel;
            }

            float curX = 0f;
            foreach (var costItem in this.Cost)
            {
                Rect thingRect = new Rect(curX, costRect.y, 24, 24);
                TooltipHandler.TipRegion(thingRect, string.Concat(costItem.count, "x ", costItem.thingDef.label));
                if (!this.Builder.HasResource(costItem.thingDef, costItem.count))
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
