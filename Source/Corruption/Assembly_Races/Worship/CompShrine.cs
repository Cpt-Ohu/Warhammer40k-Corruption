using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class CompShrine : ThingComp, IThingHolder, IOpenable
    {
        public CompProperties_Shrine cProps
        {
            get
            {
                return this.props as CompProperties_Shrine;
            }
        }

        public Thing InstalledEffigy
        {
            get
            {
                return (this.innerContainer.Count != 0) ? this.innerContainer[0] : null;
            }                
        }

        public Thing thingToInstall;
        
        protected ThingOwner innerContainer;

        public CompShrine()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public CompSoulItem soulComp
        {
            get
            {
                if (this.HasEffigy)
                {
                    return this.InstalledEffigy.TryGetComp<CompSoulItem>();
                }
                return null;
            }
        }
        
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }

        public bool HasEffigy
        {
            get
            {
                return this.InstalledEffigy != null;
            }
        }

        public bool CanOpen
        {
            get
            {
                return this.InstalledEffigy != null;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!this.HasEffigy)
            {
                foreach (Thing potentialEffigy in selPawn.Map.listerThings.AllThings.FindAll(x => x.TryGetComp<CompSoulItem>() != null))
                {
                    FloatMenuOption option2 = new FloatMenuOption("InstallEffigy".Translate( new object[] { potentialEffigy.Label }), delegate
                    {
                        Job job = new Job(JobDefOf.HaulToContainer, potentialEffigy, this.parent);
                        job.count = 1;
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                    });
                    yield return option2;
                }
            }
            else
            {
                FloatMenuOption option = new FloatMenuOption("UninstallEffigy".Translate(new object[] { this.InstalledEffigy.Label }), delegate
                {
                    Job openJob = new Job(JobDefOf.Open, this.parent);
                    selPawn.jobs.TryTakeOrderedJob(openJob, JobTag.Misc);

                });
                yield return option;
            }
        }

        public void Open()
        {
            this.DropEffigy();
        }

        public void DropEffigy()
        {
            IntVec3 c = this.parent.Position;
            if (this.parent.def.hasInteractionCell)
            {
                c = this.parent.InteractionCell;
            }
            this.innerContainer.TryDropAll(c, this.parent.Map, ThingPlaceMode.Near);
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (this.HasEffigy)
            {
                Mesh mesh = MeshPool.plane10;
                Vector3 vector = this.parent.DrawPos;
                vector.y += 1f;
                vector.z += 0.11f;
                Vector3 s = new Vector3(1.0f, 1f, 1.0f);
                Matrix4x4 matrix = default(Matrix4x4);
                Mesh drawMesh = MeshPool.plane10;
                if (this.parent.Rotation == Rot4.West)
                {
                    drawMesh = MeshPool.plane10Flip;
                }
                matrix.SetTRS(vector, Quaternion.AngleAxis(0f, Vector3.up), s);
                Graphics.DrawMesh(drawMesh, matrix, this.InstalledEffigy.Graphic.MatAt(this.parent.Rotation, null), 0);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[]
            {
             this
            });
        }
    }
}
