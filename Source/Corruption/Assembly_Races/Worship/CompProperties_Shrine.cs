using Verse;
using UnityEngine;

namespace Corruption.Worship
{
    public class CompProperties_Shrine : CompProperties
    {
        public Vector3 EffigyDrawOffset;

        public float WorshipRatePerTick;

        public bool requiresEffigy = true;

        public CompProperties_Shrine()
        {
            this.compClass = typeof(CompShrine);
        }

    }
}
