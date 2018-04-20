using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_AttendedSermonDark : ThoughtWorker
    {
        private bool movingSermon(Pawn pr)
        {
            var f = pr.skills.GetSkill(SkillDefOf.Social).Level;
            int x = Rand.RangeInclusive(0, 35);
            if ((x + f * 2) > 40)
            {
                return true;
            }
            return false;
        }

        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            bool flag = true;

            Need_Soul s1 = p.needs.TryGetNeed<Need_Soul>();
            Need_Soul s2 = p.needs.TryGetNeed<Need_Soul>();

            if (s2.NotCorrupted == true) flag = false;

            if (flag)
            {
                if (movingSermon(otherPawn))
                {
                    return ThoughtState.ActiveAtStage(3);
                }
                return ThoughtState.ActiveAtStage(2);
            }
            else
            {
                if (p.IsPrisonerOfColony)
                {
                    return ThoughtState.ActiveAtStage(1);
                }
                return ThoughtState.ActiveAtStage(0);
            }
            }
        }    
}
