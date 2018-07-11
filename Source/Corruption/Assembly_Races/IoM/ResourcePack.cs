using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
   public class ResourcePack : ThingWithComps, IOpenable
    {
        public CompResourcePack compResource
        {
            get
            {
                return this.TryGetComp<CompResourcePack>();
            }
        }

        public bool CanOpen
        {
            get
            {
                return true;
            }
        }

        public void Open()
        {
            foreach (ResourcePackEntry entry in this.compResource.Resources)
            {

                Thing thing = ThingMaker.MakeThing(entry.Def, entry.stuff);
                thing.stackCount = entry.Count;
                thing.TryGetComp<CompQuality>()?.SetQuality(entry.AvgQualityCategory, ArtGenerationContext.Outsider);
                this.holdingOwner.TryAdd(thing, true);
                Thing droppedThing;
                bool flag = false;
                int errorCounter = 0;
                while (flag == false && errorCounter < 5)
                {
                    flag = GenDrop.TryDropSpawn(thing, this.Position.RandomAdjacentCellCardinal(), this.Map, ThingPlaceMode.Near, out droppedThing);
                    if (flag == false) errorCounter++;
                }
            }

            this.compResource.Resources.Clear();
            this.compResource.IsTribute = false;
            this.compResource.LoadEnabled = false;
        }

        public float TotalValue
        {
            get
            {
                return this.MarketValue + this.compResource.Resources.Sum(x => x.MarketValue);
            }
        }


    }
}
