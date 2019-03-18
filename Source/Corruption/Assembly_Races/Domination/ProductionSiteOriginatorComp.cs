using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Corruption.Domination
{
    public class PioneerComp : WorldObjectComp
    {

        private bool isPlayerSubjugate
        {
            get
            {
                return CFind.StoryTracker.DominationTracker.GetAllianceOfFaction(this.parent.Faction).LeadingFaction == Faction.OfPlayer;
            }
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (CorruptionModSettings.AllowDomination)
            {
                yield return new Command_Action()
                {
                    defaultDesc = "OrderSendPioneersDesc".Translate(),
                    defaultLabel = "OrderSendPioneers".Translate()
                };
            }
            else
            {
                foreach (var gizmo in base.GetGizmos())
                {
                    yield return gizmo;
                }
            }

        }
    }
}
