using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class BuildingWorshipDef : RoomRoleDef
    {
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (this.workerClass.GetType() == typeof(RoomRoleWorker_Church))
            {

            }

        }
    }
}
