using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.Tithes
{
    public class Workgiver_LoadTithes : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }

        private Thing ItemToHaul;

        private int countToHaul;

        public override Job JobOnThing(Pawn pawn, Thing t)
        {
            TitheContainer container = (TitheContainer)t;

            return new Job(JobDefOf.HaulToContainer, this.ItemToHaul, container)
            {
                count = this.countToHaul,
                ignoreForbidden = false
            };

        }
        
        public override bool HasJobOnThing(Pawn pawn, Thing t)
        {
            if (t is TitheContainer)
            {
                TitheContainer container = (TitheContainer)t;
                bool flag = this.SetItemToCollect(container, out this.countToHaul, out this.ItemToHaul) && this.countToHaul >0 && container.massUsage < 1f &&  pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1) && container.compTithe.tProps.maxContainerCapacity >= container.GetInnerContainer().Count && container.currentTitheEntries.Any(x => x.active);
                return flag;
            }
            return false;
        }

        private bool SetItemToCollect(TitheContainer container, out int count, out Thing thing)
        {
            List<ThingDef> listTithes = container.titheDefsEnabled;
            if (!container.currentTitheEntries.NullOrEmpty())
            {

                TitheEntryForContainer entry = container.currentTitheEntries.RandomElement();
                TitheEntryGlobal globalEntry = entry.Tithe;
                if (globalEntry.tithePercent < 1f && entry.active)
                {
                    List<Thing> list = (from x in CaravanFormingUtility.AllReachableColonyItems(container.Map, false, false) where globalEntry.thingDefs.Contains(x.def) select x).ToList();
                    if (!list.NullOrEmpty())
                    {
                        thing = list.RandomElement();
                        float num = Mathf.Min((int)TitheUtilities.SpaceRemainingForThing(container.compTithe, thing) / thing.GetStatValue(StatDefOf.Mass), thing.stackCount);
                        float num2 = TitheUtilities.RemainingTitheToCollect(globalEntry, thing);
                        int num3 = (int)(Mathf.Min(num, num2));
                        if (num3 < 1)
                        {
                            if (globalEntry.tithePercent < 1f)
                            {
                                num3 = 1;
                            }
                            else
                            {
                                count = 0;
                                return false;
                            }
                        }
                        count = num3;
  
                        return true;
                    }
                }
            }
            thing = null;
            count = 0;
            return false;
        }
    }
}
