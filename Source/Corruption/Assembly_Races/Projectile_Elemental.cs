using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace Corruption
{
    public class Projectile_Elemental : Projectile
    {
        protected override void Impact(Thing hitThing)
        {
            this.Map.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(this.Map, base.Position));
            base.Impact(hitThing);
        }

        public override void Draw()
        {
            Vector3 vector = destination;
            Vector3 distance = this.destination - this.origin;
            Vector3 curpos = this.destination - this.Position.ToVector3();
            var num = 1 - (Mathf.Sqrt(Mathf.Pow(curpos.x, 2) + Mathf.Pow(curpos.z, 2)) / (Mathf.Sqrt(Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.z, 2))));
            float angle = 0f;
            Material mat = this.Graphic.MatSingle;
            Vector3 s = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            vector.y = 3;
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Vector3 vector = destination;
            Vector3 distance = this.destination - this.origin;
            Vector3 curpos = this.destination - this.Position.ToVector3();
            var num = 1 - (Mathf.Sqrt(Mathf.Pow(curpos.x, 2) + Mathf.Pow(curpos.z, 2)) / (Mathf.Sqrt(Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.z, 2))));
            float angle = 0f;
            Material mat = this.Graphic.MatSingle;
            Vector3 s = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            vector.y = 3;
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);            
        }

        public override Graphic Graphic
        {
            get
            {
                return base.Graphic;
            }
        }
    }
}
