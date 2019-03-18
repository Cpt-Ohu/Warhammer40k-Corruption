using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class RescuePawnMission : SinglePawnMission
    {
        public override PawnMissionType MissionType => PawnMissionType.Rescue;


        public RescuePawnMission(MissionDef def, object target = null, Faction faction = null) : base(def, target, faction)
        {
        }

        internal override void CreateTargets()
        {
            PawnKindDef pawnKind = this.GiverFaction.RandomPawnKind();
            Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, this.GiverFaction);
            WorldObject RescueMissionSite = WorldObjectMaker.MakeWorldObject(DefOfs.C_WorldObjectDefOf.)
            this.MissionTarget = pawn;
        }
    }
}
