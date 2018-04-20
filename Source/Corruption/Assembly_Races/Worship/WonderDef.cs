using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Worship
{
    public class WonderDef : Def
    {
        public IncidentDef Incident;

        public int IncidentPoints;

        public string WonderIconPath;

        public Texture2D WonderIcon;

        public int worshipCost;
        
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.WonderIcon = ContentFinder<Texture2D>.Get(this.WonderIconPath, true);
            });
        }
    }
}
