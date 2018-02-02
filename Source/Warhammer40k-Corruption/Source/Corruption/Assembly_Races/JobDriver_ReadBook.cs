using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_ReadBookInventory : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn Reader = this.pawn;
            Need_Soul soul = Reader.needs.TryGetNeed<Need_Soul>();
            CompPsyker compPsyker = soul.compPsyker;

            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);




        }
    }
}
