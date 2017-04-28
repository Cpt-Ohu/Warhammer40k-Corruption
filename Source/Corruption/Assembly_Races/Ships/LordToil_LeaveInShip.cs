using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Ships
{
    public class LordToil_LeaveInShip : LordToil
    {
        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                this.lord.ownedPawns[i].mindState.duty = new PawnDuty(DefOfs.C_DutyDefOfs.LeaveInShip);
            }
        }
    }
}
