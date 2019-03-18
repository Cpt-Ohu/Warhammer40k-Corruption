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
    [StaticConstructorOnStartup]
    public static class PsykerUtility
    {

        public static readonly Texture2D PowerLevelOmega = ContentFinder<Texture2D>.Get("UI/Background/PsykerPowerButtonOmega", true);
        public static readonly Texture2D PowerLevelSigma = ContentFinder<Texture2D>.Get("UI/Background/PsykerPowerButtonSigma", true);
        public static readonly Texture2D PowerLevelRho = ContentFinder<Texture2D>.Get("UI/Background/PsykerPowerButtonRho", true);
        public static readonly Texture2D PowerLevelOmicron = ContentFinder<Texture2D>.Get("UI/Background/PsykerPowerButtonOmicron", true);
        public static readonly Texture2D PowerLevelIota = ContentFinder<Texture2D>.Get("UI/Background/PsykerPowerButtonIota", true);
        public static readonly Texture2D PowerLevelZeta = ContentFinder<Texture2D>.Get("UI/Background/PsykerPowerButtonZeta", true);
        public static readonly Texture2D PowerLevelEpsilon = ContentFinder<Texture2D>.Get("UI/Background/PsykerPowerButtonEpsilon", true);
        public static readonly Texture2D PowerLevelDelta = ContentFinder<Texture2D>.Get("UI/Background/PsykerPowerButtonDelta", true);

        static PsykerUtility()
        {

        }

        public static readonly Dictionary<PsykerPowerLevel, int> PsykerXPCost = new Dictionary<PsykerPowerLevel, int>{
            {PsykerPowerLevel.Iota, 50 },
            {PsykerPowerLevel.Zeta, 150 },
            {PsykerPowerLevel.Epsilon, 250 },
            {PsykerPowerLevel.Delta, 400 },
            };

        public static Texture2D GetPsykerPowerLevelTexture(PsykerPowerLevel level)
        {
            switch(level)
            {
                case PsykerPowerLevel.Omega:
                    {
                        return PowerLevelOmega;
                    }
                case PsykerPowerLevel.Sigma:
                    {
                        return PowerLevelSigma;
                    }
                case PsykerPowerLevel.Rho:
                    {
                        return PowerLevelRho;
                    }
                case PsykerPowerLevel.Omicron:
                    {
                        return PowerLevelOmicron;
                    }
                case PsykerPowerLevel.Iota:
                    {
                        return PowerLevelIota;
                    }
                case PsykerPowerLevel.Zeta:
                    {
                        return PowerLevelZeta;
                    }
                case PsykerPowerLevel.Epsilon:
                    {
                        return PowerLevelEpsilon;
                    }
                case PsykerPowerLevel.Delta:
                    {
                        return PowerLevelDelta;
                    }
                default:
                    {
                        return PowerLevelRho;
                    }
            }
        }


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

        public static void PsykerShockEvents(CompPsyker psycomp)
        {
            float chance = CalculatePsykerShockProbability(psycomp, psycomp.curPower.PowerLevel);
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

        public static List<PsykerPowerDef> GetPowerDefsFor(PsykerPowerLevel powerLevel, PatronDef patronDef)
        {
            return DefDatabase<PsykerPowerDef>.AllDefsListForReading.Where(x => x.PowerLevel <= powerLevel && patronDef.PsykerPowers.Contains(x)).ToList();
        }
    }
}
