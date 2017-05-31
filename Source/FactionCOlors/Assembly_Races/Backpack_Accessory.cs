using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FactionColors
{
    public class Backpack_Accessory : Apparel
    {
        private Graphic WornGraphic;

        public Graphic BackpackGraphic(BodyType bodyType, string graphicPath)
        {
            if (bodyType == BodyType.Undefined)
            {
                Log.Error("Getting naked body graphic with undefined body type.");
                bodyType = BodyType.Male;
            }
            string path = graphicPath + bodyType.ToString();
            return GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, Vector2.one, this.DrawColor, this. DrawColorTwo);
        }

        public override void Tick()
        {
            base.Tick();
            if (this.Wearer != null)
            {
                this.WornGraphic = BackpackGraphic(this.Wearer.story.bodyType, this.Graphic.path);
            }
            else
            {
                this.WornGraphic = null;
            }
        }

        public override void DrawWornExtras()
        {
            if (this.WornGraphic != null)
            {
                Vector3 vector = this.Wearer.Drawer.DrawPos;
                vector.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead)+2;
                float angle = this.Wearer.Rotation.AsInt;
                Material bmat = this.WornGraphic.MatAt(this.Wearer.Rotation);
                Vector3 s = new Vector3(1.4f, 1.4f, 1.4f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, bmat, 0);
            }

        }

    }
}
