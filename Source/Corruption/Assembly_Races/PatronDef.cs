using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Corruption.Worship;
using Corruption.Worship.Wonders;

namespace Corruption
{
    public class PatronDef : Def
    {
        public string SmallTexturePath = "UI/Emperor_bg";

        public string TexturePath = "UI/Emperor_bg";

        public string WorshipBarPath = "UI/Background/WorshipBarEmperor";

        public bool IsChaosGod;

        public Texture2D SmallTexture;

        public Texture2D Texture;

        public Texture2D worshipBarTexture;
        
        public List<SoulTraitDef> PatronTraits = new List<SoulTraitDef>();

        public Color MainColor;

        public List<PsykerPowerDef> PsykerPowers = new List<PsykerPowerDef>();

        public List<WonderDef> Wonders = new List<WonderDef>();

        public GameConditionDef wonderOverlayDef;
        
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                this.SmallTexture = ContentFinder<Texture2D>.Get(this.SmallTexturePath, true);
                    this.Texture = ContentFinder<Texture2D>.Get(this.TexturePath, true);
                this.worshipBarTexture = ContentFinder<Texture2D>.Get(this.WorshipBarPath, true);
            });
        }
    }
}
