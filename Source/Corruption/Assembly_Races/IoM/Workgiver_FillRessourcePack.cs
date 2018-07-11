using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class Workgiver_FillRessourcePack : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.HaulableEver);
            }
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompResourcePack resPack = t.TryGetComp<CompResourcePack>();
            if (resPack == null) return null;
            List<Thing> list = CaravanFormingUtility.AllReachableColonyItems(pawn.Map, false, false);
            List<ThingAmount> chosenItems = new List<ThingAmount>();
            if (FindFittingResources(list, resPack, chosenItems))
            {
                Job haulJob = new Job(DefOfs.C_JobDefOf.FillResourcePack);
                haulJob.targetQueueA = new List<LocalTargetInfo>();
                ThingAmount foundRes = chosenItems.RandomElementByWeight(x => 1 / (x.thing.Position.DistanceTo(pawn.Position)));
                haulJob.targetA = foundRes.thing;
                haulJob.count = foundRes.count;
                chosenItems.Remove(foundRes);
                haulJob.targetQueueB = new List<LocalTargetInfo>();
                haulJob.countQueue = new List<int>();
                foreach(var item in chosenItems)
                {
                    haulJob.targetQueueB.Add(item.thing);
                    haulJob.countQueue.Add(item.count);
                }
                haulJob.haulOpportunisticDuplicates = true;
                haulJob.targetB = resPack.parent.Position;
                haulJob.targetC = resPack.parent;

                return haulJob;
            }
            else
            {
                return null;
            }
        }


        private static bool FindFittingResources(List<Thing> availableThings, CompResourcePack resorcePack, List<ThingAmount> chosen)
        {
            chosen.Clear();            
            int availableCapacity = resorcePack.Props.Capacity - resorcePack.Resources.Sum(x => x.Count);
            foreach (ThingFilter filter in resorcePack.Props.filters)
            {               
                for (int j = 0; j < availableThings.Count; j++)
                {
                    Thing thing = availableThings[j];
                    if ( filter.Allows(thing) && resorcePack.ThingAllowed(thing))
                    {
                        int num3 = Mathf.Min(Mathf.CeilToInt(availableCapacity), thing.stackCount);
                        ThingAmount.AddToList(chosen, thing, num3);
                    }
                }
            }
            if (chosen.Count > 0)
             {
                return true;
            }

            return false;
        }


        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompResourcePack resPack = t.TryGetComp<CompResourcePack>();
            if (resPack != null)
            {
                if (resPack.LoadEnabled && !resPack.Full && t.Map.reservationManager.CanReserve(pawn, t, 1, 1))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
