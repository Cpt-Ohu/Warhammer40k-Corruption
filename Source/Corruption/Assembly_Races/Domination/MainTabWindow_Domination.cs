using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Domination
{
    [StaticConstructorOnStartup]
    public class DominationCardUtility
    {

        private static Texture2D WearinessBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.8f, 0f, 0f, 0.8f));

        private static Vector2 scrollPosition = Vector2.zero;

        private static float scrollViewHeight;
        
        public static void DrawConflictTab(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            Rect position = new Rect(0f, 0f, inRect.width, inRect.height);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);
            Rect rect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, rect, true);
            float num = 0f;
            foreach (DominationConflict current in CFind.StoryTracker.DominationTracker.AllConflicts)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Widgets.DrawLineHorizontal(0f, num, rect.width);
                GUI.color = Color.white;
                num += DrawConflictEntry(num, rect.width, current);
            }
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = num;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private static float DrawConflictEntry(float curY, float width, DominationConflict conflict)
        {
            float mainRectHeight = ConflictEntryHeight(conflict);
            Rect mainRect = new Rect(0f, curY, width, mainRectHeight);
            Widgets.DrawOptionUnselected(mainRect);
            Rect titleRect = new Rect(mainRect);
            titleRect.height = 64f;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, conflict.ConflictName);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            Rect firstRect = new Rect(mainRect.ContractedBy(4f));
            firstRect.y = titleRect.yMax + 4f;
            firstRect.height -= 68f;
            firstRect.width = (mainRect.width / 2f) - 68f;
            float firstY = 0f;
            DrawAllianceRect(firstRect, conflict.First, conflict, out firstY);

            Rect vsRect = new Rect((mainRect.width / 2f) - 64f, firstRect.y, 128f, 32f); //new Rect(mainRect.width / 2 - 64f, curY, mainRect.width, mainRect.height);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(vsRect, "VS");
            Text.Anchor = TextAnchor.UpperLeft;
            Rect secRect = new Rect(firstRect);
            secRect.x = firstRect.xMax + 124f;
            float secY = 0f;
            DrawAllianceRect(secRect, conflict.Second, conflict, out secY);

            firstRect.height = curY + Math.Max(firstY, secY) + 8f;

            return firstRect.height + 8f;
        }

        private static void DrawAllianceRect(Rect inRect, PoliticalAlliance alliance, DominationConflict conflict, out float height)
        {
            Rect fullRect = inRect.ContractedBy(4f);
            Widgets.DrawOptionBackground(inRect, false);
            Rect labelRect = new Rect(fullRect.x, fullRect.y, inRect.width, 32f);
            Text.Anchor = TextAnchor.UpperCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(labelRect, alliance.AllianceName);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            float facY = labelRect.yMax + 4f;
            foreach (Faction faction in alliance.GetFactions())
            {
                Rect factionRect = new Rect(fullRect.x, facY, inRect.width, 32f);
                Widgets.Label(factionRect, faction.Name);
                Widgets.InfoCardButton(factionRect.xMax - 26f, factionRect.y, faction.def);
                facY += 34f;
            }

            float warY = facY;
            Rect wearinessRect = new Rect(fullRect.x, warY, fullRect.width - 8f, 28f);

            AllianceWarEffort effort = conflict.GetWarEffort(alliance);
            if (effort != null)
            {
                Widgets.FillableBar(wearinessRect, effort.WarWeariness, DominationCardUtility.WearinessBarFillTex, BaseContent.BlackTex, true);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(wearinessRect, "WarWeariness".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                wearinessRect.y += 32f;
                string casualties = "ConflictCasualties".Translate(new object[] { effort.totalCasualties.ToString() });
                Widgets.Label(wearinessRect, casualties);
            }
            height = wearinessRect.yMax;
        }

        private static float ConflictEntryHeight(DominationConflict conflict)
        {

            int maxAllianceCount = Math.Max(conflict.First.GetFactions().Count, conflict.Second.GetFactions().Count);
            return 196f + maxAllianceCount * 34f;
        }

    }
}
