using Corruption.ProductionSites;
using RimWorld;
using RimWorld.Planet;
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

        private List<Faction> subjugatedFactions = new List<Faction>();

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
            CFind.DominationTracker.NotifyFactionJoinedAlliance(this, faction);
        }

        public void RemoveFromAlliance(Faction faction, bool turnHostile)
        {
            this.factionsInt.Remove(faction);
            CFind.DominationTracker.NotifyFactionLeftAlliance(this, faction, turnHostile);
        }

        public bool IsPlayerAlliance
        {
            get
            {
                return this.factionsInt.Contains(Faction.OfPlayer);
            }
        }

        public bool PlayerIsLeader
        {
            get
            {
                return this.LeadingFaction == Faction.OfPlayer;
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
                    hostile.TrySetRelationKind(member, FactionRelationKind.Ally);
                }
            }
        }

        public float CombinedPowerAgainst(PoliticalAlliance alliance)
        {
            float accumulated = 0f;
            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects.FindAll(x => this.factionsInt.Contains(x.Faction));
            foreach (var settlement in worldObjects)
            {
                ProductionSites.ProductionSite site = settlement as ProductionSites.ProductionSite;
                if (site != null)
                {
                    IEnumerable<WorkForce> wf = site.TotalWorkForce.ToList();
                    accumulated += wf.Sum(x => x.PawnKind.combatPower);
                }
            }
            return accumulated;
        }

        public bool HostileTo(PoliticalAlliance alliance)
        {            
            return (this.LeadingFaction.HostileTo(alliance.LeadingFaction));
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<Faction>(ref this.factionsInt, "factionsInt", LookMode.Reference);
            Scribe_Collections.Look<Faction>(ref this.subjugatedFactions, "subjugatedFactions", LookMode.Reference);
            Scribe_Values.Look<string>(ref this.nameInt, "nameInt", "");
            Scribe_Values.Look<string>(ref this.loadID, "loadID", "");
            Scribe_References.Look<Faction>(ref this.leadingFaction, "leadingFaction", false);
        }

        public string GetUniqueLoadID()
        {
            return this.loadID;
        }

        internal void AddSubjugatedAlliance(PoliticalAlliance alliance)
        {
            foreach (var faction in alliance.GetFactions())
            {
                this.subjugatedFactions.Add(faction);
                foreach (var winnerFaction in this.GetFactions())
                {
                    faction.TrySetNotHostileTo(winnerFaction, false);
                    winnerFaction.TrySetNotHostileTo(faction, false);
                }
            }
        }
    }
}
