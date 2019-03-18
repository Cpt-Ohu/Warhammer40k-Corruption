using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class SoulTrait : IExposable
    {
        public SoulTraitDef SDef;

        public List<SoulTraitDegreeData> SoulDatas;

        private int sdegree = 0;

        public int SDegree
        {
            get
            {
                return this.sdegree;
            }
        }

        public static SoulTraitDef Named(string defName)
        {
            return DefDatabase<SoulTraitDef>.GetNamed(defName, true);
        }

        public SoulTraitDegreeData SoulCurrentData
        {
            get
            {
                if (this.SDef == null) Log.Message("No SDef?");
                return this.SDef.SDegreeDataAt(SDegree);
            }
        }

        public SoulTrait()
        {
        }

        public  SoulTrait(SoulTraitDef traitdef, int newdeg)
        {
            this.sdegree = newdeg;
            this.SDef = traitdef;
            this.NullifiedThoughtsInt = SDef.NullifiesThoughts;
        }


        private List<ThoughtDef> NullifiedThoughtsInt;

        public List<ThoughtDef> NullifiedThoughts
        {
            get
            {
                return this.NullifiedThoughtsInt;
            }
        }

        public string TipString(Pawn pawn)
        {
            StringBuilder stringBuilder = new StringBuilder();
            TraitDegreeData currentData = this.SoulCurrentData;
            stringBuilder.Append(currentData.description.AdjustedFor(pawn));
            return stringBuilder.ToString();
        }

        public void ExposeData()
        {
            Scribe_Defs.Look<SoulTraitDef>(ref this.SDef, "SDef");
            Scribe_Collections.Look<ThoughtDef>(ref this.NullifiedThoughtsInt, "NullifiedThoughtsInt", LookMode.Def, new object[0]);
            Scribe_Values.Look<int>(ref this.sdegree, "sdegree", 0, true);
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && this.SDef == null)
            {
                this.SDef = DefDatabase<SoulTraitDef>.GetRandom();
                this.sdegree =  this.SDef.SDegreeDatas.RandomElementByWeight((SoulTraitDegreeData dd) => dd.commonality).degree;
            }
        }
    }
}
