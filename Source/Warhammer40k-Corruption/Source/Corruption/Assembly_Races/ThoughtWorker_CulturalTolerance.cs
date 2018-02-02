using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_CulturalTolerance : ThoughtWorker
    {
        private void AliensPresent(Pawn p, out int peeps, out int foreigners)
        {
            foreigners = 0;
            peeps = 0;
            List<Pawn> templist = p.Map.mapPawns.AllPawnsSpawned.Where(x => x.def.race.Humanlike).ToList<Pawn>();
            foreach (Pawn m in templist)
            {
                if (m.kindDef.race != p.kindDef.race && p.Faction == m.Faction)
                {
                    foreigners++;
                }
                else
                {
                    peeps++;
                }
            }
            return;
        }

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            Need_Soul soul = p.needs.TryGetNeed<Need_Soul>();

            if (soul != null && soul.CulturalTolerance != CulturalToleranceCategory.Neutral)
            {
                int peeps;
                int foreigners;
                this.AliensPresent(p, out peeps, out foreigners);

                if (soul.CulturalTolerance == CulturalToleranceCategory.Xenophobe)
                {

                    if (foreigners > peeps)
                    {
                        return ThoughtState.ActiveAtStage(2);
                    }
                    else if(foreigners >0)
                    {
                        return ThoughtState.ActiveAtStage(1);
                    }
                    return ThoughtState.ActiveAtStage(0);
                }

                if (soul.CulturalTolerance == CulturalToleranceCategory.Xenophile)
                {
                    if (foreigners > 0)
                    {
                        return ThoughtState.ActiveAtStage(4);
                    }
                    return ThoughtState.ActiveAtStage(3);
                }
            }

            return ThoughtState.Inactive;
        }
    }
}

