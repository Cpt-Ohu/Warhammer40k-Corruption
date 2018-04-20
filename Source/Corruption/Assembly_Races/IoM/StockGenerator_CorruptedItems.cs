using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class StockGenerator_CorruptedItems : StockGenerator
    {

        [DebuggerHidden]
        public override IEnumerable<Thing> GenerateThings(int forTile)
        {
            ThingDef def = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => this.HandlesThingDef(x)).RandomElement();

            IEnumerator<Thing> enumerator = StockGeneratorUtility.TryMakeForStock(def, base.RandomCountOf(def)).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Thing current = enumerator.Current;
                yield return current;
            }
            yield break;
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.defName.Contains("MeleeWeapon_Corrupted");
        }
    }
}
