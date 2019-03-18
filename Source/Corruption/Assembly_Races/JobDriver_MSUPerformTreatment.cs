using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace Corruption
{
    public class JobDriver_MSUPerformTreatment : JobDriver
    {
        public Building_MechanicusMedTable MSU
        {
            get
            {
                return this.TargetA.Thing as Building_MechanicusMedTable;
            }
        
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            throw new NotImplementedException();
        }
    }
}
