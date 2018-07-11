using Corruption.DefOfs;
using Corruption.IoM;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Log.Message("Generating Corruption Harmony Patches");
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.ohu.corruption.main");

            harmony.Patch(AccessTools.Method(typeof(RimWorld.ThoughtUtility), "CanGetThought"), null, new HarmonyMethod(typeof(HarmonyPatches), "CanGetThoughtPostfix"), null);
            harmony.Patch(AccessTools.Method(typeof(RimWorld.MainTabWindow_Inspect), "DoInspectPaneButtons"), null, new HarmonyMethod(typeof(HarmonyPatches), "DoInspectPaneButtons", null));
            harmony.Patch(AccessTools.Method(typeof(RimWorld.Page_ConfigureStartingPawns), "CanDoNext"), null, new HarmonyMethod(typeof(HarmonyPatches), "WorldGeneratePostfix"), null);
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGenerator), "GenerateInitialHediffs"), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateInitialHediffsPostFix", null));
            harmony.Patch(AccessTools.Method(typeof(RimWorld.RecordsUtility), "Notify_PawnKilled"), null, new HarmonyMethod(typeof(HarmonyPatches), "Notify_PawnKilledPostFix", null));
            harmony.Patch(AccessTools.Method(typeof(RimWorld.InteractionWorker_DeepTalk), "Interacted"), null, new HarmonyMethod(typeof(HarmonyPatches), "InteractedDeepTalkPostfix", null));
            harmony.Patch(AccessTools.Method(typeof(RimWorld.PawnComponentsUtility), "CreateInitialComponents"), null, new HarmonyMethod(typeof(HarmonyPatches), "CreateInitialComponentsPostfix", null));

            harmony.Patch(AccessTools.Method(typeof(RimWorld.CompIngredients), "CompInspectStringExtra"), new HarmonyMethod(typeof(HarmonyPatches), "CompInspectStringExtraPrefix", null), null);


            harmony.Patch(AccessTools.Method(typeof(Verse.AI.JobDriver_Wait), "CheckForAutoAttack"), new HarmonyMethod(typeof(HarmonyPatches), "CheckForAutoAttackPrefix", null), null);
            harmony.Patch(AccessTools.Method(typeof(Faction), "Notify_MemberDied"), null, new HarmonyMethod(typeof(HarmonyPatches), "Notify_MemberDiedPostfix", null), null);

            harmony.Patch(AccessTools.Method(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike"), new HarmonyMethod(typeof(HarmonyPatches), "AppendThoughts_ForHumanlikePrefix", null), null);

            harmony.Patch(AccessTools.Property(typeof(JobDriver_Open), "Openable").GetGetMethod(true), new HarmonyMethod(typeof(HarmonyPatches), "OpenablePostfix", null), null);

            harmony.Patch(AccessTools.Property(typeof(ColonistBar), "Entries").GetGetMethod(), null, new HarmonyMethod(typeof(HarmonyPatches), "EntriesPostfix", null), null);

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.race?.Humanlike == true))
            {
                def.comps.Add(new CompProperties_Soul());
                def.comps.Add(new CompProperties_Psyker());
            }

        }

        public static void WorldGeneratePostfix()
        {
            CorruptionStoryTracker storyTracker = CorruptionStoryTrackerUtilities.CurrentStoryTracker;
            Log.Message("Going");
            storyTracker.MissionManager.LoadStartingMissions();
            if (storyTracker.FactionsEnabled)
            {
                storyTracker.GenerateAndSetFactions();
                storyTracker.CreateSubSector();
            }
            if (CorruptionModSettings.AllowDomination)
            {
                WorldObject dominationMainEnemyBase = WorldObjectMaker.MakeWorldObject(DefOfs.C_WorldObjectDefOf.DominationBase);
                dominationMainEnemyBase.SetFaction(Find.FactionManager.FirstFactionOfDef(FactionDefOf.Outlander));
                int tile = 0;

                tile = TileFinder.RandomFactionBaseTileFor(dominationMainEnemyBase.Faction, false, null); //Rand.Range(0, Find.WorldGrid.TilesCount);

                dominationMainEnemyBase.Tile = tile;

                if (Faction.OfPlayer.def == DefOfs.C_FactionDefOf.IoM_PlayerFaction)
                {
                    dominationMainEnemyBase.SetFaction(storyTracker.ChaosCult_NPC);
                }
                else if (Faction.OfPlayer.def == DefOfs.C_FactionDefOf.ChaosCult_Player)
                {
                    dominationMainEnemyBase.SetFaction(CorruptionStoryTrackerUtilities.CurrentStoryTracker.IoM_NPC);
                }
                else
                {
                    dominationMainEnemyBase = null;
                    return;
                }
                Find.WorldObjects.Add(dominationMainEnemyBase);

                storyTracker.DominationTracker.InitializeTracker();


            }
        }

        private static Texture2D patronIcon
        {
            get
            {
                Pawn selPawn = Find.Selector.SingleSelectedThing as Pawn;

                if (selPawn != null)
                {
                    CompSoul soul = CompSoul.GetPawnSoul(selPawn);
                    if (soul != null)
                    {
                        return ChaosGodsUtilities.GetPatronIcon(soul.Patron);
                    }
                    return new Texture2D(20, 20);
                }
                return new Texture2D(20, 20);
            }
        }

        public static void DoInspectPaneButtons(Rect rect, ref float lineEndWidth, MainTabWindow_Inspect __instance)
        {
            if (Find.Selector.NumSelected == 1)
            {
                Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
                if (singleSelectedThing != null)
                {
                    Widgets.InfoCardButton(rect.width - 48f, 0f, Find.Selector.SingleSelectedThing);
                    lineEndWidth += 24f;
                    Pawn pawn = singleSelectedThing as Pawn;
                    if (pawn != null && pawn.playerSettings != null && pawn.playerSettings.UsesConfigurableHostilityResponse)
                    {
                        HostilityResponseModeUtility.DrawResponseButton(new Vector2(rect.width - 72f, 0f), pawn);
                        lineEndWidth += 24f;
                    }

                    if (pawn != null)
                    {
                        CompSoul soul = CompSoul.GetPawnSoul(pawn);
                        if (soul != null)
                        {
                            float num = rect.height - 48;
                            Widgets.ListSeparator(ref num, rect.width, "PawnAlignment".Translate());
                            ColorInt colorInt = new ColorInt(65, 25, 25);
                            Texture2D soultex = SolidColorMaterials.NewSolidColorTexture(colorInt.ToColor);
                            ColorInt colorInt2 = new ColorInt(10, 10, 10);
                            Texture2D bgtex = SolidColorMaterials.NewSolidColorTexture(colorInt2.ToColor);
                            WidgetRow row = new WidgetRow(0f, rect.height - 24f);
                            row.FillableBar(93f, 16f, soul.CurLevel, soul.CurCategory.ToString(), soultex, bgtex);
                            String desc = "PawnAlignmentButtonDescription".Translate();
                            if (row.ButtonIcon(HarmonyPatches.patronIcon, desc))
                            {
                                Find.WindowStack.Add(new MainTabWindow_Alignment());
                            }
                            string culturalTolerance = "Cultural Tolerance: " + soul.CulturalTolerance.ToString();
                            Widgets.Label(new Rect(rect.width / 2, rect.height - 24, rect.width / 2, 16f), culturalTolerance);
                        }
                    }
                }
            }
        }
        public static bool IsAutomaton(Pawn pawn)
        {
            return pawn.AllComps.Any(i => i.GetType() == typeof(CompServitor) || i.GetType() == typeof(CompThoughtlessAutomaton));
        }

        private static bool HasSoulTraitNullyfyingTraits(ThoughtDef def, Pawn pawn, out CompSoul soul)
        {
            soul = pawn.TryGetComp<CompSoul>();
            List<SoulTrait> straitlist = soul.AllSoulTraits;
            foreach (SoulTrait strait in straitlist)
            {
                foreach (ThoughtDef tdef in strait.SDef.NullifiesThoughts)
                {
                    if (tdef.defName == def.defName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void CanGetThoughtPostfix(ref bool __result, ThoughtDef def, Pawn pawn)
        {
            if (__result)
            {
                try
                {
                    CompSoul soul;
                    if (HarmonyPatches.HasSoulTraitNullyfyingTraits(def, pawn, out soul))
                    {
                        __result = false;
                        return;
                    }
                    ThoughtDefCorruption Tdef = def as ThoughtDefCorruption;
                    if (Tdef != null)
                    {
                        if (IsAutomaton(pawn))
                        {
                            if (Tdef.IsAutomatonThought)
                            {
                                __result = true;
                                return;
                            }
                            else
                            {
                                __result = false;
                                return;
                            }
                        }
                        if (!Tdef.requiredSoulTraits.NullOrEmpty())
                        {
                            List<SoulTraitDef> soulTraitDefs = soul.AllSoulTraits.Select(x => x.SDef).ToList();
                            __result = soulTraitDefs.Intersect(Tdef.requiredSoulTraits).Any();                                                        
                        }
                    }
                    else
                    {
                        if (IsAutomaton(pawn))
                        {
                            __result = false;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Message(ex.Message);
                }
                finally
                {
                    ProfilerThreadCheck.EndSample();
                }
            }
        }

        public static void Notify_PawnKilledPostFix(Pawn killed, Pawn killer)
        {
            CompSoul soul = CompSoul.GetPawnSoul(killer);
            if (soul != null)
            {
                if (soul.Patron == PatronDefOf.Khorne|| soul.Patron == PatronDefOf.Slaanesh)
                {
                    soul.PawnKillTracker.lastKillTick = soul.PawnKillTracker.lastKillTick < 1 ? 1000 : soul.PawnKillTracker.lastKillTick + 1000;
                    if (killed.def.race.Humanlike)
                    {
                        soul.PawnKillTracker.curKillCount += 3;
                    }
                    else
                    {
                        soul.PawnKillTracker.curKillCount += 1;
                    }
                }
            }
        }

        public static void GenerateInitialHediffsPostFix(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn.needs != null)
            {
                CompSoul soul = CompSoul.GetPawnSoul(pawn);
                if (soul != null)
                {
                    ChaosFollowerPawnKindDef pdef = pawn.kindDef as ChaosFollowerPawnKindDef;
                    if (pdef != null)
                    {
                        soul.GenerateHediffsAndImplants(pdef);
                    }
                }
            }
        }

        public static bool CheckForAutoAttackPrefix(ref JobDriver_Wait __instance)
        {
            if (__instance.pawn.Downed)
            {
                return false;
            }
            if (__instance.pawn.stances.FullBodyBusy)
            {
                return false;
            }

            CompSoul soul = CompSoul.GetPawnSoul(__instance.pawn);
            if (soul != null)
            {
                if (CorruptionStoryTrackerUtilities.IsPsyker(__instance.pawn))
                {
                    Thing target = null;
                    AIPsykerPowerCategory cat;
                    PsykerPowerDef def = null;
                    if (CorruptionStoryTrackerUtilities.GetRandOffensivePsykerPower(__instance.pawn, out def, out cat))
                    {
                        CorruptionStoryTrackerUtilities.AIGetPsykerTarget(__instance.pawn, cat, def.MainVerb.range, out target);
                        if (target != null)
                        {
                            Job castJob = CorruptionStoryTrackerUtilities.AI_CastPsykerPowerJob(__instance.pawn, def, target);
                            if (castJob != null)
                            {
                                __instance.pawn.jobs.TryTakeOrderedJob(castJob);
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }
            return true;
        }

        public static void Notify_MemberDiedPostfix(Faction __instance, Pawn member, DamageInfo dinfo, bool wasWorldPawn)
        {
            if (CorruptionModSettings.AllowDomination)
            {
                CorruptionStoryTrackerUtilities.CurrentStoryTracker.DominationTracker.CheckPawnDiedInWar(member, dinfo);
            }
        }


        public static bool AppendThoughts_ForHumanlikePrefix(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtDef> outAllColonistsThoughts)
        {
            ChaosFollowerPawnKindDef cdef = victim.kindDef as ChaosFollowerPawnKindDef;
            if (cdef != null)
            {
                if (cdef.IsServitor)
                {
                    if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
                    {
                        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods.Where(x => x.Faction == victim.Faction))
                        {
                            outIndividualThoughts.Add(new IndividualThoughtToAdd(DefOfs.C_ThoughtDefOf.ServitorDied, pawn, null, 1f, 1f));
                        }
                    }
                    return false;
                }
            }
            return true;
        }

        public static bool OpenablePostfix(ref JobDriver __instance, ref IOpenable __result)
        {
            ThingWithComps thing = __instance.pawn.CurJob.targetA.Thing as ThingWithComps;
            IOpenable testOpenable = thing as IOpenable;
            if (testOpenable == null)
            {

                if (thing != null)
                {
                    for (int i = 0; i < thing.AllComps.Count; i++)
                    {
                        ThingComp comp = thing.AllComps[i];
                        if (comp is IOpenable)
                        {
                            __result = thing.AllComps[i] as IOpenable;
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        
        public static void FreeColonistsPostfix(ref IEnumerable<Pawn> __result)
        {
            List<Pawn> listWOservitors = __result.ToList<Pawn>();
            Predicate<Pawn> validator = delegate (Pawn t)
            {
                ChaosFollowerPawnKindDef pdef = t.kindDef as ChaosFollowerPawnKindDef;
                if (pdef != null)
                {
                    if (pdef.IsServitor)
                    {
                        return true;
                    }
                }
                return false;
            };
            __result = __result.Where(x => validator(x));
        }


        public static void EntriesPostfix(ref IEnumerable<ColonistBar.Entry> __result)
        {
            Predicate<ColonistBar.Entry> validator = delegate (ColonistBar.Entry t)
            {
                Pawn p = t.pawn;
                if (p != null)
                {
                    ChaosFollowerPawnKindDef pdef = p.kindDef as ChaosFollowerPawnKindDef;
                    if (pdef != null)
                    {
                        if (pdef.IsServitor)
                        {
                            return true;
                        }
                    }
                }
                return false;
            };
            List<ColonistBar.Entry> newList = new List<ColonistBar.Entry>();
            newList.AddRange(__result);
            newList.RemoveAll(x => validator(x));
            __result = newList;
        }


        public static void InteractedDeepTalkPostfix(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            if (initiator.Faction == Faction.OfPlayer)
            {
                CompSoul.TryDiscoverAlignment(initiator, recipient, CorruptionStoryTrackerUtilities.DiscoverAlignmentByChatModifier);
            }
        }

        public static void CreateInitialComponentsPostfix(Pawn pawn)
        {            
            CompSoul soul = CompSoul.GetPawnSoul(pawn);
            if (soul != null)
            {
                soul.SetInitialLevel();
            }
            CompPsyker compPsyker = CompPsyker.GetCompPsyker(pawn);
            if (compPsyker != null)
            {
                compPsyker.SetInitialLevel();
            }
        }

        public static bool CompInspectStringExtraPrefix(ref CompIngredients __instance , ref string __result)
        {
            IoM.CompResourcePack resComp = __instance.parent?.TryGetComp<CompResourcePack>();
            if (resComp != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (__instance.ingredients.Count > 0)
                {
                    stringBuilder.Append("ResPackContent".Translate() + ": ");
                    for (int i = 0; i < __instance.ingredients.Count; i++)
                    {
                        stringBuilder.Append(__instance.ingredients[i].label);
                        if (i < __instance.ingredients.Count - 1)
                        {
                            stringBuilder.Append(", ");
                        }
                    }
                    stringBuilder.Append(Environment.NewLine + resComp.Resources[0].AvgQualityCategory);
                }
                __result = stringBuilder.ToString();
                return false;
            }
            return true;
        }


    }
    
}