using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;
using Verse.AI.Group;

namespace AlienRace
{
    [StaticConstructorOnStartup]
    public class AlienPawn : Pawn
    {
        public string TexPathHair;
        public Color HColor;
        public Color alienskincolor;
        public string headtexpath;
        public string nakedbodytexpath = "Things/Pawn/Humanlike/Bodies/";
        public string dessicatedgraphicpath;
        public string skullgraphicpath;
        private Graphic nakedGraphic;
        private Graphic rottingGraphic;
        private Graphic desiccatedGraphic;
        private Graphic headGraphic;
        private Graphic desiccatedHeadGraphic;
        private Graphic skullGraphic;
        private Graphic hairGraphic;
        public Vector2 DrawSize;
        private bool backstoryflag;
        private List<AlienTraitEntry> fTraits;
        private bool FirstSpawn = false;
        public static int MainThreadId;

        public AlienPawn()
        {
        }

        public AlienPawn(Thing thing)
        {
            AlienPawn alienpawn = AlienRaceUtilities.ConvertTo<AlienPawn>(thing);
        }

        public AlienPawn(Pawn pawn)
        {
            AlienPawn alienpawn = AlienRaceUtilities.ConvertTo<AlienPawn>(pawn);
        }

        public void ReadXML()
        {
            Thingdef_AlienRace thingdef_alienrace = this.def as Thingdef_AlienRace;
            if (thingdef_alienrace == null) { Log.Message("Could not read Alien ThingDef."); }
            if (!FirstSpawn)
            {

                if (thingdef_alienrace.CustomSkinColors == true)
                {
                    alienskincolor = thingdef_alienrace.alienskincolorgen.NewRandomizedColor();

                }
                else alienskincolor = this.story.SkinColor;

                if (thingdef_alienrace.NakedBodyGraphicLocation.NullOrEmpty())
                {
                    nakedbodytexpath = "Things/Pawn/Humanlike/Bodies/";
                }
                else
                {
                    nakedbodytexpath = thingdef_alienrace.NakedBodyGraphicLocation;
                }

                if (thingdef_alienrace.DesiccatedGraphicLocation.NullOrEmpty())
                {
                    dessicatedgraphicpath = "Things/Pawn/Humanlike/HumanoidDessicated";
                }
                else
                {
                    dessicatedgraphicpath = thingdef_alienrace.DesiccatedGraphicLocation;
                }

                if (thingdef_alienrace.SkullGraphicLocation.NullOrEmpty())
                {
                    skullgraphicpath = "Things/Pawn/Humanlike/Heads/None_Average_Skull";
                }
                else
                {
                    skullgraphicpath = thingdef_alienrace.SkullGraphicLocation;
                }



                switch (thingdef_alienrace.HasHair)
                {
                    case AlienHairTypes.None:
                        {
                            HColor = Color.white;
                            this.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved", true);
                            TexPathHair = this.story.hairDef.texPath;
                            break;
                        }
                    case AlienHairTypes.Custom:
                        {
                            TexPathHair = this.story.hairDef.texPath;
                            if (thingdef_alienrace.alienhaircolorgen == null)
                                HColor = PawnHairColorsAlien.RandomHairColor(alienskincolor, this.ageTracker.AgeBiologicalYears, thingdef_alienrace.GetsGreyAt);
                            else
                                HColor = thingdef_alienrace.alienhaircolorgen.NewRandomizedColor();
                            break;
                        }
                    case AlienHairTypes.Vanilla:
                        {
                            TexPathHair = this.story.hairDef.texPath;
                            HColor = this.story.hairColor;
                            break;
                        }
                }

                if (thingdef_alienrace.NakedHeadGraphicLocation.NullOrEmpty())
                {
                    Log.Message(this.story.HeadGraphicPath);
                    headtexpath = this.story.HeadGraphicPath;
                }

                else
                {

                    headtexpath = thingdef_alienrace.alienpartgenerator.RandomAlienHead(thingdef_alienrace.NakedHeadGraphicLocation);
                }

                if (thingdef_alienrace.CustomDrawSize == null)
                {
                    DrawSize = Vector2.one;
                }
                else
                {
                    DrawSize = thingdef_alienrace.CustomDrawSize;
                }
                if (thingdef_alienrace.CustomGenderDistribution)
                {
                    if (Rand.Range(0.1f, 0.9f) < thingdef_alienrace.MaleGenderProbability)
                    {
                        this.gender = Gender.Male;
                    }
                    else
                    {
                        this.gender = Gender.Female;
                    }
                }

                if (thingdef_alienrace.PawnsSpecificBackstories)
                {
                    backstoryflag = true;
                }
            }
            if (thingdef_alienrace.ForcedRaceTraitEntries != null)
            {
                fTraits = thingdef_alienrace.ForcedRaceTraitEntries;
                List<TraitDef> tlist = DefDatabase<TraitDef>.AllDefsListForReading;
                if (this.story.childhood.forcedTraits == null) this.story.childhood.forcedTraits = new List<TraitEntry>();
                for (int i = 0; i < fTraits.Count; i++)
                {
                    foreach (TraitDef tdef in tlist)
                    {
                        if (tdef.defName == fTraits[i].defname)
                        {
                            this.story.childhood.forcedTraits.Add(new TraitEntry(tdef, fTraits[i].degree));
                        }
                    }
                }
            }

            this.def = thingdef_alienrace;
            FirstSpawn = true;
            MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;            
        }

