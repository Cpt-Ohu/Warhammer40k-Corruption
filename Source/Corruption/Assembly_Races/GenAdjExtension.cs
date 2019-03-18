using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    static class GenAdjExtension
    {

        public static CellRect GetAdjustedCellRect(CellRect inRect, IntVec3 center, Rot4 rot, int watchRot)
        {
            Rot4 a = new Rot4(watchRot);
            CellRect result;
            if (a.IsHorizontal)
            {
                int num = center.x + GenAdj.CardinalDirections[watchRot].x * inRect.minX;
                int num2 = center.x + GenAdj.CardinalDirections[watchRot].x * inRect.maxX;
                int num3 = center.z + inRect.maxZ / 2;
                int num4 = center.z - inRect.Width;
                if (inRect.Width % 2 == 0)
                {
                    if (a == Rot4.West)
                    {
                        num4++;
                    }
                    else
                    {
                        num3--;
                    }
                }
                result = new CellRect(Mathf.Min(num, num2), num4, Mathf.Abs(num - num2) + 1, num3 - num4 + 1);
            }
            else
            {
                int num5 = center.z + GenAdj.CardinalDirections[watchRot].z * inRect.minX;
                int num6 = center.z + GenAdj.CardinalDirections[watchRot].z * inRect.maxX;
                int num7 = center.x + inRect.Width / 2;
                int num8 = center.x - inRect.Width / 2;
                if (inRect.Width == 0)
                {
                    if (a == Rot4.North)
                    {
                        num8++;
                    }
                    else
                    {
                        num7--;
                    }
                }
                result = new CellRect(num8, Mathf.Min(num5, num6), num7 - num8 + 1, Mathf.Abs(num5 - num6) + 1);
            }
            return result;
        }

        public static CellRect GetWatchCellRect(ThingDef def, IntVec3 center, Rot4 rot, int watchRot)
        {

            Rot4 a = new Rot4(watchRot);
            if (def.building == null)
            {
                def = (def.entityDefToBuild as ThingDef);
            }
            CellRect result;
            if (a.IsHorizontal)
            {
                int num = center.x + GenAdj.CardinalDirections[watchRot].x * def.building.watchBuildingStandDistanceRange.min;
                int num2 = center.x + GenAdj.CardinalDirections[watchRot].x * def.building.watchBuildingStandDistanceRange.max;
                int num3 = center.z + def.building.watchBuildingStandRectWidth / 2;
                int num4 = center.z - def.building.watchBuildingStandRectWidth / 2;
                if (def.building.watchBuildingStandRectWidth % 2 == 0)
                {
                    if (a == Rot4.West)
                    {
                        num4++;
                    }
                    else
                    {
                        num3--;
                    }
                }
                result = new CellRect(Mathf.Min(num, num2), num4, Mathf.Abs(num - num2) + 1, num3 - num4 + 1);
            }
            else
            {
                int num5 = center.z + GenAdj.CardinalDirections[watchRot].z * def.building.watchBuildingStandDistanceRange.min;
                int num6 = center.z + GenAdj.CardinalDirections[watchRot].z * def.building.watchBuildingStandDistanceRange.max;
                int num7 = center.x + def.building.watchBuildingStandRectWidth / 2;
                int num8 = center.x - def.building.watchBuildingStandRectWidth / 2;
                if (def.building.watchBuildingStandRectWidth % 2 == 0)
                {
                    if (a == Rot4.North)
                    {
                        num8++;
                    }
                    else
                    {
                        num7--;
                    }
                }
                result = new CellRect(num8, Mathf.Min(num5, num6), num7 - num8 + 1, Mathf.Abs(num5 - num6) + 1);
            }
            return result;
        }

    }
}
