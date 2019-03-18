using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public static class MissionTargetFinder
    {
        public static List<Object> FindTargetsForMission(MissionDef def)
        {
            if (def.missionClass == typeof(KillPawnMission))
            {
                return FindKillPawnTarget(def).ToList();
            }

            return null;
        }

       public static IEnumerable<object> FindKillPawnTarget(MissionDef def)
        {
            Faction faction = Find.World.factionManager.AllFactionsVisible.Where(x => x.IsPlayer == false).RandomElement();
            WorldObject worldObject = Find.World.worldObjects.AllWorldObjects.Where(x => x.Faction != null && x.Faction == faction).RandomElement();
            yield return worldObject;
            Pawn targetPawn = PawnGenerator.GeneratePawn(faction.RandomPawnKind(), faction);
            yield return targetPawn;
        }
    }
}
