using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Corruption
{
    public class AfflictionProperty
    {
        public float LowerAfflictionLimit = 0.4f;
        public float UpperAfflictionLimit = 0.99f;
        public bool IsImmune = false;
        public int ImmuneDevotionDegree;
        public float ResolveFactor = 1f;
        public ChaosGods Patron = ChaosGods.Undivided;
        public bool UseOtherFaith = false;
        public OtherFaiths IsofFaith = OtherFaiths.Emperor;
        public SoulTraitDef CommonSoulTrait;
        public CulturalToleranceCategory PrimaryToleranceCategory = CulturalToleranceCategory.Neutral;
        public PsykerPowerLevel UpperPsykerPowerLimit = PsykerPowerLevel.Epsilon;
        public PsykerPowerLevel LowerPsykerPowerLimit = PsykerPowerLevel.Omicron;
        public List<PsykerPowerDef> CommmonPsykerPowers = new List<PsykerPowerDef>();

    }
}
