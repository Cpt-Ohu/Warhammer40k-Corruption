using Corruption.DefOfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using AlienRace;

namespace Corruption
{
    public class HediffComp_TurnServitor : HediffComp_Disappears
    {
        public override bool CompShouldRemove
        {
            get
            {
                if(base.CompShouldRemove)
                {
                    List<Building> list = this.Pawn.Map.listerBuildings.allBuildingsColonist.FindAll(x => x is Building_MechanicusMedTable);
                    foreach (Building_MechanicusMedTable current in list)
                    {
                        Building_MechanicusMedTable medTable = (Building_MechanicusMedTable)current;
                        if (medTable.patient == this.Pawn)
                        {
                            Pawn pawn = PawnGenerator.GeneratePawn(C_PawnKindDefOf.ServitorColonist);
                            pawn.story.childhood = this.Pawn.story.childhood;
                            pawn.story.adulthood = this.Pawn.story.adulthood;
                            medTable.patient.Destroy(DestroyMode.Vanish);
                            medTable.TryAcceptThing(pawn);
                            return true;
                        }
                    }

                    return false;
                }
                return false;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();

        }
    }
}
