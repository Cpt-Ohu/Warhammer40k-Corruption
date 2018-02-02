using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption
{
    public class LordJob_Sermon : LordJob_VoluntarilyJoinable
    {
        private List<SermonSpot> sermonSpots = new List<SermonSpot>();

		private IntVec3 spot;

        public List<Pawn> assignedPreachers;

        private Trigger_TicksPassed timeoutTrigger;

        public LordJob_Sermon()
        {
        }

        public LordJob_Sermon(IntVec3 spot)
        {
            this.spot = spot;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil_AttendSermon lordToil_Sermon = new LordToil_AttendSermon(this.spot);
            stateGraph.AddToil(lordToil_Sermon);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition transition = new Transition(lordToil_Sermon, lordToil_End);
            transition.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff()));
            transition.AddTrigger(new Trigger_PawnLostViolently());
            transition.AddAction(new TransitionAction_Message("MessagePartyCalledOff".Translate(), MessageSound.Negative, this.spot));
            stateGraph.AddTransition(transition);
            this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(5000, 15000));
            Transition transition2 = new Transition(lordToil_Sermon, lordToil_End);
            transition2.AddTrigger(this.timeoutTrigger);
            transition2.AddAction(new TransitionAction_Message("MessagePartyFinished".Translate(), MessageSound.Negative, this.spot));

            transition2.AddAction(new TransitionAction_Cus(delegate
            {
                this.Finished();
            }));
            stateGraph.AddTransition(transition2);
            return stateGraph;
        }

        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableMapConditionsToContinueParty() || (!this.spot.Roofed() && !JoyUtility.EnjoyableOutsideNow(null));
        }

        private void Finished()
        {
            List<Pawn> ownedPawns = this.lord.ownedPawns;
            for (int i = 0; i < ownedPawns.Count; i++)
            {
                if (PartyUtility.InPartyArea(ownedPawns[i].Position, this.spot))
                {
                    ownedPawns[i].needs.mood.thoughts.memories.TryGainMemoryThought(CorruptionDefOfs.AttendedSermon, ListenedTo(ownedPawns[i], sermonSpots));
                }
            }
        }

        private static Pawn ListenedTo(Pawn listener, List<SermonSpot> sspots)
        {
            float dist = 9999f;
            Pawn closestMatch = null;            

            for (int i = 0; i < sspots.Count; i++)
            {
                double dcurr = CommonMisc.Helper.GetDistance(listener.Position, sspots[i].AltarSpot);
                    if (dcurr < dist)
                {
                    closestMatch = sspots[i].Preacher;
                }
            }

            return closestMatch;

        }

        private static Predicate<Thing> GetAltars()
            {
            return delegate (Thing b)
            {
                return b.GetType() == typeof(BuildingAltar);
            };
        }

        public void TryStartSermon()
        {
            Log.Message("Starting Sermon");
            List<Thing> list = Find.ListerThings.AllThings.FindAll(GetAltars());
            foreach (BuildingAltar b in list)
            {
                Log.Message(b.ToString());
                if (b.DoMorningSermon || b.DoEveningSermon)
                {
                    LordMaker.MakeNewLord(b.preacher.Faction, new LordJob_Sermon(b.Position));
                    b.preacher.QueueJob(new Job(CorruptionDefOfs.HoldSermon, b, b.InteractionCell));
                    assignedPreachers.Add(b.preacher);
                    sermonSpots.Add(new SermonSpot(b.preacher, b.InteractionCell));
                }
            }
        }
        
        public override float VoluntaryJoinPriorityFor(Pawn p)
        {
            if (assignedPreachers.Contains(p))
            {
                return 0f;
            }
            if (!this.IsInvited(p, spot))
            {
                return 0f;
            }
            if (!PartyUtility.ShouldPawnKeepPartying(p))
            {
                return 0f;
            }
            if (!this.lord.ownedPawns.Contains(p) && this.IsPartyAboutToEnd())
            {
                return 0f;
            }

            Need_Soul soul = p.needs.TryGetNeed<Need_Soul>();
            if (soul != null)
            {
                switch(soul.DevotionTrait.SDegree)
                {
                    case -2:
                        {
                            return 0f;
                        }
                    case -1:
                        {
                            return 5f;
                        }
                    case 0:
                        {
                            return 20f;
                        }
                    case 1:
                        {
                            return 25f;
                        }
                    case 2:
                        {
                            return 30f;
                        }
                }
            }
            return 20f;
        }

        public override void ExposeData()
        {
            Scribe_Deep.LookDeep<List<SermonSpot>>(ref sermonSpots, "sermonSpots", false);
            Scribe_Values.LookValue<IntVec3>(ref this.spot, "spot", default(IntVec3), false);
        }

        public override string GetReport()
        {
            return "LordReportAttendingParty".Translate();
        }

        private bool IsPartyAboutToEnd()
        {
            return this.timeoutTrigger.TicksLeft < 1200;
        }

        private bool IsInvited(Pawn p, IntVec3 spot)
        {
            Room room = spot.GetRoom();
            if (room.Role == RoomRoleDefOf.PrisonBarracks)
            {
                if (p.IsPrisonerOfColony)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return p.Faction == this.lord.faction;
        }

        sealed class TransitionAction_Cus : TransitionAction
        {
            public Action action;

            public TransitionAction_Cus(Action action)
            {
                this.action = action;
            }

            public override void DoAction(Transition trans)
            {
                if (this.action != null)
                {
                    this.action();
                }
            }
        }

    }


}