        public void SpawnSetupAlien(AlienPawn newguy)
        {
            this.ReadXML();
            if (backstoryflag) UpdateBackstories(newguy, newguy.kindDef);
            if (fTraits != null) UpdateForcedTraits(newguy);
            DoOnMainThread.ExecuteOnMainThread.Enqueue(() => {
            this.nakedGraphic = GraphicGetterAlienBody.GetNakedBodyGraphicAlien(this.story.BodyType, ShaderDatabase.CutoutSkin, alienskincolor, nakedbodytexpath, DrawSize);
            this.rottingGraphic = GraphicGetterAlienBody.GetNakedBodyGraphicAlien(this.story.BodyType, ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor, nakedbodytexpath, DrawSize);
            this.headGraphic = GraphicDatabase.Get<Graphic_Multi>(headtexpath, ShaderDatabase.Cutout, DrawSize, alienskincolor);
            this.desiccatedGraphic = GraphicDatabase.Get<Graphic_Multi>(dessicatedgraphicpath, ShaderDatabase.Cutout, DrawSize, Color.white);
            this.desiccatedHeadGraphic = GraphicDatabase.Get<Graphic_Multi>(headtexpath, ShaderDatabase.Cutout, DrawSize, PawnGraphicSet.RottingColor);
            this.skullGraphic = GraphicDatabase.Get<Graphic_Multi>(skullgraphicpath, ShaderDatabase.Cutout, DrawSize, Color.white);
            this.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(TexPathHair, ShaderDatabase.Cutout, DrawSize, HColor);
            this.UpdateGraphics();
            });
        }

        public void UpdateGraphics()
        {
                base.Drawer.renderer.graphics.headGraphic = this.headGraphic;
                base.Drawer.renderer.graphics.nakedGraphic = this.nakedGraphic;
                base.Drawer.renderer.graphics.hairGraphic = this.hairGraphic;
                base.Drawer.renderer.graphics.rottingGraphic = this.rottingGraphic;
                base.Drawer.renderer.graphics.desiccatedHeadGraphic = this.desiccatedHeadGraphic;
                base.Drawer.renderer.graphics.skullGraphic = this.skullGraphic;
                foreach (Apparel current in this.apparel.WornApparel)
            {
  //              current.Graphic.data.drawSize = current.wearer.Graphic.drawSize;
            }
                this.Drawer.renderer.graphics.ResolveApparelGraphics();
        }


        private static void UpdateBackstories(Pawn pawn, PawnKindDef pawntype)
        {
            SetBackstoryInSlot(pawn, BackstorySlot.Childhood, ref pawn.story.childhood, pawntype);
            SetBackstoryInSlot(pawn, BackstorySlot.Adulthood, ref pawn.story.adulthood, pawntype);
        } 
        

        private static void SetBackstoryInSlot(Pawn pawn, BackstorySlot slot, ref Backstory backstory, PawnKindDef pawntype)
        {
            if (!(from kvp in BackstoryDatabase.allBackstories
                  where kvp.Value.shuffleable && kvp.Value.spawnCategories.Contains(pawntype.backstoryCategory) && kvp.Value.slot == slot
                  select kvp.Value).TryRandomElement(out backstory))
            {
                backstory = (from kvp in BackstoryDatabase.allBackstories
                             where kvp.Value.slot == slot
                             select kvp).RandomElement<KeyValuePair<string, Backstory>>().Value;
            }
        }

