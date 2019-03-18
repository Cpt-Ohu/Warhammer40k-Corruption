using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class IncidentWorker_VisitingCultist : IncidentWorker_WanderingTrader
    {
        protected override IoMChatType ChatType
        {
            get
            {
                return IoMChatType.ConvertChaos;
            }
        }

        protected override PawnKindDef PawnKind
        {
            get
            {
                return DefOfs.C_PawnKindDefOf.CultistVisitor;
            }
        }

        protected override TraderKindDef TraderKind
        {
            get
            {
                return DefOfs.C_TraderKindDefs.CultistVisitor;
            }
        }

        protected override float SocialSkillBoost
        {
            get
            {
                return 80000f;
            }
        }

    }
}
