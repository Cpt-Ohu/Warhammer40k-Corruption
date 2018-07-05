using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_SpawnThing : WonderWorker_Targetable
    {
        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetLocations = true,
                canTargetPawns = true,
                validator = ((TargetInfo x) => x.Cell.Standable(x.Map))
            };
        }

        protected override void TryDoEffectOnTarget(int worshipPoints)
        {
            for (int i = 0; i < this.Def.ThingsToSpawn.Count; i++)
            {
                ThingCount entry = this.Def.ThingsToSpawn[i];
                int countToCreate = entry.Count / entry.ThingDef.stackLimit;
                for (int j = 0; j < countToCreate; j++)
                {
                   Thing thing = ThingMaker.MakeThing(entry.ThingDef);
                   thing.stackCount = Math.Min(thing.def.stackLimit, countToCreate);
                   GenPlace.TryPlaceThing(thing, this.target.Cell, this.target.Map, ThingPlaceMode.Near);
                }
            }
        }
    }
}
