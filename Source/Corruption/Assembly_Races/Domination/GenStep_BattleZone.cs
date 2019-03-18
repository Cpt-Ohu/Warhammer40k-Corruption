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

        public IntVec3 battleCenter;

        public Dictionary<PoliticalAlliance, IntVec3> AllianceEntryPoints = new Dictionary<PoliticalAlliance, IntVec3>();
        public Dictionary<Faction, IntVec3> spawnPoints = new Dictionary<Faction, IntVec3>();

        public override int SeedPart => 1412216193;

        public override void Generate(Map map, GenStepParams parms)
        {
            this.battleZone = (BattleZone)map.ParentHolder;

            this.GenerateArmySpawnPoints(map);

            if (this.battleZone.DefendingFaction != null)
            {
                IntVec3 campCenter;
                CellFinder.TryRandomClosewalkCellNear(map.Center, map, 100, out campCenter);
                this.AllianceEntryPoints[this.battleZone.DefendingFaction] = campCenter;
                this.battleCenter = campCenter;
            }
            else
            {
                this.battleCenter = map.Center;
            }


            this.SpawnArmies(map);
        }

        private void GenerateArmySpawnPoints(Map map)
        {
            foreach (PoliticalAlliance alliance in this.battleZone.WarringAlliances)
            {
                IntVec3 spawnPoint;
                if(!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map) && !map.roofGrid.Roofed(c) && this.isValidArmySpawnPoint(c, map) && c.GetRoom(map, RegionType.Set_Passable).TouchesMapEdge, map, CellFinder.EdgeRoadChance_Always, out spawnPoint))
                {
                    Log.Error("No Entry point found for: " + alliance.AllianceName);
                }

                if (!this.AllianceEntryPoints.ContainsKey(alliance))
                {
                    this.AllianceEntryPoints.Add(alliance, spawnPoint);
                }
                
                foreach (Faction current in alliance.GetFactions())
                {
                    PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
                    pawnGroupMakerParms.tile = map.Tile;
                    pawnGroupMakerParms.generateFightersOnly = true;
                    pawnGroupMakerParms.faction = current;
                    pawnGroupMakerParms.points = Rand.Range(battleZone.battlePointRange[0], battleZone.battlePointRange[1]);
                    this.assembledForces.Add(current, PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms, true).ToList<Pawn>());
                }
            }
        }

        private bool isValidArmySpawnPoint(IntVec3 position, Map map)
        {
            bool result = true;
            foreach (KeyValuePair<PoliticalAlliance, IntVec3> current in this.AllianceEntryPoints)
            {
                if (position.InHorDistOf(current.Value, 100f))
                {
                    result = false;
                }
            }
            return result;
        }
        
        private void SpawnArmies(Map map)
        {
            foreach (PoliticalAlliance alliance in this.battleZone.WarringAlliances)
            {
                foreach (Faction faction in alliance.GetFactions())
                {
                    foreach (Pawn current2 in this.assembledForces[faction])
                    {
                        IntVec3 loc = CellFinder.RandomClosewalkCellNear(this.AllianceEntryPoints[alliance], map, 8, null);
                        GenSpawn.Spawn(current2, loc, map, Rot4.Random, WipeMode.Vanish);
                    }
                    IntVec3 AttackPoint = IntVec3.North;
                    if (this.battleZone.DefendingFaction != null)
                    {
                        if (alliance == this.battleZone.DefendingFaction)
                        {
                            AttackPoint = AllianceEntryPoints[alliance];
                        }
                    }
                    else
                    {
                        AttackPoint = AllianceEntryPoints.Where(x => x.Value == AllianceEntryPoints[alliance]).RandomElement().Value;
                    }
                    LordMaker.MakeNewLord(faction, new LordJob_DoBattle(this.battleCenter), map, assembledForces[faction]);
                }
            }
        }

    }
}
