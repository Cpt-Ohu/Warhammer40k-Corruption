using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class FactionItem : ThingWithComps
    {
        public Graphic newGraphic;

        private Vector2 drawSize = new Vector2(1f, 1f);

        private Vector3 meshSize;

        private Mesh drawMesh;

        private Vector3 drawPos;

        private Material Mat;

        public Color col1 = Color.white;

        public Color col2 = Color.gray;

        private string textpath;

        private bool FirstSpawned = true;

        public override Graphic Graphic
        {
            get
            {
                    textpath = this.def.graphicData.texPath;
                    GetFactionColors();  
                    return newGraphic = GraphicDatabase.Get<Graphic_Single>(textpath, ShaderDatabase.CutoutComplex, drawSize, col1, col2);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            FactionItemDef newDef = this.def as FactionItemDef;
            meshSize = newDef.ItemMeshSize;
            drawPos = this.DrawPos;
            drawMesh = this.Graphic.MeshAt(this.Rotation);
            Mat = this.Graphic.MatSingle;
        }

        public void GetFactionColors()
        {
            if (FirstSpawned)
            {
                if (this.GetComp<CompEquippable>().PrimaryVerb.CasterPawn != null)
                {
                    Pawn holder = this.GetComp<CompEquippable>().PrimaryVerb.CasterPawn;
                    if (holder != null && holder.Faction.def.GetType() == typeof(FactionDefUniform))
                    {
                        FactionDefUniform facdef = holder.Faction.def as FactionDefUniform;
                        col1 = facdef.FactionColor1;
                        col2 = facdef.FactionColor2;
                    }
                    else
                    {
                        col1 = this.DrawColor;
                        col2 = this.DrawColorTwo;
                    }
                }
                FirstSpawned = false;
            }
            drawSize.x = meshSize.y;
            drawSize.y = meshSize.z;
        }

        public override void Draw()
        {
            drawMesh = this.Graphic.MeshAt(this.Rotation);
            Mat = this.Graphic.MatAt(this.Rotation);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(0, Vector3.up), 0.7f*meshSize);
            Graphics.DrawMesh(drawMesh, matrix, Mat, 0);
            this.Comps_PostDraw()
;        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref FirstSpawned, "FirstSpawned", false, false);
            Scribe_Values.Look<Color>(ref col1, "col1", Color.white, false);
            Scribe_Values.Look<Color>(ref col2, "col2", Color.white, false);
        }

    }
}
