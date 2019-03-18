using OHUShips;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace Corruption.IoM
{
    public class LordJob_PickupResourcePacks : LordJob
    {
        private ShipBase Ship;

        public LordJob_PickupResourcePacks(ShipBase ship)
        {
            this.Ship = ship;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            LordToil_DefendSelf startingToil = new LordToil_DefendSelf();
            LordToil_LoadResourcePacks loadToil = new LordToil_LoadResourcePacks(this.Ship);
            Transition loadTransition = new Transition(startingToil, loadToil);
            loadTransition.AddTrigger(new Trigger_TicksPassed(750));
            loadTransition.AddPreAction(new TransitionAction_Message("MessageTributeLoadingStarted".Translate(new object[] { this.Ship.Faction.Name }), MessageTypeDefOf.PositiveEvent, this.Ship));
            loadTransition.AddPostAction(new TransitionAction_Custom(new Action(delegate 
            {
                Log.Message(this.lord.ownedPawns.Count.ToString());
            })));
            stateGraph.AddToil(startingToil);
            stateGraph.AddTransition(loadTransition);
            stateGraph.AddToil(loadToil);
            LordToil_LeaveInShip leaveToil = new LordToil_LeaveInShip();
            stateGraph.AddToil(leaveToil);
            Transition transitionLeave = new Transition(loadToil, leaveToil);
            transitionLeave.AddTrigger(new Trigger_Custom((TriggerSignal x) => this.NoResourcesFound()));
            transitionLeave.AddTrigger(new Trigger_TicksPassed(7500));
            transitionLeave.AddPreAction(new TransitionAction_Message("MessageTributeLeaving".Translate(new object[] { this.Ship.Faction.Name }), MessageTypeDefOf.PositiveEvent, this.Ship));

            stateGraph.AddTransition(transitionLeave);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition transitionEnd = new Transition(leaveToil, lordToil_End);
            transitionEnd.AddTrigger(new Trigger_Custom((TriggerSignal x) => this.Ship.Spawned == false));
            stateGraph.AddTransition(transitionEnd);
            return stateGraph;
        }

        private bool NoResourcesFound()
        {
            return this.Map.listerThings.AllThings.Where(x =>x.Position.InHorDistOf(this.Ship.Position, 15f)).Where(x => x is ResourcePack && (x as ResourcePack).compResource.IsTribute).Count() == 0;
            
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<ShipBase>(ref this.Ship, "Ship");
        }
    }
}
