﻿using RimWorld;
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

        public List<DominationConflict> AllConflicts = new List<DominationConflict>();

        public DominationConflict MainStoryConflict
        {
            get
            {
                return this.AllConflicts.FirstOrDefault(x => x.IsMainConflict);
            }
        }

        public void AddNewConflict(PoliticalAlliance first, PoliticalAlliance second, string conflictTitle, bool isMainConflict)
        {
            this.AllConflicts.Add(new DominationConflict(first, second, conflictTitle, isMainConflict));
        }

        public bool CheckPawnDiedInWar(Pawn pawn, DamageInfo dinfo)
        {
            if (pawn.def.race.Humanlike)
            {
                Pawn attacker = dinfo.Instigator as Pawn;
                if (attacker != null && attacker.Faction != pawn.Faction)
                {
                    DominationConflict conflict;
                    if (this.FactionsAtWar(pawn.Faction, attacker.Faction, out conflict))
                    {
                        PoliticalAlliance victimAlliance = this.GetAllianceOfFaction(pawn.Faction);
                        PoliticalAlliance instigatorAlliance = this.GetAllianceOfFaction(attacker.Faction);
                        conflict.AdjustWarWearinessFor(victimAlliance, 0.002f * pawn.kindDef.combatPower, 1);
                        float triumphFactor = victimAlliance.IsPlayerAlliance ? 0.05f : 0.001f;
                        conflict.AdjustWarWearinessFor(instigatorAlliance, -triumphFactor * pawn.kindDef.combatPower, 0);
                    }

                }
            }
            return false;
        }

        internal void NotifyFactionJoinedAlliance(PoliticalAlliance politicalAlliance, Faction faction)
        {
            foreach (var newAlly in politicalAlliance.GetFactions().Where(x => x != faction))
            {
                newAlly.TrySetRelationKind(faction, FactionRelationKind.Ally, true, "JoinedAllianceLetter".Translate());
            }
            foreach (var otherAlliances in this.politicalAlliancesInt.Where(x => x.HostileTo(politicalAlliance)))
            {
                foreach (var newEnemy in otherAlliances.GetFactions())
                {
                    newEnemy.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, true, "JoinedEnemyAlliance".Translate());
                }
            }
        }

        public void NotifyFactionLeftAlliance(PoliticalAlliance politicalAlliance, Faction faction, bool turnEnemy)
        {
            foreach (var formerAlly in politicalAlliance.GetFactions())
            {
                FactionRelation relation = formerAlly.RelationWith(faction);
                float relationImpact = turnEnemy ? -75f - relation.goodwill : -relation.goodwill;
                CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(formerAlly, faction, relationImpact);                
            }


            if (politicalAlliance.GetFactions().Count == 0)
            {
                this.politicalAlliancesInt.Remove(politicalAlliance);
            }

            PoliticalAlliance newAlliance = new PoliticalAlliance(faction.Name, GetNextAllianceID());
            newAlliance.AddToAlliance(faction);
            newAlliance.LeadingFaction = faction;
        }

        public DominationTracker()
        {

        }

        public void InitializeTracker()
        {
            this.CreateImperiumOfManAlliance();
            this.CreateChaosAlliance();
            this.CreateMainConflict();
            this.CreateInitialAlliances();
        }

        private void CreateInitialAlliances()
        {
            foreach (var faction in Find.FactionManager.AllFactionsVisible)
            {
                this.AddNewAlliance(faction.Name, faction);
            }
        }

        public PoliticalAlliance ImperiumOfMan
        {
            get
            {
                return this.politicalAlliancesInt.FirstOrDefault(x => x.AllianceName == "IoMAlliance".Translate());
            }
        }

        public PoliticalAlliance ChaosAlliance

        {
            get
            {
                return this.politicalAlliancesInt.FirstOrDefault(x => x.AllianceName == "ChaosAlliance".Translate());
            }
        }

        internal void AddPlayerConflictWith(PoliticalAlliance alliance)
        {
            this.AddNewConflict(PlayerAlliance, alliance, "War", false);
        }

        private int NextAllianceLoadID = 0;
        private int NextWarLoadID = 0;

        private List<PoliticalAlliance> politicalAlliancesInt = new List<PoliticalAlliance>();

        public PoliticalAlliance GetAllianceByName(string name)
        {
            return this.politicalAlliancesInt.FirstOrDefault(x => x.AllianceName == name);
        }

        public PoliticalAlliance GetAllianceOfFaction(Faction faction)
        {
            return this.politicalAlliancesInt.FirstOrDefault(x => x.GetFactions().Contains(faction));
        }

        public PoliticalAlliance PlayerAlliance
        {
            get
            {
                return this.politicalAlliancesInt.FirstOrDefault(x => x.IsPlayerAlliance);
            }
        }

        public List<PoliticalAlliance> AlliancesForReading
        {
            get
            {
                return this.politicalAlliancesInt;
            }
        }

        public bool GetAllianceWar(PoliticalAlliance firstAlliance, PoliticalAlliance secAlliance, out DominationConflict conflict)
        {
            DominationConflict potentialConflict = this.AllConflicts.FirstOrDefault(x => ((x.First == firstAlliance && x.Second == secAlliance) || (x.Second == secAlliance && x.First == firstAlliance)));

            if (potentialConflict != null)
            {
                conflict = potentialConflict;
                return true;
            }
            conflict = null;
            return false;
        }

        public bool FactionsAtWar(Faction first, Faction second, out DominationConflict conflict)
        {
            PoliticalAlliance firstAlliance = this.GetAllianceOfFaction(first);
            PoliticalAlliance secAlliance = this.GetAllianceOfFaction(second);
            if (firstAlliance == null || secAlliance == null)
            {
                conflict = null;
                return false;
            }
            conflict = this.AllConflicts.FirstOrDefault(x => ((x.First == firstAlliance && x.Second == secAlliance) || (x.Second == secAlliance && x.First == firstAlliance)));
            if (conflict != null)
            {
                return true;
            }

            return false;
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

        internal void EndConflict(DominationConflict conflict, ConflictResult result, PoliticalAlliance winner)
        {
            conflict.Finished = true;
            conflict.SetResult(result, winner);
        }

        public void CreateImperiumOfManAlliance()
        {
            PoliticalAlliance ioM = new PoliticalAlliance("IoMAlliance".Translate(), GetNextAllianceID(), Find.FactionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.IoM_NPCFaction));
            foreach (Faction fac in CFind.StoryTracker.ImperialFactions)
            {
                ioM.AddToAlliance(fac);
            }
            this.politicalAlliancesInt.Add(ioM);
        }

        public void CreateChaosAlliance()
        {
            this.politicalAlliancesInt.Add(new PoliticalAlliance("ChaosAlliance".Translate(), GetNextAllianceID(), Find.FactionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.ChaosCult_NPC)));
        }

        public void CreateMainConflict()
        {
            this.AddNewConflict(this.ImperiumOfMan, this.ChaosAlliance, "MainStoryConflict".Translate(), true);
            if (Find.Scenario.name == "Imperial Domination")
            {
                this.ImperiumOfMan.LeadingFaction = Faction.OfPlayer;
            }
            else
            {
                this.ChaosAlliance.LeadingFaction = Faction.OfPlayer;
            }
        }

        public int GetNextAllianceID()
        {
            this.NextAllianceLoadID++;
            return this.NextAllianceLoadID;
        }

        public int GetNextWarID()
        {
            this.NextWarLoadID++;
            return this.NextWarLoadID;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<PoliticalAlliance>(ref this.politicalAlliancesInt, "politicalAlliancesInt", LookMode.Deep);
            Scribe_Collections.Look<IBattleZone>(ref this.battleZones, "battleZones", LookMode.Reference);
            Scribe_Collections.Look<DominationConflict>(ref this.AllConflicts, "AllConflicts", LookMode.Deep);
            Scribe_Values.Look<int>(ref this.NextAllianceLoadID, "NextAllianceLoadID");
            Scribe_Values.Look<int>(ref this.NextWarLoadID, "NextWarLoadID");
        }
    }
}
