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
        public LordJob_Sermon()
        {
        }

        public LordJob_Sermon(BuildingAltar altar, bool isMorningPrayer = true)
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
            this.isMorningPrayer = isMorningPrayer;
        }

        private bool isMorningPrayer = true;

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

        private string MorningPrayerString
        {
            get
            {
                if (this.isMorningPrayer) return "MorningPrayer".Translate();
                return "EveningPrayer".Translate();
            }
        }

        private IntVec3 initialPosition;

        public override StateGraph CreateGraph()
        {
            string message = "StartedSermonMessage".Translate(new object[]
                {
                this.Preacher.Name,
                this.MorningPrayerString,
                this.altar.RoomName
            });
            Messages.Message(message, this.altar, MessageSound.Negative);
            StateGraph stateGraph = new StateGraph();
            LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(this.initialPosition).CreateGraph()).StartingToil;
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
            failedTransition.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff()));
            failedTransition.AddTrigger(new Trigger_PawnLostViolently());
            failedTransition.AddPreAction(new TransitionAction_Message("MessageSermonInterrupted".Translate(), MessageSound.Negative, new TargetInfo(this.altar.Position, base.Map, false)));
            stateGraph.AddTransition(failedTransition);
            Transition transition2 = new Transition(sermonToil, lordToil_End);
            transition2.AddTrigger(new Trigger_TickCondition(() => !this.altar.activeSermon));
            transition2.AddPreAction(new TransitionAction_Message("MessageSermonFinished".Translate(), MessageSound.Negative, new TargetInfo(this.altar.Position, base.Map, false)));

            stateGraph.AddTransition(transition2);
            return stateGraph;
        }

        private bool ShouldBeCalledOff()
        {
            return !PartyUtility.AcceptableMapConditionsToContinueParty(base.Map) || (!this.altar.Position.Roofed(base.Map) && !JoyUtility.EnjoyableOutsideNow(base.Map, null));
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.LookReference<BuildingAltar>(ref this.altar, "altar");
            Scribe_Values.LookValue<IntVec3>(ref this.initialPosition, "initialPosition", IntVec3.Zero);
            Scribe_Values.LookValue<bool>(ref this.isMorningPrayer, "isMorningPrayer", false);
        }
    }
}
