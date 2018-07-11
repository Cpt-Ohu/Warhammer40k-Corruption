using FactionColors;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;
using Corruption.BookStuff;
using Corruption.DefOfs;
using System.Linq;
using RimWorld.Planet;
using Verse.AI.Group;
using Corruption.Worship;
using Verse.AI;

namespace Corruption
{
    public class CompSoul : ThingComp
    {

        #region "Members"
        private bool IsImmune;

        public ReadablesManager readablesManager = new ReadablesManager();

        private const float ThreshCorrupted = 0.1f;

        private const float ThreshTainted = 0.3f;

        private const float ThreshWarptouched = 0.5f;

        private const float ThreshIntrigued = 0.7f;

        private const float ThreshPure = 0.9f;

        public bool KnownToPlayer = false;

        public PawnKillTracker PawnKillTracker;

        public PsykerPowerLevel PsykerPowerLevel = PsykerPowerLevel.Rho;

        public SoulTraitDegreeData DevotionTraitDegree;

        public List<PsykerPower> psykerPowers = new List<PsykerPower>();

        public PatronDef Patron = PatronDefOf.Emperor;

        public CulturalToleranceCategory CulturalTolerance;

        public AfflictionProperty PawnAfflictionProps;

        public SoulTrait DevotionTrait;

        public SoulTrait CommonSoulTrait;

        public SoulTrait PatronTrait;

        public SoulTrait SpecialTrait;

        public bool Corrupted = true;

        public bool SoulInitialized = false;
        
        public bool IsOnPilgrimage;

        public List<Pawn> OpposingDevotees = new List<Pawn>();

        private float curLevel;

        #endregion

        #region "Properties"


        public Pawn Pawn
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        public CompPsyker compPsyker
        {
            get
            {
                return this.Pawn.TryGetComp<CompPsyker>();
            }
        }

        public float CurLevel
        {
            get
            {
                return this.curLevel;
            }
            set
            {
                this.curLevel = value;
            }
        }

        public SoulAffliction CurCategory
        {
            get
            {
                if (this.curLevel <= 0.0f)
                {
                    return SoulAffliction.Lost;
                }
                if (this.curLevel < 0.3f)
                {
                    return SoulAffliction.Corrupted;
                }
                if (this.curLevel < 0.4f)
                {
                    return SoulAffliction.Tainted;
                }
                if (this.curLevel < 0.65f)
                {
                    return SoulAffliction.Warptouched;
                }
                if (this.curLevel < 0.85f)
                {
                    return SoulAffliction.Intrigued;
                }
                if (this.curLevel >= 0.85f)
                {
                    return SoulAffliction.Pure;
                }
                return SoulAffliction.Pure;
            }
        }

        #endregion

        public List<SoulTrait> AllSoulTraits
        {
            get
            {
                List<SoulTrait> list = new List<SoulTrait>();
                if (this.DevotionTrait != null) list.Add(this.DevotionTrait);
                if (this.PatronTrait != null) list.Add(this.PatronTrait);
                if (this.CommonSoulTrait != null) list.Add(this.CommonSoulTrait);
                if (this.SpecialTrait != null) list.Add(this.SpecialTrait);
                return list;
            }
        }

        private ChaosFollowerPawnKindDef cdef
        {
            get
            {
                return this.Pawn.kindDef as ChaosFollowerPawnKindDef;
            }
        }

        #region Initialize


        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public void SetInitialLevel()
        {
                this.PawnKillTracker = new PawnKillTracker();

            this.InsertStatInfo();
            this.ResolvePawnKindDef();

            if (this.DevotionTrait == null)
                {
                    if ((PawnAfflictionProps != null && PawnAfflictionProps.IsImmune))
                    {
                        this.DevotionTrait = new SoulTrait(C_SoulTraitDefOf.Devotion, PawnAfflictionProps.ImmuneDevotionDegree);
                        this.IsImmune = true;
                    }
                    else
                    {
                        this.DevotionTrait = new SoulTrait(C_SoulTraitDefOf.Devotion, Rand.RangeInclusive(-2, 2));
                    }
                }
                if (PawnAfflictionProps.CommonSoulTrait != null)
                {
                    this.CommonSoulTrait = new SoulTrait(PawnAfflictionProps.CommonSoulTrait, 0);
                }
                if (PawnAfflictionProps.canUseCalls == true)
                {
                    Pawn.caller = new Pawn_CallTracker(Pawn);
                }
                if (this.curLevel < 0.3f && Corrupted == false)
                {
                    GainChaosGod(PatronDefOf.ChaosUndivided);
                }

                if (this.Pawn.IsColonist)
                {
                    this.KnownToPlayer = true;
                }            
        }

