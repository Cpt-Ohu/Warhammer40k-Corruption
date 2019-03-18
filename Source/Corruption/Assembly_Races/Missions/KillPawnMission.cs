using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class KillPawnMission : SinglePawnMission
    {
        public override PawnMissionType MissionType => PawnMissionType.Kill;

        public KillPawnMission(MissionDef def, object target = null, Faction faction = null) : base(def, target, faction)
        {
        }
    }
}
