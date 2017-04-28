using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption
{
    public class ThingComp_SoulItem : ThingComp
    {
        public int LastGainTick;

        public CompProperties_SoulItem cprops
        {
            get
            {
                return (CompProperties_SoulItem)this.props;
            }
        }


        public float amount(SoulItemCategories cat, float res)
        {
            if (cprops.Category == SoulItemCategories.Corruption)
            {
                return -cprops.GainRate / 1333 / res;
            }
            else if (cprops.Category == SoulItemCategories.Redemption)
            {
                return cprops.GainRate / 1333 * res;
            }
            else return 0;
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();

        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.GetComp<CompEquippable>().PrimaryVerb.CasterPawn == null && LastGainTick > Find.TickManager.TicksGame)
            {
                Pawn p = this.parent.GetComp<CompEquippable>().PrimaryVerb.CasterPawn;
                ChaosFollowerPawnKindDef pdef = p.kindDef as ChaosFollowerPawnKindDef;
                Need_Soul n_soul = p.needs.TryGetNeed<Need_Soul>();
                var resistence = pdef.AfflictionProperty.ResolveFactor;
                n_soul.GainNeed(amount(cprops.Category, resistence));
                LastGainTick = Find.TickManager.TicksGame + 120;  
            }
        }
    }
}
