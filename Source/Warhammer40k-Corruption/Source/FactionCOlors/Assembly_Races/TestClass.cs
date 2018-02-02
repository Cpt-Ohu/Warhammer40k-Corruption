using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class TestClass : ThingWithComps
    {
        public override Graphic Graphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>(this.def.graphicData.texPath, ShaderDatabase.Cutout);
            }
        }

    }
}
