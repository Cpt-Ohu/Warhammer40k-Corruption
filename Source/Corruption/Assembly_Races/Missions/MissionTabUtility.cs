using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Corruption.Missions
{
    public class MissionTabUtility : MainTabWindow
    {
        private float scrollViewHeight = 380f;
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 scrollPositionWonder = Vector2.zero;
        private static Texture2D tex = SolidColorMaterials.NewSolidColorTexture(Color.cyan);
        private string curSearchText = "";

        private static MissionManager manager
        {
            get
            {
                return CorruptionStoryTrackerUtilities.CurrentMissionManager;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect searchRect = inRect.ContractedBy(16f);
            searchRect.height = 64f;
            Widgets.TextField(searchRect, this.curSearchText);
            inRect.y += 96f;
            GUI.BeginGroup(inRect);

            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0, 48f, inRect.width, inRect.height - 50f);
            Rect rect = new Rect(inRect.x, 0f, inRect.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, rect, true);
            GUI.DrawTexture(outRect, Widgets.ButtonSubtleAtlas);
            float curY = 4f;
            foreach (var mission in manager.Missions)
            {
                DrawMission(mission, rect, ref curY, ref scrollPosition);
            }
            GUI.EndGroup();
        }

        private static void DrawMission(Mission mission, Rect missionRect, ref float curY, ref Vector2 scrollPosition)
        {
            Rect inRect = missionRect.ContractedBy(8f);
            inRect.y = curY;
            Rect TitleRect = new Rect(0, curY, inRect.width, 64f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(TitleRect, mission.Def.label);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.DrawLineHorizontal(0f, TitleRect.yMax, inRect.width);

            Rect iconRect = new Rect(missionRect.x,TitleRect.y, 32, 32);

            Rect textRect = new Rect(missionRect.x, TitleRect.yMax + 8f, inRect.width, 128f);
            Widgets.TextAreaScrollable(textRect, mission.Def.description, ref scrollPosition, true);
            Rect perequisites = textRect;
            perequisites.y = textRect.yMax + 8f;
            perequisites.height = 128f;
            //GUI.DrawTexture(perequisites, MissionTabUtility.tex);
            Listing_Standard Listing_Perequisites = new Listing_Standard();
            Listing_Perequisites.Begin(perequisites);
            foreach (var perequisite in mission.Def.PerequisiteMissions)
            {
                Rect curRect = Listing_Perequisites.GetRect(24f);
                Widgets.Label(curRect, perequisite.label);
                Texture2D image = Widgets.CheckboxOffTex;
                if (manager.PerequisitesFulfilled(perequisite))
                {
                    image = Widgets.CheckboxOnTex;
                }
                Rect position = new Rect(curRect.x + curRect.width - 24f, curRect.y, 24f, 24f);
                GUI.DrawTexture(position, image);
            }
            Listing_Perequisites.End();
            Rect rewardLabelRect = perequisites;
            rewardLabelRect.y = perequisites.yMax + 18f;
            Widgets.Label(rewardLabelRect, "Rewards".Translate());

            Rect RewardRect = rewardLabelRect;
            RewardRect.y += 16f;
            RewardRect.x += 16f;
            RewardRect.height = 128f;
            Listing_Standard Listing_Rewards = new Listing_Standard();
            Listing_Rewards.Begin(RewardRect);
            foreach (var supply in mission.Def.RewardedSupplies)
            {
                foreach (var item in supply.Supplies)
                {
                    Listing_Rewards.Label(item.Count.ToString() + "x " + item.Def.label);
                }
            }
            foreach (var research in mission.Def.RewardedResearch)
            {
                Listing_Rewards.Label(research.label);
            }
            Listing_Rewards.End();

            Rect getRewardRect = Listing_Rewards.GetRect(64f);
            if (mission.Finished)
            {
                if (mission.RewardPending)
                {
                    if (Widgets.CustomButtonText(ref getRewardRect, "GetReward".Translate(), Color.green, Color.white, Color.black))
                    {
                        mission.Finish(Find.AnyPlayerHomeMap);
                    }
                }
                else
                {
                    Widgets.DrawMenuSection(getRewardRect);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(getRewardRect, "Finished".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            curY = getRewardRect.yMax;
        }
    }
}
