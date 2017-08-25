using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace FactionColors
{
    public class CompFactionColor : ThingComp
    {
        public string randomGraphicPath = "";

        public Color SecondaryColor = Color.white;
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (base.parent.GetType() == typeof(ApparelUniform))
            {
                ApparelUniform app = base.parent as ApparelUniform;
                app.FirstSpawned = false;
            }

            if (CProps.UseSecondaryColors)
            {
                this.SecondaryColor = CProps.Coloring.NewRandomizedColor();
            }
        }

        public void ResolveRandomGraphics()
        {
            if (this.randomGraphicPath.NullOrEmpty())
            {
                this.randomGraphicPath = this.CProps.RandomGraphicPaths.RandomElementByWeight(x => x.Second).First;
            }
        }

        public CompProperties_FactionColor CProps
        {
            get
            {
                return (CompProperties_FactionColor)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<string>(ref this.randomGraphicPath, "randomGraphicPath", "");
            Scribe_Values.Look<Color>(ref this.SecondaryColor, "SecondaryColor", Color.white);
        }

    }    
}
