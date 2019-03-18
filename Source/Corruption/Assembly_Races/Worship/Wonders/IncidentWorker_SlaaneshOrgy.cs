using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class IncidentWorker_SlaaneshOrgy : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Pawn> pawns = map.mapPawns.FreeColonistsAndPrisonersSpawned.ToList();
            for (int i = 0; i < pawns.Count; i++)
            {
                float randomChance = Rand.Value;
                MentalStateDef mentalState = MentalStateDefOf.Binging_DrugExtreme;
                if (randomChance > 0.8f)
                {
                    mentalState = DefOfs.C_MentalStateDefOf.LustViolent;
                }
                pawns[i].mindState.mentalStateHandler.TryStartMentalState(mentalState);
            }
            return true;
        }
    }
}
