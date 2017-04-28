using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class CorruptionUtilities
    {
        public static void PrayerTickCheckEnd(Pawn pawn)
        {
            var curJob = pawn.CurJob;

            var soul = pawn.needs.TryGetNeed<Need_Soul>();
            if (soul == null)
            {
                return;
            }

            soul.GainNeed(curJob.def.joyGainRate);

            if (!pawn.GetTimeAssignment().allowJoy)
            {
                pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);
            }
            if ((double)soul.CurLevel < 0.999899983406067)
            {
                return;
            }
            pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
        }

        public static void SermonTickCheckEnd(Pawn pawn, BuildingAltar altar)
        {
            var curJob = pawn.CurJob;

            var soul = pawn.needs.TryGetNeed<Need_Soul>();
            if (soul == null)
            {
                return;
            }

            Room temple = altar.GetRoom();
            float num = 1 + CommonMisc.Radar.FindAllPawnsInRoomCount(temple)/10;
            
            soul.GainNeed(curJob.def.joyGainRate * num);

            if (!pawn.GetTimeAssignment().allowJoy)
            {
                pawn.jobs.curDriver.EndJobWith(JobCondition.InterruptForced);
            }
            if ((double)soul.CurLevel < 0.999899983406067)
            {
                return;
            }
            pawn.jobs.curDriver.EndJobWith(JobCondition.Succeeded);
        }
        
    }
}
