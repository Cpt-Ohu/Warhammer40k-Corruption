using Corruption.IoM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public class TributeMission : Mission
    {
        public List<TributeDemand> Tributes = new List<TributeDemand>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<TributeDemand>(ref this.Tributes, "Tributes", LookMode.Deep);
        }
    }
}
