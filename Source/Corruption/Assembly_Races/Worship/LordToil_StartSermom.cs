using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Worship
{
    public class LordToil_StartSermom : LordToil
    {
        private Pawn preacher;

        private BuildingAltar altar;

        public LordToil_StartSermom(Pawn preacher, BuildingAltar altar)
        {
            this.preacher = preacher;
            this.altar = altar;
        }

        public override void UpdateAllDuties()
        {
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                Pawn pawn = this.lord.ownedPawns[i];
                PawnDuty pawnDuty = new PawnDuty(DefOfs.C_DutyDefOfs.JoinSermon, preacher, altar);
                pawnDuty.maxDanger = Danger.Some;
                pawn.mindState.duty = pawnDuty;
            }
        }
        

    }
}
