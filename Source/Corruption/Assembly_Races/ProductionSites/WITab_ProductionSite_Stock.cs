using Corruption.IoM;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public class WITab_ProductionSite_Stock : WITab
    {

        private const float ThingRowHeight = 28f;

        private const float ThingLeftX = 36f;

        private const float StandardLineHeight = 22f;

        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        private static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private List<ResourceEntry> cachedResources = new List<ResourceEntry>();

        private ProductionSite ProductionSite
        {
            get
            {
                return this.SelObject as ProductionSite;
            }
        }
        public WITab_ProductionSite_Stock()
        {
            this.size = new Vector2(512f, 512f);
            this.labelKey = "TabProductionSiteStock";
        }

        public override void OnOpen()
        {
            base.OnOpen();
            CacheResources();
        }

        private void CacheResources()
        {
            this.cachedResources = this.ProductionSite.Stock.OrderBy(x => x.Count).ToList();
            Log.Message(cachedResources.Count.ToString());
        }

        protected override void FillTab()
        {
            this.DrawStock(new Rect(0f,0f, this.size.x, this.size.y));
        }
        private void DrawStock(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Rect rect = inRect.ContractedBy(4f);
            GUI.BeginGroup(rect);
            GUI.color = Color.white;
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(0f, 0f, rect.width, 32f);
            Widgets.Label(titleRect, "Stock".Translate());
            Text.Font = GameFont.Small;
            Rect totalRect = new Rect(0f, titleRect.yMax + 8f, rect.width - 50f, 300f);
            Rect viewRect = new Rect(0f, titleRect.yMax + 8f, rect.width, this.scrollViewHeight);
            Widgets.BeginScrollView(totalRect, ref this.scrollPosition, viewRect);
            float num = 0f;

            if (!this.cachedResources.NullOrEmpty())
            {
                Text.Font = GameFont.Small;
                foreach(var resource in this.cachedResources)
                {
                    ResourceEntry entry = resource;

                    this.DrawThingRow(ref num, viewRect.width, entry);
                }
            }
            this.scrollViewHeight = num + 30f;
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawThingRow(ref float y, float width, ResourceEntry entry)
        {
            Rect rect = new Rect(0f, y, width, 28f);
            Widgets.InfoCardButton(rect.width - 24f, y, entry.Def);
            rect.width -= 24f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = WITab_ProductionSite_Stock.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (entry.Def.DrawMatSingle != null && entry.Def.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), entry.Def);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = WITab_ProductionSite_Stock.ThingLabelColor;
            Rect rect3 = new Rect(36f, y, width - 36f, 28f);
            string text = string.Concat(entry.Count, " x ", entry.Def.LabelCap);

            Widgets.Label(rect3, text);
            y += 32f;
        }
    }
}

