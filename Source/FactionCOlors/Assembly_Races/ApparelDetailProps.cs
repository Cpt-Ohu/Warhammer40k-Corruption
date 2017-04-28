using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace FactionColors
{
    public class ApparelDetailProps : CompProperties
    {
        public float DetailChance;
        public bool IsHeadDetail = false;
        public bool IsFreeFloating = false;
        public List<ApparelDetail> ApparelDetails;
    }
}