        private void InsertStatInfo()
        {
            FieldInfo info = typeof(StatsReportUtility).GetField("cachedDrawEntries", BindingFlags.NonPublic | BindingFlags.Static);
            if (info != null)
            {
                List<StatDrawEntry> entries = info.GetValue(this.Pawn) as List<StatDrawEntry>;
                if (!entries.NullOrEmpty())
                {
                    entries.Add(new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Patron", this.Patron.ToString(), 3));
                    entries.Add(new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "CulturalToleranceStat".Translate(), this.Patron.ToString(), 2));
                    entries.Add(new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "PurityOfSoulStat".Translate(), this.CurLevel.ToString(), 1));
                }
            }
        }

        private void ResolvePawnKindDef()
        {
            if (this.cdef != null)
            {
                if (cdef.IsServitor)
                {
                    CorruptionStoryTrackerUtilities.InitiateServitorComp(Pawn);
                }
                if (cdef.UseFixedGender)
                {
                    this.Pawn.gender = cdef.FixedGender;
                }
                if (cdef.AfflictionProperty != null)
                {
                    PawnAfflictionProps = new AfflictionProperty();
                    this.PawnAfflictionProps = cdef.AfflictionProperty;
                    if (CorruptionModSettings.AllowPsykers)
                    {
                        int pllow = (int)this.PawnAfflictionProps.LowerPsykerPowerLimit;
                        int plup = (int)this.PawnAfflictionProps.UpperAfflictionLimit;
                        this.PsykerPowerLevel = (PsykerPowerLevel)Rand.RangeInclusive(pllow, plup);
                    }

                    if (PawnAfflictionProps.IsImmune)
                    {
                        this.curLevel = 0.99f;
                        this.DevotionTrait = new SoulTrait(C_SoulTraitDefOf.Devotion, PawnAfflictionProps.ImmuneDevotionDegree);
                        this.IsImmune = true;
                    }
                    else if (this.PsykerPowerLevel == PsykerPowerLevel.Omega)
                    {
                        this.IsImmune = true;
                        this.curLevel = Rand.Range(0.86f, 0.99f);
                        this.Corrupted = true;
                    }
                    else
                    {
                        float afup = cdef.AfflictionProperty.UpperAfflictionLimit;
                        float afdown = cdef.AfflictionProperty.LowerAfflictionLimit;
                        this.curLevel = (Rand.Range(afup, afdown));
                    }

                    this.CulturalTolerance = PawnAfflictionProps.PrimaryToleranceCategory;

                    if (cdef.AfflictionProperty.UseForcedPatron)
                    {
                        try
                        {

                            this.GainChaosGod(cdef.AfflictionProperty.Patron);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
            else
            {
                this.InitializeVanillaPawn();
            }
        }

        private void InitializeVanillaPawn()
        {
            this.PawnAfflictionProps = new AfflictionProperty();
            float pNum = Rand.GaussianAsymmetric(2.5f, 0.45f, 2);
            if (pNum < 0)
            {
                pNum = 0;
            }
            else if (pNum > 7)
            {
                pNum = 7;
            }
            this.PsykerPowerLevel = (PsykerPowerLevel)pNum;
            this.Patron = PatronDefOf.Emperor;

            this.CulturalTolerance = (CulturalToleranceCategory)Rand.RangeInclusive(0, 2);
            if (this.PsykerPowerLevel == PsykerPowerLevel.Omega)
            {
                this.IsImmune = true;
                this.curLevel = Rand.Range(0.86f, 0.99f);
                this.Corrupted = true;
            }
            else
            {
                this.curLevel = Rand.Range(0.4f, 0.99f);
            }
        }

        #endregion

        public override void CompTickRare()
        {
            if (this.Patron == PatronDefOf.Khorne || this.Patron == PatronDefOf.Slaanesh)
            {
                this.PawnKillTracker.lastKillTick--;
            }

            this.CheckPawnCorrupted();

            if (curLevel > 1f)
            {
                curLevel = 0.99999f;
            }
        }

        private void CheckPawnCorrupted()
        {
            if (this.curLevel < 0.3f && Corrupted == true)
            {
                GainChaosGod(PatronDefOf.ChaosUndivided);

                if (Pawn.Faction == Faction.OfPlayer)
                {
                    string label = "LetterPatronGained".Translate();
                    string text2 = "LetterPatronGained_Content".Translate(new object[]
                    {
                    this.Pawn.LabelShort,
                    this.Patron.ToString()
                    });
                    Find.LetterStack.ReceiveLetter(label, text2, LetterDefOf.NegativeEvent, this.Pawn, null);
                }
            }
        }

        private void DoCorruptedPawnEffects()
        {

            if (Corrupted == false)
            {
                if (curLevel > 0.3f)
                {
                    curLevel = 0.3f;
                }
                if (Rand.Range(0f, 0.4f) > (this.Pawn.needs.mood.CurLevel + this.CurLevel) && !this.Pawn.InMentalState)
                {
                    MentalStateDef mdef;
                    if (this.Patron == PatronDefOf.Khorne)
                    {
                        mdef = ChaosGodsUtilities.KhorneEffects(this.Pawn);
                        ChaosGodsUtilities.DoEffectOn(this.Pawn, mdef);
                    }
                    else if (this.Patron == PatronDefOf.Slaanesh)
                    {
                        mdef = ChaosGodsUtilities.SlaaneshEffects(this.Pawn);
                        ChaosGodsUtilities.DoEffectOn(this.Pawn, mdef);
                    }
                }
            }
        }
        
        public void AffectSoul(float amount)
        {
            if (this.cdef != null)
            {
                if (this.IsImmune)
                {
                    return;
                }
                amount = cdef.AfflictionProperty.ResolveFactor * amount;
            }
            amount /= this.AllSoulTraits.Sum(x => x.SoulCurrentData.CorruptionResistanceFactor);
            this.curLevel += amount;
            this.compPsyker.AddXP(amount);
        }
        

        public void GeneratePatronTraits(Pawn tPawn)
        {
            this.PatronTrait = new SoulTrait(this.Patron.PatronTraits[0], 0);
        }

        public void GainChaosGod(PatronDef forcedChaosGod = null)
        {

            if (PawnAfflictionProps == null)
            {
                PawnAfflictionProps = new AfflictionProperty();
                PawnAfflictionProps.Patron = PatronDefOf.ChaosUndivided;
            }

            if (forcedChaosGod != null)
            {
                PawnAfflictionProps.Patron = forcedChaosGod;
                this.Patron = PawnAfflictionProps.Patron;
            }
            else
            {

                if (Pawn.Faction.def.GetType() == typeof(FactionDefUniform))
                {
                    FactionDefUniform Facdef = this.Pawn.Faction.def as FactionDefUniform;
                        PawnAfflictionProps.Patron = Facdef.PreferredChaosGod;                    
                }

                if (PawnAfflictionProps == null)
                {
                    PawnAfflictionProps = new AfflictionProperty();
                    PawnAfflictionProps.Patron = PatronDefOf.ChaosUndivided;
                }

                if (PawnAfflictionProps.Patron == PatronDefOf.ChaosUndivided)
                {
                    if (Rand.Range(0.1f, 1f) > 0.5f)
                    {
                        this.Patron = PatronDefOf.ChaosUndivided;
                    }
                }
                else
                {
                    this.Patron = PawnAfflictionProps.Patron;
                }
            }

            GeneratePatronTraits(Pawn);
            this.DevotionTrait = new SoulTrait(C_SoulTraitDefOf.Devotion, Rand.RangeInclusive(0, 2));
            if (Pawn.story == null)
            {
                Pawn.story = new Pawn_StoryTracker(Pawn);
            }
            this.ResetPawnGraphics();

            if (this.Patron.IsChaosGod)
            {
                this.Corrupted = false;
            }
            else
            {
                this.Corrupted = true;
            }
        }

        private void ResetPawnGraphics()
        {
            if (this.Pawn.Drawer != null && this.Pawn.story != null)
            {
                if (this.Pawn.Drawer.renderer != null)
                {
                    if (this.Pawn.Drawer.renderer.graphics != null && this.Pawn.apparel != null)
                    {
                        LongEventHandler.ExecuteWhenFinished(delegate
                        {
                            this.Pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                            PortraitsCache.SetDirty(this.Pawn);
                        });
                    }
                }
            }
        }
     
        public void GenerateHediffsAndImplants(ChaosFollowerPawnKindDef cdef)
        {
            if (cdef.AdditionalImplantCount.min > 0)
            {
                int num = Rand.RangeInclusive(cdef.AdditionalImplantCount.min, cdef.AdditionalImplantCount.max);
                for (int i = 0; i < num; i++)
                {
                    PawnTechHediffsGenerator.GeneratePartsAndImplantsFor(this.Pawn);
                }
            }
            if (!cdef.DisallowedStartingHediffs.NullOrEmpty())
            {
                foreach (HediffDef def in cdef.DisallowedStartingHediffs)
                {
                    this.Pawn.health.hediffSet.hediffs.RemoveAll(x => x.def == def);
                }
            }

            if (!cdef.ForcedStartingHediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in cdef.ForcedStartingHediffs)
                {
                    Hediff current = HediffMaker.MakeHediff(hediffDef, this.Pawn, null);
                    this.Pawn.health.AddHediff(current);
                }
            }

            if (!cdef.ForcedStartingImplantRecipes.NullOrEmpty())
            {
                foreach (RecipeDef recipeDef in cdef.ForcedStartingImplantRecipes)
                {
                    if (!recipeDef.targetsBodyPart || recipeDef.Worker.GetPartsToApplyOn(Pawn, recipeDef).Any<BodyPartRecord>())
                    {
                        recipeDef.Worker.ApplyOnPawn(Pawn, recipeDef.Worker.GetPartsToApplyOn(Pawn, recipeDef).Any<BodyPartRecord>() ? recipeDef.Worker.GetPartsToApplyOn(Pawn, recipeDef).RandomElement<BodyPartRecord>() : null, null, new List<Thing>(), null);
                    }
                }
            }

        }

        public void AddSpecialTrait(SoulTraitDef soulTraitDef)
        {
            this.SpecialTrait = new SoulTrait(soulTraitDef, 0);
            if (CorruptionModSettings.AllowPsykers)
            {
                List<PsykerPowerDef> powersToUnlock = soulTraitDef.SDegreeDataAt(0).psykerPowersToUnlock;
                for (int i = 0; i < powersToUnlock.Count; i++)
                {
                    this.compPsyker.PsykerPowerManager.AddPsykerPower(powersToUnlock[i]);
                }
            }
        }

        public void DiscoverAlignment()
        {
            this.KnownToPlayer = true;
            float effectChance = Rand.Value;

            if (this.Patron.IsChaosGod && CorruptionStoryTrackerUtilities.PlayerIsIoM)
            {
                if (effectChance > 0.8f)
                {
                    this.Pawn.SetFactionDirect(Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.ChaosCult_NPC));
                    this.Pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "CultistDiscoveryBerserk".Translate());
                }
                else if (effectChance > 0.5f)
                {
                    this.Pawn.SetFactionDirect(Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.ChaosCult_NPC));
                    this.Pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, "CultistDiscoveredFlee".Translate());
                }
            }

