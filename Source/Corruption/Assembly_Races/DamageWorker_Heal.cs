﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{

    public class DamageWorker_HealMiracle : DamageWorker
    {
        public override float Apply(DamageInfo dinfo, Thing thing)
        {
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                IEnumerable<Hediff> source = pawn.health.hediffSet.GetHediffs<Hediff>();
                if (source != null)
                {
                    HediffDef globalDef = source.RandomElement<Hediff>().def;
                    Hediff hediff = (from x in pawn.health.hediffSet.hediffs
                                     where x.def == globalDef
                                     select x).FirstOrDefault<Hediff>();
                    if (hediff != null)
                    {
                        hediff.Severity -= dinfo.Amount;
                    }
                }
            }
            return 0f;
        }
    }
}
