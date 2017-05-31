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
    public class LordJob_IntrusiveWanderer : LordJob_VisitColony
    {
        private IntVec3 chillSpot;

        private Pawn centralPawn;

        private bool isLonePawn
        {
            get
            {
                if (this.lord.ownedPawns.Count < 2)
                {
                    return true;
                }
                return false;
            }
        }

        private IoMChatType chatType;

        public bool isOfficialMission;

        public bool InquisitorFoundHeretic;

        public Faction factionInt;

        public LordJob_IntrusiveWanderer()
        {
        }

        public LordJob_IntrusiveWanderer(IntVec3 chillSpot, Pawn centralPawn, IoMChatType chatType)
        {
            this.chillSpot = chillSpot;
            this.centralPawn = centralPawn;
            this.chatType = chatType;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Travel(this.chillSpot).CreateGraph()).StartingToil;
            stateGraph.StartingToil = startingToil;
            var lordToil_WanderAndChat = IoM_StoryUtilities.GetWandererChatToil(this.chatType);
            stateGraph.AddToil(lordToil_WanderAndChat);
            LordToil_TakeWoundedGuest lordToil_TakeWoundedGuest = new LordToil_TakeWoundedGuest();
            stateGraph.AddToil(lordToil_TakeWoundedGuest);
            StateGraph stateGraph2 = new LordJob_TravelAndExit(IntVec3.Invalid).CreateGraph();
            LordToil startingToil2 = stateGraph.AttachSubgraph(stateGraph2).StartingToil;
            LordToil target = stateGraph2.lordToils[1];
            LordToil_ExitMapAndEscortCarriers lordToil_ExitMapBest = new LordToil_ExitMapAndEscortCarriers();
            stateGraph.AddToil(lordToil_ExitMapBest);
            Transition transition = new Transition(startingToil, lordToil_ExitMapBest);
            transition.AddSources(new LordToil[]
            {
                lordToil_WanderAndChat,
                lordToil_TakeWoundedGuest
            });
            transition.AddSources(stateGraph2.lordToils);
            transition.AddTrigger(new Trigger_PawnCannotReachMapEdge());
            string leavingMessage;
            if (this.isLonePawn)
            {
                leavingMessage = "MessageLoneWandererLeaving".Translate(new object[] { this.centralPawn.Name });
            }
            else
            {
                leavingMessage = "MessageGroupWandererLeaving".Translate();
            }

            transition.AddPreAction(new TransitionAction_Message(leavingMessage));
            stateGraph.AddTransition(transition);
            Transition transition2 = new Transition(lordToil_ExitMapBest, startingToil2);
            transition2.AddTrigger(new Trigger_PawnCanReachMapEdge());
            transition2.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
            transition2.AddPostAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition2);
            Transition transition3 = new Transition(startingToil, lordToil_WanderAndChat);
            transition3.AddTrigger(new Trigger_Memo("TravelArrived"));
            stateGraph.AddTransition(transition3);
            Transition transition4 = new Transition(lordToil_WanderAndChat, lordToil_TakeWoundedGuest);
            transition4.AddTrigger(new Trigger_WoundedGuestPresent());

            stateGraph.AddTransition(transition4);
            Transition transition5 = new Transition(lordToil_WanderAndChat, target);
            transition5.AddSources(new LordToil[]
            {
                lordToil_TakeWoundedGuest,
                startingToil
            });
            transition5.AddTrigger(new Trigger_PawnLostViolently());
            transition5.AddPreAction(new TransitionAction_Custom(new Action(delegate { this.SwitchToHostileFaction();})));
            transition5.AddPreAction(new TransitionAction_WakeAll());
            transition5.AddPreAction(new TransitionAction_SetDefendLocalGroup());
            transition5.AddPreAction(new TransitionAction_EndAllJobs());
            stateGraph.AddTransition(transition5);
            Transition transition6 = new Transition(lordToil_WanderAndChat, startingToil2);
            transition6.AddTrigger(new Trigger_TicksPassed(Rand.Range(8000, 22000)));
            transition6.AddPreAction(new TransitionAction_Message(leavingMessage));
            transition6.AddPreAction(new TransitionAction_WakeAll());
            transition6.AddPreAction(new TransitionAction_EnsureHaveExitDestination());
            stateGraph.AddTransition(transition6);

            LordToil_AssaultColony lordtoilAssault = new LordToil_AssaultColony();
            stateGraph.AddToil(lordtoilAssault);
            lordtoilAssault.avoidGridMode = AvoidGridMode.Smart;
            Transition transition7 = new Transition(lordToil_WanderAndChat, lordtoilAssault);
            transition7.AddPreAction(new TransitionAction_Custom(new Action(delegate { this.factionInt.SetHostileTo(Faction.OfPlayer, true); })));
            transition7.AddTrigger(new Trigger_Custom((TriggerSignal x) => this.InquisitorFoundHeretic && IoM_StoryUtilities.InquisitorShouldStartDirectAttack(this.Map, this.centralPawn.GetLord().ownedPawns)));
            stateGraph.AddTransition(transition7);

            LordToil_PanicFlee lordToilFlee = new LordToil_PanicFlee();
            Transition transition8 = new Transition(lordToil_WanderAndChat, lordToilFlee);
            transition8.AddPreAction(new TransitionAction_Custom(new Action(delegate { this.factionInt.SetHostileTo(Faction.OfPlayer, true); })));
            transition8.AddTrigger(new Trigger_Custom((TriggerSignal x) => this.InquisitorFoundHeretic && !IoM_StoryUtilities.InquisitorShouldStartDirectAttack(this.Map, this.centralPawn.GetLord().ownedPawns)));
            stateGraph.AddTransition(transition8);
            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Pawn>(ref this.centralPawn, "centralPawn", false);
            Scribe_Values.Look<IntVec3>(ref this.chillSpot, "chillSpot", default(IntVec3), false);
            Scribe_Values.Look<IoMChatType>(ref this.chatType, "chatType", IoMChatType.SimpleChat, false);
            Scribe_Values.Look<bool>(ref this.InquisitorFoundHeretic, "InquisitorFoundHeretic", false, false);
            Scribe_Values.Look<bool>(ref this.isOfficialMission, "isOfficialMission", false, false);
            Scribe_References.Look<Faction>(ref this.factionInt, "factionInt", false);
        }

        private void SwitchToHostileFaction()
        {
            if (!isOfficialMission)
            {
                for (int i = 0; i < this.lord.ownedPawns.Count; i++)
                {
                    this.lord.ownedPawns[i].SetFactionDirect(Find.FactionManager.FirstFactionOfDef(FactionDefOf.SpacerHostile));
                }
            }
            else
            {
                this.factionInt.SetHostileTo(Faction.OfPlayer, true);
            }

        }
    }
    
}
