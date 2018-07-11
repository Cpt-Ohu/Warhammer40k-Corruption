using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_Xenos : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn otherPawn)
        {
            CompSoul s1 = CompSoul.GetPawnSoul(pawn);
            CompSoul s2 = CompSoul.GetPawnSoul(otherPawn);

            if (s1 != null && s2 != null)
            {
                if (s1.CulturalTolerance == CulturalToleranceCategory.Xenophobe)
                {
                    if (pawn.kindDef.race != otherPawn.kindDef.race)
                    {
                        return ThoughtState.ActiveAtStage(1);
                    }
                    return ThoughtState.ActiveAtStage(0);
                }
                if (s1.CulturalTolerance == CulturalToleranceCategory.Xenophile)
                {
                    if(pawn.kindDef.race != otherPawn.kindDef.race)
                    {
                        return ThoughtState.ActiveAtStage(2);
                    }
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
