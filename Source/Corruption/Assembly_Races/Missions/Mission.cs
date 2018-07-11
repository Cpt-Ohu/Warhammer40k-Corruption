using Corruption.IoM;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class Mission : IExposable
    {
        public MissionDef Def;

        public Faction GiverFaction;
        
        public bool RewardPending;

        private bool missionFulfilled;

        public bool Enabled
        {
            get
            {
                return CorruptionStoryTrackerUtilities.CurrentMissionManager.PerequisitesFulfilled(this.Def);
            }
        }

        public bool Finished
        {
            get
            {
                return missionFulfilled;
            }
        }

        public Mission() { }
        
        public Mission(MissionDef def)
        {
            this.Def = def;
            this.GiverFaction = Find.FactionManager.FirstFactionOfDef(def.GiverFaction);
        }

        public bool Finish(Map map)
        {
            this.missionFulfilled = true;
            if (this.RewardRequirementsMet(map) && !missionFulfilled)
            {
                this.GetRewards(map);
                return true;
            }
            return false;
        }

        protected virtual bool RewardRequirementsMet(Map map)
        {
                return true;            
        }

        private void GetRewards(Map map)
        {
            this.GetSupplyRewards(map);
            this.GetResearchRewards();
        }

        protected virtual void GetSupplyRewards(Map map)
        {            
            if (!this.Def.RewardedSupplies.NullOrEmpty())
            {
                List<Thing> things = new List<Thing>();
                foreach (var entry in this.Def.RewardedSupplies)
                {
                    things.AddRange(CorruptionStoryTrackerUtilities.CreateSupplyCargo(entry));
                }
                IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
                DropPodUtility.DropThingsNear(intVec, map, things, 110, false, true, true, true);
                Find.LetterStack.ReceiveLetter("MissionSupplyPodsArrived".Translate(), "MissionSupplyPods".Translate(), LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null);

            }
        }

        protected void GetResearchRewards()
        {
            foreach (var project in this.Def.RewardedResearch)
            {
                CorruptionStoryTrackerUtilities.GrantResearch(project);
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look<MissionDef>(ref this.Def, "Def");
            Scribe_References.Look<Faction>(ref this.GiverFaction, "Faction", true);
        }
    }
}
