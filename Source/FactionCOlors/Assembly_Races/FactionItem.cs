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
        public bool FirstSpawned = true;
        private bool firstResolved;
        private bool secResolved;


        public Color Col1 = Color.magenta;
        public Color Col2 = Color.grey;
        public Graphic Detail;

        private Pawn Wearer
        {
            get
            {
                CompEquippable compEQ = this.TryGetComp<CompEquippable>();
                if (compEQ!=null)
                {
                    Pawn holder = compEQ.PrimaryVerb.CasterPawn;
                    return holder;
                }
                return null;
            }
        }

        private FactionDefUniform udef
        {
            get

            {
                if (this.Wearer != null)
                {
                    return this.Wearer.Faction.def as FactionDefUniform;
                }
                return null;
            }
        }

        private CompFactionColor compF
        {
            get
            {
                return this.GetComp<CompFactionColor>();
            }
        }
        
        public override Graphic Graphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>(this.def.graphicData.texPath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
            }
        }

        public override Color DrawColor
        {
            get
            {
                if (!firstResolved)
                {
                    if (this.Wearer != null)
                    {
                        FactionColorEntry myEntry;
                        if (this.compF != null)
                        {
                            if (FactionColorUtilities.currentPlayerStoryTracker.GetColorEntry(Wearer.Faction, out myEntry))
                            {
                                Col1 = myEntry.FactionColor1;
                            }
                        }
                        else
                        {
                            Col1 = this.def.graphicData.color;
                        }
                    }
                    else
                    {
                        {
                            CompColorable comp = this.GetComp<CompColorable>();
                            if (comp != null && comp.Active)
                            {
                                Col1 = comp.Color;
                            }
                            else
                            {
                                Col1 = this.def.graphicData.color;
                            }
                        }

                    }
                    if ((compF != null && compF.CProps.UseCamouflageColor))
                    {
                        Col1 = CamouflageColorsUtility.CamouflageColors[0];
                    }
                    this.firstResolved = true;
                }
                return Col1;
            }
            set
            {
                this.SetColor(value, true);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }
        
        public override Color DrawColorTwo
        {
            get
            {
                if (!secResolved)
                {
                    if (this.Wearer != null)
                    {
                        if (this.compF != null)
                        {
                            FactionColorEntry myEntry;
                            if (FactionColorUtilities.currentPlayerStoryTracker.GetColorEntry(Wearer.Faction, out myEntry))
                            {
                                Col2 = myEntry.FactionColor2;
                            }
                        }
                        else
                        {
                            Col2 = this.def.graphicData.colorTwo;
                        }
                    }
                    else
                    {
                        CompColorable comp = this.GetComp<CompColorable>();
                        if (comp != null && comp.Active)
                        {
                            Col2 = comp.Color;
                        }
                        else
                        {
                            Col2 = this.def.graphicData.colorTwo;
                        }
                    }

                    this.secResolved = true;
                }
                return Col2;
            }
        }

        public override void Draw()
        {
            base.Draw();
            Vector3 s = new Vector3(this.def.graphicData.drawSize.x, 1f, this.def.graphicData.drawSize.y);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(this.DrawPos, Quaternion.AngleAxis(0, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.Graphic.MatSingle, 0);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref secResolved, "secResolved", false, false);
            Scribe_Values.Look<bool>(ref firstResolved, "firstResolved", false, false);
            Scribe_Values.Look<Color>(ref Col1, "col1", Color.white, false);
            Scribe_Values.Look<Color>(ref Col2, "col2", Color.white, false);
        }

    }
}
