using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Corruption.BookStuff
{
    public class ReadableEffektEntry
    {
        public float ReadThreshold;
        public bool AffectOnlyOnce = true;
        public ReadableEffectCategory readableEffectCategory;
        public PsykerPowerDef PsykerPowerUnlocked;
        public HediffDef HediffGained;
        public MentalStateDef MentalBreak;
    }
}
