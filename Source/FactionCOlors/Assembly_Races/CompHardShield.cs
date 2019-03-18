using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FactionColors
{
    public class CompHardShield : ThingComp
    {
        public CompProperties_HardShield CProps
        {
            get
            {
                return this.props as CompProperties_HardShield;
            }
        }
        
    }
}
