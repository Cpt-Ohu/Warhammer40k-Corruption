using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderWorker_AddHediff : WonderWorker_Targetable
    {
        protected override TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                canTargetItems = false,
                canTargetLocations = false
            };
        }

        protected override void TryDoEffectOnTarget(int worshipPoints)
        {
            for (int i = 0; i < this.Def.HediffsToAdd.Count; i++)
            {
                Pawn pawn = (Pawn)this.target.Thing;
                pawn.health.AddHediff(this.Def.HediffsToAdd[i]);
            }
        }
    }
}
