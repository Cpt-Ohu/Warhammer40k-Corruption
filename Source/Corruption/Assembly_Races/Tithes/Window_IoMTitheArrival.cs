using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Corruption.Tithes
{
    public class Window_IoMTitheArrival : Window
    {
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(400f, 280f);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.LetterArriveBadUrgent);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect);
            titleRect.height = 30f;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "LetterLabelTithesDue".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            Rect descRect = new Rect(titleRect.x, titleRect.yMax + 30f, inRect.width, 70f);
            Widgets.Label(descRect, C_MapConditionDefOf.TitheCollectorArrived.description);
            Rect imageRect = new Rect(titleRect.x, descRect.yMax + 5f, 384f, 128f);
            GUI.DrawTexture(imageRect, CorruptionStoryTrackerUtilities.ShipsArrival);
            if (Widgets.CloseButtonFor(inRect.AtZero()))
            {
                this.Close();
            }
        }
    }
}
