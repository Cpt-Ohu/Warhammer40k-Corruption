﻿using System;
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

        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            throw new NotImplementedException();
        }
    }
}
