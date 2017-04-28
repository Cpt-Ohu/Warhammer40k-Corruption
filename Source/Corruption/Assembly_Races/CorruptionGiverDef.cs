using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CorruptionGiverDef : Def
    {

        public JobDef jobDef;
        public Type giverClass;
        public float baseChance;
        public bool desireSit;
        public float pctPawnsEverDo;
        public List<ThingDef> thingDefs;

        private CorruptionGiver workerInt;

        public CorruptionGiver Worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (CorruptionGiver)Activator.CreateInstance(giverClass);
                    workerInt.def = this;
                }
                return workerInt;
            }
        }

        public CorruptionGiverDef()
        {
            this.pctPawnsEverDo = 1f;
            this.desireSit = true;
        }

    }
}
