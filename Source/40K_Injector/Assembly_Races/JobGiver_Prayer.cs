using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class JobGiver_Prayer : ThinkNode_JobGiver
    {

        private DefMap<CorruptionGiverDef, float> giverChances;

        public override void ResolveReferences()
        {
            giverChances = new DefMap<CorruptionGiverDef, float>();
        }

        protected virtual bool Allowed(CorruptionGiverDef def)
        {
            return true;
        }

        protected virtual Job TryGiveJobFromDef(CorruptionGiverDef def, Pawn pawn)
        {
            return def.Worker.TryGiveJob(pawn);
        }

        public override float GetPriority(Pawn pawn)
        {
            var soul = pawn.needs.TryGetNeed<Need_Soul>();
            var timeAssignmentDef = pawn.timetable != null
                                        ? pawn.timetable.CurrentAssignment
                                        : TimeAssignmentDefOf.Anything;
            if (
                (soul == null) ||
                (!timeAssignmentDef.allowJoy)
            )
            {
                return 0f;
            }
            
            switch(soul.DevotionTrait.SDegree)
            {
                case -2:
                    {
                        return 0f;
                    }
                case -1:
                    {
                        return 1f;
                    }
                case 0:
                    {
                        return 4f;
                    }
                case 1:
                    {
                        return 8f;
                    }
                case 2:
                    {
                        return 12f;
                    }
            }
            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            var soul = pawn.needs.TryGetNeed<Need_Soul>();
            if (soul == null)
            {
                return null;
            }
            if (
                (pawn.InBed()) ||
                (pawn.health.PrefersMedicalRest)
            )
            {
                return null;
            }
            if (!pawn.GetTimeAssignment().allowJoy)
            {
                return null;
            }
            var defsListForReading = DefDatabase<CorruptionGiverDef>.AllDefsListForReading;
            if (defsListForReading.NullOrEmpty())
            {
                return null;
            }
            for (int index = 0; index < defsListForReading.Count; ++index)
            {
                var def = defsListForReading[index];
                this.giverChances[def] = 0.0f;
                if (this.Allowed(def))
                {
                    if (def.pctPawnsEverDo < 1.0)
                    {
                        Rand.PushSeed();
                        Rand.Seed = pawn.thingIDNumber ^ 63216713;
                        if ((double)Rand.Value >= (double)def.pctPawnsEverDo)
                        {
                            Rand.PopSeed();
                            continue;
                        }
                        Rand.PopSeed();
                    }
                    float chance = def.Worker.GetChance(pawn);
                    this.giverChances[def] = chance;
                }
            }
            CorruptionGiverDef result;
            for (int index = 0; index < this.giverChances.Count && defsListForReading.TryRandomElementByWeight(def => giverChances[def], out result); ++index)
            {
                if (result == null)
                {
                    //Log.Message( "Unable to get PastafarianGiverDef to do!" );
                    return null;
                }
                Job job = this.TryGiveJobFromDef(result, pawn);
                if (job != null)
                {
                    return job;
                }
                //Log.Message( "Unable to do PastafarianGiverDef - " + result.defName );
                this.giverChances[result] = 0.0f;
            }
            //Log.Message( "Unable to get PastafarianGiverDef to do!" );
            return null;
        }

    }
}
