using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption
{
    public static class PsykerUtility
    {
        private static float CalculatePsykerShockProbability(CompPsyker psycomp, PsykerPowerLevel spellPowerLevel)
        {
            float num = 0f;
            int psykerPower = (int)psycomp.soul.PsykerPowerLevel;
            int spellPower = (int)spellPowerLevel;
            num = 0.1f - (Mathf.Pow(psykerPower - spellPower, 2))/100;
            if (num < 0.001f)
            {
                num = 0.001f;
            }
            return num;

        }

        public static void PsykerShockEvents(CompPsyker psycomp, PsykerPowerLevel spellPowerLevel)
        {
            float chance = CalculatePsykerShockProbability(psycomp, spellPowerLevel);
            if (Rand.Range(0f, 1f) < chance)
            {
                float severity = Rand.Range(0f, 1f);
                if (severity < 0.1f)
                {
                    psycomp.psyker.health.AddHediff(C_HediffDefOf.DemonicPossession);
                    return;
                }
                else if (severity < 0.3f)
                {
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, psycomp.psyker, null);
                    psycomp.psyker.health.AddHediff(hediff, null, null);
                    return;
                }
                else
                {
                    Job vomit = new Job(JobDefOf.Vomit);
                    vomit.playerForced = true;
                    psycomp.psyker.jobs.StartJob(vomit, JobCondition.InterruptForced, null, false, true, null);
                    return;
                }     
            }
        }
    }
}