            string letterText = "AlignmentDiscovered".Translate(new object[] { this.Pawn.NameStringShort, this.Patron.label });
            Find.LetterStack.ReceiveLetter("AlignmentDiscoveredHeader".Translate(), letterText, LetterDefOf.NeutralEvent, this.Pawn, null);

        }

        public static void TryDiscoverAlignment(Pawn investigator, Pawn target, float modifier = 0f)
        {
            CompSoul soul = CompSoul.GetPawnSoul(target);
            if (soul != null && !soul.KnownToPlayer)
            {
                int socialSkillDifference = CorruptionStoryTrackerUtilities.SocialSkillDifference(investigator, target);
                float skillFactor = socialSkillDifference / 20f;
                if (Rand.Value < 0.2f + skillFactor + modifier)
                {
                    soul.DiscoverAlignment();
                }
                else
                {
                    soul.AlignmentDiscoveryFailure(investigator, target, socialSkillDifference);
                }
            }
        }

        public void AlignmentDiscoveryFailure(Pawn investigator, Pawn target, float skillfactor)
        {
            int thoughtStage = 0;
            float absSkillFactor = Math.Abs(skillfactor);
            if (absSkillFactor > 0.6f)
            {
                thoughtStage = 0;
            }
            else if (absSkillFactor > 0.2f)
            {
                thoughtStage = 1;
            }
            else
            {
                thoughtStage = 2;
            }

            Thought_MemorySocial thought = new Thought_MemorySocial();
            thought.SetForcedStage(thoughtStage);
            target.needs.mood.thoughts.memories.TryGainMemory(thought, investigator);
            Messages.Message("AlignmentDiscoveryFailed".Translate(new object[] { investigator.NameStringShort, target.NameStringShort }), target, MessageTypeDefOf.NegativeEvent);
        }

        public static CompSoul GetPawnSoul(Pawn pawn)
        {
            return pawn.TryGetComp<CompSoul>();
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption o in base.CompFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            if (!this.KnownToPlayer)
            {
                int successFactorSocial = (int)(Math.Max(0, 0.2f + (CorruptionStoryTrackerUtilities.DiscoverAlignmentByConfessionModifier + CorruptionStoryTrackerUtilities.SocialSkillDifference(selPawn, this.Pawn) / 20f) * 100));
                BuildingAltar altar = null;
                CompSoul selSoul = CompSoul.GetPawnSoul(selPawn);
                if (CorruptionStoryTrackerUtilities.IsPreacher(selPawn, out altar))
                {
                    yield return new FloatMenuOption("StartConfession".Translate(new object[] { successFactorSocial.ToString() }), delegate
                    {
                        List<Pawn> list = new List<Pawn>() { selPawn, this.Pawn };
                        Lord lord = LordMaker.MakeNewLord(altar.Faction, new LordJob_Sermon(altar, WorshipActType.Confession), altar.Map, list);

                    });
                }

                yield return new FloatMenuOption("InvestigateAlignment".Translate(new object[] { successFactorSocial.ToString() }), delegate
                {
                    Job job = new Job(C_JobDefOf.FollowAndInvestigate, this.Pawn);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                });
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<ReadablesManager>(ref this.readablesManager, "readablesManager", new object[0]);
            Scribe_Values.Look<bool>(ref this.Corrupted, "Corrupted", false, false);
            Scribe_Values.Look<bool>(ref this.IsImmune, "IsImmune", false, false);
            Scribe_Values.Look<bool>(ref this.SoulInitialized, "SoulInitialized", true, false);
            Scribe_Values.Look<bool>(ref this.KnownToPlayer, "KnownToPlayer", false, false);
            Scribe_Collections.Look<Pawn>(ref this.OpposingDevotees, "OpposingDevotees", LookMode.Reference, new object[0]);
            Scribe_Defs.Look<PatronDef>(ref this.Patron, "Patron");
            Scribe_Values.Look<float>(ref this.curLevel, "curLevel", 0.5f, false);
            Scribe_Values.Look<PsykerPowerLevel>(ref this.PsykerPowerLevel, "PsykerPowerLevel", PsykerPowerLevel.Rho, false);
            Scribe_Values.Look<CulturalToleranceCategory>(ref this.CulturalTolerance, "CulturalTolerance", CulturalToleranceCategory.Neutral, false);
            Scribe_Deep.Look<SoulTrait>(ref this.DevotionTrait, "DevotionTrait", new object[0]);
            Scribe_Deep.Look(ref this.PawnKillTracker, "PawnKillTracker", new object[0]);
            Scribe_Deep.Look<SoulTrait>(ref this.PatronTrait, "PatronTrait", new object[0]);
            Scribe_Deep.Look<SoulTrait>(ref this.CommonSoulTrait, "CommonSoulTrait", new object[0]);
            Scribe_Deep.Look<SoulTrait>(ref this.SpecialTrait, "SpecialTrait", new object[0]);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
            }
        }
    }
}
