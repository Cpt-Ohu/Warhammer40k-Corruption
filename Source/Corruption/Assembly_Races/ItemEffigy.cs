using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class ItemEffigy : ThingWithComps
    {
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(selPawn).GetEnumerator();
            while (enumerator.MoveNext())
            {
                FloatMenuOption current = enumerator.Current;
                yield return current;
            }
            if (!selPawn.CanReserve(this, 1))
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption;
            }
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
            {
                FloatMenuOption floatMenuOption2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield return floatMenuOption2;
            }
            string label = "JobTakeEffigy".Translate(new object[] { this.Label });
            Action action = delegate
            {
                Job job = new Job(JobDefOf.TakeInventory, this);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job);
            };
            yield return new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null);

        }
    }
    
}
