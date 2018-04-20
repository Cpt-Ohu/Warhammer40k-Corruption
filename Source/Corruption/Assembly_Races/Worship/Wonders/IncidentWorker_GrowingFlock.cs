using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class IncidentWorker_GrowingFlock : IncidentWorker_PawnsArrive
    {

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            List<Faction> potentialFactions = new List<Faction>()
            {
                Find.World.factionManager.FirstFactionOfDef(FactionDefOf.Outlander),
                Find.World.factionManager.FirstFactionOfDef(FactionDefOf.Tribe),
                Find.World.factionManager.FirstFactionOfDef(FactionDefOf.Pirate),
               CorruptionStoryTrackerUtilities.currentStoryTracker.IoM_NPC

            };
            parms.faction = potentialFactions.RandomElement();
            parms.points = Math.Min(parms.points, 500);
            
            IncidentParmsUtility.AdjustPointsForGroupArrivalParams(parms);
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms, false);
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, defaultPawnGroupMakerParms, true).ToList<Pawn>();
            Map map = (Map)parms.target;
            IntVec3 loc;
            if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out loc))
            {
                return false;
            }

            foreach (Pawn pawn in list)
            {
                Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(pawn);
                soul.GainPatron(PatronDefOf.Emperor, true);
                pawn.SetFaction(Faction.OfPlayer);
                pawn.workSettings.EnableAndInitialize();
                GenSpawn.Spawn(pawn, loc, map);
            }
            string text = "GrowingFlockMessage".Translate(new object[]
            {
                list.Count.ToString()
            });
            string label = "LetterLabelGrowingFlock".Translate();
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.PositiveEvent, list[0], null);
            return true;
        
        }
    }
}
