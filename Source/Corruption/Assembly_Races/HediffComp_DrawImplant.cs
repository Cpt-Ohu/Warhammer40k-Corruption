using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class HediffComp_DrawImplant : HediffComp
    {
        public HediffCompProperties_DrawImplant implantDrawProps
        {
            get
            {
                return this.props as HediffCompProperties_DrawImplant;
            }
        }
        
        public Material ImplantMaterial(Pawn pawn, Rot4 bodyFacing)
        {
            string path;
            if (this.implantDrawProps.implantDrawerType == ImplantDrawerType.Head)
            {
                path = implantDrawProps.implantGraphicPath;
            }
            else
            {
                path = implantDrawProps.implantGraphicPath + "_" + pawn.story.bodyType.ToString();
            }
            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, Vector2.one, Color.white).MatAt(bodyFacing);
        }

    }
}
