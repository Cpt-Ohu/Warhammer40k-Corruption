using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Servitors
{
    public class HediffComp_ReprogramServitors : HediffComp
    {
        public HediffCompProperties_ReprogramServitor CProps
        {
            get
            {
                return (HediffCompProperties_ReprogramServitor)this.props;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            foreach(SkillDef def in this.CProps.skillsToGain)
            {
                SkillRecord skill = this.Pawn.skills.GetSkill(def);
                skill.Level = this.CProps.SkillLevel; ;
                skill.passion = Passion.Major;
            }
        }

        private void ClearSkillsAndPassions()
        {
            this.Pawn.skills.skills.ForEach(x => 
            {
                x.Level = 0;
                x.passion = Passion.None;
            });
        }
        
    }
}
