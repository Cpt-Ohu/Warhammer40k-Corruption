using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Corruption.Tithes
{
    public class TitheContainer : Building_Casket
    {
        private CorruptionStoryTracker storyTracker
        {
            get
            {
                return CFind.StoryTracker;
            }
        }

        public CompTitheContainer compTithe;

        public bool LoadingActive = false;

        public Dictionary<TitheEntryGlobal, bool> tithesEnabled;

        public List<TitheEntryForContainer> currentTitheEntries = new List<TitheEntryForContainer>();

        public List<ThingDef> titheDefsEnabled = new List<ThingDef>();

        public TitheContainer()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            this.tithesEnabled = new Dictionary<TitheEntryGlobal, bool>();
        }

        public override void PostMake()
        {
            base.contentsKnown = true;
            base.PostMake();
        }

        public override bool Accepts(Thing thing)
        {
            return base.Accepts(thing);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.UpdateEntries();
            this.compTithe = this.TryGetComp<CompTitheContainer>();
            if (this.compTithe == null)
            {
                Log.Error("No CompTithe found for Tithe Container. Defaulting.");
                this.compTithe = new CompTitheContainer();
            }
            this.GetActiveThingDefs();
        }

        public float massUsage
        {
            get
            {
                float totalFreightMass = 0f;
                for (int i = 0; i < this.titheDefsEnabled.Count; i++)
                {
      //              Log.Message("A");
                    float mass = 0f;
                    StatModifier stat = new StatModifier();
                    if (titheDefsEnabled[i].statBases != null && titheDefsEnabled[i].statBases.Any(x => x.stat == StatDefOf.Mass))
                    {
                        stat = titheDefsEnabled[i].statBases.First(x => x.stat == StatDefOf.Mass);
                        mass = stat.value;
                    }
 
     //               Log.Message("C");
                    float stack = this.innerContainer.TotalStackCountOfDef(titheDefsEnabled[i]);
     //               Log.Message("X2");
                    totalFreightMass += stack * mass;
    //                Log.Message("X3");
                }
    //            Log.Message("B");
                return totalFreightMass / compTithe.tProps.maxContainerCapacity;
            }
        }

        public void UpdateEntries()
        {
            this.tithesEnabled.Clear();
            foreach (TitheEntryGlobal current in this.storyTracker.currentTithes)
            {
                this.tithesEnabled.Add(current, false);
                currentTitheEntries.Add(new TitheEntryForContainer(current));
            }
            this.GetActiveThingDefs();
        }

        public override void Tick()
        {
            base.Tick();
            if (storyTracker.setTithesDirty)
            {
                this.UpdateEntries();
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look<TitheEntryForContainer>(ref this.currentTitheEntries, "currentTitheEntries", LookMode.Deep);
            base.ExposeData();
        }

        public void GetActiveThingDefs()
        {
            this.titheDefsEnabled.Clear();
            foreach (TitheEntryForContainer entry in this.currentTitheEntries)
            {
                this.titheDefsEnabled.AddRange(entry.Tithe.thingDefs);
            }
        }
        
    }
}
