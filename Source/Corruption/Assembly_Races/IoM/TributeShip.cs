using OHUShips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
   public class TributeShip : ShipBase
    {

        public override void DeSpawn()
        {
            if (CorruptionStoryTrackerUtilities.CurrentStoryTracker.ImperialFactions.Contains(this.Faction))
            {
                IEnumerable<ResourcePack> resourcePacks = this.GetDirectlyHeldThings().Where(x => x is ResourcePack).Cast<ResourcePack>();
                float totalValue = 0f;
                foreach (var pack in resourcePacks)
                {
                    if (pack.compResource.IsTribute == true)
                    {
                        totalValue += pack.compResource.Resources.Sum(x => x.MarketValue);
                    }
                }
                float currentStanding = this.Faction.RelationWith(RimWorld.Faction.OfPlayer).goodwill;
                float influenceValue = totalValue / (100000f * currentStanding);
                this.Faction.RelationWith(RimWorld.Faction.OfPlayer).goodwill += influenceValue;
            }
            base.DeSpawn();
        }
    }
}
