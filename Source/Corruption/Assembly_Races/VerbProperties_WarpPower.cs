using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class VerbProperties_WarpPower : VerbProperties
    {
        public bool DrawProjectileOnTarget = true;

        public bool AlwaysHits = true;

        public bool HarmsCaster;
        public float CasterDamage = 0f;

        public float CorruptionFactor = 1f;

        public int TicksToRecharge = 600;

        public PsykerPowerLevel PowerLevelToUnlock;

        public PsykerPowerTargetCategory PsykerPowerCategory = PsykerPowerTargetCategory.TargetThing;
        public Type AoETargetClass;

        public bool ReplacesStandardAttack;

        public List<StatModifier> statModifiers;

    }
}
