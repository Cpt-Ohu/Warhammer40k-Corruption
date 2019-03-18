using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class AllPlanetFactionMission : Mission
    {
        public List<Faction> FinishedFactions = new List<Faction>();
        public List<Faction> RequiredFactions = new List<Faction>();

        public AllPlanetFactionMission() : base()
        {
            foreach (var faction in Find.FactionManager.AllFactions.Where(x => x.def.hidden == false))
            {
                this.RequiredFactions.Add(faction);
            }
        }

        public void FinishForFaction(Faction faction)
        {
            if (this.FinishedFactions.Contains(faction))
            {
                this.FinishedFactions.Add(faction);
                if (RequiredFactions.Count == FinishedFactions.Count)
                {
                    this.Finish();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Faction>(ref this.FinishedFactions, "FinishedFactions");
            Scribe_Collections.Look<Faction>(ref this.RequiredFactions, "RequiredFaction");
        }
    }
}
