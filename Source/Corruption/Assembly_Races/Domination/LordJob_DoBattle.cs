using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Domination
{
    public class LordJob_DoBattle : LordJob
    {
        private IntVec3 point;

        public LordJob_DoBattle(IntVec3 centerPoint)
        {
            this.point = centerPoint;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();

            LordToil_GoIntoBattle goToil = new LordToil_GoIntoBattle(this.point);
            stateGraph.AddToil(goToil);
            LordToil_DoBattle battleToil = new LordToil_DoBattle(this.point);
            stateGraph.AddToil(battleToil);

            Transition StartBattle = new Transition(goToil, battleToil);
            //StartBattle.AddPreAction(new TransitionAction_Custom(new Action(delegate { battleToil.ResetDefentPoint(this.NewBattleCenter()); })));
            StartBattle.AddPostAction(new TransitionAction_EndAllJobs());
            StartBattle.AddTrigger(new Trigger_PawnHarmed(1f, true));
            StartBattle.AddTrigger(new Trigger_PawnLostViolently());
            StartBattle.AddTrigger(new Trigger_Custom((TriggerSignal x) => inBattleDistance()));
            StartBattle.AddPreAction(new TransitionAction_Custom(new Action(delegate { Log.Message("Starting Battle"); })));
            stateGraph.AddTransition(StartBattle);

            LordToil_End end = new LordToil_End();
            stateGraph.AddToil(end);
            Transition EndBattle = new Transition(battleToil, end);
            return stateGraph;
        }

        private bool inBattleDistance()
        {
            if (this.lord.ownedPawns.Any(x => GenAI.EnemyIsNear(x, 30f)))
            {
                return true;
            }

            return false;
        }

        public IntVec3 NewBattleCenter()
        {
            IntVec3 curPos = this.lord.ownedPawns.RandomElement().Position;
            return curPos;
        }
    
    }
}
