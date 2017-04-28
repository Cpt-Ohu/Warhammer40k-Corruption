using Corruption.DefOfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.Ships
{
    public class JobGiver_LeaveInShip : ThinkNode_JobGiver
    {       
        protected override Job TryGiveJob(Pawn pawn)
        {
            List<Thing> ships = DropShipUtility.CurrentFactionShips(pawn).FindAll(x => x.Map == pawn.Map);
            if (!ships.NullOrEmpty())
            {
                Thing ship = ships.RandomElement();
                if (ship != null && ship.Map.reservationManager.CanReserve(pawn, ship, ship.TryGetComp<CompShip>().sProps.maxPassengers))
                {
                    Job job = new Job(C_JobDefOf.LeaveInShip, pawn, ship);

                    return job;
                }
            }
            return null;
        }

    }
}
