using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FactionColors
{
    public class Deepstriker_ThingDef : ThingDef
    {
        public int TicksToExitMap = 200;

        public ThingDef IncomingDef;

        public ThingDef RemainingDef;

        public ThingDef LeavingDef;

        public List<FactionDef> BelongsToFactions;
    }
}
