using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    public static class CamouflageColorsUtility
    {
        public static Color[] CamouflageColors
        {
            get
            {
                Color[] cols = new Color[2];
                CamouflageColorsUtility.GetCamouflageColors(out cols[0], out cols[1]);
                return cols;
            }
        }

        public static void GetCamouflageColors(out Color col1, out Color col2)
        {
            BiomeDef biomedef = Find.VisibleMap.Biome;


            col1 = new Color(0.30f, 0.46f, 0.30f);
            col2 = new Color(0.4f, 0.34f, 0.24f);

            if (Find.VisibleMap.snowGrid.TotalDepth > 0 || biomedef == BiomeDefOf.IceSheet || biomedef.defName == "SeaIce")
            {
                col1 = Color.white;
                col2 = Color.grey;
            }

            if (biomedef == BiomeDefOf.Desert || biomedef == BiomeDefOf.AridShrubland || biomedef.defName == "ExtremeDesert")
            {
                col1 = new Color(0.91f, 0.82f, 0.69f);
                col2 = new Color(0.75f, 0.71f, 0.56f);
            }
            return;
        }
    }
}
