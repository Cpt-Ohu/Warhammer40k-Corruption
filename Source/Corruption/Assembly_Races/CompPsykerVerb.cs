using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompPsykerVerb : ThingComp
    {

        public CompProperties_PsykerVerb warpprops
        {
            get
            {
                return (CompProperties_PsykerVerb)this.props;
            }
        }


        public VerbProperties_WarpPower warpverb
        {
            get
            {
                return warpprops.MainVerb;
            }
        }
    }
}
