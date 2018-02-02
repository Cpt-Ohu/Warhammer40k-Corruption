using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class SoulTraitDef : Def
    {
        public List<SoulTraitDegreeData> SDegreeDatas;

        public List<ThoughtDef> EnablesThoughts;

        public List<ThoughtDef> NullifiesThoughts = new List<ThoughtDef>();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
        }

        public SoulTraitDegreeData SDegreeDataAt(int deg)
        {
            for (int i = 0; i < this.SDegreeDatas.Count; i++)
            {
                if (this.SDegreeDatas[i].degree == deg)
                {
                    return this.SDegreeDatas[i];
                }
            }
            Log.Error(string.Concat(new object[]
            {
        this.defName,
        " found no soul data at degree ",
        deg,
        ", returning first defined."
            }));
            return this.SDegreeDatas[0];
        }

    }
}
