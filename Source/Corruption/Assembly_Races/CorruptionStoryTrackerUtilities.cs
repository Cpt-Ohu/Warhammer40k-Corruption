using Corruption.DefOfs;
using Corruption.Tithes;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using OHUShips;
using Corruption.Domination;
using Verse.AI;
using Corruption.IoM;
using System.Reflection;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public class CorruptionStoryTrackerUtilities
    {
        public static Texture2D ButtonIG = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonIG", true);
        public static Texture2D ButtonAM = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonAM", true);
        public static Texture2D ButtonAS = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonAS", true);
        public static Texture2D PlanetMedium = ContentFinder<Texture2D>.Get("UI/SectorMap/Planet_Medium", true);
        public static Texture2D PlanetSmall = ContentFinder<Texture2D>.Get("UI/SectorMap/Planet_Small", true);
        public static Texture2D Moon = ContentFinder<Texture2D>.Get("UI/SectorMap/Moon", true);
        public static Texture2D ShipsArrival = ContentFinder<Texture2D>.Get("UI/Images/ShipArrival", true);
        public static readonly Texture2D DropTexture = ContentFinder<Texture2D>.Get("UI/Buttons/UnloadShip", true);

        public static Texture2D CorruptionLogoTexture = ContentFinder<Texture2D>.Get("UI/Images/Preview", true);

        public static readonly Texture2D JoinBattle = ContentFinder<Texture2D>.Get("UI/Commands/CommandJoinBattle", true);

        public static Texture2D Aquila = ContentFinder<Texture2D>.Get("UI/Images/IoM_Aquila", true);

        public static string BuffNegGraphicPath = "UI/Psyker/BuffNegative";
        public static string BuffPosGraphicPath = "UI/Psyker/BuffPositive";

        public static bool ImperialInstitutionsSelected = true;

        public static CorruptionStoryTracker currentStoryTracker
        {
            get
            {
                return Find.WorldObjects.AllWorldObjects.FirstOrDefault(x => x.def == C_WorldObjectDefOf.CorruptionStoryTracker) as CorruptionStoryTracker;
            }
        }

        public static string ReturnImperialFactionDescription(Faction faction)
        {

            return "IG_CCM_ContactFaction".Translate(new object[]
            {
                    faction.Name
            });

        }

        public static void DrawCorruptionStoryTrackerTab(CorruptionStoryTracker tracker, Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(rect.x, rect.y + 20f, rect.width, 55f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, Faction.OfPlayer.Name);
            Text.Font = GameFont.Small;

            Rect rect3 = rect2;
            rect3.y = rect2.yMax + 30f;
            rect3.height = rect.height - rect2.height - 30f;

            Widgets.DrawMenuSection(rect3, true);
            List<TabRecord> list = new List<TabRecord>();

            list.Add(new TabRecord("ImperialInstitutions".Translate(), delegate
            {
                CorruptionStoryTrackerUtilities.ImperialInstitutionsSelected = true;
            }, CorruptionStoryTrackerUtilities.ImperialInstitutionsSelected));

            if (tracker.AcknowledgedByImperium)
            {
                list.Add(new TabRecord("TabTitheContainer".Translate(), delegate
                {
                    CorruptionStoryTrackerUtilities.ImperialInstitutionsSelected = false;
                }, !CorruptionStoryTrackerUtilities.ImperialInstitutionsSelected));
            }
            TabDrawer.DrawTabs(rect3, list);
            rect3 = rect3.ContractedBy(9f);
            GUI.BeginGroup(rect3);
            
            GUI.color = Color.white;

            if (CorruptionStoryTrackerUtilities.ImperialInstitutionsSelected)
            {
                DrawImperialFactionRows(tracker, rect3);
            }
            else
            {
                DrawImperialTitheTab(tracker, rect3);
            }

            GUI.EndGroup();
            GUI.EndGroup();
        }

        private static void DrawImperialFactionRows(CorruptionStoryTracker tracker, Rect rect)
        {
            float num = 0f;
            for (int i = 0; i < tracker.ImperialFactions.Count; i++)
            {
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Widgets.DrawLineHorizontal(0f, num, rect.width);
                GUI.color = Color.white;
                num += CorruptionStoryTrackerUtilities.DrawImperialFactionRow(tracker.ImperialFactions[i], num, rect);
            }
        }

        public static Need_Soul GetPawnSoul(Pawn pawn)
        {
            if (pawn.needs != null)
            {
                return pawn.needs.TryGetNeed<Need_Soul>();
            }
            else
            {
                Log.Error("Tried to get Pawns soul with missing needs");
                return null;
            }
        }
        public static void DrawImperialTitheTab(CorruptionStoryTracker tracker, Rect rect)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            float num = 0f;
            List<TitheEntryGlobal> list = tracker.currentTithes;
            int numEntriesFirst = Math.Min(list.Count, 5);
            for (int i = 0; i < numEntriesFirst; i++)
            {
                Rect rect3 = new Rect(30f, num, 200f, 25f);
                Widgets.Label(rect3, list[i].titheDef.LabelCap);
                Rect rect4 = new Rect(40f, num + 30f, 200f, 30f);
                Widgets.FillableBar(rect4, list[i].tithePercent, TitheUtilities.TitheBarFillTex, TitheUtilities.TitheBarBGTex, true);
                Widgets.Label(rect4, list[i].collectedTitheAmount + " / " + list[i].requestedTitheAmount);
                num += 75f;
            }
            if (list.Count > 5)
            {
                num = 0f;
                for (int i = 5; i < 11; i++)
                {
                    Rect rect3 = new Rect(240f, num, 200f, 25f);
                    Widgets.Label(rect3, list[i].titheDef.LabelCap);
                    Rect rect4 = new Rect(240f, num + 30f, 200f, 30f);
                    Widgets.FillableBar(rect4, list[i].tithePercent, TitheUtilities.TitheBarFillTex, TitheUtilities.TitheBarBGTex, true);
                    Widgets.Label(rect4, list[i].collectedTitheAmount + " / " + list[i].requestedTitheAmount);
                    num += 75f;
                }
            }

            Rect rect5 = new Rect(450f, 0f, 200f, 100f);
            StringBuilder stringBuilder = new StringBuilder();
            string text = string.Concat(new string[]
            {
                "TitheDueIn".Translate(),
                "\n",
                tracker.DaysToTitheCollection.ToString()
            });
            if (tracker.DaysToTitheCollection < 8)
            {
                GUI.color = Color.red;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect5, text);

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        public static void ListSeparatorBig(ref float curY, float width, string label)
        {
            Color color = GUI.color;
            curY += 3f;
            GUI.color = Widgets.SeparatorLabelColor;
            Rect rect = new Rect(0f, curY, width, 50f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, label);
            curY += 20f;
            GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f); ;
            Widgets.DrawLineHorizontal(0f, curY+5f, width);
            curY += 5f;
            GUI.color = color;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private static float DrawImperialFactionRow(Faction faction, float rowY, Rect fillRect)
        {
            Rect rect = new Rect(35f, rowY, 200f, 80f);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Faction current in Find.FactionManager.AllFactionsVisible)
            {
                if (current != faction && !current.IsPlayer && !current.def.hidden)
                {
                    if (faction.HostileTo(current))
                    {
                        stringBuilder.AppendLine("HostileTo".Translate(new object[]
                        {
                            current.Name
                        }));
                    }
                }
            }
            string text = stringBuilder.ToString();
            float width = fillRect.width - rect.xMax;
            float num = Text.CalcHeight(text, width);
            float num2 = Mathf.Max(80f, num);
            Rect position = new Rect(10f, rowY + 10f, 15f, 15f);
            Rect rect2 = new Rect(0f, rowY, fillRect.width, num2);
            if (Mouse.IsOver(rect2))
            {
                GUI.DrawTexture(rect2, TexUI.HighlightTex);
            }
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
   //         Widgets.DrawRectFast(position, faction.Color, null);
            string label = string.Concat(new string[]
            {
                faction.Name,
                "\n",
                faction.def.LabelCap,
                "\n",
                (faction.leader == null) ? string.Empty : (faction.def.leaderTitle + ": " + faction.leader.Name.ToStringFull)
            });
            Widgets.Label(rect, label);
            Rect rect3 = new Rect(rect.xMax, rowY, 60f, 80f);
            Widgets.InfoCardButton(rect3.x, rect3.y, faction.def);
            Rect rect4 = new Rect(rect3.xMax, rowY, 200f, 80f);
            string text2 = Mathf.RoundToInt(faction.GoodwillWith(Faction.OfPlayer)).ToStringCached();
            if (Faction.OfPlayer.HostileTo(faction))
            {
                text2 = text2 + "\n" + "Hostile".Translate();
            }
            if (faction.defeated)
            {
                text2 = text2 + "\n(" + "DefeatedLower".Translate() + ")";
            }
            if (faction.PlayerGoodwill < 0f)
            {
                GUI.color = Color.red;
            }
            else if (faction.PlayerGoodwill == 0f)
            {
                GUI.color = Color.yellow;
            }
            else
            {
                GUI.color = Color.green;
            }
            Widgets.Label(rect4, text2);
            GUI.color = Color.white;
            TooltipHandler.TipRegion(rect4, "CurrentGoodwill".Translate());
            Rect rect5 = new Rect(rect4.xMax, rowY, width, num);
            Widgets.Label(rect5, text);
            Text.Anchor = TextAnchor.UpperLeft;
            return num2;
        }

        public static Texture2D XenoFactionCrypticLabel(Faction faction)
        {
            switch (faction.def.defName)
            {
                case ("EldarWarhost"):
                    {
                        return ContentFinder<Texture2D>.Get("UI/Images/LabelEldar", true);
                    }
                case ("TauVanguard"):
                    {
                        return ContentFinder<Texture2D>.Get("UI/Images/LabelTau", true);
                    }
                case ("ChaosCult"):
                    {
                        return ContentFinder<Texture2D>.Get("UI/Images/LabelChaos", true);
                    }
                default:
                    {
                        return ContentFinder<Texture2D>.Get("UI/Images/LabelChaos", true);
                    }
            }
        }

        public static void AffectGoodwillWithSpacerFaction(Faction faction, Faction other, float goodwillChange)
        {
            if (goodwillChange > 0f && ((faction.IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(other)) || (other.IsPlayer && SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))))
            {
                return;
            }
            float value = other.GoodwillWith(faction) + goodwillChange;
            FactionRelation factionRelation = other.RelationWith(faction, false);
            factionRelation.goodwill = Mathf.Clamp(value, -100f, 100f);            
            
            if (!faction.HostileTo(other) && faction.GoodwillWith(other) < -80f)
            {
                faction.SetHostileTo(other, true);
                if (Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeBad".Translate(), "RelationsBrokenDown".Translate(new object[]
                    {
                faction.Name
                    }), LetterDefOf.BadNonUrgent, null);
                }
            }
            if (faction.HostileTo(other) && faction.GoodwillWith(other) > 0f)
            {
                faction.SetHostileTo(other, false);
                if (Current.ProgramState == ProgramState.Playing && Find.TickManager.TicksGame > 100 && other == Faction.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChangeGood".Translate(), "RelationsWarmed".Translate(new object[]
                    {
                faction.Name
                    }), LetterDefOf.BadNonUrgent, null);
                }
            }
        }

        public static void TryOpenIoMComms(Pawn negotiator, Faction faction)
        {
            Dialog_NegotiationIoM dialog_Negotiation = new Dialog_NegotiationIoM(negotiator, faction, FactionDialogMaker_IoM.FactionDialogFor(negotiator, faction), true);
            dialog_Negotiation.soundAmbient = SoundDefOf.RadioComms_Ambience;
            Find.WindowStack.Add(dialog_Negotiation);
        }

        public static void InitiateGovernorArrestEvent(Map map)
        {
            if (CorruptionStoryTrackerUtilities.currentStoryTracker.PlanetaryGovernor == null)
            {
                return;
            }
            Faction faction = CorruptionStoryTrackerUtilities.currentStoryTracker.ImperialGuard;
            List<Pawn> arbites = new List<Pawn>();
            for (int i=0; i< 5; i++)
            {
                Pawn member = PawnGenerator.GeneratePawn(PawnKindDef.Named("IoM_Arbites"), faction);
                arbites.Add(member);
            }
            
            OHUShips.ShipBase dropShip = (OHUShips.ShipBase)ThingMaker.MakeThing(ThingDef.Named("AquilaLander"));
            dropShip.shipState = OHUShips.ShipState.Incoming;
            dropShip.drawTickOffset = dropShip.compShip.sProps.TicksToImpact;
            Thing initialFuel = ThingMaker.MakeThing(DefOfs.C_ThingDefOfs.Chemfuel);
            initialFuel.stackCount = 2000;
            dropShip.refuelableComp.Refuel(initialFuel);
            dropShip.SetFaction(arbites[0].Faction);
            foreach (Pawn current in arbites)
            {
                dropShip.GetDirectlyHeldThings().TryAdd(current);
            }

            List<ShipBase> tmp = new List<ShipBase>();
            tmp.Add(dropShip);
            IntVec3 dropCenter; 
            if (!DropCellFinder.TryFindRaidDropCenterClose(out dropCenter, map))
            {
                dropCenter = DropCellFinder.FindRaidDropCenterDistant(map);
            }
            DropShipUtility.DropShipGroups(dropCenter, map, tmp, TravelingShipArrivalAction.EnterMapFriendly);
            LordMaker.MakeNewLord(dropShip.Faction, new IoM.LordJob_ArrestGovernor(dropShip, dropCenter),map, arbites);
        }

        public static bool IsPsyker(Pawn pawn)
        {
            Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(pawn);
            if (soul.PsykerPowerLevel >= PsykerPowerLevel.Iota)
            {
                return true;
            }
            return false;
        }

        public static bool PsykerReadyToFire(Pawn psyker)
        {
            CompPsyker psycomp = psyker.TryGetComp<CompPsyker>();
            if (psycomp != null)
            {
                return (psycomp.IsActive && psycomp.ShotFired);
            }
            return false;
        }

        public static Job AI_CastPsykerPowerJob(Pawn pawn, PsykerPowerDef powerDef, Thing target, JobDef forcedJobDef = null)
        {
            CompPsyker compPsyker = pawn.TryGetComp<CompPsyker>();
            if (compPsyker != null)
            {
                Verb_CastWarpPower verb = (Verb_CastWarpPower)Activator.CreateInstance(powerDef.MainVerb.verbClass);
                verb.verbProps = powerDef.MainVerb;
                verb.caster = pawn;
                compPsyker.CurTarget = null;
                compPsyker.CurTarget = target;
                compPsyker.curVerb = verb;
                compPsyker.curPower = powerDef;
                compPsyker.curRotation = target.Rotation;
                Job job = null;
                if (forcedJobDef != null)
                {
                    job = new Job(forcedJobDef, target);
                }
                else
                {
                    job = CompPsyker.PsykerJob(verb.warpverbprops.PsykerPowerCategory, target);
                }
                job.playerForced = true;
                job.verbToUse = verb;
                Pawn pawn2 = target as Pawn;
                if (pawn2 != null)
                {
                    job.killIncappedTarget = pawn2.Downed;
                }

                return job;
            }
            return null;
        }

        public static bool CheckCastOpportunicPsykerPower(Pawn pawn)
        {
            Map map = pawn.Map;





            return false;
        }

        public static void AIGetPsykerTarget(Pawn psyker, AIPsykerPowerCategory aiCategory, float maxRange, out Thing target)
        {

            switch (aiCategory)
            {

                case (AIPsykerPowerCategory.DamageDealer):
                case (AIPsykerPowerCategory.BuffGiverHostile):
                case (AIPsykerPowerCategory.MentalStateGiverHostile):
                    {
                        target = (Thing)AttackTargetFinder.BestAttackTarget(psyker, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat, null, 0f, 50f, default(IntVec3), 3.40282347E+38f, false);
                        return;
                    }
                case (AIPsykerPowerCategory.BuffGiverFriendly):
                case (AIPsykerPowerCategory.MentalStateGiverFriendly):
                    {
                        Predicate<Thing> validator = delegate (Thing t)
                        {
                            Pawn pawn3 = (Pawn)t;
                            return pawn3.Faction == psyker.Faction && !pawn3.InBed();
                        };
                        target = GenClosest.ClosestThingReachable(psyker.Position, psyker.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(psyker, Danger.Deadly, TraverseMode.ByPawn, false), 50f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
                        return;
                    }
                case (AIPsykerPowerCategory.AoEFriendly):
                    {
                        target = psyker;
                        return;
                    }
                case (AIPsykerPowerCategory.AoEHostile):
                    {
                        List<IAttackTarget> potentialTargets = (psyker.Map.attackTargetsCache.GetPotentialTargetsFor(psyker)).FindAll(x => (x as Thing).Position.InHorDistOf(psyker.Position, maxRange));
                        if (potentialTargets.Count > 1)
                        {
                            target = psyker;
                        }
                        target = null;
                        return;
                    }
                default:
                    {
                        target = null;
                        return;
                    }
            }
        }

        public static bool GetRandOffensivePsykerPower(Pawn pawn, out PsykerPowerDef powerDef, out AIPsykerPowerCategory aiCategory)
        {
            Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(pawn);
            if (soul != null)
            {
                CompPsyker compPsyker = soul.compPsyker;
                if (compPsyker != null)
                {
                    Predicate<PsykerPower> validator = delegate (PsykerPower p)
                    {
                            AIPsykerPowerCategory cat = p.powerdef.AICategory;
                            return (cat != AIPsykerPowerCategory.MentalStateGiverFriendly && cat != AIPsykerPowerCategory.AoEFriendly);                        
                    };
                    compPsyker.UpdatePowers();
                    List<PsykerPower> offensivePowers = compPsyker.allPowers.FindAll(x => validator(x));
                    if (offensivePowers.Count > 0)
                    {
                        PsykerPowerDef newDef = offensivePowers.RandomElement().powerdef;
                        if (newDef != null)
                        {
                            aiCategory = newDef.AICategory;
                            powerDef = newDef;
                            return true;
                        }
                    }
                }
            }
            powerDef = null;
            aiCategory = AIPsykerPowerCategory.DamageDealer;
            return false;
        }

         
        public static void InitiateServitorComp(Pawn pawn)
        {
            CompServitor compServitor = new CompServitor();
            compServitor.parent = pawn;
            CompProperties_Refuelable cprops = new CompProperties_Refuelable();
            cprops.compClass = typeof(CompServitor);
            cprops.fuelConsumptionRate = 0.1f;
            compServitor.Initialize(cprops);
            compServitor.Refuel(1);
            FieldInfo info = typeof(ThingWithComps).GetField("comps", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                List<ThingComp> list = info.GetValue(pawn) as List<ThingComp>;
                list.Add(compServitor);
                typeof(ThingWithComps).GetField("comps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(pawn, list);
            }
        }
    }

}
