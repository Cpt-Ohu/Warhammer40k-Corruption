using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_CorruptionStatus : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            CompSoul s1 = CompSoul.GetPawnSoul(pawn);
            CompSoul s2 = CompSoul.GetPawnSoul(otherPawn);

            if (s1 != null && s2 != null)
            {
                if (s1.Patron == s2.Patron && s1.CurLevel > 0.3f && s2.CurLevel > 0.3f && !s1.Corrupted && !s2.Corrupted)
                {
                    if ((s1.CurLevel - s2.CurLevel) > 0.3f)
                    {
                        return ThoughtState.ActiveAtStage(0);
                    }
                    if (s1.DevotionTrait.SDegree > 0 && s2.DevotionTrait.SDegree < 0)
                    {
                        return ThoughtState.ActiveAtStage(1);
                    }
                    if (s1.DevotionTrait.SDegree < 0 && s2.DevotionTrait.SDegree > 0)
                    {
                        return ThoughtState.ActiveAtStage(3);
                    }
                    if (s1.DevotionTrait.SDegree < 0 && s2.DevotionTrait.SDegree < 0)
                    {
                        return ThoughtState.ActiveAtStage(4);
                    }
                    return ThoughtState.ActiveAtStage(0);
                }

                if (!s1.Corrupted && !s2.Corrupted && s1.DevotionTrait.SDegree > 0)
                {
                    return ThoughtState.ActiveAtStage(5);
                }
                if (!s1.Corrupted && s2.Corrupted)
                {
                    return ThoughtState.ActiveAtStage(6);
                }
                if (s1.Corrupted && !s2.Corrupted)
                {
                    return ThoughtState.ActiveAtStage(7);
                }
                if (s1.Corrupted && s2.Corrupted)
                {
                    return ThoughtState.ActiveAtStage(8);
                }
                return ThoughtState.Inactive;
            }
            return ThoughtState.Inactive;
        }
    }
}
    

