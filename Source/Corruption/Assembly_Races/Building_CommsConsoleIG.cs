using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Corruption.DefOfs;

namespace Corruption
{
    public class Building_CommsConsoleIG : Building
    {
        public CorruptionStoryTracker corruptionStoryTracker
        {
            get
            {
                return CorruptionStoryTrackerUtilities.currentStoryTracker;
            }
        }

        private CompPowerTrader powerComp;

        public bool CanUseCommsNow
        {
            get
            {
                return (!base.Spawned || !base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare)) && this.powerComp.PowerOn;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.powerComp = base.GetComp<CompPowerTrader>();
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.OpeningComms, OpportunityType.GoodToKnow);
        }

        private void UseAct(Pawn myPawn, ICommunicable commTarget)
        {
            Job job = new Job(JobDefOf.UseCommsConsole, this);
            job.commTarget = commTarget;
            myPawn.jobs.TryTakeOrderedJob(job);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (!myPawn.CanReserve(this, 1))
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item
                };
            }
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Some, false, TraverseMode.ByPawn))
            {
                FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item2
                };
            }
            if (base.Spawned && base.Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare))
            {
                FloatMenuOption item3 = new FloatMenuOption("CannotUseSolarFlare".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item3
                };
            }
            if (!this.powerComp.PowerOn)
            {
                FloatMenuOption item4 = new FloatMenuOption("CannotUseNoPower".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item4
                };
            }
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
            {
                FloatMenuOption item5 = new FloatMenuOption("CannotUseReason".Translate(new object[]
                {
                    "IncapableOfCapacity".Translate(new object[]
                    {
                        PawnCapacityDefOf.Talking.label
                    })
                }), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item5
                };
            }
            if (!this.CanUseCommsNow)
            {
                Log.Error(myPawn + " could not use comm console for unknown reason.");
                FloatMenuOption item6 = new FloatMenuOption("Cannot use now", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                return new List<FloatMenuOption>
                {
                    item6
                };
            }
            List<FloatMenuOption> list = new List<FloatMenuOption>();
                string text = "UseCCC".Translate();
                Action action = delegate
                {
                    Job job = new Job(C_JobDefOf.UsingCCC, this);
                    myPawn.jobs.TryTakeOrderedJob(job);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                };
                list.Add(new FloatMenuOption(text, action, MenuOptionPriority.InitiateSocial, null, null, 0f, null, null));
            
            return list;
        }

        public static bool LeaderIsAvailableToTalk(Faction fac)
        {
            return fac.leader != null && (!fac.leader.Spawned || (!fac.leader.Downed && !fac.leader.IsPrisoner && fac.leader.Awake() && !fac.leader.InMentalState));
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

    }
}
