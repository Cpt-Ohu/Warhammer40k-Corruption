using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class CompPauldronDrawer : ThingComp
    {
        public string graphicPath;
        public Shader shader = ShaderDatabase.Cutout;
        public ShoulderPadType padType;
        private bool useSecondaryColor;
        private bool useFactionTextures;
        public Apparel apparel
        {
            get
            {
                return this.parent as Apparel;
            }
        }

        public Pawn pawn
        {
            get
            {
                return this.apparel.Wearer;
            }
        }

        public Color mainColor
        {
            get
            {
                if (this.useSecondaryColor)
                {
                    return this.parent.DrawColorTwo;
                }
                return this.parent.DrawColor;
            }           
        }

        public Graphic PauldronGraphic
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Multi>(graphicPath + "_" + pawn.story.bodyType.ToString(), shader,  Vector2.one, this.mainColor, this.parent.DrawColorTwo);
            }
        }

        public CompProperties_PauldronDrawer pprops
        {
            get
            {
                return this.props as CompProperties_PauldronDrawer;
            }
        }

        public static bool ShouldDrawPauldron(Pawn pawn, Apparel curr, Rot4 bodyFacing, out Material pauldronMaterial)
        {
            pauldronMaterial = null;
            try
            {
                if (pawn.needs != null && pawn.story != null)
                {
                    CompPauldronDrawer drawer;
                    if ((drawer = curr.TryGetComp<CompPauldronDrawer>()) != null)
                    {
                        drawer.PostSpawnSetup(false);
                        if (drawer.PauldronGraphic != null)
                        {
                            if (drawer.CheckPauldronRotation(pawn, drawer.padType))
                            {
                                pauldronMaterial = drawer.PauldronGraphic.MatAt(bodyFacing);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        public bool CheckPauldronRotation(Pawn pawn, ShoulderPadType shoulderPadType)
        {
            if (shoulderPadType == ShoulderPadType.Left && pawn.Rotation == Rot4.East)
            {
                return false;
            }
            if (shoulderPadType == ShoulderPadType.Right && pawn.Rotation == Rot4.West)
            {
                return false;
            }
            return true;
        }
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ShoulderPadEntry entry = this.pprops.PauldronEntries.RandomElementByWeight((ShoulderPadEntry x) => x.commonality);
            this.graphicPath = entry.padTexPath;

            if (entry.UseFactionTextures)
            {
                this.graphicPath += ("_" + this.apparel.Wearer.Faction.Name);
            }
            this.shader = ShaderDatabase.ShaderFromType(entry.shaderType);
            this.useSecondaryColor = entry.UseSecondaryColor;
            this.padType = entry.shoulderPadType;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<string>(ref this.graphicPath, "graphicPath", null, false);
            //Scribe_Values.Look<Shader>(ref this.shader, "shader", ShaderDatabase.Cutout, false);
            Scribe_Values.Look<ShoulderPadType>(ref this.padType, "padType", ShoulderPadType.Both, false);
            Scribe_Values.Look<bool>(ref this.useSecondaryColor, "useSecondaryColor", false, false);

        }


    }
}
