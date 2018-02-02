using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class JobGiver_MiracleHeal : JobGiver_AICastPsykerPower
    {
        protected override void ResolveTarget(Pawn pawn, float range, out Thing target, AIPsykerPowerCategory aiCategory = AIPsykerPowerCategory.DamageDealer)
        {
            Pawn patient = null;
            if (this.PatientAvailableForTreatment(pawn, out patient))
            {
                target = patient;
                return;
            }
            target = null;
            return;
        }
                
        protected override Job CastingJob(Pawn pawn)
        {
            Pawn patient = null;
            if (PatientAvailableForTreatment(pawn, out patient))
            {
                if (CorruptionStoryTrackerUtilities.IsPsyker(pawn))
                {
                    if (this.CanPerformMiracle(pawn, out this.PowerDefToCast))
                    {
                        return CorruptionStoryTrackerUtilities.AI_CastPsykerPowerJob(pawn, PowerDefToCast, patient, DefOfs.C_JobDefOf.PerformMiracleHeal);
                    }
                }
                else
                {
                    return StandardTendJob(pawn, patient);
                }
            }
            return null;
        }

        private bool PatientAvailableForTreatment(Pawn pawn, out Pawn patient)
        {
            Predicate<Thing> validator = delegate (Thing t)
            {
                Pawn pawn2 = t as Pawn;
                if (pawn != null)
                    {
                    return WorkGiver_Tend.GoodLayingStatusForTend(pawn2, pawn) && HealthAIUtility.ShouldBeTendedNow(pawn2) && pawn.CanReserve(pawn2, 1, -1, null, false) && !pawn.Faction.HostileTo(Faction.OfPlayer);
                }
                else
                {
                    return false;
                }
                };

            patient = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 25f, validator, null, 1) as Pawn;

            if (patient != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    private Job StandardTendJob(Pawn pawn, Pawn patient)
        {
            Thing thing = null;
            if (Medicine.GetMedicineCountToFullyHeal(patient) > 0)
            {
                thing = HealthAIUtility.FindBestMedicine(pawn, patient);
            }
            if (thing != null)
            {
                return new Job(JobDefOf.TendPatient, patient, thing);
            }
            return new Job(JobDefOf.TendPatient, patient);
        }

        private bool CanPerformMiracle(Pawn pawn, out PsykerPowerDef healPowerDef)
        {
            Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(pawn);
            if (soul != null)
            {
                CompPsyker compPsyker = soul.compPsyker;
                List<PsykerPower> healingPowers = compPsyker.allPowers.FindAll(x => (x.powerdef.MainVerb.defaultProjectile as ProjectileDef_WarpPower).IsHealer);
                
                if (healingPowers.Count > 0)
                {
                    healPowerDef = healingPowers.RandomElement().powerdef;
                    return true;
                }
            }

            healPowerDef = null;
            return false;
        }
    }
}
