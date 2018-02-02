using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
   public class PsykerPowerManager : IExposable
    {
        public void AddPsykerPower(PsykerPowerDef psydef, bool equipmentDependent = false, ThingDef depdef = null)
        {
            if (!this.compPsyker.learnedPowers.Any(x => x.def.defName == psydef.defName))
            {
                this.compPsyker.learnedPowers.Add(new PsykerPower(this.compPsyker.psyker, psydef));
            }
            if (!this.compPsyker.allpsykerPowers.Any(x => x.psykerPowerDef.defName == psydef.defName))
            {
                this.compPsyker.allpsykerPowers.Add(new PsykerPowerEntry(psydef, equipmentDependent, depdef));
            }

            this.compPsyker.UpdatePowers();
        }

        public int PowerSlotsIota = 4;
        public int PowerSlotsZeta = 3;
        public int PowerSlotsEpsilon = 2;
        public int PowerSlotsDelta = 1;

        public PsykerPowerManager(CompPsyker compPsyker)
        {
            this.compPsyker = compPsyker;
        }

        public void Initialize()
        {
            this.PowerLevelSlots = new Dictionary<PsykerPowerLevel, int>();
            this.PowerLevelSlots.Add(PsykerPowerLevel.Iota, PowerSlotsIota);
            this.PowerLevelSlots.Add(PsykerPowerLevel.Zeta, PowerSlotsZeta);
            this.PowerLevelSlots.Add(PsykerPowerLevel.Epsilon, PowerSlotsEpsilon);
            this.PowerLevelSlots.Add(PsykerPowerLevel.Delta, PowerSlotsDelta);
        }

        private CompPsyker compPsyker;

        public Dictionary<PsykerPowerLevel, int> PowerLevelSlots = new Dictionary<PsykerPowerLevel, int>();
        
        public void PsykerPowerManagerTick()
        {
        }

        public bool CheckAvailablePowerSlots(PsykerPowerLevel leveltocheck)
        {
            int powers = compPsyker.Powers.FindAll(x => x.powerdef.PowerLevel == leveltocheck).Count;
            int availableslots = (from entry in PowerLevelSlots where entry.Key == leveltocheck select entry.Value).FirstOrDefault();
            if (powers <= availableslots)
            {
                return true;
            }
            return false;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<PsykerPowerLevel, int>(ref this.PowerLevelSlots, "PowerLevelSlots", LookMode.Value, LookMode.Value);
  //          Scribe_Deep.Look<CompPsyker>(ref this.compPsyker, "compPsyker", null);
            Scribe_Values.Look<CompPsyker>(ref this.compPsyker, "compPsyker", null);
        }

        public List<PsykerPower> powersint = new List<PsykerPower>();

        public List<PsykerPower> Powers = new List<PsykerPower>();

    }
}
