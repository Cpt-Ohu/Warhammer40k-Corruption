using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class Projectile_WarpGrower : Projectile_WarpPower
    {
        protected override void Impact(Thing hitThing)
        {
            this.Destroy(DestroyMode.Vanish);
            if (hitThing != null)
            {
                if (hitThing.GetType() == typeof(Plant))
                {
                    Plant p = hitThing as Plant;
                    p.Growth = 1f;
                }

                this.Map.mapDrawer.SectionAt(hitThing.Position).RegenerateAllLayers();
            }
        }

        public override void Tick()
        {
            try
            {
                base.Tick();
            }
            catch
            {
            }
        }
    }
}
