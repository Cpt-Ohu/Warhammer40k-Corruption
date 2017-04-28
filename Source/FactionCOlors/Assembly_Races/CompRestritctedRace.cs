using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FactionColors
{
    public class CompRestritctedRace : ThingComp
    {
        public CompProperties_RestrictedRace Props
        {
            get
            {
                return (CompProperties_RestrictedRace)this.props;
            }
        }
    }
}
