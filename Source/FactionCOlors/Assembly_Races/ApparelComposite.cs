using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class ApparelComposite : Apparel
    {
        private Color col = Color.white;

        public override Color DrawColorTwo
        {
            get
            {
                CompFactionColor fcomp;
                if ((fcomp = this.GetComp<CompFactionColor>()) != null)
                {
                    if (fcomp.CProps.TryUseSecondaryStuffColors)
                    {
                        col = GetSecondaryStuffColor();
                    }
                    else if (fcomp.CProps.UseSecondaryColors)
                    {
                        col = this.GetComp<CompFactionColor>().SecondaryColor;
                        return col;
                    }
                    return col;
                }
                return col;
            }
        }

        private  Color GetSecondaryStuffColor()
        {
            List<ThingCountClass> list = this.def.costList;
            for (int i=0; i < list.Count; i++)
            {
                if(list[i].thingDef.IsStuff)
                {
                    return list[i].thingDef.stuffProps.color;
                }
            }
            return Color.gray;
        }

        public override Graphic Graphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>(this.def.graphicData.texPath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Color>(ref this.col, "col", Color.white, false);
        }
    }
}
