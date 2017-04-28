using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Corruption;

namespace FactionColors
{
    public class Subfaction
    {
        public string SubfactionName;
        public string SubfactionLabel;
        public string SubfactionDescription;
        public Color SubfactionColor1;
        public Color SubfactionColor2;
        public List<PawnGroupMaker> SubfactionPawnGroupMakers;
        public RulePackDef SubfactionNameMaker;
        public float weight;
        public ChaosGods SubfactionPreferredChaosGod = ChaosGods.Undivided;
    }
}
