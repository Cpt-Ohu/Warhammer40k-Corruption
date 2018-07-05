using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class CompAfflictionDrawer : ThingComp
    {
        public  Graphic HeadGraphic;

        public  Graphic BodyGraphic;

        private  string crownType;

        public  BodyType btype;

        public  Vector3 bodydrawpos = new Vector3(0f, 0f, 0f);

        public  Vector3 headvec = new Vector3(0f, 0f, 0f);

        public  Vector3 headdrawpos = new Vector3(0f, 0f, 0f);

        private  Vector2 drawSize = new Vector2(1f, 1f);

        private  Mesh headMesh;

        private  Mesh bodyMesh = MeshPool.plane10;

        private  Material headMat;

        private  Material bodyMat;

        private bool RenderBody = true;

        private  float num = 0;

        private Quaternion quat;

        public Pawn cpawn
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn p = this.parent as Pawn;
            if (p.CarriedBy != null)
            {
                this.PostDraw();
            }
        }

        private string PatronName
        {
            get
            {
                return cpawn.needs.TryGetNeed<Need_Soul>().Patron.label;
            }
        }

        public override void PostDraw()
        {
            if (cpawn.needs.TryGetNeed<Need_Soul>() == null)
            {
                //Log.Message("NoNeed");
                HarmonyPatches.CreateNewSoul(cpawn);
            }
            if (cpawn.needs.TryGetNeed<Need_Soul>().NotCorrupted)
            {
                return;
            }
            if (cpawn.needs.TryGetNeed<Need_Soul>().Patron != PatronDefOf.Slaanesh || cpawn.needs.TryGetNeed<Need_Soul>().Patron != PatronDefOf.ChaosUndivided)
            {
                this.DrawOverlays(PatronName, cpawn);
            }
        }

        public void DrawOverlays(string PatronName, Pawn corrpawn)
        {
            bodyMesh = MeshPool.humanlikeBodySet.MeshAt(corrpawn.Rotation);
            headMesh = MeshPool.humanlikeHeadSet.MeshAt(corrpawn.Rotation);

            if (corrpawn.story == null)
            {
                btype = BodyType.Undefined;
            }
            else btype = corrpawn.story.bodyType;

            HeadGraphic = GetHeadGraphic(corrpawn, PatronName);
            BodyGraphic = GetBodyOverlay(btype, PatronName);

            bodydrawpos = corrpawn.Drawer.DrawPos;

            headvec = corrpawn.Drawer.renderer.BaseHeadOffsetAt(corrpawn.Rotation);

            headdrawpos = bodydrawpos + headvec;

            headMat = HeadGraphic.MatAt(corrpawn.Rotation);
            bodyMat = BodyGraphic.MatAt(corrpawn.Rotation);

            if (corrpawn.Rotation == Rot4.North)
            {
                headMesh = MeshPool.plane03;
                bodyMesh = MeshPool.humanlikeBodySet.MeshAt(corrpawn.Rotation);
            }
            else
            {
                headMesh = MeshPool.humanlikeBodySet.MeshAt(corrpawn.Rotation);
                bodyMesh = MeshPool.humanlikeBodySet.MeshAt(corrpawn.Rotation);
            }

            bodydrawpos.y += 0.0019f;

            if (corrpawn.Rotation == Rot4.North)
            {
                headdrawpos.y += 0.0f;
            }
            else
            {
                headdrawpos.y += 0.051f;
            }
            
            if (corrpawn.Downed || corrpawn.Dead)
            {
                headMat = HeadGraphic.MatAt(LayingFacingDet(corrpawn));
                bodyMat = BodyGraphic.MatAt(LayingFacingDet(corrpawn));
                if (corrpawn.Dead)
                {
                    num = corrpawn.Corpse.InnerPawn.Drawer.renderer.wiggler.downedAngle;
                }
                else
                {
                    num = corrpawn.Drawer.renderer.wiggler.downedAngle;
                }

                if (corrpawn.thingIDNumber % 4 == 3) headMesh = MeshPool.plane10Flip;
                quat = Quaternion.AngleAxis(num, Vector3.up);
                Vector3 newPos = quat * headvec;
                headdrawpos = corrpawn.Drawer.DrawPos + newPos;
                bodydrawpos = corrpawn.Drawer.DrawPos;
                bodydrawpos.y = Altitudes.AltitudeFor(AltitudeLayer.LayingPawn) + 0.001f;
                if (corrpawn.Dead)
                {
                    if (corrpawn.Drawer != null && corrpawn.Corpse != null)
                    {
                         Vector3 headvecD = corrpawn.Drawer.renderer.BaseHeadOffsetAt(corrpawn.Corpse.Rotation);
                         Vector3 newPosD = quat * headvecD;
                         headdrawpos = corrpawn.Corpse.DrawPos + newPosD;
                    }
                    quat = Quaternion.AngleAxis(num, Vector3.up);

                    bodydrawpos = corrpawn.Corpse.DrawPos;

                    bodydrawpos.y -= 0.05f;

                } 
            }
            if (corrpawn.CarriedBy != null)
            {
                Pawn pepe = corrpawn.CarriedBy;

                Vector3 vec = corrpawn.CarriedBy.DrawPos;
                Vector3 newPos = quat * headvec;
                vec.x += 0.5f;
                bodydrawpos = vec;
                headdrawpos = bodydrawpos + newPos;
            }

            Building_Bed bbed = corrpawn.CurrentBed();

            if (bbed != null && corrpawn.RaceProps.Humanlike)
            {
                RenderBody = false;
                Rot4 rotation = bbed.Rotation;
                num = bbed.Rotation.AsAngle;
                AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)bbed.def.altitudeLayer, 15);
                Vector3 a = this.cpawn.Position.ToVector3ShiftedWithAltitude(altLayer);
                float d = -this.cpawn.Drawer.renderer.BaseHeadOffsetAt(Rot4.South).z;
                Vector3 a2 = rotation.FacingCell.ToVector3();
                headdrawpos = a + a2 * d;
                headdrawpos.y += 0.01f;
                headMat = HeadGraphic.MatAt(LayingFacingDet(corrpawn));
            }

            Vector3 s = new Vector3(1.0f, 1.0f, 1.0f);
            Matrix4x4 matrixHead = default(Matrix4x4);
            Matrix4x4 matrixBody = default(Matrix4x4);
            matrixHead.SetTRS(headdrawpos, Quaternion.AngleAxis(num, Vector3.up), s);
            matrixBody.SetTRS(bodydrawpos, Quaternion.AngleAxis(num, Vector3.up), s);
            
            Graphics.DrawMesh(headMesh, headdrawpos, Quaternion.AngleAxis(num, Vector3.up), headMat, 45);
            if (RenderBody)
            {
                Graphics.DrawMesh(bodyMesh, bodydrawpos, Quaternion.AngleAxis(num, Vector3.up), bodyMat, 40);
            }
            PortraitsCache.SetDirty(corrpawn);
        }

        public Graphic GetHeadGraphic(Pawn p, string patronname)
        {
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

                if (patronname == "Slaanesh")
            {
                patronname = "Undivided";
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

        private static Rot4 LayingFacingDet(Pawn DownedWearer)
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
