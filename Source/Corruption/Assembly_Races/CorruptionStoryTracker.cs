﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using System.Reflection;
using Verse.Sound;
using Corruption.DefOfs;

namespace Corruption
{
    public class CorruptionStoryTracker : WorldObject
    {
        public override bool SelectableNow
        {
            get
            {
                return false;
            }
        }

        public override void Draw()
        {
        }

        public static List<PawnKindDef> DemonPawnKinds = new List<PawnKindDef>();
        
        public Faction PatronFaction;

        public string SubsectorName;

        public bool activeRaid;


        public Pawn Astropath;

        public Faction ChaosCult;
        public Faction DarkEldarKabal;
        public Faction EldarWarhost;
        public Faction ImperialGuard;
        public Faction Orks;
        public Faction Mechanicus;
        public Faction Tau;
        public Faction AdeptusSororitas;

        public List<Faction> ImperialFactions = new List<Faction>();
        public List<Faction> XenoFactions = new List<Faction>();
        public List<Ships.ShipBase> shipsInOrbit = new List<Ships.ShipBase>();

        // Tithe System
        public bool PlayerIsEnemyOfMankind = false;
        public bool TitheCollectionActive = false;
        public bool AcknowledgedByImperium = false;
        public List<Tithes.TitheEntryGlobal> currentTithes = new List<Tithes.TitheEntryGlobal>();
        public int DaysToTitheCollection = -1;
        public Pawn PlanetaryGovernor;

        public bool setTithesDirty = false;
        public int curTitheID = 0;

        public List<StarMapObject> SubSectorObjects = new List<StarMapObject>();

        public bool IoMCanHelp;
        public int DaysAfterHelp;

        public float ColonyCorruptionAvg;

