using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.IoM
{
    public class CompResourcePack : ThingComp
    {
        public bool IsTribute;

        public bool LoadEnabled;

        public CompProperties_ResourcePack Props
        {
            get
            {
                return (CompProperties_ResourcePack)this.props;
            }
        }

        public int TotalCount
        {
            get
            {
              return  this.Resources.Sum(x => x.Count);
            }
        }

        public int remainingCapacity
        {
            get
            {
                return this.Props.Capacity - this.Resources.Sum(x => x.Count);
            }

        }

        public List<ResourcePackEntry> Resources = new List<ResourcePackEntry>();
                
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {

            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            
            yield return new Command_Toggle
            {
                defaultLabel = "CommandDesignateTribute".Translate(),
                defaultDesc = "CommandDesignateTributeDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
                hotKey = KeyBindingDefOf.Misc6,
                toggleAction = delegate
                {
                    this.IsTribute = !this.IsTribute;
                },
                isActive = (() => this.IsTribute)
				};

            yield return new Command_Toggle
            {
                defaultLabel = "CommandEnableRessourceLoad".Translate(),
                defaultDesc = "CommandEnableRessourceLoadDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Designators/Open", true),
                hotKey = KeyBindingDefOf.Misc5,
                toggleAction = delegate
                {
                    this.LoadEnabled = !this.LoadEnabled;
                },
                isActive = (() => this.LoadEnabled)
            };

            if (Prefs.DevMode && !this.Full)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandDebugFillResourcePack".Translate(),
                    defaultDesc = "CommandDebugFillResourceDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/Open", true),
                    action = delegate
                    {
                        this.FillRandomly();
                    }
                };
            }
        }


        public void FillRandomly()
        {
            int availableCapacity = this.remainingCapacity;
            List<ThingDef> thingDefs = DefDatabase<ThingDef>.AllDefs.Where(x => this.Props.filters.Any(y => y.Allows(x))).ToList();
            for (int i = 0; i < 3; i++)
            {
                if (availableCapacity > 0)
                {
                    ThingDef def = thingDefs.RandomElement();
                    Thing newCargo = ThingMaker.MakeThing(def);
                    newCargo.stackCount = Rand.Range((int)(availableCapacity * 0.33f), (int)(availableCapacity));
                    this.TryAddRessource(newCargo);
                    availableCapacity -= newCargo.stackCount;
                }
            }
        }

        public void TryAddRessource(Thing t)
        {
            QualityCategory quality = QualityCategory.Normal;
            t.TryGetQuality(out quality);
            ResourcePackEntry entry = this.Resources.FirstOrDefault(x => x.Def == t.def);
            if (entry != null)
            {
                entry.Count += Math.Min(t.stackCount, this.Props.Capacity - this.Resources.Sum(x => x.Count));
                entry.AvgQualityCategory = (QualityCategory)((int)(((int)entry.AvgQualityCategory * entry.Count) + (t.stackCount * (int)quality)) / (t.stackCount + entry.Count));
            }
            else
            {
                entry = new ResourcePackEntry();
                entry.Count = t.stackCount;
                entry.Def = t.def;
                entry.AvgQualityCategory = quality;
                this.Resources.Add(entry);
            }
            t.Destroy();
        }

        public bool Full
        {
            get
            {
                return this.Resources.Sum(x => x.Count) >= this.Props.Capacity;
            }
        }

        public bool ThingAllowed(Thing thing)
        {
            if (this.Resources.Count < 4)

            {
                return true;
            }
            else if (this.Resources.Any(x => x.Def == thing.def))
            {
                return true;
            }
            return false;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var resource in this.Resources)
            {
                stringBuilder.AppendLine(resource.Def.label + " x " + resource.Count + "(" + resource.AvgQualityCategory.ToString() + ")");
            }
            return stringBuilder.ToString().Trim();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.IsTribute, "isTribute", false);
            Scribe_Values.Look<bool>(ref this.LoadEnabled, "loadEnabled", false);
            Scribe_Collections.Look<ResourcePackEntry>(ref this.Resources, "Resources", LookMode.Deep);
        }
    }
}
