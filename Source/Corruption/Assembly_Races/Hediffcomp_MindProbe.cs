using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class Hediffcomp_MindProbe : HediffComp
    {
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            Pawn attacker = dinfo.Value.Instigator as Pawn;
            if (CorruptionStoryTrackerUtilities.WinPskyerBattle(attacker, this.Pawn))
            {
                CompSoul soul = CompSoul.GetPawnSoul(this.Pawn);
                soul.DiscoverAlignment();
            }


        }
    }
}
