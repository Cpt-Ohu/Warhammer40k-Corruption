using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class PatronInfo
    {
        public string PatronName = "Emperor";

        public List<SoulTrait> PatronTraits = new List<SoulTrait>();
        

        public static Color PatronColor(String PatronName)
        {
            switch (PatronName)
            {
                case "Emperor":
                    return new Color(0.93f, 0.78f, 0.19f);

                case "Undivided":
                    return new Color(1f, 1f, 1f);

                case "Khorne":
                    return new Color(0.7f, 0f, 0f);

                case "Nurgle":
                    return new Color(0.43f, 0.78f, 0.43f);

                case "Tzeentch":
                    return new Color(0.42f, 0.7f, 1f);

                case "Slaanesh":
                    return new Color(1f, 0.5f, 1f);

                case "Eldar":
                    return new Color(0, 42f, 0.08f, 44f);

                case "Orks":
                    return new Color(0f, 0.9f, 0f);

                case "Tau":
                    return new Color(1f, 0.61f, 0f);

                case "Ctan":
                    return new Color(0f, 0.69f, 0.61f);
            }
            return new Color(1f, 1f, 1f);
        }


        public void GetPatronTraits(string patName)
        {
            switch (patName)
            {
                case "Undivided":
                    this.PatronTraits.Add(new SoulTrait(C_SoulTraitDefOf.Undivided_Fervor, 0));
                    return;

                case "Khorne":
                    this.PatronTraits.Add(new SoulTrait(C_SoulTraitDefOf.Khorne_Fervor, 0));
                    return;

                case "Nurgle":
                    this.PatronTraits.Add(new SoulTrait(C_SoulTraitDefOf.Nurgle_Fervor, 0));
                    return;

                case "Tzeentch":
                    this.PatronTraits.Add(new SoulTrait(C_SoulTraitDefOf.Tzeentch_Fervor, 0));
                    return;
                case "Slaanesh":
                    this.PatronTraits.Add(new SoulTrait(C_SoulTraitDefOf.Slaanesh_Fervor, 0) );
                    return;

                case "Emperor":
                    return;

                default:
                    return;         
            
            }
        }

        public SoulTraitDef PatronSpecificTrait(string patName)
        {
            switch(patName)
            {
                case "Undivided":
                    return C_SoulTraitDefOf.Undivided_Fervor;

                case "Khorne":
                    return C_SoulTraitDefOf.Khorne_Fervor;

                case "Nurgle":
                    return C_SoulTraitDefOf.Nurgle_Fervor;

                case "Tzeentch":
                    return C_SoulTraitDefOf.Tzeentch_Fervor;

                case "Slaanesh":
                    return C_SoulTraitDefOf.Slaanesh_Fervor;

                default:
                    return C_SoulTraitDefOf.Devotion;

            }
        }

    }
}
