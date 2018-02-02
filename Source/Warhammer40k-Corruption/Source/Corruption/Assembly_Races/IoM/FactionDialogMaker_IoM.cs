using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class FactionDialogMaker_IoM
    {
        private const float MinRelationsToCommunicate = -70f;

        private const float MinRelationsFriendly = 40f;
        
        private static DiaNode root;

        private static Pawn negotiator;

        private static Faction faction;

        public static DiaNode FactionDialogFor(Pawn negotiator, Faction faction)
        {
            Map map = negotiator.Map;
            FactionDialogMaker_IoM.negotiator = negotiator;
            FactionDialogMaker_IoM.faction = faction;
            string text = (faction.leader != null) ? faction.leader.Name.ToStringFull : faction.Name;
            if (faction.PlayerGoodwill < -70f)
            {
                FactionDialogMaker_IoM.root = new DiaNode("FactionGreetingHostile".Translate(new object[]
                {
                    text
                }));
            }
            else if (faction.PlayerGoodwill < 40f)
            {
                string text2 = "FactionGreetingWary".Translate(new object[]
                {
                    text,
                    negotiator.LabelShort
                });
                text2 = text2.AdjustedFor(negotiator);
                FactionDialogMaker_IoM.root = new DiaNode(text2);
                if (!SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
                {
                    FactionDialogMaker_IoM.root.options.Add(FactionDialogMaker_IoM.OfferGiftOption(negotiator.Map));
                }
                if (!faction.HostileTo(Faction.OfPlayer) && negotiator.Spawned && negotiator.Map.IsPlayerHome)
                {
                    if (CorruptionStoryTrackerUtilities.currentStoryTracker.ImperialFactions.Contains(faction))
                    {
                        FactionDialogMaker_IoM.root.options.AddRange(FactionDialogMaker_IoM.FactionSpecificOptions(faction, map, negotiator));
                    }          
                }
            }
            else
            {
                FactionDialogMaker_IoM.root = new DiaNode("FactionGreetingWarm".Translate(new object[]
                {
                    text,
                    negotiator.LabelShort
                }));
                if (!SettlementUtility.IsPlayerAttackingAnySettlementOf(faction))
                {
                    FactionDialogMaker_IoM.root.options.Add(FactionDialogMaker_IoM.OfferGiftOption(negotiator.Map));
                }
                if (!faction.HostileTo(Faction.OfPlayer) && negotiator.Spawned && negotiator.Map.IsPlayerHome)
                {
                    if (CorruptionStoryTrackerUtilities.currentStoryTracker.IoMCanHelp)
                    {
                        FactionDialogMaker_IoM.root.options.Add(FactionDialogMaker_IoM.RequestMilitaryAidOption(map));
                    }
                    FactionDialogMaker_IoM.root.options.AddRange(FactionDialogMaker_IoM.FactionSpecificOptions(faction, map, negotiator));
                }
            }
            if (Prefs.DevMode)
            {
                foreach (DiaOption current in FactionDialogMaker_IoM.DebugOptions(FactionDialogMaker_IoM.faction))
                {
                    FactionDialogMaker_IoM.root.options.Add(current);
                }
            }
            DiaOption diaOption = new DiaOption("(" + "Disconnect".Translate() + ")");
            diaOption.resolveTree = true;
            FactionDialogMaker_IoM.root.options.Add(diaOption);
            return FactionDialogMaker_IoM.root;
        }

        private static IEnumerable<DiaOption> FactionSpecificOptions(Faction faction, Map map, Pawn negotiator)
        {
            CorruptionStoryTracker tracker = CorruptionStoryTrackerUtilities.currentStoryTracker;

            if (tracker.ImperialFactions.Contains(faction))
            {
                yield return (FactionDialogMaker_IoM.RequestAcknowledgement_IG(map));
            }

            if (faction == tracker.Mechanicus)
            {
                yield return new DiaOption("PurchaseMecTemplates".Translate())
                {
                    link = FactionDialogMaker_IoM.PurchaseMechanicusTemplates(map, faction)
                };
            }

            if (faction == tracker.AdeptusSororitas)
            {
                yield return new DiaOption("RequestNursesNode".Translate())
                {
                    link = FactionDialogMaker_IoM.RequestHealer(map)
                };
            }
            yield return new DiaOption("Debug: Spawn Battles")
            {
                link = FactionDialogMaker_IoM.DebugBattleZone()
            };
            yield break;
        }

        private static DiaNode PurchaseMechanicusTemplates(Map map, Faction faction)
        {
            DiaNode node = new DiaNode("PurchaseMecTemplates".Translate());
            node.options.AddRange(AvailableMechanicusTemplates(map, faction));
            node.options.Add(FactionDialogMaker_IoM.OKToRoot());
            return node;
        }

        private static DiaNode DebugBattleZone()
        {
            DiaNode node = new DiaNode("PurchaseMecTemplates".Translate());
            node.options.Add(new DiaOption("STuff")
            {
                action = delegate
                {
                    Domination.DominationUtilities.GenerateRandomBattleZone();
                }

            });
        
            node.options.Add(FactionDialogMaker_IoM.OKToRoot());
            return node;
        }

        private static IEnumerable<DiaOption> AvailableMechanicusTemplates(Map map, Faction faction)
        {
            float mecRelationFactor = 100* faction.GoodwillWith(Faction.OfPlayer);
            List<ResearchProjectDef> mecTecs = DefDatabase<ResearchProjectDef>.AllDefsListForReading.FindAll(x => x.defName.Contains("MecTec_"));

            foreach (ResearchProjectDef def in mecTecs)
            {
                if (def.baseCost <= mecRelationFactor && !def.IsFinished)
                {
                    string optionText = "PurchaseTemplate".Translate() + ": " + def.label + " (" + def.baseCost + ")";
                    if (FactionDialogMaker_IoM.AmountSendableSilver(map) < def.baseCost)
                    {
                        DiaOption diaOption = new DiaOption(optionText);
                        diaOption.Disable("NeedSilverLaunchable".Translate(new object[]
                        {
                    (int)def.baseCost
                        }));
                        yield return diaOption;
                    }
                    else
                    {
                        DiaOption diaOption2 = new DiaOption(optionText);
                        diaOption2.action = delegate
                        {

                        };
                        string text = "PurchaseTemplateConfirmation".Translate(new object[]
                        {
                            def.label
                        }).CapitalizeFirst();
                        diaOption2.link = new DiaNode(text)
                        {
                            options =
                            {
                                FactionDialogMaker_IoM.ConfirmPurchase(map, def),
                                new DiaOption("GoBack".Translate())
                            {
                                linkLateBind = FactionDialogMaker_IoM.ResetToRoot()
                            }
                                
                            }
                        };
                        yield return diaOption2;
                    }
                }
            }
        }

        private static void GrantMecResearch(ResearchProjectDef def)
        {
            ResearchManager manager = Find.ResearchManager;
            ResearchProjectDef oldDef = manager.currentProj;
            manager.currentProj = def;
            manager.InstantFinish(def, false);
            manager.currentProj = oldDef;
        }

        private static DiaOption RequestMilitaryAidOption(Map map)
        {
            string text = "RequestMilitaryAid".Translate(new object[]
            {
                -25f
            });
            if (!FactionDialogMaker_IoM.faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
            {
                DiaOption diaOption = new DiaOption(text);
                diaOption.Disable("BadTemperature".Translate());
                return diaOption;
            }
            DiaOption diaOption2 = new DiaOption(text);
            if (map.attackTargetsCache.TargetsHostileToColony.Any((IAttackTarget x) => !x.ThreatDisabled()))
            {
                if (!map.attackTargetsCache.TargetsHostileToColony.Any((IAttackTarget p) => ((Thing)p).Faction != null && ((Thing)p).Faction.HostileTo(FactionDialogMaker_IoM.faction)))
                {
                    IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony
                                                   where !x.ThreatDisabled()
                                                   select x into pa
                                                   select ((Thing)pa).Faction into fa
                                                   where fa != null && !fa.HostileTo(FactionDialogMaker_IoM.faction)
                                                   select fa).Distinct<Faction>();
                    string arg_1B6_0 = "MilitaryAidConfirmMutualEnemy";
                    object[] expr_17D = new object[2];
                    expr_17D[0] = FactionDialogMaker_IoM.faction.Name;
                    expr_17D[1] = GenText.ToCommaList(from fa in source
                                                      select fa.Name, true);
                    DiaNode diaNode = new DiaNode(arg_1B6_0.Translate(expr_17D));
                    DiaOption diaOption3 = new DiaOption("CallConfirm".Translate());
                    diaOption3.action = delegate
                    {
                        FactionDialogMaker_IoM.CallForAid(map);
                    };
                    diaOption3.link = FactionDialogMaker_IoM.FightersSent();
                    DiaOption diaOption4 = new DiaOption("CallCancel".Translate());
                    diaOption4.linkLateBind = FactionDialogMaker_IoM.ResetToRoot();
                    diaNode.options.Add(diaOption3);
                    diaNode.options.Add(diaOption4);
                    diaOption2.link = diaNode;
                    return diaOption2;
                }
            }
            diaOption2.action = delegate
            {
                FactionDialogMaker_IoM.CallForAid(map);
            };
            diaOption2.link = FactionDialogMaker_IoM.FightersSent();
            return diaOption2;
        }

        private static DiaNode FightersSent()
        {
            return new DiaNode("MilitaryAidSent".Translate(new object[]
            {
                FactionDialogMaker_IoM.faction.leader.LabelIndefinite()
            }).CapitalizeFirst())
            {
                options =
                {
                    FactionDialogMaker_IoM.OKToRoot()
                }
            };
        }

        private static void CallForAid(Map map)
        {

            IncidentParms incidentParms = new IncidentParms();
            incidentParms.raidArrivalMode = PawnsArriveMode.CenterDrop;
            incidentParms.target = map;
            incidentParms.faction = FactionDialogMaker_IoM.faction;
            incidentParms.points = (float)Rand.Range(600f, 1000f);
            IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
        }

        private static DiaOption OKToRoot()
        {
            return new DiaOption("OK".Translate())
            {
                linkLateBind = FactionDialogMaker_IoM.ResetToRoot()
            };
        }

        private static DiaOption ConfirmPurchase(Map map, ResearchProjectDef def)
        {
            DiaOption node = new DiaOption("Confirm".Translate())
            {
                linkLateBind = FactionDialogMaker_IoM.ResetToRoot()
            };
            node.action = delegate
            {
                TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, (int)def.baseCost, map, null);
                FactionDialogMaker_IoM.GrantMecResearch(def);
            };
            return node;
        }

        private static DiaNode RequestHealer(Map map)
        {
            DiaNode node = new DiaNode("RequestSororitasNurse".Translate());
            float sororitasGoowdill = CorruptionStoryTrackerUtilities.currentStoryTracker.AdeptusSororitas.RelationWith(Faction.OfPlayer).goodwill;
            if (sororitasGoowdill > 50)
            {
                string optionText = "RequestNurses".Translate(new object[] { 1000 });
                if (FactionDialogMaker_IoM.AmountSendableSilver(map) < 1000)
                {
                    DiaOption diaOption = new DiaOption(optionText);
                    diaOption.Disable("NeedSilverLaunchable".Translate(new object[]
                    {
                    1000
                    }));
                    node.options.Add(diaOption);
                }
                else
                {
                    DiaOption diaOption2 = new DiaOption(optionText);
                    diaOption2.action = delegate
                    {

                    };
                    string text = "RequestHealersConfirmed".Translate(new object[]
                    {
                            1000
                    }).CapitalizeFirst();
                    diaOption2.link = new DiaNode(text)
                    {
                        options =
                            {
                                FactionDialogMaker_IoM.ConfirmGetHealer(map),
                                new DiaOption("GoBack".Translate())
                            {
                                linkLateBind = FactionDialogMaker_IoM.ResetToRoot()
                            }

                            }
                    };
                    node.options.Add(diaOption2);
                }
            }
            return node;
        }


        private static DiaOption ConfirmGetHealer(Map map)
        {
            DiaOption option = new DiaOption("Confirm".Translate())
            {
                linkLateBind = FactionDialogMaker_IoM.ResetToRoot()
            };
            option.action = delegate
            {
                TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, 1000, map, null);
                Pawn pawn = null;
                IoM.IoM_StoryUtilities.GenerateIntrusiveWanderer(map, DefOfs.C_PawnKindDefOf.SororitasNurse, CorruptionStoryTrackerUtilities.currentStoryTracker.AdeptusSororitas, IoMChatType.VisitingHealer, "IoM_HealerArrives", out pawn) ;
            };
            return option;
        }

        private static Func<DiaNode> ResetToRoot()
        {
            return () => FactionDialogMaker_IoM.FactionDialogFor(FactionDialogMaker_IoM.negotiator, FactionDialogMaker_IoM.faction);
        }

        [DebuggerHidden]
        private static IEnumerable<DiaOption> DebugOptions(Faction faction)
        {
            DiaOption opt = new DiaOption("(Debug) Goodwill +10");
            opt.action = delegate
            {
                CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(Faction.OfPlayer, faction, 10f);
            };
            opt.linkLateBind = (() => FactionDialogMaker_IoM.FactionDialogFor(FactionDialogMaker_IoM.negotiator, FactionDialogMaker_IoM.faction));
            yield return opt;
            DiaOption opt2 = new DiaOption("(Debug) Goodwill -10");
            opt2.action = delegate
            {
                CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(Faction.OfPlayer, faction, -10f);
            };
            opt2.linkLateBind = (() => FactionDialogMaker_IoM.FactionDialogFor(FactionDialogMaker_IoM.negotiator, FactionDialogMaker_IoM.faction));
            yield return opt2;
        }

        private static int AmountSendableSilver(Map map)
        {
            return (from t in TradeUtility.AllLaunchableThings(map)
                    where t.def == ThingDefOf.Silver
                    select t).Sum((Thing t) => t.stackCount);
        }

        private static DiaOption OfferGiftOption(Map map)
        {
            if (FactionDialogMaker_IoM.AmountSendableSilver(map) < 300)
            {
                DiaOption diaOption = new DiaOption("OfferGift".Translate());
                diaOption.Disable("NeedSilverLaunchable".Translate(new object[]
                {
                    300
                }));
                return diaOption;
            }
            float goodwillDelta = 5f * FactionDialogMaker_IoM.negotiator.GetStatValue(StatDefOf.DiplomacyPower, true);
            DiaOption diaOption2 = new DiaOption("OfferGift".Translate() + " (" + "SilverForGoodwill".Translate(new object[]
            {
                300,
                goodwillDelta.ToString("#####0")
            }) + ")");
            diaOption2.action = delegate
            {
                TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, 300, map, null);
                CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(Faction.OfPlayer, faction, goodwillDelta);
            };
            string text = "SilverGiftSent".Translate(new object[]
            {
                FactionDialogMaker_IoM.faction.leader.LabelIndefinite(),
                Mathf.RoundToInt(goodwillDelta)
            }).CapitalizeFirst();
            diaOption2.link = new DiaNode(text)
            {
                options =
                {
                    FactionDialogMaker_IoM.OKToRoot()
                }
            };
            return diaOption2;
        }

        private static DiaOption RequestTraderOption(Map map, int silverCost)
        {
            string text = "RequestTrader".Translate(new object[]
            {
                silverCost.ToString()
            });
            if (FactionDialogMaker_IoM.AmountSendableSilver(map) < 300)
            {
                DiaOption diaOption = new DiaOption(text);
                diaOption.Disable("NeedSilverLaunchable".Translate(new object[]
                {
                    silverCost
                }));
                return diaOption;
            }
            if (!FactionDialogMaker_IoM.faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
            {
                DiaOption diaOption2 = new DiaOption(text);
                diaOption2.Disable("BadTemperature".Translate());
                return diaOption2;
            }
            int num = FactionDialogMaker_IoM.faction.lastTraderRequestTick + 240000 - Find.TickManager.TicksGame;
            if (num > 0)
            {
                DiaOption diaOption3 = new DiaOption(text);
                diaOption3.Disable("WaitTime".Translate(new object[]
                {
                    num.ToStringTicksToPeriod(true)
                }));
                return diaOption3;
            }
            DiaOption diaOption4 = new DiaOption(text);
            DiaNode diaNode = new DiaNode("TraderSent".Translate(new object[]
            {
                FactionDialogMaker_IoM.faction.leader.LabelIndefinite()
            }).CapitalizeFirst());
            diaNode.options.Add(FactionDialogMaker_IoM.OKToRoot());
            DiaNode diaNode2 = new DiaNode("ChooseTraderKind".Translate(new object[]
            {
                FactionDialogMaker_IoM.faction.leader.LabelIndefinite()
            }));
            foreach (TraderKindDef current in FactionDialogMaker_IoM.faction.def.caravanTraderKinds)
            {
                TraderKindDef localTk = current;
                DiaOption diaOption5 = new DiaOption(localTk.LabelCap);
                diaOption5.action = delegate
                {
                    IncidentParms incidentParms = new IncidentParms();
                    incidentParms.target = map;
                    incidentParms.faction = FactionDialogMaker_IoM.faction;
                    incidentParms.traderKind = localTk;
                    incidentParms.forced = true;
                    Find.Storyteller.incidentQueue.Add(IncidentDefOf.TraderCaravanArrival, Find.TickManager.TicksGame + 120000, incidentParms);
                    FactionDialogMaker_IoM.faction.lastTraderRequestTick = Find.TickManager.TicksGame;
                    TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, silverCost, map, null);
                };
                diaOption5.link = diaNode;
                diaNode2.options.Add(diaOption5);
            }
            DiaOption diaOption6 = new DiaOption("GoBack".Translate());
            diaOption6.linkLateBind = FactionDialogMaker_IoM.ResetToRoot();
            diaNode2.options.Add(diaOption6);
            diaOption4.link = diaNode2;
            return diaOption4;
        }

        // Specific DiaOptions


        private static DiaOption RequestAcknowledgement_IG(Map map)
        {
            CorruptionStoryTracker tracker =  CorruptionStoryTrackerUtilities.currentStoryTracker;
            if (tracker.ImperialFactions.Any(x => x.HostileTo(Faction.OfPlayer)) || tracker.PlayerIsEnemyOfMankind)
            {
                DiaOption diaOption = new DiaOption("RequestIoMAcknowledgement".Translate());
                diaOption.Disable("PlayerIsEnemyOfMankind".Translate());
                return diaOption;
            }

            if (CorruptionStoryTrackerUtilities.currentStoryTracker.AcknowledgedByImperium)
            {
                DiaOption diaOption = new DiaOption("RequestIoMAcknowledgement".Translate());
                diaOption.Disable("AlreadyIoMAcknowledged".Translate());
                return diaOption;
            }
            DiaOption diaOption2 = new DiaOption("RequestIoMAcknowledgement".Translate());
            string text = "IoMAcknowledgementGrant".Translate(new object[]
            {
                Find.World.info.name,
                CorruptionStoryTrackerUtilities.currentStoryTracker.SubsectorName,
                negotiator.Name
            }).CapitalizeFirst();

            DiaOption diaOption3 = new DiaOption("GoBack".Translate());
            diaOption3.linkLateBind = FactionDialogMaker_IoM.ResetToRoot();

            diaOption2.link = new DiaNode(text)
            {
                options =
                {
                    FactionDialogMaker_IoM.AcceptIoMAcknowledgement(map, negotiator),
                    diaOption3                    
                }
            };
            return diaOption2;
        }

        private static DiaOption AcceptIoMAcknowledgement(Map map, Pawn governorCandidate)
        {
            DiaOption diaOption2 = new DiaOption("AcceptIoMAcknowledgement".Translate());
            string text = "IoMAcknowledgementAccepted".Translate(new object[]
            {

            }).CapitalizeFirst();
            diaOption2.action = delegate
            {
                CorruptionStoryTracker tracker = CorruptionStoryTrackerUtilities.currentStoryTracker;
                tracker.PlanetaryGovernor = governorCandidate;
                Tithes.TitheUtilities.CalculateColonyTithes(tracker);
                Tithes.TitheUtilities.UpdateAllContaners();
                tracker.AcknowledgedByImperium = true;
            };
            diaOption2.link = new DiaNode(text)
            {
                options =
                {
                    FactionDialogMaker_IoM.OKToRoot()
                }
            };
            return diaOption2;
        }

    }
}
