using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Tithes
{
    public class GameCondition_TitheCollectors : GameCondition
    {
        public override void End()
        {
            Window_IoMTitheDue window = new Window_IoMTitheDue(CFind.StoryTracker);
            Find.WindowStack.Add(window);
            this.gameConditionManager.ActiveConditions.Remove(this);
        }
    }
}
