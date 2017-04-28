using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompStartWithHediff : ThingComp
    {

        private CompProperties_StartWithHediff cprops
        {
            get
            {
               return this.props as CompProperties_StartWithHediff;
            }
        }

        private Pawn pawn
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            if (pawn != null && pawn.health != null)
            {
                foreach (HediffDef hdef in cprops.StartsWithHediffs)
                {
                    pawn.health.AddHediff(hdef);
                }
            }
        }

    }
}
