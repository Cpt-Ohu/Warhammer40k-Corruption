using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Domination
{
    public class BattleResult : IExposable
    {
        public PoliticalAlliance Winner;

        public PoliticalAlliance Loser;

        public string BattleName;

        public string BattleDescription;

        public Dictionary<Faction, int> ArmyStrength;

        public Dictionary<Faction, int> Casualties;

        public Tale BattleTale;

        public BattleResult()
        {
            this.ArmyStrength = new Dictionary<Faction, int>();
            this.Casualties = new Dictionary<Faction, int>();
        }

        public void ResolveFactions(PoliticalAlliance winner, PoliticalAlliance loser, Map map)
        {
            this.Winner = winner;
            this.Loser = loser;
            this.CalculateCasualties(map);
        }

        private void CalculateCasualties(Map map)
        {
            foreach (Faction current in this.Winner.GetFactions())
            {
                if (!this.Casualties.ContainsKey(current))
                {                    
                    this.Casualties.Add(current, DominationUtilities.CasualtiesForFaction(current, map));                    
                }
            }

            foreach (Faction current in this.Loser.GetFactions())
            {
                if (!this.Casualties.ContainsKey(current))
                {
                    this.Casualties.Add(current, DominationUtilities.CasualtiesForFaction(current, map));
                }
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look<PoliticalAlliance>(ref this.Winner, "Winner");
            Scribe_References.Look<PoliticalAlliance>(ref this.Loser, "Loser");
            Scribe_Values.Look<string>(ref this.BattleName, "BattleName");
            Scribe_Values.Look<string>(ref this.BattleDescription, "BattleDescription");
            Scribe_Collections.Look(ref this.ArmyStrength, "ArmyStrength", LookMode.Reference);
            Scribe_Collections.Look(ref this.Casualties, "Casualties", LookMode.Reference);

        }
    }
}
