using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class GameCondition_EmperorPositive : GameCondition
    {
        public SkyColorSet WonderColor;

        public GameCondition_EmperorPositive()
        {
            ColorInt colorInt = new ColorInt(216, 255, 0);
            Color arg_50_0 = colorInt.ToColor;
            ColorInt colorInt2 = new ColorInt(PatronDefOf.Emperor.MainColor);
            this.WonderColor = new SkyColorSet(arg_50_0, colorInt2.ToColor, new Color(0.6f, 0.8f, 0.5f), 0.85f);
        }


    }
}
