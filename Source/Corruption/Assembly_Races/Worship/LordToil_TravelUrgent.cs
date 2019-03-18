using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.Worship
{
    public class LordToil_TravelUrgent : LordToil
    {
        public Danger maxDanger = Danger.Some;

        public override IntVec3 FlagLoc
        {
            get
            {
                return this.Data.dest;
            }
        }

        private LordToilData_Travel Data
        {
            get
            {
                return (LordToilData_Travel)this.data;
            }
        }

        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return false;
            }
        }

        protected virtual float AllArrivedCheckRadius
        {
            get
            {
                return 15f;
            }
        }

        public LordToil_TravelUrgent(IntVec3 dest)
        {
            this.data = new LordToilData_Travel();
            this.Data.dest = dest;
        }

        public override void UpdateAllDuties()
        {
            LordToilData_Travel data = this.Data;
            for (int i = 0; i < this.lord.ownedPawns.Count; i++)
            {
                PawnDuty pawnDuty = new PawnDuty(DefOfs.C_DutyDefOfs.TravelUrgent, data.dest, -1f);
                pawnDuty.maxDanger = this.maxDanger;
                this.lord.ownedPawns[i].mindState.duty = pawnDuty;
            }
        }

        public override void LordToilTick()
        {
            if (Find.TickManager.TicksGame % 205 == 0)
            {
                LordToilData_Travel data = this.Data;
                bool flag = true;
                int countArrived = 0;
                for (int i = 0; i < this.lord.ownedPawns.Count; i++)
                {
                    Pawn pawn = this.lord.ownedPawns[i];
                    if (countArrived < (int)(this.lord.ownedPawns.Count * 0.8f) && !pawn.Position.InHorDistOf(data.dest, this.AllArrivedCheckRadius) || !pawn.CanReach(data.dest, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                    {
                        flag = false;
                        break;
                    }
                    else
                    {
                        countArrived++;
                    }
                }
                if (flag)
                {
                    this.lord.ReceiveMemo("TravelArrived");
                }
            }
        }

        public bool HasDestination()
        {
            return this.Data.dest.IsValid;
        }

        public void SetDestination(IntVec3 dest)
        {
            this.Data.dest = dest;
        }
    }
}
