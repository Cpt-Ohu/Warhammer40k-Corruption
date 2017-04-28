﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Tithes
{
    public class MapCondition_TitheCollectors : MapCondition
    {
        public override void End()
        {
            Window_IoMTitheDue window = new Window_IoMTitheDue(CorruptionStoryTrackerUtilities.currentStoryTracker);
            Find.WindowStack.Add(window);
            this.mapConditionManager.ActiveConditions.Remove(this);
        }
    }
}
