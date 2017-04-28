using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption
{
    public class LordToil_AttendSermon : LordToil
    {

            private IntVec3 spot;

            public LordToil_AttendSermon(IntVec3 spot)
            {
                this.spot = spot;
            }

            public override ThinkTreeDutyHook VoluntaryJoinDutyHookFor(Pawn p)
            {
                return CorruptionDefOfs.Sermon.hook;
            }

            public override void UpdateAllDuties()
            {
                for (int i = 0; i < this.lord.ownedPawns.Count; i++)
                {
                    this.lord.ownedPawns[i].mindState.duty = new PawnDuty(CorruptionDefOfs.Sermon, this.spot, -1f);
                }
            }
        
    }
}
