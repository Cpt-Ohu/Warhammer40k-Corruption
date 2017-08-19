using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Domination
{
    public class LordToil_DoBattle : LordToil
    {
        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return false;
            }
        }

        protected LordToilData_DefendPoint Data
        {
            get
            {
                return (LordToilData_DefendPoint)this.data;
            }
        }

        public LordToil_DoBattle(IntVec3 battleCenter)
        {
            this.data = new LordToilData_DefendPoint();
            this.Data.defendPoint = battleCenter;
        }

        public override IntVec3 FlagLoc
        {
            get
            {
                return this.Data.defendPoint;
            }
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DominationDefOfs.DoBattleDuty);
            }
        }
    }
}
