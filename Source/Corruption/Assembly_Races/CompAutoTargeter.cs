using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
   public  class CompAutoTargeter : ThingComp
    {
        public CompProperties_AutoTargeter cProps
        {
            get
            {
                return this.props as CompProperties_AutoTargeter;
            }                
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }
    }
}
