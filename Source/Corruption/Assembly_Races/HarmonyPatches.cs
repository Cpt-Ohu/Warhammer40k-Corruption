using Corruption.DefOfs;
using Corruption.Domination;
using Corruption.IoM;
using Corruption.ProductionSites;
using Corruption.Worship;
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

            harmony.Patch(AccessTools.Method(typeof(RimWorld.Planet.Settlement), "PostRemove"), null, new HarmonyMethod(typeof(HarmonyPatches), "PostRemoveSettlementPostfix", null), null);

            harmony.Patch(AccessTools.Method(typeof(Verse.AI.JobDriver_Wait), "CheckForAutoAttack"), new HarmonyMethod(typeof(HarmonyPatches), "CheckForAutoAttackPrefix", null), null);
            //harmony.Patch(AccessTools.Method(typeof(Faction), "Notify_MemberDied"), null, new HarmonyMethod(typeof(HarmonyPatches), "Notify_MemberDiedPostfix", null), null);

            harmony.Patch(AccessTools.Method(typeof(Faction), "Notify_MemberDied"), new HarmonyMethod(typeof(HarmonyPatches), "Notify_MemberDiedPrefix", null), null);

            //harmony.Patch(AccessTools.Method(typeof(Faction), "GenerateNewLeader"), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateNewLeaderPrefix", null), null);

            //harmony.Patch(AccessTools.Method(typeof(Faction), "Notify_LeaderDied"), new HarmonyMethod(typeof(HarmonyPatches), "LeaderDiedPrefix", null), null);

            harmony.Patch(AccessTools.Method(typeof(Verse.AI.Pawn_MindState), "JoinColonyBecauseRescuedBy"), null, new HarmonyMethod(typeof(HarmonyPatches), "PrisonerRescuedPostfix", null), null);

            harmony.Patch(AccessTools.Method(typeof(PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike"), new HarmonyMethod(typeof(HarmonyPatches), "AppendThoughts_ForHumanlikePrefix", null), null);

            harmony.Patch(AccessTools.Property(typeof(VerbProperties), "CausesExplosion").GetGetMethod(true), null, new HarmonyMethod(typeof(HarmonyPatches), "CausesExplosionPrefix"), null);

            harmony.Patch(AccessTools.Property(typeof(JobDriver_Open), "Openable").GetGetMethod(true), new HarmonyMethod(typeof(HarmonyPatches), "OpenablePostfix", null), null);

            harmony.Patch(AccessTools.Property(typeof(ColonistBar), "Entries").GetGetMethod(), null, new HarmonyMethod(typeof(HarmonyPatches), "EntriesPostfix", null), null);

            harmony.Patch(AccessTools.Method(typeof(Caravan), "GetGizmos"), null, new HarmonyMethod(typeof(HarmonyPatches), "GetGizmosPostfix", null), null);

            harmony.Patch(AccessTools.Method(typeof(FactionDialogMaker), "FactionDialogFor"), null, new HarmonyMethod(typeof(HarmonyPatches), "FactionDialogForPostfix", null), null);

            harmony.Patch(AccessTools.Method(typeof(FactionGenerator), "GenerateFactionsIntoWorld"), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateFactionsIntoWorldPostfix", null), null);



            harmony.Patch(AccessTools.Method(typeof(DefeatAllEnemiesQuestComp), "GiveRewardsAndSendLetter"), new HarmonyMethod(typeof(HarmonyPatches), "GiveRewardsAndSendLetterPrefix", null), null);

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x => x.race?.Humanlike == true))
            {
                def.comps.Add(new CompProperties_Soul());
                def.comps.Add(new CompProperties_Psyker());
            }

        }

        public static void WorldGeneratePostfix()
        {
            CorruptionStoryTracker storyTracker = CFind.StoryTracker;
            if (storyTracker.FactionsEnabled)
            {
                storyTracker.GenerateAndSetFactions();
                storyTracker.CreateSubSector();
            }
            if (CorruptionModSettings.AllowDomination)
            {

                WorldObject dominationMainEnemyBase = WorldObjectMaker.MakeWorldObject(DefOfs.C_WorldObjectDefOf.DominationBase);
                dominationMainEnemyBase.SetFaction(Find.FactionManager.FirstFactionOfDef(FactionDefOf.Ancients));
                int tile = 0;

                tile = TileFinder.RandomSettlementTileFor(dominationMainEnemyBase.Faction, false, null); //Rand.Range(0, Find.WorldGrid.TilesCount);

                dominationMainEnemyBase.Tile = tile;

                if (Faction.OfPlayer.def == DefOfs.C_FactionDefOf.IoM_PlayerFaction)
                {
                    dominationMainEnemyBase.SetFaction(storyTracker.ChaosCult_NPC);
                }
                else if (Faction.OfPlayer.def == DefOfs.C_FactionDefOf.ChaosCult_Player)
                {
                    dominationMainEnemyBase.SetFaction(CFind.StoryTracker.IoM_NPC);
                }
                else
                {
                    dominationMainEnemyBase = null;
                    return;
                }
                Find.WorldObjects.Add(dominationMainEnemyBase);

                storyTracker.DominationTracker.InitializeTracker();

            }
            storyTracker.MissionManager.LoadStartingMissions();
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
                        HostilityResponseModeUtility.DrawResponseButton(new Rect(rect.width - 72f, 0f, 24f, 24f), pawn, false);
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

        public static void CanGetThoughtPostfix(ref bool __result, Pawn pawn, ThoughtDef def)
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
                if (soul.Patron == PatronDefOf.Khorne || soul.Patron == PatronDefOf.Slaanesh)
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

        //public static bool LeaderDiedPrefix(ref Faction __instance)
        //{
        //    Pawn pawn = __instance.leader;
        //    Log.Message(pawn.Name.ToStringFull);
        //    Log.Message(__instance.Name);
        //    Log.Message(__instance.def.leaderTitle);
        //    return true;
        //}

        public static bool GenerateNewLeaderPrefix(ref Faction __instance)
        {
            Log.Message("Generating");
            return true;
        }

        public static bool Notify_MemberDiedPrefix(ref Faction __instance, Pawn member, DamageInfo? dinfo)
        {
            //Log.Message(member.Name.ToStringFull + " is dying");
            //Log.Message(__instance.Name);
            //Log.Message(__instance.leader.Name.ToStringFull);

            //Log.Message("Checking Kill");

            //Find.LetterStack.ReceiveLetter("LetterLeadersDeathLabel".Translate(__instance.Name, __instance.def.leaderTitle).CapitalizeFirst(), "LetterLeadersDeath".Translate(member.Name.ToStringFull, __instance.Name, __instance.leader.Name.ToStringFull, __instance.def.leaderTitle, member.Named("OLDLEADER"), __instance.leader.Named("NEWLEADER")).CapitalizeFirst(), LetterDefOf.NeutralEvent, GlobalTargetInfo.Invalid, __instance, null);
            //Log.Message("DOne");
            if (CorruptionModSettings.AllowDomination && dinfo != null && dinfo.HasValue)
            {
                CFind.StoryTracker.DominationTracker.CheckPawnDiedInWar(member, dinfo.Value);
            }
            return true;
        }

        public static void Notify_MemberDiedPostfix(ref Faction __instance, Pawn member, Nullable<DamageInfo> dinfo = null)
        {
            Log.Message(__instance.Name + " " + member.Name);
            if (CorruptionModSettings.AllowDomination && dinfo != null && dinfo.HasValue)
            {
                CFind.StoryTracker.DominationTracker.CheckPawnDiedInWar(member, dinfo.Value);
            }
            Missions.MissionManager.CheckPawnKilledMissions(member);
        }

        public static void PrisonerRescuedPostfix(ref Pawn_MindState __instance, Pawn by)
        {
            CFind.MissionManager.TryFinishMissionForTarget(__instance.pawn);
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
                        foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive.Where(x => x.Faction == victim.Faction))
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

        public static void CausesExplosionPrefix(ref VerbProperties __instance, ref bool __result)
        {
            if (typeof(Projectile_WarpPower).IsAssignableFrom(__instance.defaultProjectile.thingClass) || typeof(Projectile_Laser).IsAssignableFrom(__instance.defaultProjectile.thingClass))
            {
                __result = true;
                return;
            }
            return;
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


        public static void GetGizmosPostfix(ref Caravan __instance, ref IEnumerable<Gizmo> __result)
        {
            if (__instance.Faction != Faction.OfPlayer)
            {
                Caravan caravan = __instance;
                List<Gizmo> newResult = new List<Gizmo>();
                PilgrimageComp pilgrimageComp = __instance.GetComponent<PilgrimageComp>();
                if (pilgrimageComp != null)
                {
                    newResult.Add(pilgrimageComp.ReturnHomeCommand());
                }

                Command_Action commandTeleport = new Command_Action();
                commandTeleport.defaultLabel = "Dev: Teleport to destination";
                commandTeleport.action = delegate
                {
                    caravan.Tile = caravan.pather.Destination;
                    caravan.pather.StopDead();
                };
                newResult.Add(commandTeleport);

                __result = newResult;
            }
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

        public static bool CompInspectStringExtraPrefix(ref CompIngredients __instance, ref string __result)
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

        private static bool GiveRewardsAndSendLetterPrefix(ref DefeatAllEnemiesQuestComp __instance)
        {
            WorldObject mapParent = __instance.parent;
            if (mapParent != null)
            {
                if (CFind.MissionManager.TryFinishMissionForTarget(mapParent))
                {
                    return false;
                }
            }
            return true;
        }

        private static void PostRemoveSettlementPostfix(ref Settlement __instance)
        {
            CFind.MissionManager.TryFinishMissionForTarget(__instance);
        }

        private static void GenerateFactionsIntoWorldPostfix()
        {
            int countSites = (int)(Find.WorldObjects.Settlements.Count * 0.3f);
            for (int i = 0; i < countSites; i++)
            {
                Settlement settlement = Find.WorldObjects.Settlements.RandomElement();
                int settlementTile = settlement.Tile;
                string name = settlement.Name;
                Faction faction = settlement.Faction;
                Find.WorldObjects.Remove(settlement);
                WorldObjectDef def = DefDatabase<WorldObjectDef>.AllDefsListForReading.FindAll(x => x.worldObjectClass == typeof(ProductionSites.ProductionSite) && x.comps.Any(s => (s as WorldObjectCompProperties_ResourceProduction)?.RequiredTechLevel <= faction.def.techLevel)).RandomElement();
                if (def != null && faction != null)
                {
                    ProductionSites.ProductionSite site = ProductionSites.ProductionSiteMaker.MakeProductionSiteAt(settlementTile, faction, def, 0);
                    site.Name = name;
                    PawnKindDef[] pawnKinds = new PawnKindDef[3];
                    for (int j = 0; j < 3; j++)
                    {
                        pawnKinds[j] = faction.RandomPawnKind();
                    }
                    while (site.MainProduction.WorkForce.Sum(x => x.WorkerCount) < site.MainProduction.SiteLevel.MaximumSupportedWorkers)
                    {
                        PawnKindDef pawnKindDef = pawnKinds.RandomElement();
                        float skill = Rand.RangeInclusive(1, 10);
                        site.MainProduction.AddToWorkforce(pawnKindDef, skill);
                    }
                    for (int k = 0; k < 2; k++)
                    {
                        site.FinishProductionCycles();
                    }                            
                    
                }
            }
        }

        private static void FactionDialogForPostfix(ref DiaNode __result, Pawn negotiator, Faction faction)
        {
            __result.options.Insert(__result.options.Count - 1, FactionDialogMakerDiplomacy.GetDiplomacyOptionsFor(faction, negotiator));
        }

    }

}