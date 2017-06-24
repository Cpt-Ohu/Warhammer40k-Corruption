using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption
{
    public class ChaosFollowerPawnKindDef : PawnKindDef
    {
        public AfflictionProperty AfflictionProperty;

        public bool UseForcedPatron = false;

        public bool RenamePawns = false;

        public bool UseFixedGender = false;

        public Gender FixedGender;

        public RulePackDef OverridingNameRulePack;

        public IntRange AdditionalImplantCount = new IntRange(0, 0);
        
        public List<HediffDef> ForcedStartingHediffs = new List<HediffDef>();

        public List<HediffDef> DisallowedStartingHediffs = new List<HediffDef>();

        public List<RecipeDef> ForcedStartingImplantRecipes = new List<RecipeDef>();
    }   
    
}
