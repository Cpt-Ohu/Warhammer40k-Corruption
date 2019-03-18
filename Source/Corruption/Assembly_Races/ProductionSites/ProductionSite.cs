using Corruption.Domination;
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
    [StaticConstructorOnStartup]
    public class ProductionSite : Settlement, IWorldObjectBuilder
    {

        public static Texture2D SendDropPodIcon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");
        public List<ResourceEntry> Stock = new List<ResourceEntry>();
        public List<WorkForce> UnemployedWorkForce = new List<WorkForce>();

        public ProductionSite()
        {
            this.trader = new ProductionSite_TraderTracker(this);
        }

        private ResourceProductionComp _mainProduction;
        public ResourceProductionComp MainProduction
        {
            get
            {
                if (_mainProduction == null)
                {
                    _mainProduction = this.GetComponent<ResourceProductionComp>();
                }
                return _mainProduction;
            }
        }

        private bool WorkersAvailable
        {
            get
            {
                return !this.UnemployedWorkForce.NullOrEmpty();
            }
        }

        private bool CanSendDropPods
        {
            get
            {
                return this.MainProduction.Upgrades.Any(x => x.Def.defName.Equals("UpgradeDropPodLauncher"));
            }
        }

        public IEnumerable<ResourceProductionComp> AllProduction
        {
            get
            {
                return this.AllComps.Where(x => typeof(ResourceProductionComp).IsAssignableFrom(x.GetType())).Cast<ResourceProductionComp>();
            }
        }

        public void AddToStock(ThingDef thingDef, int count)
        {

        }

        public IEnumerable<ProductionSiteUpgradeDef> AllUpgrades
        {
            get
            {
                return this.AllProduction.SelectMany(x => x.AvailableUpgrades);
            }
        }

        private Material cachedMat;

        public override Material Material
        {
            get
            {
                if (this.cachedMat == null)
                {
                    this.cachedMat = MaterialPool.MatFrom(this.MainProduction.SiteLevel.expandingTexturePath, ShaderDatabase.WorldOverlayTransparentLit, base.Faction.Color, WorldMaterials.WorldObjectRenderQueue);
                }
                return this.cachedMat;
            }
        }

        public override Texture2D ExpandingIcon
        {
            get
            {
                return this.MainProduction.SiteLevel.ExpandingTexture;
            }
        }
        
        public override string GetInspectString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(this.MainProduction.SiteLevel.label);
            builder.Append(base.GetInspectString());
            return builder.ToString();
        }

        public int MaxLaunchDistance { get; private set; }
        public bool IsPlayerControlled
        {
            get
            {
                return this.Faction == Faction.OfPlayer;
            }
        }

        public List<ThingDefCountClass> Resources => this.Stock.Select(x => new ThingDefCountClass(x.Def, x.Count)).ToList();

        public IEnumerable<WorkForce> TotalWorkForce
        {
            get
            {
                foreach (var unemployed in this.UnemployedWorkForce)
                {
                    yield return unemployed;
                }
                foreach (var production in this.AllProduction)
                {
                    foreach (var workForce in production.WorkForce)
                    {
                        yield return workForce;
                    }
                }
            }
        }

        public void BuildUpgrade(ProductionSiteUpgradeDef def)
        {
            ResourceProductionComp production = this.AllProduction.FirstOrDefault(x => x.AvailableUpgrades.Contains(def));
            if (production != null)
            {
                production.Upgrades.Add(new UpgradeBuildProgress(def, 0));
                foreach (var costEntry in def.Cost)
                {
                    var stock = this.Stock.FirstOrDefault(x => x.Def == costEntry.thingDef);
                    if (stock != null)
                    {
                        stock.Count -= costEntry.count;
                    }
                }
            }
        }

        public bool CanBuildUpgrade(ProductionSiteUpgradeDef def)
        {
            bool result = true;
            foreach (var costEntry in def.Cost)
            {
                if (!this.HasResource(costEntry.thingDef, costEntry.count))
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        public bool HasResource(ThingDef thingDef, int countRequired)
        {
            List<ResourceEntry> resourceEntries = this.Stock.FindAll(x => x.Def == thingDef);

            return resourceEntries != null && resourceEntries.Sum(x => x.Count) >= countRequired;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (this.CanSendDropPods)
            {
                Command_Action sendCommand = new Command_Action();
                sendCommand.defaultLabel = "SendSiteDropPods".Translate();
                sendCommand.defaultDesc = "SendSiteDropPodsDesc".Translate();
                sendCommand.action = delegate
                {
                    this.SendDropPods();
                };
                sendCommand.icon = ProductionSite.SendDropPodIcon;
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Dev: Finish Production Cycle",
                    action = delegate
                    {
                        FinishProductionCycles();
                    }
                };
            }
        }

        public void FinishProductionCycles()
        {
            foreach (var production in this.AllProduction)
            {
                production.ProductionProgress = 1f;                
                production.Production(true);
            }
        }

        private void SendDropPods()
        {


        }

        public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
        {
            foreach (var option in base.GetTransportPodsFloatMenuOptions(pods, representative))
            {
                yield return option;
            }

            if (this.IsPlayerControlled || CFind.DominationTracker.PlayerAlliance.GetFactions().Contains(this.Faction))
            {
                foreach (var o in TransportPodsArrivalAction_AddStock.GetFloatMenuOptions(representative, pods, this))
                {
                    yield return o;
                }
            }
        }
        public override void PostAdd()
        {
            base.PostAdd();
            foreach (var production in this.AllProduction)
            {
                production.PostAdd();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<ResourceEntry>(ref Stock, "Stock", LookMode.Deep, new object[0]);
            Scribe_Collections.Look<WorkForce>(ref UnemployedWorkForce, "UnemployedWorkForce", LookMode.Deep, new object[0]);
        }
    }
}
