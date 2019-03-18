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
        public List<ResourceEntry> Supplies = new List<ResourceEntry>();
        public List<FactionDef> AvailableFromFaction = new List<FactionDef>();
        public float RelationCost;
        
    }
}
