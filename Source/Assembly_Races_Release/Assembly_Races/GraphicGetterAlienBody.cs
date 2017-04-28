using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlienRace
{
    class GraphicGetterAlienBody
    {
        public static Graphic GetNakedBodyGraphicAlien(BodyType bodyType, Shader shader, Color skinColor, string userpath, Vector2 drawsize)
        {

            if (bodyType == BodyType.Undefined)
            {
                Log.Error("Getting naked body graphic with undefined body type.");
                bodyType = BodyType.Male;
            }

                string str = "Naked_" + bodyType.ToString();
            
            string path = userpath + str;
            return GraphicDatabase.Get<Graphic_Multi>(path, shader, drawsize, skinColor);

        }
    }
}
