using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class CorruptionGiver_HoldSermon : CorruptionGiver

    {
        private Thing FindAssignedAltar(Pawn pawn, int MaxParticipants)
        {
            var altars = Find.ListerBuildings
                             .AllBuildingsColonistOfClass<BuildingAltar>()
                             .Where(altar => (
                               (altar.preacher == pawn) &&
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

        protected virtual Job TryGiveSermonJob(Pawn pawn, Thing altar)
        {
            if (altar.InteractionCell.Standable() && !altar.IsForbidden(pawn) && !altar.InteractionCell.IsForbidden(pawn) && !Find.PawnDestinationManager.DestinationIsReserved(altar.InteractionCell))
            {
                return new Job(this.def.jobDef, altar, altar.InteractionCell);
            }
            return null;
        }



        public override Job TryGiveJob(Pawn pawn)
        {
            Thing altar = this.FindAssignedAltar(pawn, this.def.jobDef.joyMaxParticipants);
            if (altar != null)
            {
                return this.TryGiveSermonJob(pawn, altar);
            }
            return (Job)null;
        }
    }
}
