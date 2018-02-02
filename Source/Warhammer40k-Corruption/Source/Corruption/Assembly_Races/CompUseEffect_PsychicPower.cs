using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompUseEffect_PsychicPower : CompUseEffect
    {

        public override void DoEffect(Pawn usedBy)
        {
            Need_Soul soul;
                
            if ((soul = usedBy.needs.TryGetNeed<Need_Soul>()) != null)
            {
                soul.GainNeed(0.01f);
            }
        }
    }
}
