using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;

namespace Corruption.Worship
{
    public class CompProperties_Shrine : CompProperties
    {
        public Vector3 EffigyDrawOffset;

        public bool hasEffigy = true;

        public CompProperties_Shrine()
        {
            this.compClass = typeof(CompShrine);
        }

    }
}
