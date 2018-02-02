using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Corruption
{
    public class ThoughtDefCorruption : ThoughtDef
    {
        public bool IsAutomatonThought;
        public List<SoulTraitDef> requiredSoulTraits = new List<SoulTraitDef>();
    }
}
