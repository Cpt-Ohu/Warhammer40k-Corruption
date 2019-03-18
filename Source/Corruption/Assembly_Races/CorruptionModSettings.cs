using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CorruptionModSettings : ModSettings
    {
        public static bool AllowFactions = true;
        public static bool AllowPsykers = true;
        public static bool AllowDropships = true;
        public static bool AllowDomination = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref AllowFactions, "AllowFactions", true);
            Scribe_Values.Look(ref AllowPsykers, "AllowPsykers", true);
            Scribe_Values.Look(ref AllowDropships, "AllowDropships", true);
            Scribe_Values.Look(ref AllowDomination, "AllowDomination", true);
        }
    }
}
