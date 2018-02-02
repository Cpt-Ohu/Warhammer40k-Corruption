using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public static class AfflictionDrawerUtility
    {
        public static Graphic GetHeadGraphic(Pawn p, string patronname)
        {
            string crownType;
            if (p.story == null)
            {
                crownType = "Head_Average";
            }

            if (p.story.HeadGraphicPath.Contains("Average"))
            {
                crownType = "Head_Average";
            }
            else if (p.story.HeadGraphicPath.Contains("Narrow"))
            {
                crownType = "Head_Narrow";
            }
            else
            {
                Log.Error("Found no CrownType, returning to average");
                crownType = "Head_Average";
            }

            string path = "Things/Chaos/BodyOverlays/" + patronname + "_" + crownType;

            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Mote, Vector2.one, Color.white);
        }

        public static Graphic GetBodyOverlay(BodyType bodyType, string patronname)
        {
            if (bodyType == BodyType.Undefined)
            {
                bodyType = BodyType.Male;
            }
            string str = patronname + "_" + bodyType.ToString();
            string path = "Things/Chaos/BodyOverlays/" + str;
            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, Vector2.one, Color.white);
        }       
        
    }
}
