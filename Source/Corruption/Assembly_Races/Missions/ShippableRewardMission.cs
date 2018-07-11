using OHUShips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class ShippableRewardMission : Mission
    {
        protected override bool RewardRequirementsMet(Map map)
        {
           return DropShipUtility.MissingRunways(map);            
        }
                
        protected override void GetSupplyRewards(Map map)
        {
            if (!this.Def.RewardedSupplies.NullOrEmpty())
            {
                CorruptionStoryTrackerUtilities.CreateSupplyShip(this.Def.RewardedSupplies, map, this.GiverFaction);
            }
        }
    }
}
