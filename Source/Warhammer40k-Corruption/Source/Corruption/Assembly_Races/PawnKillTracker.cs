using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class PawnKillTracker : IExposable
    {        
        public int lastKillTick;

        public float curKillCount;

        public int oldKillCount;

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.lastKillTick, "lastKillTick");
            Scribe_Values.Look(ref this.curKillCount, "curKillCount", 0f);
            Scribe_Values.Look(ref this.oldKillCount, "oldKillCount");
        }
    }
}
