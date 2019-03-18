using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.ProductionSites
{
    public class WorldObjectCompProperties_ResourceProduction : WorldObjectCompProperties
    {
        public List<ProductionSiteLevelDef> Levels = new List<ProductionSiteLevelDef>();

        public TraderKindDef TraderKind;

        public List<BiomeDef> AllowedBiomes = new List<BiomeDef>();

        public List<Hilliness> AllowedHilliness;

        public TechLevel RequiredTechLevel = TechLevel.Neolithic;

        public List<ProductionGenOption> FixedProducts = new List<ProductionGenOption>();

        public SkillDef AssociatedSkill;
                
        public override void ResolveReferences(WorldObjectDef parentDef)
        {
            base.ResolveReferences(parentDef);
        }
    }
}
