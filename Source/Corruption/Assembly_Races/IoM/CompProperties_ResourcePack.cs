using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class CompProperties_ResourcePack : CompProperties
    {
        public List<ThingFilter> filters = new List<ThingFilter>();
        public int Capacity = 500;

        public CompProperties_ResourcePack()
        {
            this.compClass = typeof(CompResourcePack);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            foreach (ThingFilter filter in this.filters)
            {
                filter.ResolveReferences();
            }
        }
    }
}
