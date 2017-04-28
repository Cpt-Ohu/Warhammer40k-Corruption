using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_KhornateNoKill : ThoughtWorker
    {
        public int lastKill = 0;

        private int kdegree = 0;

        public List<Pawn> KilledPawns = new List<Pawn>();

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (Find.TickManager.TicksGame > (lastKill + 600))
            {
                if ((p.needs.mood.thoughts.situational.GetSituationalThoughtsAffectingMood().FindAll((Thought_Situational x) => x.def == GodThoughtDefOf.Khornate_Kill)) == null)
                {
                    if (kdegree < 3)
                    {
                        kdegree += 1;
                    }
                    lastKill = Find.TickManager.TicksGame;
                }
                else
                {
                    lastKill = Find.TickManager.TicksGame;
                    kdegree = 0;
                    return ThoughtState.Inactive;
                }
            }
            switch (kdegree)
            {
                case 0:
                    return ThoughtState.Inactive;
                case 1:
                    return ThoughtState.ActiveAtStage(0);
                case 2:
                    return ThoughtState.ActiveAtStage(1);
                case 3:
                    return ThoughtState.ActiveAtStage(2);
                default:
                    return ThoughtState.Inactive;
            }

        }
    }

}
