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
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            Need_Soul s1 = p.needs.TryGetNeed<Need_Soul>();
            Need_Soul s2 = otherPawn.needs.TryGetNeed<Need_Soul>();

            if (s1 != null && s2 != null)
            {
                if (s1.CulturalTolerance == CulturalToleranceCategory.Xenophobe)
                {
                    if (p.kindDef.race != otherPawn.kindDef.race)
                    {
                        return ThoughtState.ActiveAtStage(1);
                    }
                    return ThoughtState.ActiveAtStage(0);
                }
                if (s1.CulturalTolerance == CulturalToleranceCategory.Xenophile)
                {
                    if(p.kindDef.race != otherPawn.kindDef.race)
                    {
                        return ThoughtState.ActiveAtStage(2);
                    }
                }
            }
            return ThoughtState.Inactive;
        }
    }
}
