using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class Window_IoMAcknowledgement : Window
    {
        private CorruptionStoryTracker tracker;

        public Window_IoMAcknowledgement(CorruptionStoryTracker storyTracker)
        {
            this.tracker = storyTracker;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 500f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "IoMAcknowledgementCongrats");
            Text.Font = GameFont.Small;

            Rect descRect = new Rect(titleRect.yMax + 50f, titleRect.x, inRect.width, 300f);
            string text = "IoMAcknowledgementDesc".Translate(new object[]
            {
                Find.World.info.name,
                tracker.PlanetaryGovernor
            });
        }
    }
}
