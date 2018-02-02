using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using RimWorld;

namespace Corruption
{

    [StaticConstructorOnStartup]
    public class AfflictionDrawer
    {
        public Graphic HeadGraphic;

        public Graphic BodyGraphic;

        private string crownType;        

        public BodyType btype;

        public Vector3 bodydrawpos = new Vector3(0f, 0f, 0f);

        public Vector3 headvec = new Vector3(0f, 0f, 0f);

        public Vector3 headdrawpos = new Vector3(0f, 0f, 0f);

        private Vector2 drawSize = new Vector2(1f, 1f);

        private Mesh headMesh;

        private Mesh bodyMesh = MeshPool.plane10;

        private Material headMat;

        private Material bodyMat;

        private float num = 0;

        private Quaternion quat;

        public void DrawOverlays(Pawn p, string PatronName)
        {
            bodyMesh = MeshPool.plane10;


            if (p.story == null)
            {
                btype = BodyType.Undefined;
            }

            this.HeadGraphic = GetHeadGraphic(p, PatronName);
            this.BodyGraphic = GetBodyOverlay(btype, PatronName);


            bodydrawpos = p.Drawer.DrawPos;
            if (p.Drawer != null && p.Rotation == null)
            {
                 headvec = p.Drawer.renderer.BaseHeadOffsetAt(p.Rotation);
            }
            else headvec = new Vector3(0f, 0f, 0f);
            headdrawpos = bodydrawpos + headvec;

            headMat = this.HeadGraphic.MatAt(p.Rotation);
            bodyMat = this.BodyGraphic.MatAt(p.Rotation);

            if (p.Rotation == Rot4.West)
            {
                headMesh = MeshPool.plane10Flip;
                bodyMesh = MeshPool.plane10Flip;
            }
            else
            {
                headMesh = MeshPool.plane10;
                bodyMesh = MeshPool.plane10;
            }

            if (p.Downed || p.Dead)
            {
                headMat = this.HeadGraphic.MatAt(LayingFacingDet(p));
                bodyMat = this.BodyGraphic.MatAt(LayingFacingDet(p));
                if (p.Dead)
                {
                    num = p.corpse.innerPawn.Drawer.renderer.wiggler.downedAngle;
                }
                else
                {
                    num = p.Drawer.renderer.wiggler.downedAngle;
                }

                if (p.thingIDNumber % 4 == 3) headMesh = MeshPool.plane10Flip;
                quat = Quaternion.AngleAxis(num, Vector3.up);
                Vector3 newPos = quat * headvec;
                headdrawpos = p.Drawer.DrawPos + newPos;
                bodydrawpos = p.Drawer.DrawPos;
                if (p.Dead)
                {
                    if (p.Drawer != null && p.corpse != null)
                    {
                        Log.Message(p.corpse.ToString());
                       // Vector3 headvecD = p.Drawer.renderer.BaseHeadOffsetAt(p.corpse.Rotation);
                       // Vector3 newPosD = quat * headvecD;
                       // headdrawpos = p.corpse.DrawPos + newPosD;
                    }
                    quat = Quaternion.AngleAxis(num, Vector3.up);

                    bodydrawpos = p.corpse.DrawPos;
                }
            }

            if (p.CarriedBy != null)
            {
                Log.Message(p.CarriedBy.def.defName.ToString());
                Pawn pepe = p.CarriedBy;
                headdrawpos = pepe.Drawer.DrawPos;
                headdrawpos.y -= 1.5f;
                bodydrawpos = pepe.Drawer.DrawPos;
                bodydrawpos.y -= 1.5f;
            }

            Building_Bed bbed = p.CurrentBed();
            if (bbed != null && p.RaceProps.Humanlike)
            {
                Rot4 rotation = bbed.Rotation;
                num = bbed.Rotation.AsAngle + 180;
                headdrawpos = p.DrawPos + Quaternion.Euler(0, num, 0) * new Vector3(0, 0, 0f);
                if (p.thingIDNumber % 4 == 3) headMesh = MeshPool.plane10Flip;
                headMat = this.HeadGraphic.MatAt(LayingFacingDet(p));
            }

            headdrawpos.y = 30;
            bodydrawpos.y = 30;
            Vector3 s = new Vector3(1.0f, 1.0f, 1.0f);
            Matrix4x4 matrixHead = default(Matrix4x4);
            Matrix4x4 matrixBody = default(Matrix4x4);
            matrixHead.SetTRS(headdrawpos, Quaternion.AngleAxis(num, Vector3.up), s);
            matrixHead.SetTRS(bodydrawpos, Quaternion.AngleAxis(num, Vector3.up), s);
            Graphics.DrawMesh(headMesh, matrixHead, headMat, 0);
            Graphics.DrawMesh(bodyMesh, matrixBody, bodyMat, 0);
            Log.Message("OverlayDrawn");
        }

        public Graphic GetHeadGraphic(Pawn p, string patronname)
        {
            if (p.story != null)
            {
                if (p.story.HeadGraphicPath.Contains("Average"))
                {
                    crownType = "Head_Average";
                }
                else if (p.story.HeadGraphicPath.Contains("Narrow"))
                {
                    crownType = "Head_Average";
                }
                else
                {
                    Log.Error("Found no CrownType, returning to average");
                    crownType = "Head_Average";
                }
            }
            else
            {
                crownType = "Head_Average";
            }
            string path = "Things/Chaos/BodyOverlays/" + patronname + "_" + crownType;


            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one, Color.white);
        }

        public static Graphic GetBodyOverlay(BodyType bodyType, string patronname)
        {
            if (bodyType == BodyType.Undefined)
            {
                bodyType = BodyType.Male;
            }
            string str = patronname + "_" + bodyType.ToString();
            string path = "Things/Chaos/BodyOverlays/" + str;
            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutSkin, Vector2.one, Color.white);
        }







    private Rot4 LayingFacingDet(Pawn DownedWearer)
    {
        if (DownedWearer.GetPosture() == PawnPosture.LayingFaceUp)
        {
            return Rot4.South;
        }
        switch (DownedWearer.thingIDNumber % 4)
        {
            case 0:
                return Rot4.South;
            case 1:
                return Rot4.South;
            case 2:
                return Rot4.East;
            case 3:
                return Rot4.West;
        }
        return Rot4.Random;
    }

}
}
