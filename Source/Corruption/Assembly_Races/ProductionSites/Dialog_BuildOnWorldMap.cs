using Corruption.Domination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public abstract class Dialog_BuildOnWorldMap : Window
    {
        protected static Color ExpansionColor = new Color(0.53f, 0.81f, 1f);

        protected bool CanBuild = true;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(400f, 300f);
            }
        }

        protected Def DefToBuild;

        protected abstract IWorldObjectBuilder Builder { get; }

        protected abstract IEnumerable<ThingDefCountClass> Cost { get; }

        protected virtual float CostRectHeight
        {
            get
            {
                return 32f;
            }
        }

        protected virtual string BuildActionString
        {
            get
            {
                return "BuildCommand".Translate();
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(0f, 0f, inRect.width, Text.LineHeight);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(titleRect, "BuildOnWorldMap".Translate(this.DefToBuild.label));
            float curY = this.DrawInfo(titleRect.yMax + 8f, inRect.width);

            Rect costLabel = new Rect(0f, curY, inRect.width, Text.LineHeight);
            Widgets.Label(costLabel, "ResourceCost".Translate());
            Rect costRect = new Rect(0f, costLabel.yMax + 2f, inRect.width, this.CostRectHeight);
            this.DrawCostInfo(costRect.ContractedBy(2f));

            Rect buttonRect = new Rect((inRect.xMax - 256f)/ 2f, inRect.yMax - 52f, 256f, 48f);
            if (Widgets.ButtonText(buttonRect, this.BuildActionString, true, true, this.CanBuild))
            {
                this.Build();
                this.Close();
            }

            if (Widgets.CloseButtonFor(inRect.AtZero())) this.Close();
        }

        protected abstract void Build();

        protected abstract void DrawCostInfo(Rect costRect);

        protected virtual float DrawInfo(float curY, float width)
        {
            Rect descRect = new Rect(0f, curY, width, Text.LineHeight * 3f);
            Text.Font = GameFont.Tiny;
            Widgets.TextArea(descRect, this.DefToBuild.description);
            return descRect.yMax;
        }
    }
}
