using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class ApparelUniform : Apparel
    {
        public ApparelUniform()
        {
        }

        public bool FirstSpawned = true;

        public Color Col1 = Color.red;
        public Color Col2 = Color.grey;
        public Graphic Detail;

        private FactionDefUniform udef
        {
            get

            {
                return this.Wearer.Faction.def as FactionDefUniform;
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
                if (this.compF.CProps.IsRandomMultiGraphic)
                {
                    string singlePath = this.def.apparel.wornGraphicPath + "/" + this.compF.randomGraphicPath;
                    return GraphicDatabase.Get<Graphic_Single>(singlePath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.Col1, this.Col2);

                }
                
                return GraphicDatabase.Get<Graphic_Single>(this.def.graphicData.texPath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.Col1, this.Col2);
            }
        }

        public override Color DrawColor
        {
            get
            {
                if (FirstSpawned)
                {
                    CompFactionColor compF = this.GetComp<CompFactionColor>();
                    if (compF == null)
                    {
                        Log.Error("Uniform Apparel " + this.ToString() + " is missing a CompFactionColors");
                    }
                    if (this.Wearer != null)
                    {
                        FactionColorEntry myEntry;
                        if (FactionColorUtilities.currentPlayerStoryTracker.GetColorEntry(Wearer.Faction, out myEntry))
                        {
                            Col1 = myEntry.FactionColor1;
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
                        }

                    }
                    if ((compF != null && compF.CProps.UseCamouflageColor))
                    {
                        Col1 = CamouflageColorsUtility.CamouflageColors[0];
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
                if (FirstSpawned)
                {
                    CompFactionColor compF = this.GetComp<CompFactionColor>();                    
                    if (this.Wearer != null)
                    {
                        FactionColorEntry myEntry;
                        if (FactionColorUtilities.currentPlayerStoryTracker.GetColorEntry(Wearer.Faction, out myEntry))
                        {
                            Col2 = myEntry.FactionColor2;
                        }
                    }
                    else
                    {
                            CompColorable comp = this.GetComp<CompColorable>();
                            if (comp != null && comp.Active)
                            {
                                Col2 = comp.Color;
                            }                        
                    }
                    if ((compF != null && compF.CProps.UseCamouflageColor))
                    {
                        Col2 = CamouflageColorsUtility.CamouflageColors[1];
                    }
                }
                return Col2;
            }
        }

        //public Graphic_RandomApparelMulti RandomGraphic
        //{
        //    get
        //    {
        //        if (this.isRandomizedGraphic)
        //        {
        //            return GraphicDatabase.Get<Graphic_Single>(this.def.graphicData.texPath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo) as Graphic_RandomApparelMulti;
        //        }
        //        return null;
        //    }
        //}

        //public override Graphic Graphic
        //{
        //    get
        //    {
        //        if (this.isRandomizedGraphic)
        //        {
        //            return GraphicDatabase.Get<Graphic_Single>(this.randomGraphicPath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo) as Graphic_RandomApparelMulti;
        //        }
        //        return GraphicDatabase.Get<Graphic_Single>(this.def.graphicData.texPath, ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
        //    }
        //}
        
        private void SetFactionColor(ref Color color, CompFactionColor compF)
        {
            if (this.Wearer != null)
            {
                if (udef != null)
                {
                    if ((compF != null && compF.CProps.UseCamouflageColor))
                    {
                        color = CamouflageColorsUtility.CamouflageColors[1];
                    }
                    else
                    {
                        color = udef.FactionColor2;
                    }
                }
            }
            else
            {
                    CompColorable comp = this.GetComp<CompColorable>();
                    if (comp != null && comp.Active)
                    {
                        color = comp.Color;
                    }                
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            if (this.compF.CProps.IsRandomMultiGraphic)
            {
                this.compF.ResolveRandomGraphics();
            }
            PlayerFactionStoryTracker tracker = FactionColorUtilities.currentPlayerStoryTracker;
            if (tracker != null)
            {
                Col1 = tracker.PlayerColorOne;
                Col2 = tracker.PlayerColorTwo;
                if (compF == null)
                {
                    Log.Error("Uniform Apparel " + this.ToString() + " is missing a CompFactionColors");
                }
                if ((compF != null && compF.CProps.UseCamouflageColor))
                {
                    Col1 = CamouflageColorsUtility.CamouflageColors[0];
                    Col2 = CamouflageColorsUtility.CamouflageColors[1];
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref FirstSpawned, "FirstSpawned", false, false);
            Scribe_Values.Look<Color>(ref Col1, "Col1", Color.white, false);
            Scribe_Values.Look<Color>(ref Col2, "Col2", Color.white, false);
        }
    }
}
