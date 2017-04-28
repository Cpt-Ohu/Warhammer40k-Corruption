using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Corruption
{
    public class PsykerPowerEntry : IExposable
    {
        public PsykerPowerEntry(PsykerPowerDef power, bool equipmentDependent = false, ThingDef depdef = null)
        {
            this.psykerPowerDef = power;
            if (depdef != null)
            {
                this.DependendOn = depdef;
            }
            if (equipmentDependent)
            {
                this.EquipmentDependent = true;
            }
        }

        public PsykerPowerDef psykerPowerDef;

        public bool EquipmentDependent;

        public ThingDef DependendOn;

        public void ExposeData()
        {
            Scribe_Values.LookValue<bool>(ref this.EquipmentDependent, "EquipmentDependent", false, false);
            Scribe_Defs.LookDef<ThingDef>(ref this.DependendOn, "DependendOn");
            Scribe_Defs.LookDef<PsykerPowerDef>(ref this.psykerPowerDef, "psykerPowerDef");
        }
    }
}
