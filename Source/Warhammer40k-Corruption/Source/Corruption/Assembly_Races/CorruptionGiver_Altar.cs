using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
        public class CorruptionGiver_Pray : CorruptionGiver
        {

            private Thing FindClosestAltar(Pawn pawn, int MaxParticipants)
            {
                var altars = Find.ListerBuildings
                                 .AllBuildingsColonistOfClass<BuildingAltar>()
                                 .Where(altar => (
                                   (
                                       (!altar.def.socialPropernessMatters) ||
                                       (SocialProperness.IsSociallyProper(altar, pawn))
                                   ) &&
                                   (pawn.CanReserveAndReach(altar, PathEndMode.Touch, pawn.NormalMaxDanger(), MaxParticipants))
                                )).ToList();
                if (altars.NullOrEmpty())
                {
                    return null;
                }
                altars.Sort((x, y) =>
                {
                    var distX = (x.Position - pawn.Position).LengthManhattan;
                    var distY = (y.Position - pawn.Position).LengthManhattan;
                    return distX < distY
                        ? -1
                        : 1;
                });
                return altars[0];
            }

            protected virtual Job TryGiveAltarJob(Pawn pawn, Thing totem)
            {
                IntVec3 result;
                Building chair;
                if (!WatchBuildingUtility.TryFindBestWatchCell(totem, pawn, this.def.desireSit, out result, out chair))
                {
                    if (this.def.desireSit)
                    {
                        if (!WatchBuildingUtility.TryFindBestWatchCell(totem, pawn, false, out result, out chair))
                        {
                            return (Job)null;
                        }
                    }
                }
                if (chair != null)
                {
                    return new Job(this.def.jobDef, totem, chair);
                }
                return new Job(this.def.jobDef, totem, result);
            }

        public override Job TryGiveJob(Pawn pawn)
        {
            Thing altar = this.FindClosestAltar(pawn, this.def.jobDef.joyMaxParticipants);
            if (altar != null)
            {
                return this.TryGiveAltarJob(pawn, altar);
            }
            return (Job)null;
        }

        }
    
}
