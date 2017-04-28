﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class ApparelUniform : Apparel
    {
        public bool FirstSpawned = true;

        public Color Col1 = Color.red;
        public Color Col2 = Color.grey;
        public Graphic Detail;

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
                    if (this.wearer != null)
                    {
                        FactionDefUniform udef = this.wearer.Faction.def as FactionDefUniform;
                        if (udef != null)
                        {
                                Col1 = udef.FactionColor1;
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
                    if (this.wearer != null)
                    {
                        FactionDefUniform udef = this.wearer.Faction.def as FactionDefUniform;
                        if (udef != null)
                        {
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
                    }
                    if ((compF != null && compF.CProps.UseCamouflageColor))
                    {
                        Col2 = CamouflageColorsUtility.CamouflageColors[1];
                    }
                }
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

        private void SetFactionColor(ref Color color, CompFactionColor compF)
        {
            if (this.wearer != null)
            {
                FactionDefUniform udef = this.wearer.Faction.def as FactionDefUniform;
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
            PlayerFactionStoryTracker tracker = FactionColorUtilities.currentPlayerStoryTracker;
            if (tracker != null)
            {
                Col1 = tracker.PlayerColorOne;
                Col2 = tracker.PlayerColorTwo;
                CompFactionColor compF = this.GetComp<CompFactionColor>();
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

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<bool>(ref FirstSpawned, "FirstSpawned", false, false);
            Scribe_Values.LookValue<Color>(ref Col1, "Col1", Color.white, false);
            Scribe_Values.LookValue<Color>(ref Col2, "Col2", Color.white, false);
        }
    }
}
