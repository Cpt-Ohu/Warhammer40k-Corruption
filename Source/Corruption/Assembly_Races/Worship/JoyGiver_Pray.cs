using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class JoyGiver_Pray : JoyGiver
    {        
        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.ownership == null)
            {
                return null;
            }
            Job prayAlone = this.PrayInOwnRoom(pawn);            
            if (prayAlone != null)
            {
                return prayAlone;
            }
            else
            {
                return this.PrayInTemple(pawn);
            }
        }

        private Job PrayInOwnRoom(Pawn pawn)
        {
            Room ownedRoom = pawn.ownership.OwnedRoom;
            if (ownedRoom == null)
            {
                return null;
            }
            CompSoul soul = CompSoul.GetPawnSoul(pawn);
            if (soul != null)
            {
                float chance;
                switch (soul.DevotionTrait.SDegree)
                {
                    case -2:
                        {
                            chance = 0f;
                            break;
                        }
                    case -1:
                        {
                            chance = 0.1f;
                            break;
                        }
                    case 0:
                        {
                            chance = 0.5f;
                            break;
                        }
                    case 1:
                        {
                            chance = 0.8f;
                            break;
                        }
                    case 2:
                        {
                            chance = 1f;
                            break;
                        }
                    default:
                        {
                            chance = 0f;
                            break;
                        }
                }

                if (chance > Rand.Range(0f, 1f))
                {
                    IntVec3 c2;
                    //Look for Items of Worship
                    List<Thing> worshipBuildings = ownedRoom.ContainedAndAdjacentThings.FindAll(x => x.TryGetComp<CompShrine>() != null);
                    if (!worshipBuildings.NullOrEmpty())
                    {
                        Thing chosen = null;
                        if ((from b in worshipBuildings
                             where b is Building && !b.IsForbidden(pawn) && pawn.CanReserveAndReach(b, PathEndMode.OnCell, Danger.None, 1)
                             select b).TryRandomElement(out chosen))
                        {
                            if (chosen.def.hasInteractionCell)
                            {
                                c2 = chosen.InteractionCell;
                            }
                            else
                            {
                                c2 = GenAdj.CellsAdjacent8Way(chosen).Where(x => x.Standable(pawn.Map)).RandomElement();
                            }

                            return new Job(this.def.jobDef, c2);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    //Try random cell
                    if (!(from c in ownedRoom.Cells
                          where c.Standable(pawn.Map) && !c.IsForbidden(pawn) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None, 1)
                          select c).TryRandomElement(out c2))
                    {
                        return null;
                    }
                    return new Job(this.def.jobDef, c2);
                }
            }
            return null;
        }

        private Job PrayInTemple(Pawn pawn)
        {
            List<Thing> altars = pawn.Map.listerThings.AllThings.FindAll(x => x is BuildingAltar);

            if (!altars.NullOrEmpty())
            {
                BuildingAltar altar = altars.RandomElementByWeight(x => x.Position.DistanceTo(pawn.Position)) as BuildingAltar;
                IntVec3 cell;
                Building chair;
                if (SermonUtility.TryGetSermonWatchPosition(altar, pawn, out cell, out chair))
                {
                    IntVec3 jobTargetCell = chair == null ? cell : chair.Position;
                    return new Job(this.def.jobDef, jobTargetCell);
                }
            }
            return null;
        }
    }
}
