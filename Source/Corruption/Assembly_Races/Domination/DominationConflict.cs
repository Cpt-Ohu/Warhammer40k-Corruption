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
        public DominationConflict()
        {
            this.First = null;
            this.Second = null;
            this.nameInt = "Minor War";
            this.IsMainConflict = false;
        }

        public DominationConflict(PoliticalAlliance first, PoliticalAlliance second, string name = "", bool isMainConflict = false)
        {
            this.First = first;
            this.WarEfforts.Add(new AllianceWarEffort(first));
            this.WarEfforts.Add(new AllianceWarEffort(second));
            this.Second = second;
            this.nameInt = name;
            this.IsMainConflict = isMainConflict;
            foreach (var factionFirst in first.GetFactions())
            {
                foreach (var factionSecond in second.GetFactions())
                {
                    CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(factionFirst, factionSecond, -100);
                    CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(factionSecond, factionFirst, -100);
                }
            }
        }

        public bool Finished;

        public ConflictResult Result;

        public PoliticalAlliance Winner { get; private set; }
        public PoliticalAlliance Loser { get; private set; }

        public bool IsMainConflict;

        public int DaysOfTruce;

        public int LastDiplomaticActionTick;

        private string nameInt;
        
        public string ConflictName
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
        
        public void AdjustWarWearinessFor(PoliticalAlliance alliance, float amount, int casualties)
        {
            AllianceWarEffort effort = this.GetWarEffort(alliance);
            effort.totalCasualties += casualties;
            if (!effort.isWeary)
            {
                effort.WarWeariness += amount;
                this.CheckWarWeariness(effort);
            }
        }

        public bool TruceActive => this.DaysOfTruce > 0;
        
        public AllianceWarEffort GetWarEffort(PoliticalAlliance alliance)
        {
            return this.WarEfforts.FirstOrDefault(x => x.alliance == alliance);
        }

        public bool IsWarParty(PoliticalAlliance alliance)
        {
            return this.First == alliance || this.Second == alliance;
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
            Scribe_Values.Look<ConflictResult>(ref this.Result, "Result");
            Scribe_Values.Look<bool>(ref this.IsMainConflict, "IsMainConflict");
        }

        private int warId = -1;

        private int LoadID
        {
            get
            {
                if (this.warId == -1)
                {
                    this.warId = CFind.StoryTracker.DominationTracker.GetNextWarID();
                }
                return this.warId;
            }
        }
        public string GetUniqueLoadID()
        {
            return "DominationConflict_" + this.warId;
        }

        internal void SetResult(ConflictResult result, PoliticalAlliance winner)
        {
            this.Result = result;
            AllianceWarEffort winnerEffort = this.GetWarEffort(winner);
            winnerEffort.Won = true;
            AllianceWarEffort loserEffort = this.WarEfforts.FirstOrDefault(x => x.alliance != winner);
            loserEffort.Won = false;

            if (result == ConflictResult.Subjugation)
            {
                winner.AddSubjugatedAlliance(loserEffort.alliance);
            }

        }

        internal void ActivateTruce()
        {
            this.DaysOfTruce = 10;
        }
    }
}
