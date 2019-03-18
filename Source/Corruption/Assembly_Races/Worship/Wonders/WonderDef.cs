using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.Worship.Wonders
{
    public class WonderDef : Def
    {
        public Type workerClass;

        public PatronDef DefaultGod = PatronDefOf.Emperor;

        public List<ThingDefCount> ThingsToSpawn = new List<ThingDefCount>();

        public List<HediffDef> HediffsToAdd = new List<HediffDef>();

        public float minHediffSeverity = 0.1f;

        public SoulTraitDef SpecialSoulTrait;

        public List<PsykerPowerDef> UnlockedPowers = new List<PsykerPowerDef>();

        public IncidentDef Incident;

        public MentalStateDef mentalStateToStart;

        public int IncidentPoints;

        public string WonderIconPath;

        public Texture2D WonderIcon;

        public bool PointsScalable;

        public SimpleCurve PointsCurve;

        public int worshipCost;
        
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.WonderIcon = ContentFinder<Texture2D>.Get(this.WonderIconPath, true);
            });
        }


        private WonderWorker workerInt;
        public WonderWorker Worker
        {
            get
            {
                if (this.workerInt == null)
                {
                    this.workerInt = (WonderWorker)Activator.CreateInstance(this.workerClass);
                    this.workerInt.Def = this;
                }
                return this.workerInt;
            }
        }

        public int ResolveWonderPoints(int worshipPoints)
        {
            if (this.PointsScalable)
            {

                int result = (int)this.PointsCurve.Evaluate(worshipPoints);
                return result;
            }
            else
            {
                return this.IncidentPoints;
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (this.mentalStateToStart == null && this.workerClass == typeof(WonderWorker_StartMentalState))
            {
                yield return this.defName + " has type " + typeof(WonderWorker_StartMentalState).Name + " but mentalStateToStart is null";
            }
            if (this.SpecialSoulTrait == null && this.workerClass == typeof(WonderWorker_AddSpecialTrait))
            {
                yield return this.defName + " has type " + typeof(WonderWorker_AddSpecialTrait).Name + " but SpecialSoulTrait is null";
            }
            if (this.HediffsToAdd.NullOrEmpty() && this.workerClass == typeof(WonderWorker_AddHediff))
            {
                yield return this.defName + " has type " + typeof(WonderWorker_AddHediff).Name + " but HediffsToAdd are null or empty";
            }
        }
    }
}
