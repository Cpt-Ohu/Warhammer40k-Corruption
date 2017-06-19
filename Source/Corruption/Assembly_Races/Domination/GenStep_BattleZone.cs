using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Domination
{
    public class GenStep_BattleZone : GenStep
    {
        private BattleZone battleZone;

        public Dictionary<Faction, List<Pawn>> assembledForces = new Dictionary<Faction, List<Pawn>>();

        public Dictionary<Faction, IntVec3> spawnPoints = new Dictionary<Faction, IntVec3>();

        public override void Generate(Map map)
        {
            this.battleZone = (BattleZone)map.ParentHolder;
            foreach(Faction current in this.battleZone.WarringFactions)
            {
                PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
                pawnGroupMakerParms.tile = map.Tile;
                pawnGroupMakerParms.generateFightersOnly = true;
                pawnGroupMakerParms.faction = current;
                pawnGroupMakerParms.points = Rand.Range(battleZone.battlePointRange[0], battleZone.battlePointRange[1]);
                this.assembledForces.Add(current, PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, pawnGroupMakerParms, true).ToList<Pawn>());
                IntVec3 spawnPoint;
                //CellFinder.TryRandomClosewalkCellNear(CellFinder.RandomEdgeCell(map), map, 100, out spawnPoint);
                CellFinder.TryFindRandomEdgeCellWith(delegate (IntVec3 x)
                {

                    if (!this.isValidArmySpawnPoint(x, map))
                    {
                        return false;
                    }               

                    return true;
                }, map, Rot4.Random, CellFinder.EdgeRoadChance_Always, out spawnPoint);

                this.spawnPoints.Add(current, spawnPoint);
            }

            if (this.battleZone.DefendingFaction != null)
            {
                IntVec3 campCenter;
                CellFinder.TryRandomClosewalkCellNear(map.Center, map, 100, out campCenter);
                this.spawnPoints[this.battleZone.DefendingFaction] = campCenter;
            }


            foreach(Faction current in this.battleZone.WarringFactions)
            {
                Log.Message("SPawning for: " + current.Name);
                foreach (Pawn current2 in this.assembledForces[current])
                {
                    Log.Message("SPawning Soldier");
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(this.spawnPoints[current], map, 8, null);
                    GenSpawn.Spawn(current2, loc, map, Rot4.Random, false);
                }
                IntVec3 AttackPoint = IntVec3.North;
                if (this.battleZone.DefendingFaction != null)
                {
                    if (current == this.battleZone.DefendingFaction)
                    {
                        AttackPoint = spawnPoints[current];
                    }
                }
                else
                {
                    AttackPoint = spawnPoints.Where(x => x.Value == spawnPoints[current]).RandomElement().Value;
                }
                LordMaker.MakeNewLord(current, new LordJob_DefendPoint(map.Center),map, assembledForces[current]); 
            }
        }

        private bool isValidArmySpawnPoint(IntVec3 position, Map map)
        {
            bool result = true;
            foreach (KeyValuePair<Faction, IntVec3> current in this.spawnPoints)
            {
                if (position.InHorDistOf(current.Value, 50))
                {
                    result = false;
                }
            }
            return result;
        }
    }
}
