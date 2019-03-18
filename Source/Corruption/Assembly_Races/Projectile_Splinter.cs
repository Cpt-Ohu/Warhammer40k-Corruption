using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class Projectile_Splinter : Bullet
    {
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            Pawn pawn = hitThing as Pawn;
            if ( pawn != null && !pawn.health.hediffSet.HasHediff(C_HediffDefOf.DE_Toxin))
            {
                Hediff hediff = HediffMaker.MakeHediff(C_HediffDefOf.DE_Toxin, pawn, null);
                hediff.Severity = 0.01f;
                pawn.health.AddHediff(hediff, null, null);
                
            }
        }
    }
}
