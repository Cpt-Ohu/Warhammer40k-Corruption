using Corruption.IoM;
using OHUShips;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public static class ShippableRewardMissionUtility
    {
        public static bool RewardRequirementsMet(Map map)
        {
            bool requirementMet = DropShipUtility.MissingRunways(map);
            if (requirementMet == false)
            {
                CFind.MissionManager.ResetRunwayMission();
                Messages.Message("MissionFailedMissingRunways".Translate(), new GlobalTargetInfo(map.Tile), MessageTypeDefOf.NegativeEvent);
            }
            else
            {
                CFind.MissionManager.FinishMission("BuildRunway");
            }
            return requirementMet;           
        }
                
        public static void GetSupplyRewards(Map map, List<SupplyDropDef> supplyDropDefs, Faction faction)
        {
                CorruptionStoryTrackerUtilities.CreateSupplyShip(supplyDropDefs, map, faction);            
        }


    }
}
