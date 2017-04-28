using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class IncidentWorker_CorruptedEffigy : IncidentWorker
    {
        private IEnumerable<Pawn> PotentialVictims(Map map)
        {
            return map.mapPawns.FreeColonistsAndPrisoners.Where(delegate (Pawn p)
            {
                if (p.holdingContainer != null && p.holdingContainer.owner is Building_CryptosleepCasket)
                {
                    return false;
                }
                Need_Soul soul = p.needs.TryGetNeed<Need_Soul>();
                if (soul != null)
                {
                    if (soul.CurLevel < 0.9f)
                    {
                        return true;
                    }
                }
                return false;
            });
        }


        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            return this.PotentialVictims((Map)target).Any<Pawn>();
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            Pawn pawn = this.PotentialVictims(map).RandomElement();
            ThingDef defToMake = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.defName.Contains("Effigy_")).RandomElement();
            Thing effigy = ThingMaker.MakeThing(defToMake, GenStuff.RandomStuffFor(defToMake));
            pawn.inventory.TryAddItemNotForSale(effigy);
            return true;
            
        }
    }
}
