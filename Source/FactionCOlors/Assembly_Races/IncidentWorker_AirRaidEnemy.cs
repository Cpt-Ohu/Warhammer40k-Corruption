using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace FactionColors
{
    public class IncidentWorker_AirRaidEnemy : IncidentWorker_Raid
    {
        protected override bool FactionCanBeGroupSource(Faction f, bool desperate = false)
        {
            return base.FactionCanBeGroupSource(f, desperate) && f.HostileTo(Faction.OfPlayer) && (desperate || (float)GenDate.DaysPassed >= f.def.earliestRaidDays);
        }

        public override bool TryExecute(IncidentParms parms)
        {
            {
                this.ResolveRaidPoints(parms);
                if (!this.TryResolveRaidFaction(parms))
                {
                    return false;
                }
                this.ResolveRaidStrategy(parms);
                this.ResolveRaidArriveMode(parms);
                this.ResolveRaidSpawnCenter(parms);
                PawnGroupMakerUtility.AdjustPointsForGroupArrivalParams(parms);
                List<Pawn> list = PawnGroupMakerUtility.GenerateArrivingPawns(parms, true).ToList<Pawn>();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms);
                    return false;
                }
                TargetInfo letterLookTarget = TargetInfo.Invalid;

                Deepstriker_Utilities.UnloadThingsNear(parms.faction, parms.spawnCenter, list.Cast<Thing>(), parms.raidPodOpenDelay, false, true, true);
                
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Points = " + parms.points.ToString("F0"));
                foreach (Pawn current2 in list)
                {
                    string str = (current2.equipment == null || current2.equipment.Primary == null) ? "unarmed" : current2.equipment.Primary.LabelCap;
                    stringBuilder.AppendLine(current2.KindLabel + " - " + str);
                }
                Find.LetterStack.ReceiveLetter(this.GetLetterLabel(parms), this.GetLetterText(parms, list), this.GetLetterType(), letterLookTarget, stringBuilder.ToString());
                if (this.GetLetterType() == LetterType.BadUrgent)
                {
                    TaleRecorder.RecordTale(TaleDefOf.RaidArrived, new object[0]);
                }
                PawnRelationUtility.Notify_PawnsSeenByPlayer(list, this.GetRelatedPawnsInfoLetterText(parms), true);
                Lord lord = LordMaker.MakeNewLord(parms.faction, parms.raidStrategy.Worker.MakeLordJob(ref parms), list);
                AvoidGridMaker.RegenerateAvoidGridsFor(parms.faction);
                LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
                if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.PersonalShields))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        Pawn pawn = list[i];
                        if (pawn.apparel.WornApparel.Any((Apparel ap) => ap is PersonalShield))
                        {
                            LessonAutoActivator.TeachOpportunity(ConceptDefOf.PersonalShields, OpportunityType.Critical);
                            break;
                        }
                    }
                }
                if (DebugViewSettings.drawStealDebug && parms.faction.HostileTo(Faction.OfPlayer))
                {
                    Log.Message(string.Concat(new object[]
                    {
            "Market value threshold to start stealing: ",
            StealAIUtility.StartStealingMarketValueThreshold(lord),
            " (colony wealth = ",
            Find.StoryWatcher.watcherWealth.WealthTotal,
            ")"
                    }));
                }
                return true;
            }
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            if (parms.faction != null)
            {
                return true;
            }
            float maxPoints = parms.points;
            if (maxPoints <= 0f)
            {
                maxPoints = 999999f;
            }
            if (!(from f in Find.FactionManager.AllFactions
                  where this.FactionCanBeGroupSource(f, false) && maxPoints >= f.def.MinPointsToGenerateNormalPawnGroup()
                  select f).TryRandomElementByWeight((Faction f) => f.def.raidCommonality, out parms.faction))
            {
                if (!(from f in Find.FactionManager.AllFactions
                      where this.FactionCanBeGroupSource(f, true) && maxPoints >= f.def.MinPointsToGenerateNormalPawnGroup()
                      select f).TryRandomElementByWeight((Faction f) => f.def.raidCommonality, out parms.faction))
                {
                    Log.Error("IncidentWorker_RaidEnemy could not fire even though we thought we could: no faction could generate with " + maxPoints + " points.");
                    return false;
                }
            }
            return true;
        }

        protected override void ResolveRaidStrategy(IncidentParms parms)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            parms.raidStrategy = (from d in DefDatabase<RaidStrategyDef>.AllDefs
                                  where d.Worker.CanUseWith(parms)
                                  select d).RandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionChance);
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = null;
            switch (parms.raidArrivalMode)
            {
                case PawnsArriveMode.EdgeWalkIn:
                    text = "EnemyRaidWalkIn".Translate(new object[]
                    {
                    parms.faction.def.pawnsPlural,
                    parms.faction.Name
                    });
                    break;
                case PawnsArriveMode.EdgeDrop:
                    text = "EnemyRaidEdgeDrop".Translate(new object[]
                    {
                    parms.faction.def.pawnsPlural,
                    parms.faction.Name
                    });
                    break;
                case PawnsArriveMode.CenterDrop:
                    text = "EnemyRaidCenterDrop".Translate(new object[]
                    {
                    parms.faction.def.pawnsPlural,
                    parms.faction.Name
                    });
                    break;
            }
            text += "\n\n";
            text += parms.raidStrategy.arrivalTextEnemy;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (pawn != null)
            {
                text += "\n\n";
                text += "EnemyRaidLeaderPresent".Translate(new object[]
                {
                    pawn.Faction.def.pawnsPlural,
                    pawn.LabelShort
                });
            }
            return text;
        }

        protected override LetterType GetLetterType()
        {
            return LetterType.BadUrgent;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidEnemy".Translate(new object[]
            {
                parms.faction.def.pawnsPlural
            });
        }
    }
}
