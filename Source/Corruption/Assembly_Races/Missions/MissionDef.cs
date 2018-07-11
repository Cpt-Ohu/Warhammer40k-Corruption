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

        public List<MissionDef> PerequisiteMissions = new List<MissionDef>();

        public bool StartingMission;

        public List<FactionDef> AllowedPlayerFactions = new List<FactionDef>();

        public List<SupplyDropDef> RewardedSupplies = new List<SupplyDropDef>();

        public List<ResearchProjectDef> RewardedResearch = new List<ResearchProjectDef>();

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

    }
}
