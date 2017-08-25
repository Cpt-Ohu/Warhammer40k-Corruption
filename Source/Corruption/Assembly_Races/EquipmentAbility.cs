using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public struct EquipmentAbilityStruct
    {
        public string Label;

        public string Description;

        public string IconPath;

        public HediffDef BuffDef;

        public bool TargetSelf;

        public int RechargeTime;

        public int Range;
    }
}
