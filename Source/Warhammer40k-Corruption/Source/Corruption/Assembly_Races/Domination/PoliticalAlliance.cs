using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Domination
{
    public class PoliticalAlliance : IExposable, ILoadReferenceable
    {
        private List<Faction> factionsInt = new List<Faction>();

        private Faction leadingFaction;

        private string loadID;

        private string nameInt;

        public PoliticalAlliance()
        {
            this.leadingFaction = null;
            this.AllianceName = NameGenerator.GenerateName(RulePackDefOf.NamerScenario, from fac in Find.FactionManager.AllFactionsVisible select fac.Name, false);
        }

        public PoliticalAlliance(string Name, int numericID, Faction leader = null)
        {
            if (leader != null)
            {
                this.leadingFaction = leader;
                this.factionsInt.Add(leader);
            }
            this.loadID = "DominationAlliance_" + numericID;
            this.nameInt = Name;
        }


        public Faction LeadingFaction
        {
            get
            {
                return this.leadingFaction;
            }
            set
            {
                this.leadingFaction = value;
            }                
        }

        public string AllianceName
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

        public List<Faction> GetFactions()
        {
            return this.factionsInt;
        }

        public void AddToAlliance(Faction faction)
        {
            this.factionsInt.Add(faction);
        }

        public void RemoveFromAlliance(Faction faction)
        {
            this.factionsInt.Remove(faction);
        }

        public bool IsPlayerAlliance
        {
            get
            {
                return this.factionsInt.Contains(Faction.OfPlayer);
            }
        }

        public void SetHostileTo(PoliticalAlliance alliance)
        {
            for (int i = 0; i < alliance.GetFactions().Count; i++)
            {
                for (int j = 0; j < this.GetFactions().Count; j++)
                {
                    Faction hostile = alliance.GetFactions()[i];
                    Faction member = this.GetFactions()[j];
                    hostile.SetHostileTo(member, true);
                }
            }
        }

        public bool HostileTo(PoliticalAlliance alliance)
        {
            return (this.LeadingFaction.HostileTo(alliance.LeadingFaction));
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<Faction>(ref this.factionsInt, "factionsInt", LookMode.Reference);
            Scribe_Values.Look<string>(ref this.nameInt, "nameInt", "");
            Scribe_Values.Look<string>(ref this.loadID, "loadID", "");
            Scribe_References.Look<Faction>(ref this.leadingFaction, "leadingFaction", false);
        }

        public string GetUniqueLoadID()
        {
            return this.loadID;
        }
    }
}
