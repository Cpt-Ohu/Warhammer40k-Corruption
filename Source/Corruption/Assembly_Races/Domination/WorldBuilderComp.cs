using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Domination
{
    public class WorldBuilderComp : WorldObjectComp, IWorldObjectBuilder
    {
        public Caravan Caravan
        {
            get
            {
                return this.parent as Caravan;
            }
        }

        public OHUShips.WorldShip Ship
        {
            get
            {
                return this.parent as OHUShips.WorldShip;
            }
        }

        public List<ThingDefCountClass> Resources
        {
            get
            {
                if (this.Ship != null)
                {
                    return this.Ship.Goods.Select(x => new ThingDefCountClass(x.def, x.stackCount)).ToList();
                }
                else if (this.Caravan != null)
                {
                    return this.Caravan.Goods.Select(x => new ThingDefCountClass(x.def, x.stackCount)).ToList();
                }
                return new List<ThingDefCountClass>();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            foreach (WorldMapBuildableDef def in DefDatabase<WorldMapBuildableDef>.AllDefs)
            {
                Command_Action command = new Command_Action();
                command.defaultDesc = def.description;
                command.defaultLabel = "CommandBuildWorldObject".Translate(def.label);

                command.action = delegate
                {
                    Find.WindowStack.Add(new ProductionSites.Dialog_BuildProductionSite(def, this));
                };

                yield return command;

            }
        }

        public bool HasResource(ThingDef def, int count)
        {
            return this.Resources.Any(x => x.thingDef == def && x.count >= count);
        }

        private bool CanBuild(WorldMapBuildableDef buildableDef, out string missingResources)
        {
            bool result = true;
            StringBuilder builder = new StringBuilder();


            foreach (var thingdefCount in buildableDef.Cost)
            {
                var fittingGoods = Resources.Where(x => x.thingDef == thingdefCount.thingDef).ToList();
                if (fittingGoods.Count == 0)
                {
                    result = false;
                    builder.AppendLine(String.Concat(thingdefCount.thingDef.label, " ", "0", "/", thingdefCount.count));
                }
                else
                {
                    int availableCount = fittingGoods.Sum(x => x.count);
                    result = availableCount >= thingdefCount.count;
                    builder.AppendLine(String.Concat(thingdefCount.thingDef.label, " ", availableCount, "/", thingdefCount.count));
                }
            }
            missingResources = builder.ToString();
            return result;
        }

    }
}
