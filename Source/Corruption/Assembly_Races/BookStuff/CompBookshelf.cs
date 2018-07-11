using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.BookStuff
{
    public class CompBookshelf : ThingComp
    {
        public CompProperties_Bookshelf Props
        {
            get
            {
                return this.props as CompProperties_Bookshelf;
            }
        }
    }
}
