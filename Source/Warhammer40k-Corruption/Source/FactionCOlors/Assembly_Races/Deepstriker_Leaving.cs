using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    public class Deepstriker_Leaving : Deepstriker_Base
    {
        private const int SoundAnticipationTicks = 100;

        public DropPodInfo contents;

        private int ticksToExit = 0;

        private int MaxTicksToExit = 200;

        private bool soundPlayed;

        private static readonly Material ShadowMat = MaterialPool.MatFrom("Things/IoM/Valkyrie/Valkyrie_Shadow", ShaderDatabase.Transparent);

        public Deepstriker_ThingDef tdef
        {
            get
            {
                return this.def as Deepstriker_ThingDef;
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                Vector3 result = base.Position.ToVector3ShiftedWithAltitude(AltitudeLayer.FlyingItem);
                float num = (float)(this.ticksToExit * this.ticksToExit) * 0.01f;
                result.x += num * 0.9f;
                result.z += num * 0.3f;
                return result;
            }
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            if (tdef != null)
            {
                this.MaxTicksToExit = tdef.TicksToExitMap + Rand.Range(-40, 60);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.ticksToExit, "ticksToExit", 0, false);
            Scribe_Deep.LookDeep<DropPodInfo>(ref this.contents, "contents", new object[0]);
        }

        public override void Tick()
        {
            this.ticksToExit++;

            if (!this.soundPlayed && this.ticksToExit < 100)
            {
                this.soundPlayed = true;
                SoundDefOf.DropPodFall.PlayOneShot(base.Position);
            }

            if (this.ticksToExit == this.MaxTicksToExit)
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = this.contents.containedThings.Count - 1; i >= 0; i--)
            {
                this.contents.containedThings[i].Destroy(DestroyMode.Vanish);
            }
            base.Destroy(mode);
        }

        public override void DrawAt(Vector3 drawLoc)
        {
            base.DrawAt(drawLoc);
            Vector3 pos = this.TrueCenter();
            pos.y = Altitudes.AltitudeFor(AltitudeLayer.Shadows);
            float num = 1 / (float)this.ticksToExit;
            Vector3 s = new Vector3(num, 1f, num);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, base.Rotation.AsQuat, s);
            Graphics.DrawMesh(MeshPool.plane10Back, matrix, Deepstriker_Leaving.ShadowMat, 0);
        }

    }
}
