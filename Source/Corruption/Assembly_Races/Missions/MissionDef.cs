using Corruption.IoM;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Missions
{
    public class MissionDef : Def
    {
        public FactionDef GiverFaction;

        public Type missionClass;

        public MissionRewardMode RewardMode;
        
        public List<MissionDef> PerequisiteMissions = new List<MissionDef>();

        public List<MissionDef> FollowUpMissions = new List<MissionDef>();

        public bool NeedsTarget = false;

        public bool StartingMission;

        public bool IsGeneric;

        public int DaysToFinish;

        public List<FactionDef> AllowedPlayerFactions = new List<FactionDef>();

        public List<SupplyDropDef> RewardedSupplies = new List<SupplyDropDef>();

        public List<ResearchProjectDef> RewardedResearch = new List<ResearchProjectDef>();

        public List<GenStepDef> GenStepsForMap = new List<GenStepDef>();

        public IncidentDef PunishingIncident;

        public IncidentDef RewardingIncident;

        public string PunishmentDescription;

        public int ReputationLoss = 5;

        public int ReputationReward = 2;

        private string uiIconPath = "";

        [Unsaved]
        public Texture2D uiIcon = BaseContent.BadTex;

        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (!this.uiIconPath.NullOrEmpty())
                {
                    this.uiIcon = ContentFinder<Texture2D>.Get(this.uiIconPath, true);
                }
                
            });
        }
        
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (this.RewardMode == MissionRewardMode.SupplyShip)
            {
                if (!this.PerequisiteMissions.Contains(DefOfs.MissionDefOf.BuildRunway))
                {
                    this.PerequisiteMissions.Add(DefOfs.MissionDefOf.BuildRunway);
                }
            }
        }
    }
}
