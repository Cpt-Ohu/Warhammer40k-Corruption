using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_AutomatonThoughts : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.AllComps.Any(i => i.GetType() == typeof(CompServitor)))
            {
                return true;
            }
            return false;
        }
    }
}
