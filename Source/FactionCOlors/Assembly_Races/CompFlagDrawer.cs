using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class CompFlagDrawer : ThingComp
    {
        public CompProperties_FlagDrawer cprops
        {
            get
            {
                return this.props as CompProperties_FlagDrawer;
            }
        }

        public PlayerFactionStoryTracker storyTracker
        {
            get
            {
                return FactionColorUtilities.currentPlayerStoryTracker;
            }
        }

        public Graphic bannerGraphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>(storyTracker.BannerGraphicPath, ShaderDatabase.CutoutComplex, Vector2.one, storyTracker.PlayerColorOne, storyTracker.PlayerColorTwo);
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (this.cprops != null)
            {
                Mesh mesh = MeshPool.plane10;
                Vector3 vector = this.parent.DrawPos + cprops.DrawOffset;
                vector.y += 5f;
                Vector3 s = new Vector3(1.5f, 1f, 1.5f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(0f, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, this.bannerGraphic.MatAt(this.parent.Rotation, null), 0);
            }
        }
    }
}
