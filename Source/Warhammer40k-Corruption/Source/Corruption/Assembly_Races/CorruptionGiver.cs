using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public  abstract class CorruptionGiver
    {
            public CorruptionGiverDef def;

            private static List<Thing> tmpCandidates;
        
            protected List<Thing> ThingsOfRequiredThingDefs
            {
                get
                {
                    if (def.thingDefs == null)
                    {
                        CorruptionGiver.tmpCandidates.Clear();
                        return CorruptionGiver.tmpCandidates;
                    }
                    if (def.thingDefs.Count == 1)
                    {
                        return Find.ListerThings.ThingsOfDef(def.thingDefs[0]);
                    }
                    CorruptionGiver.tmpCandidates.Clear();
                    for (int index = 0; index < def.thingDefs.Count; ++index)
                    {
                    CorruptionGiver.tmpCandidates.AddRange(Find.ListerThings.ThingsOfDef(def.thingDefs[index]));
                    }
                    return CorruptionGiver.tmpCandidates;
                }
            }

            static CorruptionGiver()
            {
            CorruptionGiver.tmpCandidates = new List<Thing>();
            }

            protected CorruptionGiver() : base()
            {
            }

            public virtual float GetChance(Pawn pawn)
            {
                return def.baseChance;
            }

            public abstract Job TryGiveJob(Pawn pawn);

        }
    
}
