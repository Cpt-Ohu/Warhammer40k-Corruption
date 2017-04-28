﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class MapCondition_CorruptiveDrone : MapCondition
    {
        private SkyColorSet CorruptiveDroneColors;

        public MapCondition_CorruptiveDrone()
        {
            ColorInt colorInt = new ColorInt(240, 141, 74);
            ColorInt colorInt2 = new ColorInt(254, 245, 176);
            ColorInt colorInt3 = new ColorInt(170, 95, 60);
            this.CorruptiveDroneColors = new SkyColorSet(colorInt.ToColor, colorInt2.ToColor, colorInt3.ToColor, 1.0f);
        }

        public override SkyTarget? SkyTarget()
        {
            return new SkyTarget?(new SkyTarget(this.CorruptiveDroneColors)
            {
                glow = 0.85f
            });
        }

    }
}
