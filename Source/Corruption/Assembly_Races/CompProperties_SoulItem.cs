using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompProperties_SoulItem : CompProperties
    {
        public SoulItemCategories Category = SoulItemCategories.Neutral;

        public List<PsykerPowerDef> UnlockedPsykerPowers = new List<PsykerPowerDef>();

        public List<VerbProperties_WarpPower> UnlockedPsykerVerbs;

        public float GainRate = 0f;

        public CompProperties_SoulItem()
        {
            this.compClass = typeof(ThingComp_SoulItem);
        }

        public bool IsHolyItem = false;

    }
}
