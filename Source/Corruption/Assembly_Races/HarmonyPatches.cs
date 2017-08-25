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
            harmony.Patch(AccessTools.Method(typeof(RimWorld.FactionGenerator), "GenerateFactionsIntoWorld"), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateFactionsIntoWorldPostFix"), null);
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGenerator), "GenerateInitialHediffs"), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateInitialHediffsPostFix", null));
            harmony.Patch(AccessTools.Method(typeof(RimWorld.RecordsUtility), "Notify_PawnKilled"), null, new HarmonyMethod(typeof(HarmonyPatches), "Notify_PawnKilledPostFix", null));

            harmony.Patch(AccessTools.Method(typeof(Verse.AI.JobDriver_Wait), "CheckForAutoAttack"), new HarmonyMethod(typeof(HarmonyPatches), "CheckForAutoAttackPrefix", null), null);
            harmony.Patch(AccessTools.Method(typeof(Faction), "Notify_MemberDied"), null, new HarmonyMethod(typeof(HarmonyPatches), "Notify_MemberDiedPostfix", null), null);

            harmony.Patch(AccessTools.Method(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike"), new HarmonyMethod(typeof(HarmonyPatches), "AppendThoughts_ForHumanlikePrefix", null), null);

            harmony.Patch(AccessTools.Property(typeof(JobDriver_Open), "Openable").GetGetMethod(true), new HarmonyMethod(typeof(HarmonyPatches), "OpenablePostfix", null), null);

          // harmony.Patch(AccessTools.Method(typeof(Pawn_NeedsTracker), "ShouldHaveNeed"), new HarmonyMethod(typeof(HarmonyPatches), "ShouldHaveNeedPrefix", null), null);
            
        }

        public static void GenerateFactionsIntoWorldPostFix()
        {
            Log.Message("Generating Corruption Story Tracker");
            CorruptionStoryTracker corrTracker = (CorruptionStoryTracker)WorldObjectMaker.MakeWorldObject(DefOfs.C_WorldObjectDefOf.CorruptionStoryTracker);
            Log.Message("GeneratedSuccesfully");
            int tile = 0;
            while (!(Find.WorldObjects.AnyWorldObjectAt(tile) || Find.WorldGrid[tile].biome == BiomeDefOf.Ocean))
            {
                tile = Rand.Range(0, Find.WorldGrid.TilesCount);
            }
            corrTracker.Tile = tile;
            Find.WorldObjects.Add(corrTracker);
        }

        private static Texture2D patronIcon
        {
            get
            {
                Pawn selPawn = Find.Selector.SingleSelectedThing as Pawn;

                if (selPawn != null)
                {
                    Need_Soul soul = selPawn.needs.TryGetNeed<Need_Soul>();
                    if (soul != null)
                    {
                        return ChaosGodsUtilities.GetPatronIcon(soul.patronInfo.PatronName);
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
                        Need_Soul soul;
                        if ((soul = pawn.needs.TryGetNeed<Need_Soul>()) != null)
                        {
                            float num = rect.height - 48;
                            Widgets.ListSeparator(ref num, rect.width, "PawnAlignment".Translate());
                            ColorInt colorInt = new ColorInt(65, 25, 25);
                            Texture2D soultex = SolidColorMaterials.NewSolidColorTexture(colorInt.ToColor);
                            ColorInt colorInt2 = new ColorInt(10, 10, 10);
                            Texture2D bgtex = SolidColorMaterials.NewSolidColorTexture(colorInt2.ToColor);
                            WidgetRow row = new WidgetRow(0f, rect.height - 24f);
                            row.FillableBar(93f, 16f, soul.CurLevelPercentage, soul.CurCategory.ToString(), soultex, bgtex);
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
            return pawn.AllComps.Any(i => i.GetType() == typeof(CompThoughtlessAutomaton));
        }

        private static bool HasSoulTraitNullyfyingTraits(ThoughtDef def, Pawn p, out Need_Soul soul)
        {
            if (p.needs.TryGetNeed<Need_Soul>() == null)

            {
                HarmonyPatches.CreateNewSoul(p);
            }
            soul = p.needs.TryGetNeed<Need_Soul>();
            List<SoulTrait> straitlist = soul.SoulTraits;
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
                    Need_Soul soul;
                    if (HarmonyPatches.HasSoulTraitNullyfyingTraits(def, pawn, out soul))
                    {
                        //Log.Message("HasSoulTrait");
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
                            //Log.Message("Checking: " + Tdef.defName.ToString());
                            //foreach (SoulTraitDef cur in Tdef.requiredSoulTraits)
                            //{
                            //    Log.Message("Requires " + cur.defName);
                            //}
                            //foreach (SoulTrait cur in soul.SoulTraits)
                            //{
                            //    Log.Message("pawn has " + cur.SDef.defName);
                            //}

                            //List<SoulTrait> straitlist = soul.SoulTraits;
                            //foreach (SoulTrait strait in straitlist)
                            //{
                            //    foreach (SoulTraitDef ttrait in Tdef.requiredSoulTraits)
                            //    {
                            //        if (ttrait.defName == strait.SDef.defName)
                            //        {
                            //            __result = true;
                            //        }
                            //        else
                            //        {
                            //            __result = false;
                            //            break;
                            //        }
                            //    }
                            //}
                            List<SoulTraitDef> soulTraitDefs = soul.SoulTraits.Select(x => x.SDef).ToList();
                            __result = soulTraitDefs.Intersect(Tdef.requiredSoulTraits).Any();


                            //for (int i = 0; i < soul.SoulTraits.Count; i++)
                            //{
                            //    if (Tdef.requiredSoulTraits.Any(x => soul.SoulTraits[i].SDef.defName == x.defName))
                            //    {
                            //        __result = false;
                            //    }
                            //    //else
                            //    //{
                            //    //    Log.Message("Accepted Thought");
                            //    //}
                            //}
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

        public static void CreateNewSoul(Pawn pepe)
        {
            {
                Need need = (Need)Activator.CreateInstance(typeof(Need_Soul), new object[]
            {
                pepe
            });
                need.def = C_NeedDefOf.Need_Soul;
                int x = pepe.needs.AllNeeds.Count;
                pepe.needs.AllNeeds.Insert(x - 1, need);
                //          pepe.needs.AllNeeds.Add(need);
            }
        }
        public static void Notify_PawnKilledPostFix(Pawn killed, Pawn killer)
        {
            Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(killer);
            if (soul != null)
            {
                if (soul.patronInfo.PatronName == "Khorne" || soul.patronInfo.PatronName == "Slaanesh")
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
                Need_Soul soul = pawn.needs.TryGetNeed<Need_Soul>();
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

            Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(__instance.pawn);
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
                CorruptionStoryTrackerUtilities.currentStoryTracker.DominationTracker.CheckPawnDiedInWar(member, dinfo);
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

        public static bool ShouldHaveNeedPrefix(ref Pawn_NeedsTracker __instance, NeedDef nd, bool __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            
            if (pawn != null)
            {
                ChaosFollowerPawnKindDef pdef = pawn.kindDef as ChaosFollowerPawnKindDef;
                if (pdef != null)
                {
                    if (pdef.IsServitor)
                    {
                        Log.Message("Servitor");
                        __result = false;
                        return false;
                    }
                }
            }

            return true;
        }

    }



    
}