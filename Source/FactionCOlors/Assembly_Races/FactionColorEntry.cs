using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class FactionColorEntry : IExposable
    {
        public void ExposeData()
        {
            Scribe_References.Look<Faction>(ref this.Faction, "Faction");
            Scribe_Values.Look<Color>(ref this.FactionColor1, "MainColor");
            Scribe_Values.Look<Color>(ref this.FactionColor2, "SecColor");
        }
        
        public FactionColorEntry()
        {

        }

        public FactionColorEntry(Faction faction, Color main, Color second)
        {
            this.Faction = faction;
            this.FactionColor1 = main;
            this.FactionColor2 = second;
        }

        public Faction Faction;

        public Color FactionColor1;

        public Color FactionColor2;


    }
}
