using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Corruption.DefOfs;

namespace Corruption
{
    public class HediffComp_NurglesMark : HediffComp
    {
        private int lastTick;

        public override void Notify_PawnDied()
        {
            if (this.Pawn.Corpse.Spawned)
            {
                GenExplosion.DoExplosion(this.Pawn.Position, this.Pawn.Map, 5, C_DamageDefOf.RottenBurst ,null, 0, null, null, null, ThingDefOf.FilthVomit, 1);
                Pawn.Corpse.Destroy(DestroyMode.Vanish);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (lastTick < Find.TickManager.TicksGame + 6000)
            {
                FilthMaker.MakeFilth(this.Pawn.DrawPos.ToIntVec3(), this.Pawn.Map, ThingDefOf.FilthVomit, 1);
                lastTick = Find.TickManager.TicksGame;
            }
        }
    }
}
