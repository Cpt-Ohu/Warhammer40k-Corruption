using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{

    public class WorkForceWidget
    {
        internal sealed class WorkForceWidgetEntry
        {
            public WorkForce WorkForce;

            public WorkForce Unemployed;

            public int EditCount { get; set; }

            public string EditBuffer { get; set; }

            public int MaxCount
            {
                get
                {
                    return this.WorkForce.WorkerCount + this.Unemployed.WorkerCount;
                }
            }

            public WorkForceWidgetEntry(WorkForce workForce, WorkForce unemployed)
            {
                WorkForce = workForce;
                Unemployed = unemployed;
                this.EditCount = WorkForce.WorkerCount;
            }

            public bool AdjustWorkForce(WorkForceWidgetEntry entry, int currentCount)
            {
                int offset = currentCount - this.EditCount;
                if (offset == 0)
                {
                    return false;
                }
                if (offset > 0)
                {
                    WorkForce.TransferWorkers(this.Unemployed, this.WorkForce, Math.Abs(offset), true);
                }
                else
                {
                    WorkForce.TransferWorkers(this.WorkForce, this.Unemployed, Math.Abs(offset), false);
                }
                this.EditCount = this.WorkForce.WorkerCount;
                return true;
            }
        }

        public ProductionSite ProductionSite;

        public ResourceProductionComp Production;

        private List<WorkForceWidgetEntry> TotalWorkForce = new List<WorkForceWidgetEntry>();

        private int CurrentCount;
        private Vector2 scrollPositionWorkForce;

        public WorkForceWidget(ProductionSite site)
        {
            this.ProductionSite = site;
        }

        public void SetProduction(ResourceProductionComp productionComp)
        {
            this.Production = productionComp;
            this.CacheWorkForce();
        }

        private void CacheWorkForce()
        {
            this.TotalWorkForce.Clear();
            foreach (var worker in this.Production.WorkForce)
            {
                WorkForce workForce = WorkForce.GetOrCreateWorkForce(ref this.ProductionSite.UnemployedWorkForce, worker.PawnKind);
                this.TotalWorkForce.Add(new WorkForceWidgetEntry(worker, workForce));
            }

            foreach (var unemployed in this.ProductionSite.UnemployedWorkForce)
            {
                WorkForceWidgetEntry entry = this.TotalWorkForce.FirstOrDefault(x => x.WorkForce.PawnKind == unemployed.PawnKind);
                if (entry == null)
                {
                    WorkForce potentialWorkers = new WorkForce(unemployed.PawnKind);
                    this.TotalWorkForce.Add(new WorkForceWidgetEntry(potentialWorkers, unemployed));
                }
            }
        }

        public void DoOnGUI(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Rect titleRect = new Rect(0f, 0f, inRect.width, Text.LineHeight);
            Widgets.Label(titleRect, "WorkForce".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, this.TotalWorkForce.Count * Text.LineHeight * 4f);
            Rect totalRect = new Rect(0f, titleRect.yMax + 4f, inRect.width, inRect.height - titleRect.yMax);
            Widgets.BeginScrollView(totalRect, ref scrollPositionWorkForce, viewRect);
            float num = 0f;
            foreach (var entry in this.TotalWorkForce)
            {
                Rect entryRect = new Rect(0f, num, viewRect.width, 100f);
                Widgets.DrawOptionBackground(entryRect, false);
                Text.Anchor = TextAnchor.UpperCenter;
                Rect nickRect = new Rect(0f, num, inRect.width, Text.LineHeight);
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(nickRect, entry.WorkForce.PawnKind.LabelCap);
                Rect countRect = new Rect(32f, nickRect.yMax + 5f, viewRect.xMax - 64f, Text.LineHeight);
                int currentCount = entry.EditCount;
                string buffCount = entry.EditBuffer;
                Widgets.TextFieldNumeric(countRect, ref currentCount, ref buffCount, 0, entry.MaxCount);
                Rect addRect = new Rect(0f, countRect.y, 30f, countRect.height);
                Rect subRect = new Rect(countRect.xMax + 2f, countRect.y, 30f, countRect.height);
                if (Widgets.ButtonText(addRect, "+"))
                {
                    currentCount++;
                }
                if (Widgets.ButtonText(subRect, "-"))
                {
                    currentCount--;
                }
                if (entry.AdjustWorkForce(entry, currentCount))
                {
                    break;
                }

                Rect skillRect = new Rect(countRect);
                skillRect.y = countRect.yMax + 4f;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(skillRect, "AverageWorkSkill".Translate(entry.WorkForce.AverageSkill.ToString()));
                Rect potentialsRect = new Rect(skillRect);
                potentialsRect.y = skillRect.yMax + 1f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(potentialsRect, "AvailableWorkerCount".Translate(entry.Unemployed.WorkerCount));
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                num = entryRect.yMax + 2f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

      
    }
}

