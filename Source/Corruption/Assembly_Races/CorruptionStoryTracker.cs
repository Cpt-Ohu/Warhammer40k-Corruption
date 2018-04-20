using System;
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
using OHUShips;

namespace Corruption
{
    public class CorruptionStoryTracker : WorldComponent
    {
        public bool FactionsEnabled = true;
        public bool DominationEnabled = true;
        public bool PsykersEnabled = true;
        public bool DropshipsEnabled = true;


        public CorruptionStoryTracker(World world) : base(world)
        {           
            this.FactionsEnabled = CorruptionModSettings.AllowFactions;
            this.DominationEnabled = CorruptionModSettings.AllowDomination;
            this.PsykersEnabled = CorruptionModSettings.AllowPsykers;
            this.DropshipsEnabled = CorruptionModSettings.AllowDropships;
            foreach (PatronDef def in DefDatabase<PatronDef>.AllDefsListForReading)
            {
                this.PlayerWorshipProgressLookup.Add(def, 0);
            }
            Log.Message("Created Storytracker");
        }

        public static List<PawnKindDef> DemonPawnKinds = new List<PawnKindDef>();
        
        public Faction PatronFaction;

        public string SubsectorName;

        public bool activeRaid;

        //Worship
        public PatronDef PlayerGod = PatronDefOf.Emperor;
        public Dictionary<PatronDef, int> PlayerWorshipProgressLookup = new Dictionary<PatronDef, int>();
        public List<PatronDef> AvailableGods
        {
            get
            {
                return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Colonists.Select(x => x.needs.TryGetNeed<Need_Soul>()?.Patron).Distinct().ToList();
            }
        }

        public Pawn Astropath;

