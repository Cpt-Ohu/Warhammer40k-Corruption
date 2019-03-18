using OHUShips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Xenos
{
    public class Ork_Rok : ShipBase
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.ShipUnload(false, false, true);
        }
    }
}
