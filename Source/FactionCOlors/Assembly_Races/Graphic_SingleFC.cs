using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class Graphic_SingleFC : Graphic_Single
    {
        public Shader shader;

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            //return base.GetColoredVersion(newShader, newColor, newColorTwo);
            return GraphicDatabase.Get<Graphic_Single>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data);
            //return base.GetColoredVersion(newShader, newColor, newColorTwo);
        }
    }
}
