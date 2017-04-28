using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public static class ApparelGraphicGetterFC
    {

        public static bool TryGetGraphicApparelModded(Apparel apparel, BodyType bodyType, out ApparelGraphicRecord rec)
        {
            if (bodyType == BodyType.Undefined)
            {
                Log.Error("Getting apparel graphic with undefined body type.");
                bodyType = BodyType.Male;
            }
            if (apparel.def.apparel.wornGraphicPath.NullOrEmpty())
            {
                rec = new ApparelGraphicRecord(null, null);
                return false;
            }
            string path;
            if (apparel.def.apparel.LastLayer == ApparelLayer.Overhead)
            {
                path = apparel.def.apparel.wornGraphicPath;
            }
            else
            {
                path = apparel.def.apparel.wornGraphicPath + "_" + bodyType.ToString();
            }

            Graphic graphic = new Graphic();
            graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, apparel.def.graphicData.drawSize, apparel.DrawColor, apparel.DrawColorTwo);
     //       Log.Message(apparel.DrawColor.ToString());
     //       Log.Message(apparel.DrawColorTwo.ToString());
            rec = new ApparelGraphicRecord(graphic, apparel);
            return true;

        }
    }
}
