using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Corruption
{
    public class StockableBookContainer : Building, IThingContainerOwner
    {

        public ThingContainer BookContainer;

        public ThingContainer GetContainer()
        {

            return this.BookContainer;
        }

        public IntVec3 GetPosition()
        {
            return base.Position;
        }
        
    }
}
