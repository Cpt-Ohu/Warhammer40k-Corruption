﻿using Corruption.DefOfs;
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
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.ohu.corruption.main");

            harmony.Patch(AccessTools.Method(typeof(RimWorld.ThoughtHandler), "CanGetThought", new Type[] { typeof(ThoughtDef) }), null, new HarmonyMethod(typeof(HarmonyPatches), "CanGetThoughtPostfix"), null);
            harmony.Patch(AccessTools.Method(typeof(RimWorld.MainTabWindow_Inspect), "DoInspectPaneButtons", null), null, new HarmonyMethod(typeof(HarmonyPatches), "DoInspectPaneButtons", null));
            harmony.Patch(AccessTools.Method(typeof(RimWorld.FactionGenerator), "GenerateFactionsIntoWorld", new Type[] { typeof(string) }), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateFactionsIntoWorldPostFix"), null);
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGenerator), "GenerateInitialHediffs", null), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateInitialHediffsPostFix", null));

        }

        public static void GenerateFactionsIntoWorldPostFix()
        {
            Log.Message("Generating Corruption Story Tracker");
            CorruptionStoryTracker corrTracker = (CorruptionStoryTracker)WorldObjectMaker.MakeWorldObject(DefOfs.C_WorldObjectDefOf.CorruptionStoryTracker);
            corrTracker.Tile = TileFinder.RandomStartingTile();
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
        
        public static void CanGetThoughtPostfix(ThoughtDef def, SituationalThoughtHandler __instance, ref bool __result)
        {
            if (__result)
            {
                try
                {
                    Need_Soul soul;
                    if (HarmonyPatches.HasSoulTraitNullyfyingTraits(def, __instance.pawn, out soul))
                    {
                        __result = false;
                        return;
                    }
                    ThoughtDefCorruption Tdef = def as ThoughtDefCorruption;
                    if (Tdef != null)
                    {
                        if (IsAutomaton(__instance.pawn))
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
                            for (int i = 0; i < soul.SoulTraits.Count; i++)
                            {
                                if (!Tdef.requiredSoulTraits.Contains(soul.SoulTraits[i].SDef))
                                {
                                    __result = false;
                                }
                            }
                        }
                    }
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
    }
}