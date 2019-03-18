using Corruption.IoM;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class Mission : IExposable, ILoadReferenceable
    {
        public MissionDef Def;

        public Faction GiverFaction;

        public object MissionTarget;

        public bool RewardPending;

        private bool missionFulfilled;

        public List<object> nextTargets = new List<object>();

        public int DaysLeft;

        public bool Enabled
        {
            get
            {
                return CFind.MissionManager.PerequisitesFulfilled(this.Def);
            }
        }

        public bool Finished
        {
            get
            {
                return missionFulfilled;
            }
        }

        public void Fail()
        {
            this.GiverFaction.TryAffectGoodwillWith(Faction.OfPlayer, this.Def.ReputationLoss, false, false);
            if (this.Def.PunishingIncident != null)
            {
                IncidentParms parms = new IncidentParms();
                parms.faction = this.GiverFaction;
                parms.target = Find.AnyPlayerHomeMap;
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
                this.Def.PunishingIncident.Worker.TryExecute(parms);
                Find.LetterStack.ReceiveLetter("MissionFailed".Translate(this.Def.LabelCap), string.Format(this.Def.PunishmentDescription, this.GiverFaction), LetterDefOf.NegativeEvent);
            }
            else
            {
                Find.LetterStack.ReceiveLetter("MissionFailed".Translate(this.Def.LabelCap), "MissionFailedGeneric".Translate(this.GiverFaction), LetterDefOf.NegativeEvent);
            }
        }

        public bool CanRunOut
        {
            get
            {
                return this.Def.DaysToFinish > 0;
            }
        }

        public IEnumerable<Object> AllTargets
        {
            get
            {
                if (this.MissionTarget != null) yield return this.MissionTarget;
                foreach (var target in this.nextTargets)
                {
                    yield return target;
                }
            }
        }

        public Mission() { }

        public Mission(MissionDef def, object target = null, Faction faction = null)
        {
            this.Def = def;
            this.GiverFaction = faction != null ? faction : Find.FactionManager.FirstFactionOfDef(def.GiverFaction);
            if (this.GiverFaction == null)
            {
                Log.Error("Giving Mission " + def.defName + " resulted in a null Giver Faction");
            }
            this.MissionTarget = target;
            this.DaysLeft = def.DaysToFinish;
        }

        public bool Finish()
        {
            this.RewardPending = true;
            Find.LetterStack.ReceiveLetter("MissionFinished".Translate(), "MissionFinishedDesc".Translate(new object[] { this.Def.label, this.GiverFaction?.Name }), LetterDefOf.PositiveEvent, null);

            return true;
        }

        public bool GiveRewards(Map map)
        {
            if (this.RewardRequirementsMet(map))
            {
                this.missionFulfilled = true;
                this.GiveRewardsInt(map);
                return true;
            }
            return false;
        }


        public void Reset()
        {
            this.RewardPending = false;
            this.missionFulfilled = false;
        }

        protected virtual bool RewardRequirementsMet(Map map)
        {
            if (this.Def.RewardMode == MissionRewardMode.DropPod)
            {
                return true;
            }
            else if (this.Def.RewardMode == MissionRewardMode.SupplyShip)
            {
                return ShippableRewardMissionUtility.RewardRequirementsMet(map);
            }
            return false;
        }

        private void GiveRewardsInt(Map map)
        {
            this.GetSupplyRewards(map);
            this.GetResearchRewards();
        }

        protected virtual void GetSupplyRewards(Map map)
        {
            if (!this.Def.RewardedSupplies.NullOrEmpty())
            {
                if (this.Def.RewardMode == MissionRewardMode.DropPod)
                {
                    this.SpawnSupplyDropPod(map);
                }
                else if (this.Def.RewardMode == MissionRewardMode.SupplyShip)
                {
                    ShippableRewardMissionUtility.GetSupplyRewards(map, this.Def.RewardedSupplies, this.GiverFaction);
                }
            }
        }

        /// <summary>
        /// Creates appropriate targets for a given mission
        /// </summary>
        internal virtual void CreateTargets()
        {

        }

        private void SpawnSupplyDropPod(Map map)
        {
            List<Thing> things = new List<Thing>();
            foreach (var entry in this.Def.RewardedSupplies)
            {
                things.AddRange(CorruptionStoryTrackerUtilities.CreateSupplyCargo(entry));
            }
            IntVec3 intVec = DropCellFinder.TradeDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, things, 110, false, false, true);
            Find.LetterStack.ReceiveLetter("MissionSupplyPodsArrived".Translate(), "MissionSupplyPods".Translate(new object[] { this.Def.label }), LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null);

        }

        protected void GetResearchRewards()
        {
            foreach (var project in this.Def.RewardedResearch)
            {
                CorruptionStoryTrackerUtilities.GrantResearch(project);
            }
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.missionFulfilled, "missionFulfilled", false);
            Scribe_Values.Look<bool>(ref this.RewardPending, "rewardPending", false);
            Scribe_Defs.Look<MissionDef>(ref this.Def, "Def");
            Scribe_References.Look<Faction>(ref this.GiverFaction, "Faction", true);
            Scribe_Collections.Look<object>(ref this.nextTargets, "NextTargets", LookMode.Reference);
            Scribe_Values.Look<object>(ref this.MissionTarget, "MissonTarget");
            Scribe_Values.Look<int>(ref this.DaysLeft, "DaysLeft");
        }

        public string GetUniqueLoadID()
        {
            return "Mission_" + this.GiverFaction + "_" + CFind.MissionManager.GetLoadID();
        }
        
    }
}
