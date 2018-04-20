using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class PsykerPowerManager : IExposable
    {
        private int psykerXP = 150;
        public int PsykerXP
        {
            get
            {
                return psykerXP;
            }
            set
            {
                this.psykerXP = Math.Max(value, 0);
            }
        }

        public CompPsyker psyComp;             

        public const int PowerSlotsIota = 4;
        public const int PowerSlotsZeta = 3;
        public const int PowerSlotsEpsilon = 2;
        public const int PowerSlotsDelta = 1;

        public List<PsykerPowerEntry> IotaPowers = new List<PsykerPowerEntry>();
        public List<PsykerPowerEntry> ZetaPowers = new List<PsykerPowerEntry>();
        public List<PsykerPowerEntry> EpsilonPowers = new List<PsykerPowerEntry>();
        public List<PsykerPowerEntry> DeltaPowers = new List<PsykerPowerEntry>();

        public List<PsykerPowerEntry> EquipmentPowers = new List<PsykerPowerEntry>();
        
        public PsykerPowerManager(CompPsyker psyComp)
        {
            this.psyComp = psyComp;
        }

        private CompPsyker CompPsyker
        {
            get
            {
                return psyComp;
            }
        }

        public void LearnPsykerPower(PsykerPowerDef psydef)
        {
            this.AddPsykerPower(psydef);
            this.PsykerXP -= PsykerUtility.PsykerXPCost[psydef.PowerLevel];
        }

        public void AddPsykerPower(PsykerPowerDef psydef, bool equipmentDependent = false, ThingDef depdef = null)
        {
            if (psydef.PowerLevel <= PsykerPowerLevel.Iota)
            {
                if (CheckAvailablePowerSlots(PsykerPowerLevel.Iota))
                {
                    this.IotaPowers.Add(new PsykerPowerEntry(psydef, equipmentDependent, depdef));
                }

            }
            else if (psydef.PowerLevel == PsykerPowerLevel.Zeta)
            {
                if (CheckAvailablePowerSlots(PsykerPowerLevel.Zeta))
                {
                    this.ZetaPowers.Add(new PsykerPowerEntry(psydef, equipmentDependent, depdef));
                }
            }
            else if (psydef.PowerLevel == PsykerPowerLevel.Epsilon)
            {
                if (CheckAvailablePowerSlots(PsykerPowerLevel.Epsilon))
                {
                    this.EpsilonPowers.Add(new PsykerPowerEntry(psydef, equipmentDependent, depdef));
                }
            }
            else if (psydef.PowerLevel == PsykerPowerLevel.Delta)
            {
                if (CheckAvailablePowerSlots(PsykerPowerLevel.Delta))
                {
                    this.DeltaPowers.Add(new PsykerPowerEntry(psydef, equipmentDependent, depdef));
                }
            }
            else
            {
                Log.Error("Tried to add Psyker power with invalid power level. Power will be discarded.");
            }
            this.CompPsyker.UpdatePowers();
        }

        public static readonly Dictionary<PsykerPowerLevel, int> PowerLevelSlots = new Dictionary<PsykerPowerLevel, int>{
            {PsykerPowerLevel.Iota, PowerSlotsIota },
            {PsykerPowerLevel.Zeta, PowerSlotsZeta },
            {PsykerPowerLevel.Epsilon, PowerSlotsEpsilon },
            {PsykerPowerLevel.Delta, PowerSlotsDelta },
            };


        
        public List<PsykerPowerEntry> GetPsykerPowerList(PsykerPowerLevel level)
        {
            switch(level)
            {
                case PsykerPowerLevel.Iota:
                    {
                        return this.IotaPowers;
                    }
                case PsykerPowerLevel.Zeta:
                    {
                        return this.ZetaPowers;
                    }
                case PsykerPowerLevel.Epsilon:
                    {
                        return this.EpsilonPowers;
                    }
                case PsykerPowerLevel.Delta:
                    {
                        return this.DeltaPowers;
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public bool CheckAvailablePowerSlots(PsykerPowerLevel leveltocheck)
        {
            int availableslots = (from entry in PowerLevelSlots where entry.Key == leveltocheck select entry.Value).FirstOrDefault();
            if (GetPsykerPowerList(leveltocheck).Count <= availableslots)
            {
                return true;
            }
            return false;
        }

        public void AddXP(float amount)
        {
            int adjustedXP = (int)(amount * 100);
            this.PsykerXP += Math.Abs(adjustedXP);
        }

        public void ExposeData()
        {

            Scribe_Values.Look<int>(ref this.psykerXP, "psykerXP", 0);
            Scribe_Collections.Look<PsykerPowerEntry>(ref this.IotaPowers, "IotaPowers", LookMode.Deep);
            Scribe_Collections.Look<PsykerPowerEntry>(ref this.ZetaPowers, "ZetaPowers", LookMode.Deep);
            Scribe_Collections.Look<PsykerPowerEntry>(ref this.EpsilonPowers, "EpsilonPowers", LookMode.Deep);
            Scribe_Collections.Look<PsykerPowerEntry>(ref this.DeltaPowers, "DeltaPowers", LookMode.Deep);
            Scribe_Collections.Look<PsykerPowerEntry>(ref this.EquipmentPowers, "EquipmentPowers", LookMode.Deep);

        }       

    }
}
