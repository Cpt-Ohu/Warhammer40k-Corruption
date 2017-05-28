using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI.Group;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class LordToil_WanderAndChat : LordToil_DefendPoint
    {
        private DutyDef dutyDef;

        public LordToil_WanderAndChat(DutyDef def)
        {
            this.dutyDef = def;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(dutyDef);
            }
        }
        
    }
}
