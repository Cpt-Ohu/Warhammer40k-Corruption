using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class WarpPowerUtilities
    {
        public static void DivinePronouncement(Pawn target)
        {
            target.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee);
        }


    }
}
