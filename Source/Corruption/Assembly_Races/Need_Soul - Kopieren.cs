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

namespace Corruption
{
    public class Need_Soul : Need
    {
        private bool IsImmune;

        public ReadablesManager readablesManager = new ReadablesManager();

        private const float ThreshCorrupted = 0.1f;

        private const float ThreshTainted = 0.3f;

        private const float ThreshWarptouched = 0.5f;

        private const float ThreshIntrigued = 0.7f;

        private const float ThreshPure = 0.9f;
        
        public bool KnownToPlayer = false;

        public PawnKillTracker PawnKillTracker;

        public CompPsyker compPsyker
        {
            get
            {
                return this.pawn.TryGetComp<CompPsyker>();
            }
        }

        public PsykerPowerLevel PsykerPowerLevel;

        public SoulTraitDegreeData DevotionTraitDegree;

        public List<SoulTrait> SoulTraits
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

        public List<PsykerPower> psykerPowers = new List<PsykerPower>();

        public PatronDef Patron;

        public CulturalToleranceCategory CulturalTolerance;
        
        public AfflictionProperty PawnAfflictionProps;

        public SoulTrait DevotionTrait;

        public SoulTrait CommonSoulTrait;

        public SoulTrait PatronTrait;

        public SoulTrait SpecialTrait;

        public bool NotCorrupted = true;

        public bool SoulInitialized = false;

        public List<Pawn> OpposingDevotees = new List<Pawn>();

        private ChaosFollowerPawnKindDef cdef
        {
            get
            {
                return this.pawn.kindDef as ChaosFollowerPawnKindDef;
            }
        }

        public SoulAffliction CurCategory
        {
            get
            {
                if (this.curLevelInt <= 0.0f)
                {
                    return SoulAffliction.Lost;
                }
                if (this.curLevelInt < 0.3f)
                {
                    return SoulAffliction.Corrupted;
                }
                if (this.curLevelInt < 0.4f)
                {
                    return SoulAffliction.Tainted;
                }
                if (this.curLevelInt < 0.65f)
                {
                    return SoulAffliction.Warptouched;
                }
                if (this.curLevelInt < 0.85f)
                {
                    return SoulAffliction.Intrigued;
                }
                if (this.curLevelInt >= 0.85f)
                {
                    return SoulAffliction.Pure;
                }
                return SoulAffliction.Pure;
            }
        }

        public bool IsOnPilgrimage;

        public Need_Soul(Pawn pawn) : base(pawn)
        {
            this.threshPercents = new List<float>();
            this.threshPercents.Add(0.1f);
            this.threshPercents.Add(0.3f);
            this.threshPercents.Add(0.5f);
            this.threshPercents.Add(0.7f);
            this.threshPercents.Add(0.9f);
        }

