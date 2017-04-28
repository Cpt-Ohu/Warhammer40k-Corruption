using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Tithes
{
    public class TitheDef : Def
    {
        public float wealthFactor = 1f;

        public List<ThingCategoryDef> categoryDefs = new List<ThingCategoryDef>();

        public List<ThingDef> fixedTitheThings = new List<ThingDef>();

        public List<ThingDef> excludedTitheThings = new List<ThingDef>();

        public bool optionalTithe = false;
    }
}
