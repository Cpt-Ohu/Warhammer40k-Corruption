using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.ProductionSites
{
    public class WorkForce : IExposable
    {
        public PawnKindDef PawnKind;

        private int _workerCount;

        public int WorkerCount
        {
            get { return _workerCount; }
            set
            {
                _workerCount = Math.Max(0, value);
            }
        }

        public List<float> SkillValues = new List<float>();

        public WorkForce()
        {

        }

        public WorkForce(PawnKindDef pawnKind)
        {
            this.PawnKind = pawnKind;
        }

        public int AverageSkill
        {
            get
            {
                if (this.SkillValues.Count == 0)
                {
                    return 0;
                }
                return (int)Math.Round(this.SkillValues.Average());
            }
        }


        public void AddWorker(Pawn pawn, SkillDef skillDef)
        {
            if (pawn.Spawned)
            {
                pawn.DeSpawn();
            }

            this.WorkerCount++;
            float skill = pawn.skills.GetSkill(skillDef).Level;
            this.SkillValues.Add(skill);
        }

        public void AddWorker(float skill = 5f)
        {
            this.WorkerCount++;
            this.SkillValues.Add(skill);
        }

        internal void JoinWorkForce(WorkForce workForce)
        {
            this.SkillValues.AddRange(workForce.SkillValues);
            this.WorkerCount += workForce.WorkerCount;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this._workerCount, "WorkerCount", 0);
            Scribe_Collections.Look<float>(ref this.SkillValues, "SkillValues", LookMode.Value);
            Scribe_Defs.Look<PawnKindDef>(ref this.PawnKind, "PawnKind");
        }

        internal static void TransferWorkers(WorkForce giverWorkForce, WorkForce receiverWorkForce, int count, bool skilledFirst)
        {
            for (int i = 0; i < count; i++)
            {
                if (giverWorkForce.WorkerCount > 0)
                {
                    float workerSkill = skilledFirst ? giverWorkForce.SkillValues.Max() : giverWorkForce.SkillValues.Min();
                    if (giverWorkForce.SkillValues.Contains(workerSkill))
                    {
                        giverWorkForce.SkillValues.Remove(workerSkill);
                        giverWorkForce.WorkerCount--;
                        receiverWorkForce.AddWorker(workerSkill);
                    }
                }
            }
        }

        public static WorkForce GetOrCreateWorkForce(ref List<WorkForce> workForces, PawnKindDef pawnKind)
        {
            WorkForce fittingWorkforce = workForces.FirstOrDefault(x => x.PawnKind == pawnKind);
            if (fittingWorkforce == null)
            {
                WorkForce newWorkers = new WorkForce(pawnKind);
                workForces.Add(newWorkers);
                return newWorkers;
            }
            return fittingWorkforce;
        }
    }
}
