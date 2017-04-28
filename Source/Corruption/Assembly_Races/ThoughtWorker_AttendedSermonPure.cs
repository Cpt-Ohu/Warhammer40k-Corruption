using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_AttendedSermonPure : ThoughtWorker
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
            Need_Soul s2 = otherPawn.needs.TryGetNeed<Need_Soul>();

            if (s2.NoPatron == false) flag = false;

            if (flag)
            {
                if (s1.DevotionTrait.SDegree == -2)
                {
                    if (p.IsPrisonerOfColony)
                    {
                        return ThoughtState.ActiveAtStage(4);
                    }

                    return ThoughtState.ActiveAtStage(3);
                }
                else if (s1.DevotionTrait.SDegree == -1)
                {
                    if (movingSermon(otherPawn))
                    {
                        return ThoughtState.ActiveAtStage(2);
                    }
                    return ThoughtState.ActiveAtStage(0);
                }
                else
                {
                    if (movingSermon(otherPawn))
                    {
                        return ThoughtState.ActiveAtStage(2);
                    }
                    return ThoughtState.ActiveAtStage(1);
                }
            }
            else
            {
                if (p.IsPrisonerOfColony)
                {
                    return ThoughtState.ActiveAtStage(8);
                }
                if (s1.DevotionTrait.SDegree > 0)
                {
                    return ThoughtState.ActiveAtStage(7);
                }
                return ThoughtState.ActiveAtStage(6);
            }
            }
        }    
}
