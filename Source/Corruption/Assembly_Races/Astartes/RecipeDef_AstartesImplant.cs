using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class RecipeDef_MSU : RecipeDef
    {
        public HediffDef RequiresHediff;

        public RecipeTypeMSU RecipeType;
    }

    public enum RecipeTypeMSU
    {
        Standard,
        ServitorReprogram,
        AstartesImplant
    }
}
