using Corruption.DefOfs;
using Corruption.Worship;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption
{
    public static class SermonUtility
    {
        private const float SermonAreaIfNotInside = 15f;

        private const int MaxRoomCellsCountToUseWholeRoom = 324;
        
        public static List<Building> FreeChairsInRoom(Room room)
        {
            List<Building> chairs = new List<Building>();

            foreach (Building t in room.ContainedAndAdjacentThings)
            {
                if (t.def.building.isSittable)
                {
                    chairs.Add(t);
                }
            }
            return chairs;
        }

        public static bool IsInside(IntVec3 sermonSpot, Map map)
        {
            Room room = sermonSpot.GetRoom(map);
            return room != null && !room.PsychologicallyOutdoors && room.CellCount <= 400;
        }

        public static ThoughtDef GetSermonThoughts(Pawn preacher, Pawn listener)
        {
            CompSoul s1 = CompSoul.GetPawnSoul(preacher);
            CompSoul s2 = CompSoul.GetPawnSoul(listener);

            if (!s1.Corrupted)
            {

                if (!s2.Corrupted)
                {
                    if (s2.DevotionTrait.SDegree == -2)
                    {
                        if (listener.IsPrisonerOfColony)
                        {
                            return SermonThoughtDefOf.AttendedSermonPureAtheistForced;
                        }

                        return SermonThoughtDefOf.AttendedSermonPureAtheist;
                    }
                    else if (s1.DevotionTrait.SDegree == -1)
                    {
                        if (SermonUtility.movingSermon(preacher))
                        {
                            return SermonThoughtDefOf.AttendedSermonPureMoving;
                        }
                        return SermonThoughtDefOf.AttendedSermonPureAgnostic;
                    }
                    else
                    {
                        if (movingSermon(preacher))
                        {
                            return SermonThoughtDefOf.AttendedSermonPureMoving;
                        }
                        return SermonThoughtDefOf.AttendedSermonPureNice;
                    }
                }
                else
                {
                    if (listener.IsPrisonerOfColony)
                    {
                        return SermonThoughtDefOf.AttendedSermonDarkPureForced;
                    }
                    s2.OpposingDevotees.Add(preacher);
                    return SermonThoughtDefOf.AttendedSermonDarkPure;
                }
            }
            else
            {
                if (!s2.Corrupted)
                {
                    if (listener.IsPrisonerOfColony)
                    {
                        return SermonThoughtDefOf.AttendedSermonPureHereticalForced;
                    }
                    if (movingSermon(preacher))
                    {
                        s2.OpposingDevotees.Add(preacher);
                        return SermonThoughtDefOf.AttendedSermonPureHeretical;
                    }
                    s2.OpposingDevotees.Add(preacher);
                    return SermonThoughtDefOf.AttendedSermonPureUnholy;
                }
                else
                {
                    if (movingSermon(preacher))
                    {
                        return SermonThoughtDefOf.AttendedSermonDarkGlorious;
                    }
                    return SermonThoughtDefOf.AttendedSermonDarkGood;
                }
            }
        }

        private static bool movingSermon(Pawn pr)
        {
            var f = pr.skills.GetSkill(SkillDefOf.Social).Level;
            int x = Rand.RangeInclusive(0, 35);
            if ((x + f * 2) > 40)
            {
                return true;
            }
            return false;
        }

        public static void AttendSermonTickCheckEnd(Pawn pawn, Pawn preacher, WorshipActType worshipActType)
        {
            var soul = CompSoul.GetPawnSoul(pawn);
            if (soul == null)
            {
                return;
            }

            float num = 0f;
            if (movingSermon(preacher))
            {
                num += 0.01f;
            }
            else
            {
                num += 0.005f;
            }

            soul.AffectSoul(num);
            CFind.WorshipTracker.AddWorshipProgress(num * 10000, soul.Patron);
            if (worshipActType == WorshipActType.Confession)
            {
                CompSoul.TryDiscoverAlignment(preacher, pawn, -0.2f);
            }
            else
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(SermonUtility.GetSermonThoughts(preacher, pawn));
            }
        }

        public static void HoldSermonTickCheckEnd(Pawn preacher, BuildingAltar altar)
        {
            var soul = CompSoul.GetPawnSoul(preacher);
            if (soul == null)
            {
                return;
            }

            float num = 0f;

            foreach (Pawn l in altar.Map.mapPawns.AllPawnsSpawned.FindAll(x => x.Position.InHorDistOf(preacher.Position, 20f) == true))
            {
                num += 0.002f;
            }

            if (movingSermon(preacher))
            {
                num += 0.01f;
            }

            soul.AffectSoul(num);
            CFind.WorshipTracker.AddWorshipProgress(num * 20000, soul.Patron);
            altar.activeSermon = false;
        }

        public static bool ShouldAttendSermon(Pawn pawn, Pawn preacher)
        {
            if (!pawn.HostileTo(Faction.OfPlayer) && pawn != preacher)
            {
                if (pawn.GetLord() != null)
                {
                    return false;
                }
                if (!pawn.Drafted)
                {
                    int num = 0;
                    CompSoul soul = CompSoul.GetPawnSoul(pawn);

                    switch (soul.DevotionTrait.SDegree)
                    {
                        case -2:
                            {
                                num = 0;
                                break;
                            }
                        case -1:
                            {
                                num = 5;
                                break;
                            }
                        case 0:
                            {
                                num = 10;
                                break;
                            }
                        case 1:
                            {
                                num = 15;
                                break;
                            }
                        case 2:
                            {
                                num = 20;
                                break;
                            }
                    }

                    if (pawn.CurJob.playerForced)
                    {
                        num = 0;
                        if (soul.DevotionTrait.SDegree == 2)
                        {
                            num = 10;
                        }
                    }

                    if (pawn.CurJob.def == C_JobDefOf.AttendSermon)
                    {
                        num = 0;
                    }

                    if (!SermonUtility.IsBestPreacher(pawn, preacher))
                    {
                        num = 0;
                    }
                    if ((Rand.RangeInclusive(0, 15) + num) >= 20)
                    {
                        return true;
                    }
                }
            }
            return false;

        }

        public static void GiveAttendSermonJob(BuildingAltar altar, Pawn attendee)
        {
            if (!SermonUtility.IsPreacher(attendee))
            {
                IntVec3 result;
                Building chair;
                if (!WatchBuildingUtility.TryFindBestWatchCell(altar, attendee, true, out result, out chair))
                {

                    if (!WatchBuildingUtility.TryFindBestWatchCell(altar as Thing, attendee, false, out result, out chair))
                    {
                        return;
                    }
                }
                if (chair != null)
                {
                    Job J = new Job(C_JobDefOf.AttendSermon, altar.preacher, altar, chair);
                    attendee.jobs.jobQueue.EnqueueLast(J);
                    attendee.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                else
                {
                    Job J = new Job(C_JobDefOf.AttendSermon, altar.preacher, altar, result);
                    attendee.jobs.jobQueue.EnqueueLast(J);
                    attendee.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
        }

        public static bool IsInSermonArea(Pawn pawn)
        {
            IntVec3 cell = pawn.mindState.duty.focus.Cell;
            if (pawn.Position.InHorDistOf(cell, 20f) && GenSight.LineOfSight(pawn.Position, cell, pawn.Map))
            {
                return true;
            }

            return false;
        }

        public static void ForceSermon(BuildingAltar altar, WorshipActType worshipActType)
        {
            altar.activeSermon = true;            
            List<Pawn> list = new List<Pawn>();
            if (!list.Contains(altar.preacher))
            {
                list.Add(altar.preacher);
            }
            list.AddRange(SermonUtility.GetSermonFlock(altar));

            Lord lord = LordMaker.MakeNewLord(altar.Faction, new LordJob_Sermon(altar, worshipActType), altar.Map, list);                
        }

        public static void ForceSermonV2(BuildingAltar altar)
        {
            IntVec3 b = altar.def.interactionCellOffset.RotatedBy(altar.Rotation) + altar.Position;
            Job job = new Job(C_JobDefOf.HoldSermon, altar, b);
            altar.preacher.jobs.jobQueue.EnqueueLast(job);
            altar.preacher.jobs.EndCurrentJob(JobCondition.InterruptForced);
        //    BuildingAltar.GetSermonFlock(altar);
        }

        public static bool IsPreacher(Pawn p)
        {
            List<Thing> list = p.Map.listerThings.AllThings.FindAll(s => s.GetType() == typeof(BuildingAltar));
            foreach (BuildingAltar b in list)
            {
                if (b.preacher == p) return true;
            }
            return false;
        }

        public static List<Pawn> GetSermonFlock(BuildingAltar altar)
        {
            List<Pawn> tmp = new List<Pawn>();
            Room room = altar.GetRoom();

            if (room.Role != RoomRoleDefOf.PrisonBarracks && room.Role != RoomRoleDefOf.PrisonCell)
            {
                List<Pawn> listeners = altar.Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
                bool[] flag = new bool[listeners.Count];
                for (int i = 0; i < listeners.Count; i++)
                {
                    if (!flag[i] && SermonUtility.ShouldAttendSermon(listeners[i], altar.preacher))
                    {
                        tmp.Add(listeners[i]);
                        flag[i] = true;
                    }
                }
            }
            else
            {
                List<Pawn> prisoners = altar.Map.mapPawns.PrisonersOfColonySpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike);
                bool[] flag2 = new bool[prisoners.Count];
                for (int i = 0; i < prisoners.Count; i++)
                {
                    if (!flag2[i] && SermonUtility.ShouldAttendSermon(prisoners[i], altar.preacher))
                    {
                        tmp.Add(prisoners[i]);
                        flag2[i] = true;
                    }
                }
            }

            return tmp;

        }

        public static bool GetBestPreacher(Pawn p, out Pawn bestPreacher, out BuildingAltar altar)
        {
            List<Pawn> opposingDevotees = CompSoul.GetPawnSoul(p).OpposingDevotees;
            if (opposingDevotees == null) opposingDevotees = new List<Pawn>();
            List<Pawn> availablePreachers = p.Map.mapPawns.FreeColonistsSpawned.ToList<Pawn>().FindAll(s => s.CurJob.def == C_JobDefOf.HoldSermon);

            //Select best preacher of colony

            bestPreacher = availablePreachers.Aggregate((i1, i2) => i1.skills.GetSkill(SkillDefOf.Social).Level > i2.skills.GetSkill(SkillDefOf.Social).Level ? i1 : i2);
            altar = SermonUtility.chosenAltar(bestPreacher);
            //Check if pawn has listened to this preacher before and if he is of an opposing faith. If so, another preacher will be chosen

            while (opposingDevotees.Contains(bestPreacher))
            {
                if (availablePreachers.Count > 1)
                {
                    availablePreachers.Remove(bestPreacher);
                    bestPreacher = availablePreachers.Aggregate((i1, i2) => i1.skills.GetSkill(SkillDefOf.Social).Level > i2.skills.GetSkill(SkillDefOf.Social).Level ? i1 : i2);
                    altar = chosenAltar(bestPreacher);
                }
                else
                {
                    bestPreacher = null;
                    altar = null;
                }
                
            }
            if (bestPreacher != null && altar != null)
            {
                return true;
            }
            return false;
        }

        public static bool IsBestPreacher(Pawn pawn, Pawn preacher)
        {
            List<Pawn> opposingDevotees = CompSoul.GetPawnSoul(pawn).OpposingDevotees;
            if (opposingDevotees == null) opposingDevotees = new List<Pawn>();
            List<Pawn> availablePreachers = pawn.Map.mapPawns.AllPawnsSpawned.ToList<Pawn>().FindAll(x => SermonUtility.IsPreacher(x));
            Pawn bestcurrentPreacher;
            if (availablePreachers != null)
            {
                bestcurrentPreacher = availablePreachers.Aggregate((i1, i2) => i1.skills.GetSkill(SkillDefOf.Social).Level > i2.skills.GetSkill(SkillDefOf.Social).Level ? i1 : i2);

                if (bestcurrentPreacher == preacher && !opposingDevotees.Contains(preacher))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool PawnIsPossiblePreacher(Pawn pawn)
        {
            List<BuildingAltar> altars = pawn.Map.listerBuildings.allBuildingsColonist.FindAll(x => x is BuildingAltar).Cast<BuildingAltar>().ToList();
            foreach (BuildingAltar altar in altars)
            {
                if (altar.preacher == pawn)
                {
                    return true;
                }
            }
            return false;
        }

        public static BuildingAltar chosenAltar(Pawn preacher)
        {
            return SermonUtility.allAltars(preacher).Find(x => x.preacher == preacher);            
        }

        public static List<BuildingAltar> allAltars(Pawn preacher)
        {
            List<BuildingAltar> y = preacher.Map.listerThings.AllThings.FindAll(a => a.GetType() == typeof(BuildingAltar)).Cast<BuildingAltar>().ToList<BuildingAltar>();
            return y;
        }

        public static bool TryGetSermonWatchPosition(Thing altar, Pawn pawn, out IntVec3 cell, out Building chair)
        {
            if (!WatchBuildingUtility.TryFindBestWatchCell(altar, pawn, true, out cell, out chair))
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(altar, pawn, false, out cell, out chair))
                {
                    Log.Warning("No watch cell found");
                    return false;
                }
            }
            return true;
        }
    }

}
