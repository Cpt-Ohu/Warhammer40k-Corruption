using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class FightWorldBattleMission : Mission
    {
        public WorldObject worldObject
        {
            get
            {
                return this.MissionTarget as WorldObject;
            }
        }

    }
}