        private void UpdateForcedTraits(AlienPawn apawn)
        {
            apawn.story.traits.allTraits.Clear();
            GiveRandomTraits(apawn, true);          
        }
        private static void GiveRandomTraits(Pawn pawn, bool allowGay)
        {
            if (pawn.story == null)
            {
                return;
            }
            if (pawn.story.childhood.forcedTraits != null)
            {
                List<TraitEntry> forcedTraits = pawn.story.childhood.forcedTraits;
                for (int i = 0; i < forcedTraits.Count; i++)
                {
                    TraitEntry traitEntry = forcedTraits[i];
                    if (traitEntry.def == null)
                    {
                        Log.Error("Null forced trait def on " + pawn.story.childhood);
                    }
                    else if (!pawn.story.traits.HasTrait(traitEntry.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree));
                    }
                }
            }
            if (pawn.story.adulthood.forcedTraits != null)
            {
                List<TraitEntry> forcedTraits2 = pawn.story.adulthood.forcedTraits;
                for (int j = 0; j < forcedTraits2.Count; j++)
                {
                    TraitEntry traitEntry2 = forcedTraits2[j];
                    if (traitEntry2.def == null)
                    {
                        Log.Error("Null forced trait def on " + pawn.story.adulthood);
                    }
                    else if (!pawn.story.traits.HasTrait(traitEntry2.def))
                    {
                        pawn.story.traits.GainTrait(new Trait(traitEntry2.def, traitEntry2.degree));
                    }
                }
            }
            int num = Rand.RangeInclusive(2, 3);
            if (allowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
            {
                Trait trait = new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay));
                pawn.story.traits.GainTrait(trait);
            }
            while (pawn.story.traits.allTraits.Count < num)
            {
                TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn));
                if (!pawn.story.traits.HasTrait(newTraitDef))
                {
                    if (newTraitDef == TraitDefOf.Gay)
                    {
                        if (!allowGay)
                        {
                            continue;
                        }
                        if (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))
                        {
                            continue;
                        }
                    }
                    if (!pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) && (newTraitDef.conflictingTraits == null || !newTraitDef.conflictingTraits.Any((TraitDef tr) => pawn.story.traits.HasTrait(tr))))
                    {
                        if (newTraitDef.requiredWorkTypes == null || !pawn.story.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes))
                        {
                            if (!pawn.story.WorkTagIsDisabled(newTraitDef.requiredWorkTags))
                            {
                                int degree = PawnGenerator.RandomTraitDegree(newTraitDef);
                                if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree))
                                {
                                    Trait trait2 = new Trait(newTraitDef, degree);
                                    if (pawn.mindState == null || pawn.mindState.mentalBreaker == null || pawn.mindState.mentalBreaker.BreakThresholdExtreme + trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold) <= 40f)
                                    {
                                        pawn.story.traits.GainTrait(trait2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void SetFaction(Faction newFaction, Pawn recruiter = null)
        {
            var x = this.kindDef;
            AlienPawn temprecruitee = this as AlienPawn;
            if (newFaction == base.Faction)
            {
                Log.Warning(string.Concat(new object[]
                {
            "Used ChangePawnFactionTo to change ",
            this,
            " to same faction ",
            newFaction
                }));
                return;
            }
            if (this.guest != null)
            {
                this.guest.SetGuestStatus(null, false);
            }
            Find.MapPawns.DeRegisterPawn(this);
            Find.PawnDestinationManager.RemovePawnFromSystem(this);
            Find.DesignationManager.RemoveAllDesignationsOn(this, false);
            if (newFaction == Faction.OfPlayer || base.Faction == Faction.OfPlayer)
            {
                Find.ColonistBar.MarkColonistsListDirty();
            }
            Lord lord = this.GetLord();
            if (lord != null)
            {
                lord.Notify_PawnLost(this, PawnLostCondition.ChangedFaction);
            }
            base.SetFaction(newFaction, null);
            PawnComponentsUtility.AddAndRemoveDynamicComponents(this, false);
            if (base.Faction != null && base.Faction.IsPlayer)
            {
                if (this.workSettings != null)
                {
                    this.workSettings.EnableAndInitialize();
                }
                Find.Storyteller.intenderPopulation.Notify_PopulationGained();
            }
            if (this.Drafted)
            {
                this.drafter.Drafted = false;
            }
            Reachability.ClearCache();
            this.health.surgeryBills.Clear();
            if (base.Spawned)
            {
                Find.MapPawns.RegisterPawn(this);
            }
            this.GenerateNecessaryName();
            if (this.playerSettings != null)
            {
                this.playerSettings.medCare = ((!this.RaceProps.Humanlike) ? (this.playerSettings.medCare = MedicalCareCategory.NoMeds) : MedicalCareCategory.Best);
            }
            this.ClearMind(true);
            if (!this.Dead && this.needs.mood != null)
            {
                this.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
            Find.AttackTargetsCache.UpdateTarget(this);
            Find.GameEnder.CheckGameOver();
            temprecruitee.kindDef = x;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.LookDef<ThingDef>(ref this.def, "def");
            Scribe_Values.LookValue<bool>(ref this.FirstSpawn, "FirstSpawn", false, false);
            Scribe_Values.LookValue<string>(ref this.headtexpath, "headtexpath", null, false);
            Scribe_Values.LookValue<string>(ref this.TexPathHair, "TexPathHair", null, false);
            Scribe_Values.LookValue<string>(ref this.nakedbodytexpath, "nakedbodytexpath", null, false);
            Scribe_Values.LookValue<string>(ref this.dessicatedgraphicpath, "dessicatedgraphicpath", null, false);
            Scribe_Values.LookValue<string>(ref this.skullgraphicpath, "skullgraphicpath", null, false);
            Scribe_Values.LookValue<Color>(ref this.alienskincolor, "alienskincolor");
            Scribe_Values.LookValue<Color>(ref this.HColor, "HColor");
        }

    }
}
