using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class ITab_FactionColor : ITab
    {
        public ITab_FactionColor()
        {
            this.size = new Vector2(400f, 250f);
            this.labelKey = "TabCoA";
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(5f);
            FactionColorUtilities.DrawFactionColorTab(rect);
        }
    }
}
