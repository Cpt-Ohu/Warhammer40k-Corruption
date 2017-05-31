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

        public CompProperties_FactionColor CProps
        {
            get
            {
                return (CompProperties_FactionColor)this.props;
            }
        }

    }    
}
