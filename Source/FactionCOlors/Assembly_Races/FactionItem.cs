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

        public Color Col1 = Color.red;
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
                if (FirstSpawned)
                {
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
                }
                return Col2;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref FirstSpawned, "FirstSpawned", false, false);
            Scribe_Values.Look<Color>(ref Col1, "col1", Color.white, false);
            Scribe_Values.Look<Color>(ref Col2, "col2", Color.white, false);
        }

    }
}