        public bool GovernorIsPlayer
        {
            get
            {
                if (this.PlanetaryGovernor!= null)
                {
                    if (this.PlanetaryGovernor.Faction == Faction.OfPlayer)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override void Tick()
        {
            for (int i = 0; i < Find.Maps.Count; i++)

            {
                if (Find.Maps[i].lordManager.lords.Any(x => x.LordJob.GetType() == typeof(LordJob_AssaultColony)))
                {
                    if (activeRaid == false)
                    {
                        activeRaid = true;
                        if (!PlayerIsEnemyOfMankind && this.PatronFaction != null)
                        {
                            this.PatronFactionAssaultTick(this.PatronFaction);
                        }
                    }
                }
            }
            if (this.PlanetaryGovernor != null && this.PlanetaryGovernor.Dead)
            {
                this.PlanetaryGovernor = Find.WorldPawns.AllPawnsAlive.Where(x => x.Faction == Faction.OfPlayer).RandomElement();
                string deadDesc = "GovernorDiedDesc".Translate(new object[]
                {
                    PlanetaryGovernor.LabelCap,
                    PlanetaryGovernor.filth
                });
                Find.LetterStack.ReceiveLetter("LetterLabelGovernorDied".Translate(), deadDesc, LetterType.BadUrgent, null);
            }

            if (GenLocalDate.HourOfDay(Find.VisibleMap) == 4)
            {
                if (this.DaysToTitheCollection > 0)
                {
                    this.DaysToTitheCollection--;
                }
                if (!this.currentTithes.NullOrEmpty() && this.DaysToTitheCollection == 0 && !TitheCollectionActive && this.AcknowledgedByImperium)
                {
                    InitializeTitheCollection();
                }
                CalculateColonyCorruption();
                EldarTicksDaily();
                CorruptionTicksDaily();
            }
        }        
        private void InitializeTitheCollection()
        {
            Tithes.MapCondition_TitheCollectors condition = (Tithes.MapCondition_TitheCollectors)MapConditionMaker.MakeCondition(C_MapConditionDefOf.TitheCollectorArrived, 420000, 0);
            if (this.PlanetaryGovernor == null) Log.Error("Planetary Governor is null");
            this.PlanetaryGovernor.Map.mapConditionManager.RegisterCondition(condition);
            Find.WindowStack.Add(new Tithes.Window_IoMTitheArrival());
        //    Find.LetterStack.ReceiveLetter("LetterLabelTithesDue".Translate(), condition.def.description, LetterType.BadUrgent, null);
            TitheCollectionActive = true;
        }

        public override void PostAdd()
        {
   //           Log.Message("CreatingSubSector");
            CreateSubSector();
   //           Log.Message("Objects created : " + this.SubSectorObjects.Count.ToString());

            this.ChaosCult = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.ChaosCult);
            this.DarkEldarKabal = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.DarkEldarKabal);
            this.EldarWarhost = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.EldarWarhost);
            this.ImperialGuard = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.ImperialGuard);
            this.Orks = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.Orks);
            this.AdeptusSororitas = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.AdeptusSororitas);
            this.Mechanicus = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.Mechanicus);
            this.Tau = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.TauVanguard);
            if (!this.ImperialFactions.Contains(this.ImperialGuard))
            {
                this.ImperialFactions.Add(this.ImperialGuard);
            }
            if (!this.ImperialFactions.Contains(this.Mechanicus)) this.ImperialFactions.Add(this.Mechanicus);
            if (!this.ImperialFactions.Contains(this.AdeptusSororitas)) this.ImperialFactions.Add(this.AdeptusSororitas);

            if (!this.XenoFactions.Contains(this.EldarWarhost)) this.XenoFactions.Add(this.EldarWarhost);
            if (!this.XenoFactions.Contains(this.Tau)) this.XenoFactions.Add(this.Tau);
            if (!this.XenoFactions.Contains(this.ChaosCult)) this.XenoFactions.Add(this.ChaosCult);


            List<Faction> list = new List<Faction>();
            list.AddRange(this.ImperialFactions);
            list.AddRange(this.XenoFactions);
     //               Log.Message("CheckingFactionLeaders for " + list.Count.ToString() + " factions");
            foreach (Faction current in list)
            {
                if (current.leader == null)
                {
  //                  Log.Message("NoLeader");
  //                       Log.Message("NoLeader for "+ current.GetCallLabel());
                    PawnKindDef kinddef = DefDatabase<PawnKindDef>.AllDefsListForReading.FirstOrDefault(x => x.defaultFactionType == current.def && x.factionLeader);
                    if (kinddef != null)
                    {
 //                               Log.Message("Generating Leader with: " + kinddef.defName);
                        PawnGenerationRequest request = new PawnGenerationRequest(kinddef, current, PawnGenerationContext.NonPlayer, null, true, false, false, false, true, false, 1f, false, true, true, null, null, null, null, null, null);

    //                                  Log.Message("GeneratedRequest");
                        Pawn pawn = PawnGenerator.GeneratePawn(kinddef, current);

    //                                  Log.Message("Generated Leader");
                        current.leader = pawn;
                        if (current.leader.RaceProps.IsFlesh)
                        {
                            current.leader.relations.everSeenByPlayer = true;
                        }
                        if (!Find.WorldPawns.Contains(current.leader))
                        {
                            Find.WorldPawns.PassToWorld(current.leader, PawnDiscardDecideMode.KeepForever);
                        }
                    }
                    else
                    {
                        Log.Error("No Leader KindDef found for Faction: " + current.Name);
                    }
                }
                else
                {
           //         Log.Message("Leader Found");
         //                        Log.Message("Leader for " + current.Name + " is " + current.leader.Label);
                }
            }
            base.PostAdd();
        }

        public void CreateSubSector()
        {
            Vector2 center = new Vector2(360f, 300f);
            int p = Rand.RangeInclusive(1, 2);
            this.CreateObjects(p, StarMapObjectType.PlanetMedium, center);
            int s = Rand.RangeInclusive(1, 2);
            this.CreateObjects(s, StarMapObjectType.PlanetSmall, center);
            int m = Rand.RangeInclusive(1, 2);
            this.CreateObjects(m, StarMapObjectType.Moon, center);

            List<string> planetName = new List<string>();
            planetName.Add(Find.World.info.name);
            this.SubsectorName = NameGenerator.GenerateName(RulePackDefOf.NamerWorld, planetName, false);
        }
        
        private void CreateObjects(int num, StarMapObjectType type, Vector2 center)
        {
            int angle = 0;
            for (int i=0; i < num; i++)
            {
                List<string> existingObjectNames = new List<string>();
                foreach(StarMapObject current in this.SubSectorObjects)
                {
                    existingObjectNames.Add(current.objectName);
                }
                StarMapObject newEntry = new StarMapObject(angle, out angle, center, existingObjectNames, type);
                angle += 40;
                this.SubSectorObjects.Add(newEntry);
            }
        }

        public void CalculateColonyCorruption()
        {
            List<Pawn> ColonyPawns = Find.VisibleMap.mapPawns.FreeColonistsAndPrisonersSpawned.ToList<Pawn>();
            float totalCorruption = 0f;
            foreach (Pawn cpawn in ColonyPawns)
            {
                if (cpawn.needs != null && cpawn.needs.TryGetNeed<Need_Soul>() != null)
                    totalCorruption += cpawn.needs.TryGetNeed<Need_Soul>().CurLevel;
            }
            ColonyCorruptionAvg = totalCorruption / ColonyPawns.Count;
        }

        public void HelpTickReset()
        {
            this.DaysAfterHelp += 1;
            if (this.DaysAfterHelp > 7)
            {
                this.IoMCanHelp = true;
            }
        }

        public void EldarTicksDaily()
        {
            if (ColonyCorruptionAvg < 0.4)
            {
                EldarWarhost.SetHostileTo(Faction.OfPlayer, true);
                if (EldarWarhost.def.raidCommonality < 100)
                {
                    EldarWarhost.def.raidCommonality += 10;
                }
            }
            else if (Rand.Range(0, 100) < 2)
            {
                if (EldarWarhost.HostileTo(Faction.OfPlayer))
                {
                    EldarWarhost.SetHostileTo(Faction.OfPlayer, false);
                }
                else
                {
                    EldarWarhost.SetHostileTo(Faction.OfPlayer, true);
                }
            }
            if (CheckForSpiritStones() > 0)
            {
                if (!EldarWarhost.HostileTo(Faction.OfPlayer))
                {
                    IncidentParms parms = new IncidentParms();
                    parms.faction = EldarWarhost;
                    parms.points = 500;
                    IncidentDef EldarVisitorIncident = DefDatabase<IncidentDef>.AllDefs.First(x => x.defName == "VisitorGroup");
                    EldarVisitorIncident.workerClass = typeof(IncidentWorker_VisitorGroup);
                    EldarVisitorIncident.Worker.TryExecute(parms);
                }
                else
                {
                    IncidentParms parms = new IncidentParms();
                    parms.faction = EldarWarhost;
                    parms.points = 1000;
                    IncidentDef EldarRaidIncident = DefDatabase<IncidentDef>.AllDefs.First(x => x.defName == "Assault");
                    EldarRaidIncident.workerClass = typeof(IncidentWorker_VisitorGroup);
                    EldarRaidIncident.Worker.TryExecute(parms);
                }
            }
        }

        public void CollectTithes()
        {
            List<Building> list = Tithes.TitheUtilities.allTitheContainers;

            for (int i=0; i < list.Count; i++)
            {
                list[i].Destroy(DestroyMode.Vanish);
            }
            SoundStarter.PlayOneShotOnCamera(SoundDefOf.Thunder_OnMap);
            
            Tithes.TitheUtilities.CalculateColonyTithes(this);
        }

        public void ResetIoMAcknowledgement()
        {
            this.DaysToTitheCollection = -1;
            this.currentTithes.Clear();
            Tithes.TitheUtilities.UpdateAllContaners();
            this.TitheCollectionActive = false;
            this.AcknowledgedByImperium = false;
        }

        public int CheckForSpiritStones()
        {
            List<Thing> list = Find.VisibleMap.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways);
            int num = 0;
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (!thing.Position.Fogged(Find.VisibleMap) && thing.def == C_ThingDefOfs.SpiritStone)
                {
                    num += 1;
                }
            }
            return num;
        }

        public void CorruptionTicksDaily()
        {
            if (ColonyCorruptionAvg < 0.4)
            {
                EldarWarhost.SetHostileTo(Faction.OfPlayer, true);
                if (EldarWarhost.def.raidCommonality < 100)
                {
                    EldarWarhost.def.raidCommonality += 10;
                }
                AdeptusSororitas.SetHostileTo(Faction.OfPlayer, true);
                if (this.PatronFaction == AdeptusSororitas)
                {
                    this.PatronFaction = null;
                }
                if (AdeptusSororitas.def.raidCommonality < 100)
                {
                    AdeptusSororitas.def.raidCommonality += 10;
                }
            }

            if (this.DaysToTitheCollection == 0)
            {
                
            }
        }

        
        public void SororitasAssist()
        {
            IncidentParms parms = new IncidentParms();
            parms.faction = AdeptusSororitas;
            parms.points = 1000;
            parms.raidArrivalMode = PawnsArriveMode.CenterDrop;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            IncidentDef relief = new IncidentDef();
            relief.workerClass = typeof(IncidentWorker_RaidFriendly);
            relief.Worker.TryExecute(parms);
            this.IoMCanHelp = false;
        }

        public void IGAssist()
        {
            IncidentParms parms = new IncidentParms();
            parms.faction = AdeptusSororitas;
            parms.points = 1000;
            parms.raidArrivalMode = PawnsArriveMode.CenterDrop;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            IncidentDef relief = new IncidentDef();
            relief.workerClass = typeof(IncidentWorker_RaidFriendly);
            relief.Worker.TryExecute(parms);
            this.IoMCanHelp = false;
        }

        public void MechanicusAssist()
        {
            IncidentParms parms = new IncidentParms();
            parms.faction = AdeptusSororitas;
            parms.points = 1000;
            parms.raidArrivalMode = PawnsArriveMode.CenterDrop;
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            IncidentDef relief = new IncidentDef();
            relief.workerClass = typeof(IncidentWorker_RaidFriendly);
            relief.Worker.TryExecute(parms);
            this.IoMCanHelp = false;
        }


        public void PatronFactionAssaultTick(Faction patronFaction)
        {
            switch(patronFaction.def.defName)
            {
                case ("Mechanicus"):
                    {
                        MechanicusAssist();
                        break;
                    }
                case ("ImperialGuard"):
                    {
                        IGAssist();
                        break;
                    }
                case ("AdeptusSororitas"):
                    {
                        SororitasAssist();
                        break;
                    }
            }
        }

        public int GetTitheID()
        {
            return CorruptionStoryTracker.GetNextTitheID(ref this.curTitheID);
        }

        private static int GetNextTitheID(ref int nextID)
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Log.Warning("Getting next unique ID during saving or loading. This may cause bugs.");
            }
            int result = nextID;
            nextID++;
            if (nextID == 2147483647)
            {
                Log.Warning("Next ID is at max value. Resetting to 0. This may cause bugs.");
                nextID = 0;
            }
            return result;
        }

        public override void ExposeData()
        {
            Scribe_References.LookReference<Faction>(ref this.PatronFaction, "PatronFaction");
            Scribe_References.LookReference<Faction>(ref this.ImperialGuard, "ImperialGuard");
            Scribe_References.LookReference<Faction>(ref this.AdeptusSororitas, "AdeptusSororitas");
            Scribe_References.LookReference<Faction>(ref this.Mechanicus, "Mechanicus");
            Scribe_References.LookReference<Faction>(ref this.EldarWarhost, "EldarWarhost");
            Scribe_References.LookReference<Faction>(ref this.DarkEldarKabal, "DarkEldarKabal");
            Scribe_References.LookReference<Faction>(ref this.ChaosCult, "ChaosCult");
            Scribe_References.LookReference<Faction>(ref this.Tau, "Tau");
            Scribe_Collections.LookList<Faction>(ref this.ImperialFactions, "ImperialFactions", LookMode.Reference, new object[0]);
            Scribe_Collections.LookList<Faction>(ref this.XenoFactions, "XenoFactions", LookMode.Reference, new object[0]);
            Scribe_Collections.LookList<StarMapObject>(ref this.SubSectorObjects, "SubSectorObjects", LookMode.Deep, new object[0]);
            Scribe_Values.LookValue<bool>(ref this.IoMCanHelp, "FactionCanHelp", false, true);
            Scribe_Values.LookValue<bool>(ref this.activeRaid, "activeRaid", false, true);
            Scribe_Values.LookValue<bool>(ref this.PlayerIsEnemyOfMankind, "PlayerIsEnemyOfMankind", false, true);
            Scribe_Values.LookValue<bool>(ref this.AcknowledgedByImperium, "AcknowledgedByImperium", false, true);
            Scribe_Values.LookValue<int>(ref this.DaysAfterHelp, "DaysAfterHelp", 4, false);
            Scribe_Values.LookValue<int>(ref this.DaysToTitheCollection, "DaysToTitheCollection", 30, false);
            Scribe_Values.LookValue<float>(ref this.ColonyCorruptionAvg, "ColonyCorruptionAvg", 0.8f, false);
            Scribe_Values.LookValue<string>(ref this.SubsectorName, "SubsectorName", "Aurelia", false);
            Scribe_Collections.LookList<Tithes.TitheEntryGlobal>(ref this.currentTithes, "currentTithes", LookMode.Deep, new object[0]);
            base.ExposeData();
        }
    }
}
