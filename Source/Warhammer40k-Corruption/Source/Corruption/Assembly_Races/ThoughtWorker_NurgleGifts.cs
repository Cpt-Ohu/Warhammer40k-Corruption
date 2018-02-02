using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_NurgleGifts : ThoughtWorker
    {

        private List<HediffDef> HediffDefs
        {
            get
            {
                List<HediffDef> tmplist = new List<HediffDef>();
                tmplist.Add(HediffDefOf.WoundInfection);
                tmplist.Add(HediffDefOf.Flu);
                tmplist.Add(HediffDefOf.ToxicBuildup);
                tmplist.Add(HediffDefOf.Malaria);
                tmplist.Add(HediffDefOf.Plague);
                tmplist.Add(C_HediffDefOf.NurglesRot);
                return tmplist;
            }
        }

        private int SumOfGifts;      

        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            foreach(HediffDef hdef in this.HediffDefs)
            {
                if (p.health.hediffSet.hediffs.Any(x => x.def == hdef))
                {
                    SumOfGifts += 1;
                    if (hdef == C_HediffDefOf.NurglesRot)
                    {
                        SumOfGifts += 2;
                    }
                }
            }

            if (SumOfGifts > 3)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            if (SumOfGifts > 1)
            {
                return ThoughtState.ActiveAtStage(1);
            }
            if (SumOfGifts > 0)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            return ThoughtState.Inactive;

        }


    }
}
