using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class Deepstriker_Base : ThingWithComps
    {
        public bool FirstSpawned = true;

        public Color Col1 = Color.white;
        public Color Col2 = Color.black;
        public Graphic Detail;

        public override Color DrawColor
        {
            get
            {
                ; if (FirstSpawned)
                {
                    Log.Message("Getting DrawColorOne");
                    FactionDefUniform udef = this.Faction.def as FactionDefUniform;
                    CompFactionColor compF;
                    if (udef != null)
                    {
                        if ((compF = this.GetComp<CompFactionColor>()) != null && compF.CProps.UseCamouflageColor)
                        {
                            //            Log.Message("GettingCamoColor");
                            Col1 = CamouflageColorsUtility.CamouflageColors[0];
                        }
                        else
                        {
                            //                  Log.Message("StandardColor");
                            //                  Log.Message(udef.FactionColor1.ToString());
                            Col1 = udef.FactionColor1;
                        }
                    }
                    else
                    {
                        CompColorable comp = this.GetComp<CompColorable>();
                        if (comp != null && comp.Active)
                        {
                            Col1 = comp.Color;
                        }
                        else
                        {
                            Col1 = Color.white;
                        }

                    }
                }
                return Col1;
            }

            set
            {
                this.SetColor(value, true);
            }
        }

        public override Color DrawColorTwo
        {
            get
            {
                CompFactionColor compF;
                if (FirstSpawned)
                {
                    FactionDefUniform udef = this.Faction.def as FactionDefUniform;
                    if (udef != null)
                    {
                        if ((compF = this.GetComp<CompFactionColor>()) != null && compF.CProps.UseCamouflageColor)
                        {
                            //                   Log.Message("GettingCamoColor");
                            Col2 = CamouflageColorsUtility.CamouflageColors[1];
                        }
                        else
                        {
                            //                       Log.Message("StandardColor");
                            Col2 = udef.FactionColor2;
                        }
                    }
                    else
                    {
                        CompColorable comp = this.GetComp<CompColorable>();
                        if (comp != null && comp.Active)
                        {
                            Col2 = comp.Color;
                        }
                        Col2 = Color.white;
                    }
                }
                FirstSpawned = false;
                return Col2;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>(this.def.graphicData.texPath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
            }
        }



    }
}
