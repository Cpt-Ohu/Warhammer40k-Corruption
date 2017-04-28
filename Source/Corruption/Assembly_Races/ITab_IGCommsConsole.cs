using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class ITab_IGCommsConsole : ITab
    {
        public Building_CommsConsoleIG console
        {
            get
            {
                return this.SelThing as Building_CommsConsoleIG;
            }
        }

        public ITab_IGCommsConsole()
        {
            this.size = new Vector2(800f, 500f);
            this.labelKey = "TabIGConsole";
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(5f);
            CorruptionStoryTrackerUtilities.DrawCorruptionStoryTrackerTab(console.corruptionStoryTracker, rect);
        }
    }
}
