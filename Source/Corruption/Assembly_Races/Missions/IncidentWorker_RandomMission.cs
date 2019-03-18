using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Missions
{
    public abstract class IncidentWorker_RandomMission : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            List<MissionDef> allDefs = DefDatabase<MissionDef>.AllDefsListForReading.FindAll(x => x.StartingMission == false && x.PerequisiteMissions.NullOrEmpty());
            MissionDef def = allDefs.RandomElement();

            Mission mission = this.CreateMission(def);
            if (mission == null)
            {
                return false;
            }
            CFind.MissionManager.Missions.Add(mission);

            return true;
        }

        protected virtual Mission CreateMission(MissionDef def)
        {
            Faction faction = Find.FactionManager.AllFactionsListForReading.Where(x => def.GiverFaction == x.def).RandomElement();
            if (faction == null)
            {
                return null;
            }
            return CFind.MissionManager.CreateMission(def, faction, null);
        }
    }
}