        public override void SetInitialLevel()
        {
            if (!SoulInitialized)
            {
                this.PawnKillTracker = new PawnKillTracker();
                FieldInfo info = typeof(StatsReportUtility).GetField("cachedDrawEntries", BindingFlags.NonPublic | BindingFlags.Static);
                if (info != null)
                {
                    List<StatDrawEntry> entries = info.GetValue(this.pawn) as List<StatDrawEntry>;
                    if (!entries.NullOrEmpty())
                    {
                        entries.Add(new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Patron", this.Patron.ToString(), 3));
                        entries.Add(new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "CulturalToleranceStat".Translate(), this.Patron.ToString(), 2));
                        entries.Add(new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "PurityOfSoulStat".Translate(), this.CurLevel.ToString(), 1));
                    }
                }

                if (CorruptionModSettings.AllowPsykers)
                {
                    InitiatePsykerComp();
                }

                ChaosFollowerPawnKindDef pdef = this.pawn.kindDef as ChaosFollowerPawnKindDef;
                //       Log.Message("Name is: " + this.pawn.Name.ToStringFull);
                if (pdef != null)
                {
                    if (pdef.IsServitor)
                    {
                        CorruptionStoryTrackerUtilities.InitiateServitorComp(pawn);
                    }
                    if (pdef.UseFixedGender)
                    {
                        this.pawn.gender = pdef.FixedGender;
                    }
                    if (pdef.AfflictionProperty != null)
                    {

                        PawnAfflictionProps = new AfflictionProperty();
                        this.PawnAfflictionProps = pdef.AfflictionProperty;
                        if (CorruptionModSettings.AllowPsykers)
                        {
                            int pllow = (int)this.PawnAfflictionProps.LowerPsykerPowerLimit;
                            int plup = (int)this.PawnAfflictionProps.UpperAfflictionLimit;
                            this.PsykerPowerLevel = (PsykerPowerLevel)Rand.RangeInclusive(pllow, plup);
                        }
                        
                        if (PawnAfflictionProps.IsImmune)
                        {
                            this.curLevelInt = 0.99f;
                            this.DevotionTrait = new SoulTrait(C_SoulTraitDefOf.Devotion, PawnAfflictionProps.ImmuneDevotionDegree);
                            this.IsImmune = true;
                        }
                        else if (this.PsykerPowerLevel == PsykerPowerLevel.Omega)
                        {
                            this.IsImmune = true;
                            this.curLevelInt = Rand.Range(0.86f, 0.99f);
                            this.NotCorrupted = true;
                        }
                        else
                        {
                            float afup = pdef.AfflictionProperty.UpperAfflictionLimit;
                            float afdown = pdef.AfflictionProperty.LowerAfflictionLimit;
                            this.curLevelInt = (Rand.Range(afup, afdown));
                        }

                        this.CulturalTolerance = PawnAfflictionProps.PrimaryToleranceCategory;
                        if (pdef.AfflictionProperty.UseForcedPatron)
                        {
                            try
                            {

                                this.GainPatron(pdef.AfflictionProperty.Patron, true);
                            }
                            catch (Exception)
                            {
                                //Log.Message(ex.Message);
                                throw;
                            }
                        }
                    }
                }
                else
                {
                    PawnAfflictionProps = new AfflictionProperty();
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
                        this.curLevelInt = Rand.Range(0.86f, 0.99f);
                        this.NotCorrupted = true;
                    }
                    else
                    {
                        this.curLevelInt = Rand.Range(0.4f, 0.99f);
                    }
                }

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
                    pawn.caller = new Pawn_CallTracker(pawn);
                }
                if (this.curLevelInt < 0.3f && NotCorrupted == true)
                {
                    GainPatron(PatronDefOf.ChaosUndivided, false);

                }
                if (NotCorrupted == false)
                {
                    if (curLevelInt > 0.3f)
                    {
                        curLevelInt = 0.3f;
                    }
                }
                if (this.PsykerPowerLevel == PsykerPowerLevel.Omega)
                {
                    this.IsImmune = true;
                }

                this.SoulInitialized = true;

                if (CorruptionModSettings.AllowPsykers)
                {
                    if (this.compPsyker.Patron != this.Patron)
                    {
                        this.compPsyker.Patron = this.Patron;
                        PortraitsCache.SetDirty(this.pawn);
                    }
                }

