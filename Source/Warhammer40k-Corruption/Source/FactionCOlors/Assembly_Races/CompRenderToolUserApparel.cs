using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class CompRenderToolUserApparel : ThingComp
    {
        public bool RenderApparel = true;

        private Apparel app;

        private Pawn Wearer;

        private PawnRenderer renderer;

        private bool DoRender;

        public override void PostSpawnSetup(bool respawnAfterLoad)
        {
            base.PostSpawnSetup(respawnAfterLoad);
            if ((this.app = this.parent as Apparel) != null)
            {
                if ((this.Wearer = app.Wearer) != null)
                {
                    if((this.renderer = Wearer.Drawer.renderer) == null)
                    {
                        Log.Error("No PawnRenderer for :" + this.Wearer.ToString());
                    }
                    else
                    {
                        this.DoRender = true;
                    }
                }
            }
        }

        private void DrawApparelOnTooluser(PawnRenderer renderer, Vector3 drawLoc, Rot4 bodyFacing, Quaternion quat)
        {
            for (int k = 0; k < renderer.graphics.apparelGraphics.Count; k++)
            {
                ApparelGraphicRecord apparelGraphicRecord = renderer.graphics.apparelGraphics[k];
                Mesh mesh = renderer.graphics.nakedGraphic.MeshAt(bodyFacing);
                Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                material2 = renderer.graphics.flasher.GetDamagedMat(material2);
                GenDraw.DrawMeshNowOrLater(mesh, drawLoc, quat, material2, false);
            }
        }

        public override void PostDraw()
        {
            if (DoRender)
            {
                Vector3 drawLoc = this.Wearer.DrawPos;
                if (this.Wearer.GetPosture() == PawnPosture.Standing)
                {
                    Rot4 bodyFacing = this.Wearer.Rotation;
                    Quaternion quat = Quaternion.identity;
                    DrawApparelOnTooluser(this.renderer, drawLoc, bodyFacing, quat);
                }
            }
        }
    }
}
