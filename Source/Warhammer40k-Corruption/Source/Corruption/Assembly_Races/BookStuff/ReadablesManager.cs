using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.BookStuff
{
    public class ReadablesManager : IExposable
    {
        public Dictionary<ThingDef, int> ReadableProgressEntry = new Dictionary<ThingDef, int>();
        
        public void ExposeData()
        {
            Scribe_Collections.Look<ThingDef, int>(ref this.ReadableProgressEntry, "ReadableProgressEntry", LookMode.Def, LookMode.Value);
        }
    }
}
