using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class PsykerPowerDef : ThingDef
    {
        public string DivineOrigin;

        public PsykerPowerLevel PowerLevel = PsykerPowerLevel.Iota;

        public SoulAffliction MinAfflictionToGet = SoulAffliction.Warptouched;

        public SoulAffliction MaxAfflictionToGet = SoulAffliction.Lost;

        public int RechargeTicks;

        public int CastTime = 0;

        public SoulAffliction MinAfflictionCategory;

        public float CorruptionFactor;

        public string IconGraphicPath = "UI/Psyker/PowerOff";

        public Texture2D iconTex;
        
        public VerbProperties_WarpPower MainVerb;
        
        public AIPsykerPowerCategory AICategory = AIPsykerPowerCategory.DamageDealer;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.iconTex = ContentFinder<Texture2D>.Get(this.IconGraphicPath, true);
            });
        }
    }
}
