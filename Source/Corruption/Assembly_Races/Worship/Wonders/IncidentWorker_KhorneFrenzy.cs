using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class IncidentWorker_KhorneFrenzy : IncidentWorker_AnimalInsanityMass
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (base.TryExecuteWorker(parms))
            {
                Map map = (Map)parms.target;
                List<Pawn> pawns = map.mapPawns.AllPawnsSpawned.FindAll(x => x.def.race.Humanlike);
                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn pawn = pawns[i];
                    Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(pawn);
                    if (soul?.Patron == PatronDefOf.Khorne || pawn.Faction != Faction.OfPlayer)
                    {
                        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "MentalStateKhorneGlory".Translate(), true);
                    }
                }
            }
            return true;
        }
    }
}
