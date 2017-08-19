using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Domination
{
    public class LordToil_GoIntoBattle : LordToil
    {
        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return false;
            }
        }

        protected LordToilData_Travel Data
        {
            get
            {
                return (LordToilData_Travel)this.data;
            }
        }

        public LordToil_GoIntoBattle(IntVec3 battleCenter)
        {
            this.data = new LordToilData_Travel();
            Log.Message(battleCenter.ToString());
            this.Data.dest = battleCenter;
        }

        public override IntVec3 FlagLoc
        {
            get
            {
                return this.Data.dest;
            }
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                PawnDuty pawnDuty = new PawnDuty(DominationDefOfs.GoIntoBattleDuty, this.Data.dest);
                pawnDuty.maxDanger = Danger.Deadly;
                this.lord.ownedPawns[i].mindState.duty = pawnDuty; 
            }
        }
    }
}
