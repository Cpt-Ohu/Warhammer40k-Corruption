using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Corruption;

namespace FactionColors
{
    public class FactionDefUniform : FactionDef
    {
        public Color FactionColor1 = Color.white;
        public Color FactionColor2 = Color.black;
        public ChaosGods PreferredChaosGod = ChaosGods.Undivided;


        public List<Subfaction> Subfactions;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (Subfactions != null)
            {
                Subfaction SubFac = Subfactions.RandomElementByWeight((Subfaction sub2) => sub2.weight);
                this.fixedName = SubFac.SubfactionName;
                this.description = SubFac.SubfactionDescription;
                this.FactionColor1 = SubFac.SubfactionColor1;
                this.FactionColor2 = SubFac.SubfactionColor2;
                if (SubFac.SubfactionPawnGroupMakers != null) this.pawnGroupMakers = SubFac.SubfactionPawnGroupMakers;
                this.PreferredChaosGod = SubFac.SubfactionPreferredChaosGod;
                if (SubFac.SubfactionNameMaker != null) this.pawnNameMaker = SubFac.SubfactionNameMaker;
            }

        }
    }
}
