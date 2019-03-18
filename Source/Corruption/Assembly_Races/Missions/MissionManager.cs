using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace Corruption.Missions
{
    public class MissionManager : IExposable
    {
        public List<Mission> Missions = new List<Mission>();
        private int lastMissionID;

        public MissionManager()
        {

        }

        public void DailyTick()
        {
            foreach (var faction in CFind.StoryTracker.ImperialFactions)
            {
                if (Rand.Value > 0.2f)
                {
                    
                }
            }

            foreach (Mission mission in this.AvailableMissions.Where(x => x.CanRunOut))
            {
                mission.DaysLeft--;
                if (mission.DaysLeft <= 0)
                {
                    mission.Fail();
                }
            }
        }

        public void LoadStartingMissions()
        {
            List<MissionDef> startingMissions = DefDatabase<MissionDef>.AllDefsListForReading.FindAll(x => x.StartingMission == true && x.AllowedPlayerFactions.Contains(Faction.OfPlayer.def));
            
            foreach (var missionDef in startingMissions)
            {
                this.CreateMission(missionDef, null);
            }
        }

        public List<Mission> AvailableMissions
        {
            get
            {
                return this.Missions.FindAll(x => !x.Finished);
            }
        }

        public Mission GetMission(MissionDef missionDef)
        {
            return this.Missions.FirstOrDefault(x => x.Def == missionDef);
        }
        
        public bool PerequisitesFulfilled(MissionDef missionDef)
        {
            return missionDef.PerequisiteMissions.All(x => this.GetMission(x)?.Finished == true);
        }

        public List<Mission> FinishedMissions
        {
            get
            {
                return this.Missions.FindAll(x => x.Finished);
            }
        }

        public Map RewardMap()
        {
            Map CurrentMap = Find.CurrentMap;
            return CurrentMap.IsPlayerHome ? CurrentMap : Find.AnyPlayerHomeMap;
        }
        
        public bool FinishMission(MissionDef missionDef)
        {
            return this.FinishMissionInternal(missionDef);
        }

        public bool FinishMission(string missionDefName)
        {
            MissionDef missionDef = DefDatabase<MissionDef>.GetNamedSilentFail(missionDefName);
            if (missionDef != null)
            {
                return this.FinishMissionInternal(missionDef);
            }
            else
            {
                return false;
            }
        }

        public bool FinishMission(Mission mission)
        {
            if (!this.AvailableMissions.Contains(mission))
            {
                return false;
            }
            return this.FinishMissionDirect(mission);
        }

        private bool FinishMissionInternal(MissionDef missionDef)
        {
            if (this.FinishedMissions.FirstOrDefault(x => x.Def == missionDef) != null)
            {
                return false;
            }
            Map rewardMap = this.RewardMap();
            Mission mission = this.AvailableMissions.FirstOrDefault(x => x.Def == missionDef);
             if (mission != null)
            {
                return FinishMissionDirect(mission);
            }
            return false;
        }

        private bool FinishMissionDirect(Mission mission)
        {
            UnlockFollowUpMissions(mission);
            return mission.Finish();
        }
        
        public Mission CreateMission(MissionDef missionDef, Faction faction, List<Object> targets = null)
        {            
            Mission mission = new Mission(missionDef, null, faction);
            if (!targets.NullOrEmpty())
            {
                object newTarget = targets[0];
                var nextTargets = targets.FindAll(x => x != newTarget);
                mission.MissionTarget = newTarget;
                mission.nextTargets = nextTargets;
            }
            else if (missionDef.NeedsTarget)
            {
                mission.CreateTargets();
            }
            this.Missions.Add(mission);
            return mission;
        }
        
        private void UnlockFollowUpMissions(Mission mission)
        {
            foreach (var def in mission.Def.FollowUpMissions)
            {
                this.CreateMission(def, mission.GiverFaction, mission.nextTargets);
            }
        }

        public bool TryFinishMissionForTarget(object target, object nextTarget = null)
        {            
            Mission mission = this.AvailableMissions.FirstOrDefault(x => x.MissionTarget == target);
            
            if (mission != null)
            {
                if (nextTarget != null)
                {
                    mission.nextTargets.Insert(0, nextTarget);
                }
                this.FinishMissionDirect(mission);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryFailMissionForTarget(object target)
        {
            Mission mission = this.AvailableMissions.FirstOrDefault(x => x.MissionTarget == target);
            if (mission != null)
            {
                this.FailMission(mission);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FailMissionInternal(Mission mission)
        {
            this.Missions.Remove(mission);
        }

        public void FailMission(MissionDef missionDef)
        {
            Mission mission = this.AvailableMissions.FirstOrDefault(x => x.Def == missionDef);
            if (mission != null)
            {
                this.FailMissionInternal(mission);
            }
        }

        public void FailMission(Mission mission)
        {
            this.FailMissionInternal(mission);
        }

        public void FailMission(string missionDefName)
        {
            MissionDef missionDef = DefDatabase<MissionDef>.GetNamedSilentFail(missionDefName);
            if (missionDef != null)
            {
                this.FailMission(missionDef);
            }
        }

        public static void CheckPawnKilledMissions(Pawn pawn)
        {
            CFind.MissionManager?.TryFinishMissionForTarget(pawn);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<Mission>(ref this.Missions, "Missions", LookMode.Deep);
            Scribe_Values.Look<int>(ref this.lastMissionID, "lastMissionID");
        }

        public void ResetRunwayMission()
        {
            Mission runwayMission = this.Missions.FirstOrDefault(x => x.Def == DefOfs.MissionDefOf.BuildRunway);
            if (runwayMission != null)
            {
                runwayMission.Reset();
            }
        }

        internal string GetLoadID()
        {
            this.lastMissionID++;
            return this.lastMissionID.ToString();
        }

        private void CreateRandomMission(Faction faction)
        {
            MissionDef def = DefDatabase<MissionDef>.GetRandom();
            this.CreateMission(def, faction);
        }
    }
}
