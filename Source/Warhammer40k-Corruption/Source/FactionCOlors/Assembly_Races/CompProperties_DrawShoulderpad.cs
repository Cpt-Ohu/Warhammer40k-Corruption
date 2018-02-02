using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace FactionColors
{
    public enum ShoulderPadType
    {
        Both,
        Right,
        Left
    }

    public struct ShoulderPadEntry
    {
        public ShoulderPadType shoulderPadType;
        public ShaderType shaderType;
        public string padTexPath;
        public int commonality;
        public bool UseSecondaryColor;
        public bool UseFactionTextures;
    }

    public class CompProperties_PauldronDrawer : CompProperties
    {
        public List<ShoulderPadEntry> PauldronEntries;
        public float PauldronEntryChance = 0.5f;
        public CompProperties_PauldronDrawer()
        {
            this.compClass = typeof(CompPauldronDrawer);
        }

    }
}
