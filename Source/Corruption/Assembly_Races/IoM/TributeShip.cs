using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using OHUShips;
using Corruption.Missions;

namespace Corruption.IoM
{
    public class TributeShip : ShipColorable
    {
        private TributeMission assignedTributeMission;

        public void SetMission(TributeMission mission)
        {
            this.assignedTributeMission = mission;
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.assignedTributeMission != null)
            {
                foreach (var thing in this.GetDirectlyHeldThings())
                {
                    var tribute = this.assignedTributeMission.Tributes.FirstOrDefault(x => x.ThingDef == thing.def);
                    if (tribute != null)
                    {
                        tribute.SatisfiedAmount += thing.stackCount;
                    }
                }
                if (this.assignedTributeMission.Tributes.All(x => x.SatisfiedAmount >= x.RequestedAmount))
                {
                    CFind.MissionManager.FinishMission(this.assignedTributeMission);
                }
            }
            else if (CFind.StoryTracker.ImperialFactions.Contains(this.Faction))
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
                int influenceValue = (int)(totalValue / (100000f * currentStanding));
                this.Faction.RelationWith(RimWorld.Faction.OfPlayer).goodwill += influenceValue;
            }
            base.DeSpawn();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<TributeMission>(ref this.assignedTributeMission, "assignedTributeMission");
        }
    }
}
