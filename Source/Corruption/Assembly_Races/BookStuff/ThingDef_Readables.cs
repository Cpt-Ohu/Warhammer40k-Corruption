using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Corruption.BookStuff
{
    public class ThingDef_Readables : ThingDef
    {
        //Books
        public List<string> BookText = new List<string>();
        public bool IsABook = false;
        public string CloseTexture = "";
        public int TicksToRead = 1500;
        public float TextSpeedShowFactor = 1f;
        public SkillDef SkillToLearn;
        public float SkillGainFactor;
        public List<ReadableEffektEntry> ReadableEffectEntries;
        public SoulItemCategories soulItemCategory = SoulItemCategories.Neutral;
        public float SoulGainRate = 0f;
        public List<String> BookCategories;

        public override void ResolveReferences()
        {
            if (this.IsABook)
            {
                this.uiIconPath = this.CloseTexture;
            }
            base.ResolveReferences();

        }

    }
}
