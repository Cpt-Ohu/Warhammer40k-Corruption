using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Corruption
{
    public class SoulTraitDegreeData : TraitDegreeData
    {
        public float CorruptionResistanceFactor = 1f;
        public float PrayerJoyGainFactor = 1f;
        public List<PsykerPowerDef> psykerPowersToUnlock = new List<PsykerPowerDef>();
    }
}
