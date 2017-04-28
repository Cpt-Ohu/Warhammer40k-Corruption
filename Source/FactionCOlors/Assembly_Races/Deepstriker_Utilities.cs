using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    public static class Deepstriker_Utilities
    {
        private static List<List<Thing>> tempList = new List<List<Thing>>();

        private static List<Deepstriker_ThingDef> DeepStrikerDefs
        {
            get
            {
                return DefDatabase<Deepstriker_ThingDef>.AllDefsListForReading;
            }
        }       

        private static bool ResolveDeepStrikerDef(Faction faction, out Deepstriker_ThingDef DeepStrikerDef)
        {
            Log.Message("Starting DeepStriker Resolve");
            for(int i = 0; i < Deepstriker_Utilities.DeepStrikerDefs.Count; i++)
            {
                for(int j = 0; j < DeepStrikerDefs[i].BelongsToFactions.Count; j++)
                {
                    if (faction.def == DeepStrikerDefs[i].BelongsToFactions[j])
                    {
                        DeepStrikerDef = DeepStrikerDefs[i];
                        return true;
                    }
                }
            }
            DeepStrikerDef = null;
            return false;
        }

        public static void MakeDeepStrikerAt(IntVec3 center, DropPodInfo info, Faction faction)
        {
            Deepstriker_ThingDef def;
            if (ResolveDeepStrikerDef(faction, out def))
            {
                Deepstriker_Incoming deepStrikerIncoming = (Deepstriker_Incoming)ThingMaker.MakeThing(def, null);
                deepStrikerIncoming.contents = info;
                GenSpawn.Spawn(deepStrikerIncoming, center);
            }
            else
            {
                Log.Error("No Deepstriker vehicle found for Faction " + faction.ToString());
            }
        }

        public static void UnloadThingsNear(Faction faction, IntVec3 dropCenter, IEnumerable<Thing> things, int openDelay = 110, bool canInstaDropDuringInit = false, bool leaveSlag = false, bool canRoofPunch = true)
        {
            Log.Message("Starting Spawn");

            foreach (Thing current in things)
            {
                List<Thing> list = new List<Thing>();
                list.Add(current);
                Deepstriker_Utilities.tempList.Add(list);
            }
            Deepstriker_Utilities.UnloadThingGroupsNear(faction, dropCenter, Deepstriker_Utilities.tempList, openDelay, canInstaDropDuringInit, leaveSlag, canRoofPunch);
            Deepstriker_Utilities.tempList.Clear();
        }

        public static void UnloadThingGroupsNear(Faction faction, IntVec3 dropCenter, List<List<Thing>> thingsGroups, int openDelay = 110, bool instaDrop = false, bool leaveSlag = false, bool canRoofPunch = true)
        {
            foreach (List<Thing> current in thingsGroups)
            {
                IntVec3 intVec;
                if (!DropCellFinder.TryFindDropSpotNear(dropCenter, out intVec, true, canRoofPunch))
                {
                    Log.Warning(string.Concat(new object[]
                    {
                "DropThingsNear failed to find a place to drop ",
                current.FirstOrDefault<Thing>(),
                " near ",
                dropCenter,
                ". Dropping on random square instead."
                    }));
                    intVec = CellFinderLoose.RandomCellWith((IntVec3 c) => c.Walkable(), 1000);
                }
                for (int i = 0; i < current.Count; i++)
                {
                    current[i].SetForbidden(true, false);
                }
                if (instaDrop)
                {
                    foreach (Thing current2 in current)
                    {
                        GenPlace.TryPlaceThing(current2, intVec, ThingPlaceMode.Near, null);
                    }
                }
                else
                {
                    DropPodInfo dropPodInfo = new DropPodInfo();
                    foreach (Thing current3 in current)
                    {
                        dropPodInfo.containedThings.Add(current3);
                    }
                    dropPodInfo.openDelay = openDelay;
                    dropPodInfo.leaveSlag = leaveSlag;
                    Deepstriker_Utilities.MakeDeepStrikerAt(intVec, dropPodInfo, faction);
                }
            }
        }
    }
}
