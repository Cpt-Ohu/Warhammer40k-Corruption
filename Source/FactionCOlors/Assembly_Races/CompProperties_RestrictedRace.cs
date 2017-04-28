using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FactionColors
{
    public class CompProperties_RestrictedRace : CompProperties
    {
        public string RestrictedToRace = "Human";

        public CompProperties_RestrictedRace()
        {
            this.compClass = typeof(CompRestritctedRace);
        }
    }
}
