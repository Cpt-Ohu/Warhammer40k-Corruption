using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.BookStuff
{
    public class CompProperties_Bookshelf : CompProperties
    {
        public int BookshelfCapacity = 3;
        public string StoredBookGraphicPath;
        public List<ThingDef> BooksList = new List<ThingDef>();

        public CompProperties_Bookshelf()
        {
            this.compClass = typeof(CompBookshelf);
        }
    }
}
