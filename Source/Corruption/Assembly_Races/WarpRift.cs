using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI.Group;
using Corruption.DefOfs;

namespace Corruption
{
    public class WarpRift : ThingWithComps
    {
        private Graphic lightningOverlay = GraphicDatabase.Get<Graphic_Single>("Things/Chaos/Demons/WarpRiftB_Overlay", ShaderDatabase.MoteGlow);
        private float curRotation = 0;
        private Faction chaos = Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.ChaosCult);
        List<Pawn> spawnedGroup = new List<Pawn>();

        public override void TickRare()
        {
   //         Log.Message("Ticking");
            base.TickRare();
            if (Rand.Range(0f, 1f) > 0.9f)
            {
                Pawn demon = DemonUtilities.GenerateDemon();
                GenSpawn.Spawn(demon, this.Position, this.Map);
                spawnedGroup.Add(demon);
            }
            if (spawnedGroup.Count > 5)
            {
                LordMaker.MakeNewLord(chaos, new LordJob_AssaultColony(chaos, false, false, false, true, false), this.Map, spawnedGroup.AsEnumerable<Pawn>());
                spawnedGroup.Clear();
            }
        }
        
        public override void DrawAt(Vector3 drawLoc)
        {
    //        base.DrawAt(drawLoc);
        }

        public override void DrawGUIOverlay()
        {
          //  base.DrawGUIOverlay();
        }

        public override void Draw()
        {
            Vector3 drawPos = this.DrawPos;
            drawPos.y += 0.1f;
     //       Log.Message("Drawing");
            curRotation += 0.5f;
            float scale = 1f;
            if (curRotation > 360)
            {
                curRotation = 0;
            }

            scale = 1f + Mathf.Sin(Time.time * 5) * 0.1f;

            Vector3 s = new Vector3(scale, 1f, scale);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(curRotation, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane20, matrix, this.Graphic.MatAt(this.Rotation), 1);
            this.lightningOverlay.color.a = (curRotation / 360);
            drawPos.y += 0.1f;
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(curRotation, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane20, matrix, this.lightningOverlay.MatAt(this.Rotation), 3);
        }
    }
}
