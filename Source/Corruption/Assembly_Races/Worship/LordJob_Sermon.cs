using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;

namespace Corruption.Worship
{
    public class LordJob_Sermon : LordJob
    {
        public const int ticksToTimeout = 5000;

        public LordJob_Sermon()
        {
        }

        public LordJob_Sermon(BuildingAltar altar, WorshipActType worshipType)
        {
            this.altar = altar;
            if (this.altar != null)
            {
                this.initialPosition = (WatchBuildingUtility.CalculateWatchCells(altar.def, altar.Position, altar.Rotation, altar.Map)).RandomElement();
            }
            else
            {
                this.initialPosition = IntVec3.Invalid;
            }
            this.worshipType = worshipType;
        }

        private WorshipActType worshipType;

        public BuildingAltar altar;

        public Pawn Preacher
        {
            get
            {
                if (this.altar != null)
                {
                    return this.altar.preacher;
                }
                return null;
            }
        }

        private string SermonMessageString
        {
            get
            {
                switch(this.worshipType)
                {
                    case WorshipActType.MorningPrayer:
                        {
                            return "MorningPrayer".Translate();
                        }

                    case WorshipActType.EveningPrayer:
                        {
                            return "EveningPrayer".Translate();
                        }

                    case WorshipActType.Confession:
                        {
                            return "PreacherConfession".Translate();
                        }
                }
                return "";
            }
        }

        private IntVec3 initialPosition;

        public override StateGraph CreateGraph()
        {
            string message = "StartedSermonMessage".Translate(new object[]
                {
                this.Preacher.Name,
                this.SermonMessageString,
                this.altar.RoomName
            });
            Messages.Message(message, this.altar, MessageTypeDefOf.PositiveEvent);
            StateGraph stateGraph = new StateGraph();
            LordToil startingToil = new LordToil_TravelUrgent(this.altar.Position);
            stateGraph.AddToil(startingToil);
            stateGraph.StartingToil = startingToil;
            LordToil_StartSermom sermonToil = new LordToil_StartSermom(this.Preacher, this.altar);
            stateGraph.AddToil(sermonToil);
            Transition startSermonTransition = new Transition(startingToil, sermonToil);
            startSermonTransition.AddTrigger(new Trigger_Memo("TravelArrived"));
            stateGraph.AddTransition(startSermonTransition);          
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition failedTransition = new Transition(startingToil, lordToil_End);
            failedTransition.AddSource(sermonToil);
            failedTransition.AddTrigger(new Trigger_TicksPassed(ticksToTimeout));
            failedTransition.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff()));
            failedTransition.AddTrigger(new Trigger_PawnLostViolently());
            failedTransition.AddPreAction(new TransitionAction_Message("MessageSermonInterrupted".Translate(), MessageTypeDefOf.NegativeEvent, new TargetInfo(this.altar.Position, base.Map, false)));
            stateGraph.AddTransition(failedTransition);
            Transition transition2 = new Transition(sermonToil, lordToil_End);
            transition2.AddTrigger(new Trigger_TickCondition(() => !this.altar.activeSermon));
            transition2.AddTrigger(new Trigger_TicksPassed(ticksToTimeout));
            transition2.AddPreAction(new TransitionAction_Message("MessageSermonFinished".Translate(), MessageTypeDefOf.NegativeEvent, new TargetInfo(this.altar.Position, base.Map, false)));

            stateGraph.AddTransition(transition2);
            return stateGraph;
        }

        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableGameConditionsToContinueParty(base.Map) || (!this.altar.Position.Roofed(base.Map) && !JoyUtility.EnjoyableOutsideNow(base.Map, null));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<BuildingAltar>(ref this.altar, "altar");
            Scribe_Values.Look<IntVec3>(ref this.initialPosition, "initialPosition", IntVec3.Zero);
            Scribe_Values.Look<WorshipActType>(ref this.worshipType, "worshipType", 0);
        }
    }
}
