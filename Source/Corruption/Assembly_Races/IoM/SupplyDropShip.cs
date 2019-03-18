using OHUShips;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class SupplyDropShip : ShipColorable
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.ShipUnload(false, false, true);
            this.shipState = ShipState.Outgoing;
        }
    }
}
