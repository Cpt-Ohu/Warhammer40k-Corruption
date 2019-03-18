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
            CompSoul soul;
                
            if ((soul = CompSoul.GetPawnSoul(usedBy)) != null)
            {
                soul.AffectSoul(0.01f);
            }
        }
    }
}
