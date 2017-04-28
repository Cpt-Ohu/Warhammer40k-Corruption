using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Verse;
using RimWorld;

namespace AlienRace
{
    public class AlienPartGenerator
    {
        
        public List<string> aliencrowntypes = new List<string> {};

        public string AlienHeadTypeLoc;

        public string RandomAlienHead(string userpath)
        {
            System.Random r = new System.Random();
            int index = r.Next(aliencrowntypes.Count);
            return AlienHeadTypeLoc = userpath + aliencrowntypes[index];            
        }
    }
}
