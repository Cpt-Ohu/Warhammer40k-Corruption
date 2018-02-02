using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_Khornate : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(p);
            if (soul != null)
            {
                PawnKillTracker tracker = soul.PawnKillTracker;

                if (tracker.lastKillTick <= 0)
                {
                    if (tracker.lastKillTick < -2000)
                    {
                        return ThoughtState.ActiveAtStage(0);
                    }
                    if (tracker.lastKillTick < -400)
                    {
                        return ThoughtState.ActiveAtStage(1);
                    }
                    if (tracker.lastKillTick < -150)
                    {
                        return ThoughtState.ActiveAtStage(2);
                    }
                    return ThoughtState.Inactive;                                  
                }
                else
                {
                    if (tracker.curKillCount < 7)
                    {
                        return ThoughtState.ActiveAtStage(3);
                    }
                    if (tracker.curKillCount < 16)
                    {
                        return ThoughtState.ActiveAtStage(4);
                    }
                    return ThoughtState.ActiveAtStage(5);
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
