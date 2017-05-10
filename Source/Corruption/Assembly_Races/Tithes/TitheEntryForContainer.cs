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
            Scribe_Values.LookValue<bool>(ref this.active, "active");
            Scribe_References.LookReference(ref this.Tithe, "Tithe");
        }
    }
}
