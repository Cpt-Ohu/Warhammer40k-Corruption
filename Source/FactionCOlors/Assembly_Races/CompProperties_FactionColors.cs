using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using System.Text;

namespace FactionColors
{
    public class CompProperties_FactionColor : CompProperties
    {

        public bool UseFactionColor = true;        

        public bool UseCamouflageColor = false;

        public bool TryUseSecondaryStuffColors = false;

        public bool UseSecondaryColors = false;

        public bool IsRandomMultiGraphic = false;

        public List<Pair<string, int>> RandomGraphicPaths = new List<Pair<string, int>>();

        public ColorGenerator_Options Coloring = new ColorGenerator_Options();

        public CompProperties_FactionColor()
        {
            this.compClass = typeof(CompFactionColor);
        }
    }
}
