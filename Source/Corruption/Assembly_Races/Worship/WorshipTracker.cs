using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship
{
    public class WorshipTracker : IExposable
    {
        public void Initialize()
        {
            foreach (PatronDef def in DefDatabase<PatronDef>.AllDefsListForReading)
            {
                this.PlayerWorshipProgressLookup.Add(def, 0);
            }
        }

        public PatronDef PlayerGod = PatronDefOf.Emperor;
        public Dictionary<PatronDef, int> PlayerWorshipProgressLookup = new Dictionary<PatronDef, int>();
        public List<PatronDef> AvailableGods
        {
            get
            {
                return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Select(x => CompSoul.GetPawnSoul(x)?.Patron).Distinct().ToList();
            }
        }

        public void ConsumeFavour(int amount, PatronDef god)
        {
            PlayerWorshipProgressLookup[god] -= amount;
            if (PlayerWorshipProgressLookup[god] <= 0)
            {
                PlayerWorshipProgressLookup[god] = 1;
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<PatronDef, int>(ref this.PlayerWorshipProgressLookup, "Progress", LookMode.Def, LookMode.Value);
        }


        public float PlayerPatronWorshipProgress
        {
            get
            {
                return this.PlayerWorshipProgressLookup[this.PlayerGod];
            }
        }
        
        public void AddWorshipProgress(float amount, PatronDef god)
        {
            int accumulatedProgress = this.PlayerWorshipProgressLookup[god];
            this.PlayerWorshipProgressLookup[god] += AdjustWorshipGain(amount, accumulatedProgress);
            if (this.PlayerWorshipProgressLookup[god] > 3000)
            {
                CFind.MissionManager.FinishMission("EcclesiarchyGainFavour");
                CFind.MissionManager.FinishMission("ChaosGainFavour");
            }
        }

        private int AdjustWorshipGain(float amount, float currentProgress)
        {
            double dampeningFactor = (0.8f) / (1 + Math.Exp(-0.001f * (currentProgress - 6000)));

            return (int)(amount * (1 - dampeningFactor));
        }

    }

}
