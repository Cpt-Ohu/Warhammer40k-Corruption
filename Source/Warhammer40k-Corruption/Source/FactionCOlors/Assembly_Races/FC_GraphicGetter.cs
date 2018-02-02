using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    public static class FC_GraphicGetter
    {
        public static Graphic GetFCGraphic(string textpath, Shader shader, Vector2 drawSize, Color col1, Color col2)
        {
            return GraphicDatabase.Get<Graphic_Single>(textpath, ShaderDatabase.CutoutComplex, drawSize, col1, col2);
        }
    }
}
