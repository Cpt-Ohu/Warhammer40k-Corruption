using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class Projectile_WarpRecruiter : Projectile_WarpPower
    {

        protected override void Impact(Thing hitThing)
        {
            this.Destroy(DestroyMode.Vanish);

            if (CanOverpowerMind(this.Caster, hitThing))
            {
                Pawn victim = hitThing as Pawn;
                InteractionWorker_RecruitAttempt.DoRecruit(this.Caster, victim, 1f, true);
            }

        }
    }
}
