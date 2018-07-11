using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class SupplyDropDef : Def
    {
        public List<ResourcePackEntry> Supplies = new List<ResourcePackEntry>();
        public List<FactionDef> AvailableFromFaction = new List<FactionDef>();
        public float RelationCost;
        
    }
}