        public Faction IoM_NPC
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.IoM_NPCFaction);
            }
        }
        public Faction IoM_Administratum
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.IoM_Administratum);
            }
        }
        public Faction IoM_Ecclesiarchy
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.IoM_Ecclesiarchy);
            }
        }
        public Faction IoM_Inquisition
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.IoM_Inquisition);
            }
        }
        public Faction ChaosCult_NPC
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.ChaosCult_NPC);
            }
        }
        public Faction DarkEldarKabal
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.DarkEldarKabal);
            }
        }
        public Faction EldarWarhost
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.EldarWarhost);
            }
        }
        public Faction ImperialGuard
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.ImperialGuard);
            }
        }
        public Faction AdeptusAstartes
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.Astartes);
            }
        }
        public Faction Orks
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.Orks);
            }
        }
        public Faction Mechanicus
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.Mechanicus);
            }
        }
        public Faction Tau
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.TauVanguard);
            }
        }
        public Faction AdeptusSororitas
        {
            get
            {
                return Find.World.factionManager.FirstFactionOfDef(DefOfs.C_FactionDefOf.AdeptusSororitas);
            }
        }

        public List<Faction> ImperialFactions = new List<Faction>();
        public List<Faction> XenoFactions = new List<Faction>();

        // Tithe System
        public bool PlayerIsEnemyOfMankind = false;
        public bool TitheCollectionActive = false;
        public bool AcknowledgedByImperium = false;
        public List<Tithes.TitheEntryGlobal> currentTithes = new List<Tithes.TitheEntryGlobal>();
        public int DaysToTitheCollection = -1;
        public Pawn PlanetaryGovernor;
        public Domination.DominationTracker DominationTracker;

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
        
        public float PlayerPatronWorshipProgress
        {
            get
            {
                return this.PlayerWorshipProgressLookup[this.PlayerGod];
            }

        }


        public override void WorldComponentTick()
        {
            if (this.FactionsEnabled)
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
                    Find.LetterStack.ReceiveLetter("LetterLabelGovernorDied".Translate(), deadDesc, LetterDefOf.NegativeEvent, null);
                }

                if (GenLocalDate.HourOfDay(Find.VisibleMap) == 4)
                {
                    if (this.DaysToTitheCollection > 0)
                    {
                        this.DaysToTitheCollection--;
                    }
                    if (!this.currentTithes.NullOrEmpty() && this.DaysToTitheCollection == 0 && !TitheCollectionActive && this.AcknowledgedByImperium && this.PlanetaryGovernor != null)
                    {
                        InitializeTitheCollection();
                    }
                    CalculateColonyCorruption();

                    EldarTicksDaily();
                }
                CorruptionTicksDaily();
            }
        }       
        
        private void InitializeTitheCollection()
        {
            Tithes.GameCondition_TitheCollectors condition = (Tithes.GameCondition_TitheCollectors)GameConditionMaker.MakeCondition(C_GameConditionDefOf.TitheCollectorArrived, 420000, 0);
            if (this.PlanetaryGovernor == null)
            {
                Log.Error("Initiated Tithe Collection with no Planetary Governor assigned");
            }
            else
            {
                this.PlanetaryGovernor.Map.gameConditionManager.RegisterCondition(condition);
                Find.WindowStack.Add(new Tithes.Window_IoMTitheArrival());
                //    Find.LetterStack.ReceiveLetter("LetterLabelTithesDue".Translate(), condition.def.description, LetterDefOf.BadUrgent, null);
                TitheCollectionActive = true;
            }
        }

        public void GenerateAndSetFactions()
        {
            if (this.DominationEnabled)
            {
                this.GenerateGenericAlliances();
            }
            if (this.IoM_NPC == null)
            {
                Faction IoM_NPC = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.IoM_NPCFaction);
                Find.World.factionManager.Add(IoM_NPC);
            }
            if (this.ChaosCult_NPC == null)
            {
                Faction ChaosCult = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.ChaosCult_NPC);
                Find.World.factionManager.Add(ChaosCult);
            }
            if (this.DarkEldarKabal == null)
            {
                Faction DarkEldarKabal = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.DarkEldarKabal);
                Find.World.factionManager.Add(DarkEldarKabal);
            }
            if (this.EldarWarhost == null)
            {
                Faction EldarWarhost = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.EldarWarhost);
                Find.World.factionManager.Add(EldarWarhost);
            }
            if (this.ImperialGuard == null)
            {
                Faction ImperialGuard = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.ImperialGuard);
                Find.World.factionManager.Add(ImperialGuard);
            }
            if (this.AdeptusAstartes == null)
            {
                Faction AdeptusAstartes = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.Astartes);
                Find.World.factionManager.Add(AdeptusAstartes);
            }
            if (this.Orks == null)
            {
                Faction Orks = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.Orks);
                Find.World.factionManager.Add(Orks);
            }

            if (this.AdeptusSororitas == null)
            {
                Faction AdeptusSororitas = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.AdeptusSororitas);
                Find.World.factionManager.Add(AdeptusSororitas);
            }
            if (this.Mechanicus == null)
            {
                Faction Mechanicus = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.Mechanicus);
                Find.World.factionManager.Add(Mechanicus);
            }
            if (this.Tau == null)
            {
                Faction Tau = FactionGenerator.NewGeneratedFaction(C_FactionDefOf.TauVanguard);
                Find.World.factionManager.Add(Tau);
            }

            //if (!this.ImperialFactions.Contains(this.IoM_Administratum))
            //{
            //    this.ImperialFactions.Add(this.IoM_Administratum);
            //}
            //if (!this.ImperialFactions.Contains(this.IoM_Ecclesiarchy))
            //{
            //    this.ImperialFactions.Add(this.IoM_Ecclesiarchy);
            //}
            //if (!this.ImperialFactions.Contains(this.IoM_Inquisition))
            //{
            //    this.ImperialFactions.Add(this.IoM_Inquisition);
            //}
            if (!this.ImperialFactions.Contains(this.ImperialGuard))
            {
                this.ImperialFactions.Add(this.ImperialGuard);
            }
            if (!this.ImperialFactions.Contains(this.Mechanicus))
            {
                this.ImperialFactions.Add(this.Mechanicus);
            }
            
            if (!this.ImperialFactions.Contains(this.AdeptusSororitas))
            {
                this.ImperialFactions.Add(this.AdeptusSororitas);
            }
            
            if (!this.ImperialFactions.Contains(this.AdeptusAstartes))
            {
                this.ImperialFactions.Add(this.AdeptusAstartes);
            }
            

            if (!this.XenoFactions.Contains(this.EldarWarhost))
            {
                this.XenoFactions.Add(this.EldarWarhost);
            }
            
            if (!this.XenoFactions.Contains(this.Tau))
            {
                this.XenoFactions.Add(this.Tau);
            }
            
            if (!this.XenoFactions.Contains(this.ChaosCult_NPC))
            {
                this.XenoFactions.Add(this.ChaosCult_NPC);
            }
            
            foreach (Faction fac in this.ImperialFactions)
            {
                fac.SetHostileTo(this.ChaosCult_NPC, true);
                this.ChaosCult_NPC.SetHostileTo(fac, true);
            }
            
            foreach (Faction fac in this.XenoFactions.FindAll(x => x.def != C_FactionDefOf.ChaosCult_NPC))
            {
                fac.SetHostileTo(this.ChaosCult_NPC, true);
                this.ChaosCult_NPC.SetHostileTo(fac, true);
            }
            
            if (this.DominationEnabled)
            {
                this.DominationTracker.AddNewAlliance(EldarWarhost.Name, EldarWarhost);
                this.DominationTracker.AddNewAlliance(Tau.Name, this.Tau);
                this.DominationTracker.AddNewAlliance(ChaosCult_NPC.Name, ChaosCult_NPC);
            }


            List<Faction> list = new List<Faction>();
            list.AddRange(this.ImperialFactions);
            list.AddRange(this.XenoFactions);
            //               Log.Message("CheckingFactionLeaders for " + list.Count.ToString() + " factions");
            foreach (Faction current in list)
            {
                if (current.leader == null)
                {
                    //                  Log.Message("NoLeader");
                                           //Log.Message("NoLeader for "+ current.GetCallLabel());
                    PawnKindDef kinddef = DefDatabase<PawnKindDef>.AllDefsListForReading.FirstOrDefault(x => x.defaultFactionType == current.def && x.factionLeader);
                    if (kinddef != null)
                    {
                                                          //Log.Message("GeneratedRequest");
                        Pawn pawn = PawnGenerator.GeneratePawn(kinddef, current);

                                                          //Log.Message("Generated Leader");
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
        }

        private void GenerateGenericAlliances()
        {
            foreach (Faction current in Find.World.factionManager.AllFactions)
            {
                if (current.HasName)
                {
                    this.DominationTracker.AddNewAlliance(current.Name, current);
                }
            }            
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            if (this.DominationEnabled)
            {
                this.DominationTracker = new Domination.DominationTracker();
            }


           
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

        public void AddWorshipProgress(float amount, PatronDef god)
        {
            int accumulatedProgress = this.PlayerWorshipProgressLookup[god];
            this.PlayerWorshipProgressLookup[god] += AdjustWorshipGain(amount, accumulatedProgress);
        }

        private int AdjustWorshipGain(float amount, float currentProgress)
        {
            double dampeningFactor = (0.8f) / (1 + Math.Exp(-0.001f * (currentProgress - 6000)));

            return (int)(amount * (1 - dampeningFactor));
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
                //if (EldarWarhost.def.RaidCommonalityFromPoints( < 100)
                //{
                //    EldarWarhost.def.raidCommonality += 10;
                //}
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
            if (this.FactionsEnabled)
            {
                if (ColonyCorruptionAvg < 0.4)
                {
                    EldarWarhost.SetHostileTo(Faction.OfPlayer, true);
                    //if (EldarWarhost.def.raidCommonality < 100)
                    //{
                    //    EldarWarhost.def.raidCommonality += 10;
                    //}
                    AdeptusSororitas.SetHostileTo(Faction.OfPlayer, true);
                    if (this.PatronFaction == AdeptusSororitas)
                    {
                        this.PatronFaction = null;
                    }
                    //if (AdeptusSororitas.def.raidCommonality < 100)
                    //{
                    //    AdeptusSororitas.def.raidCommonality += 10;
                    //}
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

        private void InitializeAvailableGods()
        {
            List<PatronDef> GodsFromEnum = DefDatabase<PatronDef>.AllDefsListForReading;
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Faction>(ref this.PatronFaction, "PatronFaction");
            Scribe_Collections.Look<Faction>(ref this.ImperialFactions, "ImperialFactions", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Faction>(ref this.XenoFactions, "XenoFactions", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<StarMapObject>(ref this.SubSectorObjects, "SubSectorObjects", LookMode.Deep, new object[0]);
            Scribe_Values.Look<bool>(ref this.IoMCanHelp, "FactionCanHelp", false, true);
            Scribe_Values.Look<bool>(ref this.activeRaid, "activeRaid", false, true);
            Scribe_Values.Look<bool>(ref this.PlayerIsEnemyOfMankind, "PlayerIsEnemyOfMankind", false, true);
            Scribe_Values.Look<bool>(ref this.AcknowledgedByImperium, "AcknowledgedByImperium", false, true);
            Scribe_Values.Look<int>(ref this.DaysAfterHelp, "DaysAfterHelp", 4, false);
            Scribe_Values.Look<int>(ref this.DaysToTitheCollection, "DaysToTitheCollection", 30, false);
            Scribe_Values.Look<float>(ref this.ColonyCorruptionAvg, "ColonyCorruptionAvg", 0.8f, false);
            Scribe_Values.Look<string>(ref this.SubsectorName, "SubsectorName", "Aurelia", false);
            Scribe_Collections.Look<Tithes.TitheEntryGlobal>(ref this.currentTithes, "currentTithes", LookMode.Deep, new object[0]);
            Scribe_Collections.Look <PatronDef, int>(ref this.PlayerWorshipProgressLookup, "Progress", LookMode.Def, LookMode.Value);
            if (CorruptionModSettings.AllowDomination)
            {
                Scribe_Deep.Look<Domination.DominationTracker>(ref this.DominationTracker, "DominationTracker", new object[0]);
            }
            Scribe_Values.Look<bool>(ref this.DropshipsEnabled, "DropshipsEnabled", true);
            Scribe_Values.Look<bool>(ref this.DominationEnabled, "DominationEnabled", true);
            Scribe_Values.Look<bool>(ref this.PsykersEnabled, "PsykersEnabled", true);
            Scribe_Values.Look<bool>(ref this.DropshipsEnabled, "DropshipsEnabled", true);
            base.ExposeData();
        }
    }
}
