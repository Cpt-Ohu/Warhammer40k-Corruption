using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_Undivided : ThoughtWorker
    {

        public float ColonyCorruptionAvg;
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            List<Pawn> ColonyPawns = p.Map.mapPawns.FreeColonistsAndPrisonersSpawned.ToList<Pawn>();
            float totalCorruption = 0f;
            foreach (Pawn cpawn in ColonyPawns )
            {
                CompSoul soul = CompSoul.GetPawnSoul(cpawn);
                if (soul != null)
                {
                    totalCorruption += soul.CurLevel;
                }
            }
            ColonyCorruptionAvg = totalCorruption / ColonyPawns.Count;
            switch(ColonyCorruptionCategory)
            {
                case SoulAffliction.Corrupted:
                    return ThoughtState.ActiveAtStage(0);

                case SoulAffliction.Warptouched:
                    return ThoughtState.ActiveAtStage(1);

                case SoulAffliction.Intrigued:
                    return ThoughtState.ActiveAtStage(2);

                case SoulAffliction.Pure:
                    return ThoughtState.ActiveAtStage(3);
                default:
                    return ThoughtState.ActiveAtStage(2);
            }

        }

        public SoulAffliction ColonyCorruptionCategory
        {
            get
            {
                if (this.ColonyCorruptionAvg <= 0.0f)
                {
                    return SoulAffliction.Corrupted;
                }
                if (this.ColonyCorruptionAvg < 0.4f)
                {
                    return SoulAffliction.Warptouched;
                }
                if (this.ColonyCorruptionAvg < 0.7f)
                {
                    return SoulAffliction.Intrigued;
                }
                if (this.ColonyCorruptionAvg < 0.9f)
                {
                    return SoulAffliction.Pure;
                }
                return SoulAffliction.Pure;
            }
        }
    }
}
