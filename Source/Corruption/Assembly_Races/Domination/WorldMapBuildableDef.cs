using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Domination
{
    public class WorldMapBuildableDef : Def
    {
        public WorldObjectDef WorldObjectDef;

        public List<ThingDefCountClass> Cost = new List<ThingDefCountClass>();

        public int ConstructionTimeDays = 20;


        public Gizmo BuildCommand(int tile)
        {
            Command_Action command = new Command_Action();
            command.action = delegate
            {
                UnfinishedWorldObject unfinishedWorldObject = (UnfinishedWorldObject)WorldObjectMaker.MakeWorldObject(DefOfs.C_WorldObjectDefOf.UnfinishedWorldObject);
                unfinishedWorldObject.Tile = tile;
                Find.WorldObjects.Add(unfinishedWorldObject);
                unfinishedWorldObject.StartConstruction(tile, WorldObjectDef);
            };
            return command;
        }
    }
}

