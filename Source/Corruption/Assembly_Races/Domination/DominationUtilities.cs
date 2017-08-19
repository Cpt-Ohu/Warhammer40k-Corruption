using Corruption.DefOfs;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.Domination
{
    public static class DominationUtilities
    {
        public static int ActiveFightersFor(Faction faction, Map map)
        {
            List<IAttackTarget> targets = new List<IAttackTarget>();
            targets.AddRange(map.attackTargetsCache.TargetsHostileToFaction(faction));
            return targets.FindAll(x => GenHostility.IsActiveThreat(x)).Count;
        }

        public static int CasualtiesForFaction(Faction faction, Map map)
        {
            return map.mapPawns.AllPawns.Where(x => x.Faction == faction && (x.Dead || x.Downed)).Count();
        }


        public static void GenerateRandomBattleZone()
        {
            BattleZone zone = (BattleZone)WorldObjectMaker.MakeWorldObject(C_WorldObjectDefOf.BattleZone);
            List<PoliticalAlliance> alliances = new List<PoliticalAlliance>();
            int num = Rand.RangeInclusive(2, 3);
            while (alliances.Count < 2)
            {
                PoliticalAlliance alliance = CorruptionStoryTrackerUtilities.currentStoryTracker.DominationTracker.GetRandomAlliance();
                if (!alliances.Contains(alliance) && alliances.FindAll(x => !alliance.HostileTo(x)).Count == 0 && !alliance.LeadingFaction.def.pawnGroupMakers.NullOrEmpty())
                {
                    Log.Message("Adding Alliance " + alliance.AllianceName);
                    alliances.Add(alliance);
                }
            }
            zone.InitializeBattle(BattleSize.Random, BattleType.OpenField, alliances, "NamerBattleGeneric");
            zone.Tile = TileFinder.RandomStartingTile();
            Find.WorldObjects.Add(zone);
        }

    }
}
