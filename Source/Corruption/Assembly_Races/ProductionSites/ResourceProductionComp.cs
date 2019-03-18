using Corruption.IoM;
using Corruption.Missions;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    public class ResourceProductionComp : WorldObjectComp
    {
        public static readonly Vector2 IconSizeTiny = new Vector2(124f, 124f);
        public static readonly Vector2 IconSizeMedium = new Vector2(164f, 164f);

        public int _curLevelInt;

        public List<UpgradeBuildProgress> Upgrades = new List<UpgradeBuildProgress>();

        public List<WorkForce> WorkForce = new List<WorkForce>();

        public float ProductionProgress;

        private static List<ResourceEntry> tmpContents = new List<ResourceEntry>();

        private static List<string> tmpContentsStr = new List<string>();

        public WorldObjectCompProperties_ResourceProduction Props
        {
            get
            {
                return this.props as WorldObjectCompProperties_ResourceProduction;
            }
        }

        protected ProductionSite ProductionSite
        {
            get
            {
                return this.parent as ProductionSite;
            }
        }

        public IEnumerable<ProductionSiteUpgradeDef> AvailableUpgrades
        {
            get
            {
                foreach (var level in this.Props.Levels.Where(x => x.Level <= this.SiteLevel.Level))
                {
                    foreach (var upgrade in level.UnlocksUpgrades)
                    {
                        yield return upgrade;
                    }
                }
            }
        }

        public List<UpgradeBuildProgress> FinishedUpgrades
        {
            get
            {
                return this.Upgrades.FindAll(x => x.ConstructionProgress >= 1f);
            }
        }

        public float UpgradeYieldFactor
        {
            get
            {
                float effect = 1f;
                foreach (var upgrade in this.FinishedUpgrades)
                {
                    effect *= upgrade.Def.YieldBoost;
                }
                return effect;
            }
        }

        public float UpgradeSpeedFactor
        {
            get
            {
                float effect = 1f;
                foreach (var upgrade in this.FinishedUpgrades)
                {
                    effect *= upgrade.Def.SpeedBoost;
                }
                return effect;
            }
        }

        protected virtual float ProductionSpeedModifier => 1f;


        public float ProductionIncreasePerTick
        {
            get
            {
                return 1 / (this.SiteLevel.DaysProductionCycle * GenDate.TicksPerDay) * this.WorkForceInfluence * this.UpgradeSpeedFactor * this.ConstructionInfluence * ProductionSpeedModifier;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            this.Production();
            this.BuildUpgrades();
        }

        private void BuildUpgrades()
        {
            foreach (var upgrade in this.Upgrades)
            {
                upgrade.ConstructionProgress += upgrade.Def.ConstructionTimeDays / GenDate.TicksPerDay;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            ProductionSiteLevelDef nextLevel = this.Props.Levels.FirstOrDefault(x => x.Level > this.SiteLevel.Level);
            if (nextLevel != null)
            {
                var UpgradeCommand = new Command_Action();
                UpgradeCommand.defaultLabel = "UpgradeProduction".Translate();
                UpgradeCommand.defaultDesc = "UpgradeProductionDesc".Translate(nextLevel.LabelCap);
                UpgradeCommand.icon = nextLevel.ExpandingTexture;
                UpgradeCommand.action = delegate
                {
                    this._siteLevel = nextLevel;
                };
                yield return UpgradeCommand;
            }
        }

        internal virtual void Production(bool ignoreRestrictions = false)
        {
            this.ProductionProgress += this.ProductionIncreasePerTick;
            if (this.ProductionProgress >= 1f)
            {
                this.ProductionProgress = 0f;
                this.CreateProducts();
            }
        }

        protected virtual void CreateProducts()
        {
            foreach (var product in this.ProductionOptions)
            {
                ThingDef stuff = product.thingDef.IsStuff ? GenStuff.DefaultStuffFor(product.thingDef) : null;
                int yield = this.ActualYieldPerCycle(product);
                ResourceEntry.InsertOrUpdate(ref this.ProductionSite.Stock, product.thingDef, stuff, yield);
            }
        }

        private int ActualYieldPerCycle(ProductionGenOption product)
        {
            int potentialYield = (int)(this.Yield * product.yieldFactor);
            int limitingResource = potentialYield;
            foreach (var resource in product.RequiredResources)
            {
                ResourceEntry entry = this.ProductionSite.Stock.FirstOrDefault(x => x.Def == resource.ThingDef);
                if (entry == null)
                {
                    limitingResource = 0;
                    break;
                }

                limitingResource = Math.Min(limitingResource, entry.Count);
            }
            return Math.Min(limitingResource, potentialYield);
        }

        public virtual void PostAdd()
        {

        }

        public virtual IEnumerable<ProductionGenOption> ProductionOptions
        {
            get
            {
                foreach (var fixedProduct in this.Props.FixedProducts)
                {
                    yield return fixedProduct;
                }
            }
        }

        public override void PostPostRemove()
        {
            base.PostPostRemove();
        }

        public virtual int Yield
        {
            get
            {
                return (int)(this.SiteLevel.BaseProduction * this.WorkForceInfluence * this.ProductivityBoost * this.UpgradeYieldFactor);
            }
        }

        private float ProductivityBoost = 1f;

        private float WorkForceInfluence = 1f;

        private void RecalculateWorkForce()
        {
            float temp = this.WorkForce.Sum(x => x.WorkerCount * x.AverageSkill) / 50f;
            this.WorkForceInfluence = temp;
        }

        public void AddToWorkforce(Pawn pawn)
        {
            WorkForce workForce = this.WorkForce.FirstOrDefault(x => x.PawnKind == pawn.kindDef);
            if (workForce != null)
            {
                workForce.AddWorker(pawn, this.Props.AssociatedSkill);
                this.RecalculateWorkForce();
            }
            else
            {
                WorkForce newWorkers = new WorkForce(pawn.kindDef);
                this.WorkForce.Add(newWorkers);
                newWorkers.AddWorker(pawn, this.Props.AssociatedSkill);
            }
            this.RecalculateWorkForce();
        }

        public void AddToWorkforce(PawnKindDef pawnKind, float skill)
        {
            WorkForce workForce = this.WorkForce.FirstOrDefault(x => x.PawnKind == pawnKind);
            if (workForce != null)
            {
                workForce.AddWorker(skill);
            }
            else
            {
                WorkForce newWorkers = new WorkForce(pawnKind);
                this.WorkForce.Add(newWorkers);
                newWorkers.AddWorker(skill);
            }
            this.RecalculateWorkForce();
        }

        private ProductionSiteLevelDef _siteLevel;

        public ProductionSiteLevelDef SiteLevel
        {
            get
            {
                if (_siteLevel == null)
                {
                    _siteLevel = this.Props.Levels.FirstOrDefault(x => x.Level == this._curLevelInt);
                }
                return _siteLevel;
            }
        }

        public float ConstructionInfluence
        {
            get
            {
                return 1f;
            }
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
            foreach (var level in this.Props.Levels)
            {
                level.PostLoad();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look<WorkForce>(ref WorkForce, "WorkForce", LookMode.Deep, new object[0]);
            Scribe_Values.Look<float>(ref this.ProductionProgress, "ProductionProgress", 0f);
            Scribe_Values.Look<float>(ref this.WorkForceInfluence, "WorkForceInfluence", 1f);
        }

        internal void SetLevel(ProductionSiteLevelDef nextLevel)
        {
            this._curLevelInt = nextLevel.Level;
            this._siteLevel = null;
        }
    }
}
