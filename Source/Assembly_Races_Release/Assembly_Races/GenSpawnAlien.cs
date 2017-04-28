using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AlienRace
{
    public static class GenSpawnAlien
    {

        public static Thing Spawn(Thing newThing, IntVec3 loc)
        {
            return GenSpawnAlien.SpawnModded(newThing, loc, Rot4.North);
        }

        public static Thing SpawnModded(Thing newThing, IntVec3 loc, Rot4 rot)
        {
            if (!loc.InBounds())
            {
                Log.Error(string.Concat(new object[]
                {
                    "Tried to spawn ",
                    newThing,
                    " out of bounds at ",
                    loc,
                    "."
                }));
                return null;
            }
            GenSpawn.WipeExistingThings(loc, rot, newThing.def, false);
            if (newThing.def.randomizeRotationOnSpawn)
            {
                newThing.Rotation = Rot4.Random;
            }
            else
            {
                newThing.Rotation = rot;
            }
            newThing.SetPositionDirect(IntVec3.Invalid);
            newThing.Position = loc;
            newThing.SpawnSetup();
            if (newThing.stackCount == 0)
            {
                Log.Error("Spawned thing with 0 stackCount: " + newThing);
                newThing.Destroy(DestroyMode.Vanish);
                return null;
            }
            if (newThing.def.GetType() != typeof(Thingdef_AlienRace))
            {
                return newThing;
            }
            else
            {
                AlienPawn alienpawn2 = newThing as AlienPawn;
                alienpawn2.SpawnSetupAlien(alienpawn2);
                if (alienpawn2.TryGetComp<CompImmuneToAge>() != null)
                {
                    alienpawn2.health.hediffSet.Clear();
                    PawnTechHediffsGenerator.GeneratePartsAndImplantsFor(alienpawn2);
                }
                Log.Message(alienpawn2.kindDef.race.ToString());
                return alienpawn2;
            }
        }        
    }
}
