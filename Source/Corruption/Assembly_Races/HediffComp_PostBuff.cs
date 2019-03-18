using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class HediffComp_PostBuff : HediffComp_Disappears
    {
        private HediffCompProperties_PostBuff buffprops
        {
            get
            {
                return this.props as HediffCompProperties_PostBuff;
            }

        }

        private Material buffOverlay
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>(this.buffGraphicPath, ShaderDatabase.MoteGlow, Vector2.one, Color.white).MatSingle;
            }
        }

        private string buffGraphicPath = "UI/Psyker/BuffPositive";

        public override void CompPostMake()
        {
            if (!this.buffprops.IsPositiveBuff)
            {
                this.buffGraphicPath = "UI/Psyker/BuffNegative";
            }
            base.CompPostMake();
        }

        public void DrawBuff()
        {
            float scale = 1f;
            Vector3 s = new Vector3(scale, 1f, scale);
            Matrix4x4 matrix = default(Matrix4x4);
            Vector3 drawPos = new Vector3();

            drawPos = this.Pawn.DrawPos;
            drawPos.y -= 1f;
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(this.Pawn.Rotation.AsInt, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane20, matrix, buffOverlay, 0);
        }

        public override  void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            DrawBuff();
        }
        
        public override bool CompShouldRemove
        {
            get
            {
                if (base.CompShouldRemove)
                {
                    if (this.buffprops.postBuffHediff != null)
                    {
                        this.Pawn.health.AddHediff(buffprops.postBuffHediff);
                    }
                    return true;
                }
                return false;

            }
        }
    }
}
