using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Domination
{

    public class UnfinishedWorldObject : MapParent
    {
        private WorldObjectDef objectToBuild;
        private int totalTicksRequired;
        private int constructionTicks;
        private bool stopped;


        public int ConstructionTimeRemaining
        {
            get
            {
                return (this.totalTicksRequired - this.constructionTicks) / 60;
            }
        }

        public bool Stopped
        {
            get
            {
                return this.stopped;
            }
        }

        public UnfinishedWorldObject()
        {

        }


        public void StartConstruction(int constructionTime, WorldObjectDef worldObjectDef)
        {
            this.objectToBuild = worldObjectDef;
            this.totalTicksRequired = constructionTime;
            this.stopped = false;
        }

        public override void Tick()
        {
            base.Tick();
            if (!this.Stopped)
            {
                constructionTicks++;
            }
            if (this.ConstructionTimeRemaining <= 0)
            {
                this.FinishConstruction();
            }
        }

        internal void SetObjectToBuild(WorldObjectDef def)
        {
            this.objectToBuild = def;
        }

        private void FinishConstruction()
        {
            int tile = this.Tile;
            Find.WorldObjects.Remove(this);
            if (this.objectToBuild.comps.Any(x => x is ProductionSites.WorldObjectCompProperties_ResourceProduction))
            {
                ProductionSites.ProductionSiteMaker.MakeProductionSiteAt(tile, this.Faction, this.objectToBuild, 0);
            }
            else
            {
                WorldObject worldObject = WorldObjectMaker.MakeWorldObject(this.objectToBuild);
                worldObject.Tile = tile;
                Find.WorldObjects.Add(worldObject);
            }
        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine("ConstructingWorldObject".Translate(this.objectToBuild.label));
            stringBuilder.AppendLine("ConstructionTimeWorldObjectLeft".Translate(this.ConstructionTimeRemaining));
            stringBuilder.AppendLine(this.stopped ? "ConstructionStopped".Translate() : "ConstructionOngoing".Translate());
            return stringBuilder.ToString();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<WorldObjectDef>(ref this.objectToBuild, "objectToBuild");
            Scribe_Values.Look<int>(ref this.constructionTicks, "constructionTicks");
            Scribe_Values.Look<int>(ref this.totalTicksRequired, "totalTicksRequired");
            Scribe_Values.Look<bool>(ref this.stopped, "stopped");

        }

    }
}
