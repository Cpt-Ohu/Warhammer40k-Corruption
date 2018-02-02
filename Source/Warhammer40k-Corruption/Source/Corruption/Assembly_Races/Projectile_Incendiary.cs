using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class Projectile_Incendiary : Bullet
    {
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            if (hitThing != null)
            {
                hitThing.TryAttachFire(0.2f);
            }
            else
            {
                GenSpawn.Spawn(ThingDefOf.FilthFuel, base.Position); 
                FireUtility.TryStartFireIn(base.Position, 0.2f);
            }
            MoteMaker.MakeStaticMote(this.Position, ThingDefOf.Mote_ShotFlash, 6f);
            MoteMaker.ThrowMicroSparks(base.Position.ToVector3Shifted());
        }
    }
}
