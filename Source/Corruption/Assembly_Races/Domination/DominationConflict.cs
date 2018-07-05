using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption.Domination
{
    public class DominationConflict : ILoadReferenceable, IExposable
    {
        public DominationConflict(PoliticalAlliance first, PoliticalAlliance second, string name = "", bool isMainConflict = false)
        {
            this.First = first;
            this.WarEfforts.Add(new AllianceWarEffort());
            this.Second = second;
            this.nameInt = name;
            this.IsMainConflict = isMainConflict;
        }

        public bool Finished;

        public bool IsMainConflict;

        private string nameInt;
        
        public string BattleName
        {
            get
            {
                return nameInt;
            }
            set
            {
                this.nameInt = value;
            }
        }

        public PoliticalAlliance First;

        public PoliticalAlliance Second;

        public List<AllianceWarEffort> WarEfforts = new List<AllianceWarEffort>();
        
        public void AdjustWarWearinessFor(PoliticalAlliance alliance, float amount)
        {
            AllianceWarEffort effort = this.GetWarEffort(alliance);
            if (!effort.isWeary)
            {
                effort.WarWeariness += amount;
                this.CheckWarWeariness(effort);
            }
        }
        
        public AllianceWarEffort GetWarEffort(PoliticalAlliance alliance)
        {
            return this.WarEfforts.FirstOrDefault(x => x.alliance == alliance);
        }

        public void CheckWarWeariness(AllianceWarEffort effort)
        {
            if (effort.WarWeariness >= 1f)
            {
                effort.isWeary = true;
                effort.WarWeariness = 1f;
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<AllianceWarEffort>(ref this.WarEfforts, "WarEfforts", LookMode.Deep);
            Scribe_Values.Look<int>(ref this.warId, "warID");
            Scribe_Values.Look<string>(ref this.nameInt, "nameInt");
            Scribe_References.Look<PoliticalAlliance>(ref this.First, "First");
            Scribe_References.Look<PoliticalAlliance>(ref this.Second, "Second");
            Scribe_Values.Look<bool>(ref this.IsMainConflict, "IsMainConflict");
        }

        private int warId = -1;

        private int LoadID
        {
            get
            {
                if (this.warId == -1)
                {
                    this.warId = CorruptionStoryTrackerUtilities.currentStoryTracker.DominationTracker.GetNextWarID();
                }
                return this.warId;
            }
        }
        public string GetUniqueLoadID()
        {
            return "DominationConflict_" + this.warId;
        }
    }
}
