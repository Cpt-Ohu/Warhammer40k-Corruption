using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class CompMechanicusImplantManager : ThingComp, IThingHolder
    {
        public ThingOwner innerContainer;

        public Pawn pawn;

        public CompMechanicusImplantManager()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public CompMechanicusImplantManager(Pawn pawn)
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            this.pawn = pawn;
        }

        public bool Spawned
        {
            get
            {
                return this.parent.Spawned;
            }
        }

        public IntVec3 GetPosition()
        {
            return this.parent.PositionHeld;
        }

        public Map GetMap()
        {
            return this.parent.MapHeld;
        }

        public void DrawThing(Thing thingToDraw)
        {
            float angle = this.pawn.Rotation.AsAngle;
            Material bodymat = this.pawn.Drawer.renderer.graphics.nakedGraphic.MatSingle;
            Material headmat = this.pawn.Drawer.renderer.graphics.headGraphic.MatSingle;
            Material hairmat = this.pawn.Drawer.renderer.graphics.hairGraphic.MatSingle;
            Vector3 sBody = new Vector3(1.0f, 1f, 1.0f);
            Matrix4x4 matrixBody = default(Matrix4x4);
            Vector3 vector = this.parent.DrawPos;
            vector.y += 0.05f;
            matrixBody.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), sBody);

            Graphics.DrawMesh(MeshPool.humanlikeBodySet.MeshAt(this.parent.Rotation), matrixBody, bodymat, 0);

            Matrix4x4 matrixHead = default(Matrix4x4);
            Vector3 headVec = vector + new Vector3(Mathf.Sin(angle) * 0.2f, 0.03f, Mathf.Cos(angle) * 0.2f);
            matrixHead.SetTRS(headVec, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(1.0f, 1f, 1.0f));
            Graphics.DrawMesh(MeshPool.humanlikeHeadSet.MeshAt(this.parent.Rotation), matrixHead, headmat, 0);
            Graphics.DrawMesh(MeshPool.humanlikeHairSetAverage.MeshAt(this.parent.Rotation), matrixHead, hairmat, 0);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }
    }
}
