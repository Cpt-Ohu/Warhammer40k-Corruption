using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class IncidentWorker_VisitingTauEnvoy : IncidentWorker_WanderingTrader
    {
        protected override IoMChatType ChatType
        {
            get
            {
                return IoMChatType.ConvertTau;
            }
        }

        protected override PawnKindDef PawnKind
        {
            get
            {
                return DefOfs.C_PawnKindDefOf.TauPorUi;
            }
        }

        protected override TraderKindDef TraderKind
        {
            get
            {
                return DefOfs.C_TraderKindDefs.Visitor_TauEnvoy;
            }
        }
    }
}
