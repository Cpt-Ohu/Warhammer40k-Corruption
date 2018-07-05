using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Servitors
{
    public class HediffCompProperties_ReprogramServitor : HediffCompProperties
    {
        public List<SkillDef> skillsToGain = new List<SkillDef>();

        public int SkillLevel;
    }
}
