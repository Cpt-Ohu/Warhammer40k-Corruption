using OHUShips;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.IoM
{
    public class JobGiver_LoadResourcePack : ThinkNode_JobGiver
    {   
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Spawned)
            {
                ShipBase ship = (ShipBase)pawn.mindState.duty.focus;
                if (ship != null)
                {
                    List<Thing> resourcePacks = pawn.Map?.listerThings.AllThings.FindAll(x => x is ResourcePack && x.Position.InHorDistOf(ship.Position, 15f));
                    if (!resourcePacks.NullOrEmpty())
                    {
                        ResourcePack foundPack = (ResourcePack)resourcePacks.RandomElement();
                        if (foundPack.compResource.IsTribute && pawn.Map.reservationManager.CanReserve(pawn, ship, 3, -1, null))
                        {
                            Job haulJob = new Job(DefOfs.C_JobDefOf.HaulToContainerMulti, foundPack, ship);
                            haulJob.count = foundPack.stackCount;
                            return haulJob;
                        }
                    }
                }
            }

            return null;
        }


    }
}
