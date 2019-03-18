using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class SinglePawnMission : Mission
    {
        public SinglePawnMission(MissionDef def, object target = null, Faction faction = null) : base(def, target, faction)
        {
        }

        public virtual PawnMissionType MissionType
        {
            get
            {
                return PawnMissionType.None;
            }
        }

        public Pawn Pawn
        {
            get
            {
                return (Pawn)this.AllTargets.FirstOrDefault(x => x is Pawn);
            }
        }


    }
}
