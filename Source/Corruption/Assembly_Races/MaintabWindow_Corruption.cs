using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class MaintabWindow_Corruption : MainTabWindow
    {
        private Missions.MissionTabUtility missionTab = new Missions.MissionTabUtility();
        private Worship.WorshipTabWindow worshipTab = new Worship.WorshipTabWindow();


        private enum Tab
        {
            Conflict,
            Worship,
            Missions,
            Diplomacy
        }

        private MaintabWindow_Corruption.Tab tab;
        private static List<TabRecord> tabsList = new List<TabRecord>();

        public override void PreOpen()
        {
            Worship.WorshipTabWindow.SelectedGod = CFind.WorshipTracker.PlayerGod;
            base.PreOpen();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(0f, 32f, inRect.width, 40f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, "CorruptionOverView".Translate());
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            MaintabWindow_Corruption.tabsList.Clear();
            MaintabWindow_Corruption.tabsList.Add(new TabRecord("ConflictTab".Translate(), delegate
            {
                this.tab = MaintabWindow_Corruption.Tab.Conflict;
            }, this.tab == MaintabWindow_Corruption.Tab.Conflict));
            MaintabWindow_Corruption.tabsList.Add(new TabRecord("WorshipTab".Translate(), delegate
            {
                this.tab = MaintabWindow_Corruption.Tab.Worship;
            }, this.tab == MaintabWindow_Corruption.Tab.Worship));

            MaintabWindow_Corruption.tabsList.Add(new TabRecord("MissionTab".Translate(), delegate
            {
                this.tab = MaintabWindow_Corruption.Tab.Missions;
            }, this.tab == MaintabWindow_Corruption.Tab.Missions));

            MaintabWindow_Corruption.tabsList.Add(new TabRecord("DiplomacyTab".Translate(), delegate
            {
                this.tab = MaintabWindow_Corruption.Tab.Diplomacy;
            }, this.tab == MaintabWindow_Corruption.Tab.Diplomacy));
            inRect.yMin += 72f;
            Widgets.DrawMenuSection(inRect);
            TabDrawer.DrawTabs(inRect, MaintabWindow_Corruption.tabsList);
            inRect = inRect.ContractedBy(17f);
            if (this.tab == Tab.Conflict)
            {
                Domination.DominationCardUtility.DrawConflictTab(inRect);
            }
            else if (this.tab == Tab.Worship)
            {
                this.worshipTab.DoWindowContents(inRect);
            }
            else if (this.tab == Tab.Missions)
            {
                this.missionTab.DoWindowContents(inRect);
            }
            else
            {

            }


            if (Widgets.CloseButtonFor(inRect.AtZero()))
            {
                Close();
            }
        }
    }
}
