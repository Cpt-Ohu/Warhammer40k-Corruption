using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Tithes
{
    [StaticConstructorOnStartup]
    public class Window_IoMTitheDue : Window
    {
        private static Texture RebellionIoMTexture = ContentFinder<Texture2D>.Get("UI/Images/Rebellion", true);

        private bool ChoseOption;

        private CorruptionStoryTracker tracker;

        public Window_IoMTitheDue(CorruptionStoryTracker tracker)
        {
            this.tracker = tracker;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(600f, 500f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "IoMTitheDueTitle".Translate());
            Text.Font = GameFont.Small;

            Rect imageRect = new Rect(inRect.x + (inRect.width / 2) - 64f, titleRect.yMax + 5f, 128f, 128f);
            GUI.DrawTexture(imageRect, CorruptionStoryTrackerUtilities.Aquila);

            Rect descRect = new Rect(titleRect.x, imageRect.yMax + 10f, inRect.width, 150f);
            string text = "IoMTitheDueDesc".Translate(new object[]
            {
                Find.World.info.name,
                tracker.PlanetaryGovernor
            });

            Widgets.Label(descRect, text);
            Rect rectA = new Rect(0f, descRect.yMax + 10f, 100f, 30f);
            Rect rectB = new Rect(inRect.xMax - 100f, descRect.yMax + 10f, 100f, 30f);

            bool possible = tracker.currentTithes.All(x => x.tithePercent >= 0.99f);

            if (!possible)
            {
                GUI.color = Color.red;
            }
            if (Widgets.ButtonText(rectA, "PayTithe".Translate(), true, false, possible))
            {
                this.ChoseOption = true;
                tracker.CollectTithes();
                this.Close();
            }

            GUI.color = Color.white;
            if (Widgets.ButtonText(rectB, "RefuseTithe".Translate()))
            {
                this.ChoseOption = true;
                Find.WindowStack.Add(new Window_RefusedTithePayment(tracker));
                this.Close();
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            if (!this.ChoseOption)
            {
                Find.WindowStack.Add(new Window_RefusedTithePayment(tracker));
            }
            base.Close(doCloseSound);
        }

        internal sealed class Window_RefusedTithePayment : Window
        {

            private CorruptionStoryTracker tracker;

            public Window_RefusedTithePayment(CorruptionStoryTracker tracker)
            {
                this.tracker = tracker;
            }

            public override Vector2 InitialSize
            {
                get
                {
                    return new Vector2(400f, 400f);
                }
            }

            public override void DoWindowContents(Rect inRect)
            {
                Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 30f);
                Widgets.Label(titleRect, "RebellionIoM".Translate());
                Rect descRect = new Rect(inRect.x, titleRect.yMax, inRect.width, 150f);
                Widgets.Label(descRect, "RebellionIoMDesc".Translate());

                Rect imageRect = new Rect(inRect.x, descRect.yMax, 384f, 128f);
                GUI.DrawTexture(imageRect, RebellionIoMTexture);

                if (Widgets.CloseButtonFor(inRect.AtZero()))
                {
                    this.Close();
                }
            }

            public override void PostClose()
            {
                Tithes.TitheUtilities.PerformRefusedTitheEvaluation();
                base.PostClose();
            }
        }
    }
}
