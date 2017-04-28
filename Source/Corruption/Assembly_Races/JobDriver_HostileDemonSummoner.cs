using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;
using Corruption.DefOfs;

namespace Corruption
{
    public class JobDriver_Summoning : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            IntVec3 centercell;
            if (!DropCellFinder.TryFindDropSpotNear(this.pawn.Position, this.Map, out centercell, true, false))
            {
                yield break;
            }
            else
            {
                yield return Toils_Goto.GotoCell(centercell, PathEndMode.OnCell);
                Toil summoning = new Toil();
                summoning.defaultCompleteMode = ToilCompleteMode.Delay;
                summoning.defaultDuration = 10000;
                summoning.tickAction = delegate
                {
                    List<Pawn> list = this.Map.mapPawns.AllPawnsSpawned.FindAll(x => x.CurJob.def == C_JobDefOf.SummoningTribute);
                    if (list.Count <= 0)
                    {
                        this.EndJobWith(JobCondition.InterruptForced);
                    }
                };
                yield return summoning;

                this.AddFinishAction(delegate
                {
                    DemonUtilities.FinishedSummoningRitual(this.pawn);



                });
            }
        }
    }
}
