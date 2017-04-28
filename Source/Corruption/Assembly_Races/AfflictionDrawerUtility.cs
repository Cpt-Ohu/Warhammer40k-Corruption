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
        
        public static bool TryGetChaosOverlayGraphics(Pawn pawn, out Graphic headgraphic, out Graphic bodygraphic)
        {
            if (pawn.needs != null && pawn.story != null && !pawn.kindDef.factionLeader && pawn.Drawer.renderer.graphics.AllResolved)
            {
                Need_Soul soul = pawn.needs.TryGetNeed<Need_Soul>();
                if (soul != null && !soul.NoPatron && soul.patronInfo.PatronName != "Slaanesh")
                {
                    headgraphic = AfflictionDrawerUtility.GetHeadGraphic(pawn, soul.patronInfo.PatronName);
                    bodygraphic = AfflictionDrawerUtility.GetBodyOverlay(pawn.story.bodyType, soul.patronInfo.PatronName);
                    return true;
                }
            }

            headgraphic = null;
            bodygraphic = null;
            return false;
        }

                
        public static void DrawHeadOverlay(Pawn pawn, string patronName)
        {
            Graphic headmark = AfflictionDrawerUtility.GetHeadGraphic(pawn, patronName);
            Rot4 rot = pawn.Rotation;
            Quaternion quat = pawn.Rotation.AsQuat;
            Vector3 b = quat * pawn.Drawer.renderer.BaseHeadOffsetAt(rot);
            b.y += 0.01f;
            Mesh mesh4 = headmark.MeshAt(rot);
            Material mat2 = headmark.MatAt(rot);
            GenDraw.DrawMeshNowOrLater(mesh4, pawn.DrawPos + b, quat, mat2, false);
        }

        private static ThingDef GetOverlayDef(string patronName)
        {
            switch(patronName)
            {
                case "Undivided":
                    {
                        return C_ThingDefOfs.Overlay_Undivided;
                    }
                case "Khorne":
                    {
                        return C_ThingDefOfs.Overlay_Khorne;
                    }
                case "Nurgle":
                    {
                        return C_ThingDefOfs.Overlay_Nurgle;
                    }
                case "Tzeentch":
                    {
                        return C_ThingDefOfs.Overlay_Tzeentch;
                    }
            }

            return C_ThingDefOfs.Overlay_Undivided;

        }
        
    }
}
