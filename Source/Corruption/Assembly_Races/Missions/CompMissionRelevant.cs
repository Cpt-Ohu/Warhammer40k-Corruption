using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class CompMissionRelevant : ThingComp
    {
        public CompProperties_MissionRelevant Props
        {
            get
            {
                return this.props as CompProperties_MissionRelevant;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                var mapParent = Find.WorldObjects.MapParentAt(this.parent.Map.Tile);
                CFind.MissionManager.TryFinishMissionForTarget(mapParent, this.parent);                
            }
        }
    }
}
