using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class SermonSpot
    {
        public Pawn Preacher;
        public IntVec3 AltarSpot;

        public SermonSpot(Pawn p, IntVec3 s)
        {
            this.Preacher = p;
            this.AltarSpot = s;
        }
    }
}
