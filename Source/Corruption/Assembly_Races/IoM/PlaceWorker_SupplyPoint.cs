using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.IoM
{
    class PlaceWorker_SupplyPoint : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            foreach (IntVec3  current in WatchBuildingUtility.CalculateWatchCells((ThingDef)def, center, rot, map))
            {
                List<Thing> list = map.thingGrid.ThingsListAt(current);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != thingToIgnore && list[i].def.passability != Traversability.Standable)
                    {
                        return "MustHaveFreeLandingZone".Translate();
                    }
                }
            }

            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol)
        {
            Map CurrentMap = Find.CurrentMap;
            GenDraw.DrawFieldEdges(WatchBuildingUtility.CalculateWatchCells(def, center, rot, CurrentMap).ToList<IntVec3>());
            
            GenRadial.RadialCellsAround(center, def.specialDisplayRadius, true);
        }

        //public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        //{
        //    Map CurrentMap = Find.CurrentMap;
        //    CellRect landingZone = def.GetCompProperties<CompProperties_SupplyPoint>().LandingZoneRect;
        //    GenDraw.DrawFieldEdges(landingZone.Cells.ToList());

        //}
    }
}
