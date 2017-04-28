using Corruption.Ships;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.IoM
{
    public class LordJob_ArrestGovernor : LordJob
    {

        public LordJob_ArrestGovernor(ShipBase ship, IntVec3 center)
        {
            this.ship = ship;
            this.baseCenter = center;
        }
        
        public ShipBase ship;

        private IntVec3 baseCenter;

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil lordToil_wait = new LordToil_DefendPoint(baseCenter, 50f);
            lordToil_wait.AddFailCondition(() => CorruptionStoryTrackerUtilities.currentStoryTracker.PlanetaryGovernor.Dead);
            stateGraph.AddToil(lordToil_wait);
            Log.Message(ship.Position.ToString());

            LordToil lordToil_leaveInShip = new LordToil_LeaveInShip();

            LordToil lordToil_leaveMap = new LordToil_ExitMapBest(LocomotionUrgency.Jog);

            Transition leaveMapEarly = new Transition(lordToil_leaveInShip, lordToil_leaveMap);
            leaveMapEarly.AddTrigger(new Trigger_Custom((TriggerSignal x) => ship.Destroyed || !ship.Spawned));

            stateGraph.AddTransition(leaveMapEarly);
            
            LordToil lordToil_main = new LordToil_ArrestGovernor();

            stateGraph.AddToil(lordToil_main);

            Transition governorDiedWaiting = new Transition(lordToil_wait, lordToil_leaveInShip);
            governorDiedWaiting.AddTrigger(new Trigger_Custom((TriggerSignal x) => CorruptionStoryTrackerUtilities.currentStoryTracker.PlanetaryGovernor.Dead));
            
            stateGraph.AddTransition(governorDiedWaiting);

            Transition transition_fetchingGovernor = new Transition(lordToil_wait, lordToil_main);
            transition_fetchingGovernor.AddTrigger(new Trigger_TicksPassed(2500));
            transition_fetchingGovernor.AddPreAction(new TransitionAction_Message("MessageGovernorIsBeingArrested".Translate(new object[]
            {
                CorruptionStoryTrackerUtilities.currentStoryTracker.PlanetaryGovernor.Name
            })));
            stateGraph.AddTransition(transition_fetchingGovernor);
            
            LordToil lordToil_goAggressive = new LordToil_AssaultColony();
            lordToil_goAggressive.avoidGridMode = AvoidGridMode.Smart;

            stateGraph.AddToil(lordToil_goAggressive);

            Transition transition_useForce = new Transition(lordToil_main, lordToil_goAggressive);
            transition_useForce.AddTrigger(new Trigger_BecameColonyEnemy());
            transition_useForce.AddTrigger(new Trigger_PawnHarmed());
            transition_useForce.AddPreAction(new TransitionAction_Message("MessageGovernorArrestGoneHostile".Translate()));

            transition_useForce.AddPreAction(new TransitionAction_Custom(new Action(delegate
            {
                this.lord.ownedPawns[0].Faction.SetHostileTo(Faction.OfPlayer, true);
            })));

            stateGraph.AddTransition(transition_useForce);

            Transition downedGovernor = new Transition(lordToil_goAggressive, lordToil_main);
            downedGovernor.AddTrigger(new Trigger_Custom((TriggerSignal x) => CorruptionStoryTrackerUtilities.currentStoryTracker.PlanetaryGovernor.Downed));
            stateGraph.AddTransition(downedGovernor);

            Transition killedGovernor = new Transition(lordToil_goAggressive, lordToil_leaveInShip);
            killedGovernor.AddTrigger(new Trigger_Custom((TriggerSignal x) => CorruptionStoryTrackerUtilities.currentStoryTracker.PlanetaryGovernor.Dead));
            stateGraph.AddTransition(killedGovernor);

            Transition transition_leaveWithGovernor = new Transition(lordToil_main, lordToil_leaveInShip);
            transition_leaveWithGovernor.AddTrigger(new Trigger_Custom((TriggerSignal x) => this.ship.GetInnerContainer().Contains(CorruptionStoryTrackerUtilities.currentStoryTracker.PlanetaryGovernor)));

            stateGraph.AddToil(lordToil_leaveInShip);
            stateGraph.AddTransition(transition_leaveWithGovernor);


            Transition transition_leaveUnderFire = new Transition(lordToil_main, lordToil_leaveInShip);
            transition_leaveUnderFire.AddPreAction(new TransitionAction_Custom(new Action(delegate
            {
                this.lord.ownedPawns[0].Faction.SetHostileTo(Faction.OfPlayer, true);
            })));

            stateGraph.AddToil(lordToil_leaveMap);
            LordToil_End lordToil_End = new LordToil_End();
            stateGraph.AddToil(lordToil_End);
            Transition endTransition = new Transition(lordToil_leaveMap, lordToil_End);
            endTransition.AddTrigger(new Trigger_TicksPassed(60000));
            stateGraph.AddTransition(endTransition);

            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.LookReference<ShipBase>(ref this.ship, "ship", false);
            Scribe_Values.LookValue<IntVec3>(ref this.baseCenter, "baseCenter", IntVec3.North);
        }

    }   
}
