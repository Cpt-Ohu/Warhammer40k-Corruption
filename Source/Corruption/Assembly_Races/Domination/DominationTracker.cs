using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.Domination
{
    public class DominationTracker : IExposable
    {
        public List<IBattleZone> battleZones = new List<IBattleZone>();
        
        public PoliticalAlliance ImperiumOfMan;

        private int NextAllianceLoadID = 0;        
                        
        private List<PoliticalAlliance> politicalAlliancesInt = new List<PoliticalAlliance>();

        public void GetAllianceByName(string name)
        {

        }

        public void GetAllianceOfFaction(Faction faction)
        {

        }

        public PoliticalAlliance PlayerAlliance
        {
            get
            {
                return this.politicalAlliancesInt.FirstOrDefault(x => x.IsPlayerAlliance);
            }
        }

        public PoliticalAlliance GetRandomAlliance()
        {
            return this.politicalAlliancesInt.RandomElement();
        }

        public void AddNewAlliance(string Name, Faction leader = null)
        {
            PoliticalAlliance alliance = new PoliticalAlliance(Name, GetNextAllianceID(), leader);
            this.politicalAlliancesInt.Add(alliance);            
        }

        public void CreateImperiumOfManAlliance()
        {
            PoliticalAlliance ioM = new PoliticalAlliance("IoM".Translate(), GetNextAllianceID(), Find.FactionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.IoM_NPCFaction));
            foreach (Faction fac in CorruptionStoryTrackerUtilities.currentStoryTracker.ImperialFactions)
            {
                ioM.AddToAlliance(fac);
            }
            this.politicalAlliancesInt.Add(ioM);
        }

        public int GetNextAllianceID()
        {
            this.NextAllianceLoadID++;
            return this.NextAllianceLoadID;
                
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<PoliticalAlliance>(ref this.politicalAlliancesInt, "politicalAlliancesInt", LookMode.Deep);
            Scribe_Deep.Look<PoliticalAlliance>(ref this.ImperiumOfMan, "ImperiumOfMan", new object[0]);
            Scribe_Collections.Look<IBattleZone>(ref this.battleZones, "battleZones", LookMode.Reference);
            Scribe_Values.Look<int>(ref this.NextAllianceLoadID, "NextAllianceLoadID");            
        }
    }
}
