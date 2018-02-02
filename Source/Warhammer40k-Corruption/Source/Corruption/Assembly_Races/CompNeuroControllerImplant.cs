using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompNeuroControllerImplant : ThingComp
    {

        private bool Spawned
        {
            get
            {
                return this.parent.Spawned;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
        }

        public override void PostDraw()
        {
            base.PostDraw();
        }
    }
}
