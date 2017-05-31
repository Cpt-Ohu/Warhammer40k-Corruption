using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Tithes
{
    public class ITab_TitheContainer : ITab
    {
        private List<TitheEntryGlobal> titheEntries;
        
        private List<bool> titheEnabled;

        public ITab_TitheContainer()
        {
            this.size = new Vector2(400f, 500f);
            this.labelKey = "TabTitheContainer";
        }

        private TitheContainer titheContainer
        {
            get
            {
                return this.SelThing as TitheContainer;
            }
        }

        private Vector2 scrollPosition = Vector2.zero;

        public override void OnOpen()
        {
            //       TitheUtilities.CalculateColonyTithes(CorruptionStoryTrackerUtilities.currentStoryTracker);
            this.titheEntries = this.titheContainer.tithesEnabled.Keys.ToList();
            this.titheEnabled = this.titheContainer.tithesEnabled.Values.ToList();
            base.OnOpen();
        }

        protected override void FillTab()
        {
            Rect mainrect = new Rect(0f, 80f, this.size.x, this.size.y);
     //       Log.Message(CorruptionStoryTrackerUtilities.currentStoryTracker.currentTithes.Count.ToString());
            Rect rect = new Rect(0f, 0f, this.size.x, 30f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, "AllowedTithes".Translate());
            Rect rect5 = new Rect(30f, 35f, 200f, 25f);
            Widgets.FillableBar(rect5, titheContainer.massUsage, TitheUtilities.TitheBarFillTex, TitheUtilities.TitheBarBGTex, true);
            Text.Font = GameFont.Small;
            Widgets.Label(rect5, "MassUsage".Translate());
            if (Prefs.DevMode)
            {
                Rect debugRect = new Rect(rect5.xMax + 5f, rect5.y, 100f, 20f);
                if (Widgets.ButtonText(debugRect, "Debug: FillTithes"))
                {
                    for (int i = 0; i < titheContainer.currentTitheEntries.Count; i++)
                    {
                        TitheEntryGlobal entry = titheContainer.currentTitheEntries[i].Tithe;
                        Thing stuff = ThingMaker.MakeThing(entry.thingDefs.RandomElement());
                        stuff.stackCount = (int)((1 - entry.tithePercent) * entry.requestedTitheAmount / stuff.def.BaseMarketValue);
                        titheContainer.GetDirectlyHeldThings().TryAdd(stuff);
                    }
                }
            }

            Rect rect2 = new Rect(0f, rect5.yMax + 50f, rect.width, 450f).ContractedBy(10f);
            Rect tithesRect = new Rect(0f, rect2.y, rect.width, 900f);
            Widgets.BeginScrollView(mainrect, ref this.scrollPosition, rect2);
            float num = rect2.y + 5f;
      //      Log.Message("tithes enabled" + this.titheContainer.tithesEnabled.Count());
      //      foreach (KeyValuePair<TitheEntry, bool> current in this.titheContainer.tithesEnabled)
            for (int i = 0; i < titheContainer.currentTitheEntries.Count; i++)
            {
                Rect rect3 = new Rect(30f, num, 200f, 25f);
                Widgets.CheckboxLabeled(rect3, titheContainer.currentTitheEntries[i].Tithe.titheDef.LabelCap, ref titheContainer.currentTitheEntries[i].active, false);
                Rect rect4 = new Rect(40f, num + 30f, 200f, 30f);
                Widgets.FillableBar(rect4, titheEntries[i].tithePercent, TitheUtilities.TitheBarFillTex, TitheUtilities.TitheBarBGTex, true);
                Widgets.Label(rect4, titheContainer.currentTitheEntries[i].Tithe.collectedTitheAmount + " / " + titheContainer.currentTitheEntries[i].Tithe.requestedTitheAmount);
                num += 75f;
            }
            Widgets.EndScrollView();

            Text.Anchor = TextAnchor.UpperLeft;

        }
    }
}
