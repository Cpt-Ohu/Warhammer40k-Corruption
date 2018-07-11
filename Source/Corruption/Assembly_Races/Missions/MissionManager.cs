using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption.Missions
{
    public class MissionManager : IExposable
    {
        public List<Mission> Missions = new List<Mission>();

        public MissionManager()
        {
        }

        public void LoadStartingMissions()
        {
            List<MissionDef> startingMissions = DefDatabase<MissionDef>.AllDefsListForReading.FindAll(x => x.StartingMission == true && x.AllowedPlayerFactions.Contains(Faction.OfPlayer.def));
            foreach (var missionDef in startingMissions)
            {
                this.CreateMission(missionDef);
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

       public bool FinishMission(MissionDef missionDef)
        {
            Map visibleMap = Find.VisibleMap;
            Map rewardMap = visibleMap.IsPlayerHome ? visibleMap : Find.AnyPlayerHomeMap;
            Mission mission = this.AvailableMissions.FirstOrDefault(x => x.Def == missionDef);
             if (mission != null)
            {
              return  mission.Finish(rewardMap);
            }
            return false;
        }

        public void CreateMission(MissionDef missionDef)
        {
            Mission mission = new Mission(missionDef);
            this.Missions.Add(mission);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<Mission>(ref this.Missions, "Missions", LookMode.Deep);
        }
    }
}
