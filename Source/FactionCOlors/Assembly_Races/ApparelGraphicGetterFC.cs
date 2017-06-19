using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
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
            path = apparel.def.apparel.wornGraphicPath;
            CompFactionColor compF = apparel.TryGetComp<CompFactionColor>();
            if (compF != null)
            {
                if (compF.CProps.IsRandomMultiGraphic)
                {
                    path += "/" + compF.randomGraphicPath + "/" + compF.randomGraphicPath;
                }
            }

            if (apparel.def.apparel.LastLayer != ApparelLayer.Overhead)
            {
                path += "_" + bodyType.ToString();
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