                if (this.pawn.IsColonist)
                {
                    this.KnownToPlayer = true;
                }

            }
        }

        public override void NeedInterval()
        {
            if (this.Patron == PatronDefOf.Khorne || this.Patron == PatronDefOf.Slaanesh)
            {
                this.PawnKillTracker.lastKillTick--;
            }

            if (this.curLevelInt < 0f)
            {
                this.curLevelInt = 0f;
            }
            if (this.curLevelInt < 0.3f && NotCorrupted == true)
            {
                GainPatron(PatronDefOf.ChaosUndivided, false);

                if (pawn.Faction == Faction.OfPlayer)
                {
                    string label = "LetterPatronGained".Translate();
                    string text2 = "LetterPatronGained_Content".Translate(new object[]
                    {
                    this.pawn.LabelShort,
                    this.Patron.ToString()
                    });
                    Find.LetterStack.ReceiveLetter(label, text2, LetterDefOf.NegativeEvent, this.pawn, null);
                }
            }
            if (NotCorrupted == false)
            {
                if (curLevelInt > 0.3f)
                {
                    curLevelInt = 0.3f;
                }
                if (Rand.Range(0f, 0.4f) > (this.pawn.needs.mood.CurLevel + this.CurLevel) && !this.pawn.InMentalState)
                {
                    MentalStateDef mdef;
                    if (this.Patron == PatronDefOf.Khorne)
                    {
                        mdef = ChaosGodsUtilities.KhorneEffects(this.pawn, this);
                        ChaosGodsUtilities.DoEffectOn(this.pawn, mdef);
                    }
                    else if (this.Patron == PatronDefOf.Slaanesh)
                    {
                        mdef = ChaosGodsUtilities.SlaaneshEffects(this.pawn, this);
                        ChaosGodsUtilities.DoEffectOn(this.pawn, mdef);
                    }
                }
            }
            if (curLevelInt > 1f)
            {
                curLevelInt = 0.99999f;
            }            
        }

        public void GainNeed(float amount)
        {
            if (this.cdef != null)
            {
                if (this.IsImmune)
                {
                    return;
                }
                amount = cdef.AfflictionProperty.ResolveFactor * amount;
            }
            amount /= this.SoulTraits.Sum(x => x.SoulCurrentData.CorruptionResistanceFactor);
            this.curLevelInt += amount;
            this.compPsyker.AddXP(amount);
        }

        public void InitiatePsykerComp()
        {
            //CompPsyker compPsyker = this.pawn.TryGetComp<CompPsyker>();
            //compPsyker.parent = this.pawn;
            //CompProperties_Psyker cprops = new CompProperties_Psyker();
            //cprops.compClass = typeof(CompPsyker);
            //compPsyker.Initialize(cprops);
            //compPsyker.Patron = this.Patron;
            //FieldInfo info = typeof(ThingWithComps).GetField("comps", BindingFlags.NonPublic | BindingFlags.Instance);
            //if (info != null)
            //{
            //    List<ThingComp> list = info.GetValue(this.pawn) as List<ThingComp>;
            //    if (list.FirstOrDefault(x => x.GetType() == typeof(CompPsyker)) == null)
            //    {
            //        list.Add(compPsyker);
            //        info.SetValue(this.pawn, list);
            //        compPsyker.PostSpawnSetup(true);
            //        this.compPsyker = compPsyker;
            //    }
            //}
        }



        public void GeneratePatronTraits(Pawn tpawn)
        {
            this.PatronTrait = new SoulTrait(this.Patron.PatronTraits[0], 0);
        }

        public void GainPatron(PatronDef forcedPatron, bool UseForcedPatron)
        {

            if (PawnAfflictionProps == null)
            {
                PawnAfflictionProps = new AfflictionProperty();
                PawnAfflictionProps.Patron = PatronDefOf.ChaosUndivided;
            }

            if (UseForcedPatron)
            {
                PawnAfflictionProps.Patron = forcedPatron;
                this.Patron = PawnAfflictionProps.Patron;
            }
            else
            {

                if (pawn.Faction.def.GetType() == typeof(FactionDefUniform))
                {
                    FactionDefUniform Facdef = this.pawn.Faction.def as FactionDefUniform;

                    if (Facdef.PreferredChaosGod == PatronDefOf.ChaosUndivided)
                    {
                        PawnAfflictionProps.Patron = PatronDefOf.ChaosUndivided;
                    }
                    else
                    {
                        PawnAfflictionProps.Patron = Facdef.PreferredChaosGod;
                    }
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
            
            GeneratePatronTraits(pawn);
            this.DevotionTrait = new SoulTrait(C_SoulTraitDefOf.Devotion, Rand.RangeInclusive(0, 2));
            if (pawn.story == null)
            {
                pawn.story = new Pawn_StoryTracker(pawn);
            }
            //         Hediff chaosmark = HediffMaker.MakeHediff(ChaosMarkDef(this.patronInfo.PatronName), this.pawn,null);
            //         this.pawn.health.hediffSet.AddHediffDirect(chaosmark);
            if (this.pawn.Drawer != null && this.pawn.story != null)
            {
                if (this.pawn.Drawer.renderer != null)
                {
                    if (this.pawn.Drawer.renderer.graphics != null && this.pawn.apparel != null)
                    {
                        LongEventHandler.ExecuteWhenFinished(delegate
                        {
                            this.pawn.Drawer.renderer.graphics.ResolveApparelGraphics();
                            PortraitsCache.SetDirty(this.pawn);
                        });
                    }
                }
            }
            if (this.Patron.IsChaosGod)
            {
                this.NotCorrupted = false;
            }
            else
            {
                this.NotCorrupted = true;
            }
        }

        public string PatronName(Pawn pawn)
        {
                return pawn.needs.TryGetNeed<Need_Soul>().Patron.label;            
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1F, bool drawArrows = true, bool doTooltip = true)
        {
            if (this.KnownToPlayer)
            {


                if (rect.height > 70f)
                {
                    float num = (rect.height - 70f) / 2f;
                    rect.height = 70f;
                    rect.y += num;
                }
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }

                if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
                {
                    Find.WindowStack.Add(new MainTabWindow_Alignment());
                }
                TooltipHandler.TipRegion(rect, new TipSignal(() => this.GetTipString(), rect.GetHashCode()));
                float num2 = 14f;
                float num3 = num2 + 15f;
                if (rect.height < 50f)
                {
                    num2 *= Mathf.InverseLerp(0f, 50f, rect.height);
                }
                Text.Font = ((rect.height <= 55f) ? GameFont.Tiny : GameFont.Small);
                Text.Anchor = TextAnchor.LowerLeft;
                Rect rect2 = new Rect(rect.x + num3 + rect.width * 0.1f, rect.y, rect.width - num3 - rect.width * 0.1f, rect.height / 2f);
                Widgets.Label(rect2, this.LabelCap);
                Text.Anchor = TextAnchor.UpperLeft;
                Rect rect3 = new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
                rect3 = new Rect(rect3.x + num3, rect3.y, rect3.width - num3 * 2f, rect3.height - num2);
                Widgets.FillableBar(rect3, this.CurLevelPercentage);
                Widgets.FillableBarChangeArrows(rect3, this.GUIChangeArrow);
                if (this.threshPercents != null)
                {
                    for (int i = 0; i < this.threshPercents.Count; i++)
                    {
                        this.DrawBarThreshold(rect3, this.threshPercents[i]);
                    }
                }
                float curInstantLevelPercentage = this.CurInstantLevelPercentage;
                if (curInstantLevelPercentage >= 0f)
                {
                    this.DrawBarInstantMarkerAt(rect3, curInstantLevelPercentage);
                }
                Text.Font = GameFont.Small;
            }
        }

        private void DrawBarThreshold(Rect barRect, float threshPct)
        {
            float num = (float)((barRect.width <= 60f) ? 1 : 2);
            Rect position = new Rect(barRect.x + barRect.width * threshPct - (num - 1f), barRect.y + barRect.height / 2f, num, barRect.height / 2f);
            Texture2D image;
            if (threshPct < this.CurLevelPercentage)
            {
                image = BaseContent.BlackTex;
                GUI.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                image = BaseContent.GreyTex;
                GUI.color = new Color(1f, 1f, 1f, 0.5f);
            }
            GUI.DrawTexture(position, image);
            GUI.color = Color.white;
        }

        public void GenerateHediffsAndImplants(ChaosFollowerPawnKindDef pdef)
        {
            if (pdef.AdditionalImplantCount.min > 0)
            {
                int num = Rand.RangeInclusive(pdef.AdditionalImplantCount.min, pdef.AdditionalImplantCount.max);
                for (int i = 0; i < num; i++)
                {
                    PawnTechHediffsGenerator.GeneratePartsAndImplantsFor(this.pawn);
                }
            }
            if (!pdef.DisallowedStartingHediffs.NullOrEmpty())
            {
                foreach (HediffDef def in pdef.DisallowedStartingHediffs)
                {
                    this.pawn.health.hediffSet.hediffs.RemoveAll(x => x.def == def);
                }
            }
            
            if (!pdef.ForcedStartingHediffs.NullOrEmpty())
            {
                foreach (HediffDef hediffDef in pdef.ForcedStartingHediffs)
                {
                    Hediff current = HediffMaker.MakeHediff(hediffDef, this.pawn, null);
                    this.pawn.health.AddHediff(current);
                }
            }
            
            if (!pdef.ForcedStartingImplantRecipes.NullOrEmpty())
            {
                foreach (RecipeDef recipeDef in pdef.ForcedStartingImplantRecipes)
                {
                    if (!recipeDef.targetsBodyPart || recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).Any<BodyPartRecord>())
                    {
                        recipeDef.Worker.ApplyOnPawn(pawn, recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).Any<BodyPartRecord>() ? recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).RandomElement<BodyPartRecord>() : null, null, new List<Thing>(), null);
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
        
        private void DiscoverAlignment()
        {
            this.KnownToPlayer = true;
            float effectChance = Rand.Value;

            if (this.Patron.IsChaosGod && CorruptionStoryTrackerUtilities.PlayerIsIoM) 
            {
                if (effectChance > 0.8f)
                {
                    this.pawn.SetFactionDirect(Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.ChaosCult_NPC));
                    this.pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, "CultistDiscoveryBerserk".Translate());
                }
                else if (effectChance > 0.5f)
                {
                    this.pawn.SetFactionDirect(Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.ChaosCult_NPC));
                    this.pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee, "CultistDiscoveredFlee".Translate());
                }
            }
        }

        public static void TryDiscoverAlignment(Pawn investigator, Pawn target, float modifier = 0f)
        {
            int socialSkillDifference = CorruptionStoryTrackerUtilities.SocialSkillDifference(investigator, target);
            float skillFactor = socialSkillDifference / 20f;
            if (Rand.Value < 0.3f + skillFactor + modifier)
            {
                CorruptionStoryTrackerUtilities.GetPawnSoul(target).KnownToPlayer = true;
            }
        }

        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ReadablesManager>(ref this.readablesManager, "readablesManager", new object[0]);
    //        Scribe_Deep.Look<PatronInfo>(ref this.patronInfo, "patronInfo", new object());
            Scribe_Values.Look<bool>(ref this.NotCorrupted, "NoPatron", true, false);
            Scribe_Values.Look<bool>(ref this.IsImmune, "IsImmune", false, false);
            Scribe_Values.Look<bool>(ref this.SoulInitialized, "SoulInitialized", true, false);
            Scribe_Values.Look<bool>(ref this.KnownToPlayer, "KnownToPlayer", false, false);


            //        Scribe_Collections.Look<SoulTrait>(ref this.SoulTraits, "SoulTraits", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.OpposingDevotees, "OpposingDevotees", LookMode.Reference, new object[0]);

            Scribe_Defs.Look<PatronDef>(ref this.Patron, "Patron");

            Scribe_Values.Look<PsykerPowerLevel>(ref this.PsykerPowerLevel, "PsykerPowerLevel", PsykerPowerLevel.Rho, false);
            Scribe_Values.Look<CulturalToleranceCategory>(ref this.CulturalTolerance, "CulturalTolerance", CulturalToleranceCategory.Neutral, false);

            //        Scribe_Deep.Look<AfflictionProperty>(ref this.PawnAfflictionProps, "PawnAfflictionProps", null);

            Scribe_Deep.Look<SoulTrait>(ref this.DevotionTrait, "DevotionTrait", new object[0]);
            Scribe_Deep.Look(ref this.PawnKillTracker, "PawnKillTracker", new object[0]);
            Scribe_Deep.Look<SoulTrait>(ref this.PatronTrait, "PatronTrait", new object[0]);
            Scribe_Deep.Look<SoulTrait>(ref this.CommonSoulTrait, "CommonSoulTrait", new object[0]);
            Scribe_Deep.Look<SoulTrait>(ref this.SpecialTrait, "SpecialTrait", new object[0]);
            //   {
            //       CorruptionDefOfs.Devotion,
            //       this.DevotionTrait.SDegree                
            //   });

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
            }
        }
    }
}
