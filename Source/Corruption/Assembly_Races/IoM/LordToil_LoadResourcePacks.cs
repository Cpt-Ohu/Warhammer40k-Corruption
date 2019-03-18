using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.IoM
{
    public class LordToil_LoadResourcePacks : LordToil
    {
       public  LordToil_LoadResourcePacks(OHUShips.ShipBase ship)
        {
            this.ship = ship;
        }

        private OHUShips.ShipBase ship;
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DefOfs.C_DutyDefOfs.LoadResourcePacks);
                this.lord.ownedPawns[i].mindState.duty.radius = 15f;
                this.lord.ownedPawns[i].mindState.duty.focus = this.ship; 
            }
        }

    }
}
