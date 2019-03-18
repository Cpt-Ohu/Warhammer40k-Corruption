using OHUShips;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Corruption.Missions
{
    [StaticConstructorOnStartup]
    public class MissionTabUtility : MainTabWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionWonder = Vector2.zero;
        private static Texture2D backTex = SolidColorMaterials.NewSolidColorTexture(Color.cyan);
        private static Texture2D searchReticule = ContentFinder<Texture2D>.Get("UI/Buttons/SearchReticule", true);
        private string curSearchText = "";

        private static MissionManager manager
        {
            get
            {
                return CFind.MissionManager;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect mainRect = inRect;
            GUI.BeginGroup(mainRect);
            Rect searchRect = new Rect(16f, 4f, mainRect.width - 32f, 32f);
            this.curSearchText = Widgets.TextField(searchRect, curSearchText);

            Rect innerRect = new Rect(0f, searchRect.yMax + 4f, mainRect.width, mainRect.height - searchRect.yMax - 16f);
            Rect viewRect = new Rect(innerRect);
            viewRect.height = manager.AvailableMissions.Count * 320f;
            Widgets.BeginScrollView(innerRect, ref this.scrollPosition, viewRect);
            GUI.BeginGroup(viewRect);
            float curY = 0f;
            foreach (var mission in manager.AvailableMissions)
            {
                this.DrawMission(mission, ref curY, mainRect.width);
            }
            GUI.EndGroup();



            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void DrawMission(Mission mission, ref float curY, float width)
        {
            float maxHeight = 164f + Text.CalcHeight(mission.Def.description, width) + Text.LineHeight * (mission.Def.RewardedResearch.Count + mission.Def.RewardedSupplies.Count + mission.Def.PerequisiteMissions.Count);
            Rect missionRect = new Rect(64f, curY, width - 128f, maxHeight);
            if (this.MissionFilter(mission) == true)
            {
                Widgets.DrawOptionSelected(missionRect);
            }
            else
            {
                Widgets.DrawOptionUnselected(missionRect);
            }
            GUI.BeginGroup(missionRect.ContractedBy(4f));
            float nextY = this.DrawMissionTitle(mission.Def.label, 0f, missionRect.width);

            nextY = this.DrawMissionGiverFaction(mission.GiverFaction.Name, nextY, missionRect.width);

            nextY = this.DrawMissionDescription(mission.Def.description, nextY, missionRect.width);
            
            nextY = this.DrawTargetIfExists(mission, nextY, width);

            if (mission.Def.PerequisiteMissions.Count > 0)
            {
                Widgets.ListSeparator(ref nextY, missionRect.width, "Perequisites".Translate());

                Rect pereqRect = new Rect(8f, nextY, missionRect.width - 16f, Text.LineHeight * mission.Def.PerequisiteMissions.Count);
                Listing_Standard listing_perequisites = new Listing_Standard();
                listing_perequisites.Begin(pereqRect);
                foreach (var pereq in mission.Def.PerequisiteMissions)
                {
                    bool finished = MissionTabUtility.manager.FinishedMissions.Any(x => x.Def == pereq);
                    listing_perequisites.CheckboxLabeled(pereq.label, ref finished, pereq.description);
                }
                listing_perequisites.End();
                nextY = pereqRect.yMax + 4f;
            }
            Widgets.ListSeparator(ref nextY, missionRect.width, "Rewards".Translate());
            Listing_Standard listing_rewards = new Listing_Standard();
            Rect rewardListRect = new Rect(8f, nextY + 4f, missionRect.width - 16f, 64f + Text.LineHeight * (mission.Def.RewardedResearch.Count + mission.Def.RewardedSupplies.Count));

            listing_rewards.Begin(rewardListRect);
            IEnumerable<Def> rewards = mission.Def.RewardedResearch.Cast<Def>().Concat(mission.Def.RewardedSupplies.Cast<Def>());
            foreach (var reward in rewards)
            {
                float lineHeight = Text.LineHeight;
                Rect rewardRect = listing_rewards.GetRect(lineHeight);
                if (Mouse.IsOver(rewardRect))
                {
                    Widgets.DrawHighlight(rewardRect);
                }
                if (reward.description != null) TooltipHandler.TipRegion(rewardRect, reward.description);
                Widgets.Label(rewardRect, reward.label);
            }
            if (canCollectRewards(mission))
            {
                if (listing_rewards.ButtonText("GetReward".Translate(), "CollectRewards".Translate()))
                {
                    Map rewardMap = manager.RewardMap();
                    mission.GiveRewards(rewardMap);
                }
            }

            listing_rewards.End();
            GUI.EndGroup(); //missionRect
            curY = missionRect.yMax + 12f;
        }

        private float DrawMissionGiverFaction(string name, float curY, float width, string timeLimit = "")
        {
            Rect descRect = new Rect(0f, curY, width, Text.CalcHeight(name, width));
            Widgets.Label(descRect, "MissionGiverFaction".Translate(name));
            if (timeLimit != "")
            {
                Text.Anchor = TextAnchor.UpperRight;
                Widgets.Label(descRect, "MissionTimeLimit".Translate(timeLimit));
                Text.Anchor = TextAnchor.UpperLeft;
            }
            return descRect.yMax + 4f;
        }

        private float DrawMissionTitle(string title, float curY, float width)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(0f, curY, width, 32f);
            Widgets.Label(titleRect, title);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            Widgets.DrawLineHorizontal(0f, titleRect.yMax + 2f, width);
            return titleRect.yMax + 6f;
        }

        private float DrawMissionDescription(string desc, float curY, float width)
        {
            Rect descRect = new Rect(0f, curY, width, Text.CalcHeight(desc, width));
            Widgets.Label(descRect, desc);
            return descRect.yMax + 4f;
        }

        private float DrawTargetIfExists(Mission mission, float curY, float rectWidth)
        {
            float nextY = curY;
            var target = mission.MissionTarget;
            if (target != null)
            {
                GlobalTargetInfo lookTarget = new GlobalTargetInfo();
                Rect targetRect = new Rect((rectWidth / 2f) - 16f, nextY, 32f, 32f);
                nextY = targetRect.yMax + 8f;
                if (Widgets.ButtonImage(targetRect, MissionTabUtility.searchReticule))
                {
                    if (target is Pawn)
                    {
                        lookTarget = new GlobalTargetInfo(target as Pawn);
                    }
                    else if (target is WorldObject)
                    {
                        lookTarget = new GlobalTargetInfo(target as WorldObject);
                    }

                    if (Current.ProgramState == ProgramState.Playing && lookTarget.IsValid)
                    {
                        CameraJumper.TryJumpAndSelect(lookTarget);
                    }
                }
            }
            return nextY;
        }

        private static bool canCollectRewards(Mission mission)
        {
            if (mission.Def.RewardMode == MissionRewardMode.SupplyShip)
            {
                return mission.RewardPending && !DropShipUtility.MissingRunways(Find.CurrentMap);
            }
            return mission.RewardPending;
        }

        private bool MissionFilter(Mission mission)
        {
            MissionDef def = mission.Def;
            return !this.curSearchText.NullOrEmpty() && (string.Equals(mission.Def.label, curSearchText, StringComparison.CurrentCultureIgnoreCase) || string.Equals(mission.Def.description, curSearchText, StringComparison.CurrentCultureIgnoreCase) || string.Equals(mission.GiverFaction.Name, curSearchText, StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
