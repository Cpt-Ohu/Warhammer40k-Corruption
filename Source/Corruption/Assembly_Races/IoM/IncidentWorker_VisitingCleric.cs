using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace Corruption.IoM
{
    public class IncidentWorker_VisitingHealer : IncidentWorker_NeutralGroup
    {

        protected IoMChatType ChatType = IoMChatType.VisitingHealer;

        protected PawnKindDef healerDef = DefOfs.C_PawnKindDefOf.SororitasNurse;
        
        protected override void ResolveParmsPoints(IncidentParms parms)
        {
            base.ResolveParmsPoints(parms);
            parms.faction = CorruptionStoryTrackerUtilities.currentStoryTracker.AdeptusSororitas;            
        }

        public override bool TryExecute(IncidentParms parms)
        {
            this.ResolveParmsPoints(parms);
            Map map = (Map)parms.target;
            if (!this.TryResolveParmsGeneral(parms))
            {
                return false;
            }

            Pawn pawn = null;
            if (IoM.IoM_StoryUtilities.GenerateIntrusiveWanderer(map, healerDef, parms.faction, ChatType, "IoM_HealerArrives", out pawn))
            {
                return true;
            }

            return false;
        }

    }
}
