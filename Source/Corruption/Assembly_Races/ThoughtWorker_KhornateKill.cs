using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_KhornateKill : ThoughtWorker
    {
        public int timetobattle=1000;

        public float KillCount;

        public float oldKillCount = 0;

        public int KillCountTimed = 0;

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            KillCount = p.records.GetValue(RecordDefOf.Kills) - oldKillCount;

            if (KillCount >= 1)
            {
                KillCountTimed = 0;
            }
            if (KillCount > 4)
            {
                timetobattle += 500;
                KillCountTimed = 1;
            }
            if (KillCount > 8)
            {
                timetobattle += 1000;
                KillCountTimed = 2;
            }

            if(KillCountTimed != 0)
            {
                Log.Message(KillCountTimed.ToString());
                FleetingRush(p);
                return ThoughtState.ActiveAtStage(KillCountTimed);
            }
            else
            {
                FleetingRush(p);
                return ThoughtState.Inactive;
            }
        }

        private void FleetingRush(Pawn pw)
        {
            if (Find.TickManager.TicksGame > timetobattle)
            {
                KillCountTimed = 0;
                oldKillCount = pw.records.GetValue(RecordDefOf.Kills);
                timetobattle = Find.TickManager.TicksGame + 1000;
            }
        }
    }
}
