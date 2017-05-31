using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Tithes
{
    public class TitheEntryForContainer : IExposable
    {
        public TitheEntryGlobal Tithe;

        public bool active = false;

        public TitheEntryForContainer()
        {
            this.Tithe = new TitheEntryGlobal();
        }

        public TitheEntryForContainer(TitheEntryGlobal tithe)
        {
            this.Tithe = tithe;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.active, "active");
            Scribe_References.Look(ref this.Tithe, "Tithe");
        }
    }
}
