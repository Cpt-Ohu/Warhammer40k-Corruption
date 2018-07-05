using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class HediffComp_NurglesRot : HediffComp
    {

        private Pawn Victim;

        private Need_Soul soul
        {
            get
            {
                Need_Soul soulInt;
                if ((soulInt = this.Pawn.needs.TryGetNeed<Need_Soul>()) != null)
                    return soulInt;
                else
                {
                    return new Need_Soul(this.Pawn);
                }
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            this.Victim = this.Pawn;           
        }

        public override  void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (this.Pawn.def.race.Humanlike)
            {
                soul.GainNeed(-0.00005f);
                if (this.parent.Severity > 0.4f)
                {
                    if (soul.CurLevel < 0.5f)
                    {
                        this.Pawn.health.AddHediff(C_HediffDefOf.MarkNurgle);
                        soul.GainPatron(PatronDefOf.Nurgle, true);
                        soul.GainNeed(-0.3f);
                        this.parent.Heal(1f);
                    }
                }
            }
        }

        public override void Notify_PawnDied()
        {
            if (this.Pawn.Corpse.Spawned)
            {
                GenExplosion.DoExplosion(this.Pawn.Position, this.Pawn.Corpse.Map, 1, C_DamageDefOf.RottenBurst, null, 1, null, null, null, ThingDefOf.FilthVomit, 1);
            }
        }

    }
}
