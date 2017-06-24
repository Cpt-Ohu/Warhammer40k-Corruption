using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public class CorruptionModController : Mod
    {
        public CorruptionModController(ModContentPack content) : base(content)
        {
            base.GetSettings<CorruptionModSettings>();
        }
        
        public override void DoSettingsWindowContents(Rect inRect)
        {
            GUI.BeginGroup(new Rect(0f, 60f, 800f, 800f));
            //Text.Font = GameFont.Medium;
            GUI.DrawTexture(new Rect(100f, 0f, 430f, 230f), CorruptionStoryTrackerUtilities.CorruptionLogoTexture);
            //Widgets.Label(new Rect(inRect.width / 2 - 150f, 0f, 300f, 40f), "CorruptionSettings".Translate());
            //Text.Font = GameFont.Small;
            Widgets.Label(new Rect(100f, 250f, 600f, 40f), "CorruptionSettingsDesc".Translate());


            GUI.BeginGroup(new Rect(0f, 300f, 600f, 600f));
            Rect rect1 = new Rect(100f, 0f, 400f, 30f);
            Rect rect2 = new Rect(100f, 35f, 400f, 30f);
            Rect rect3 = new Rect(100f, 70f, 400f, 30f);
            Rect rect4 = new Rect(100f, 105f, 400f, 30f);


            Widgets.CheckboxLabeled(rect1, "AllowFactions".Translate(), ref CorruptionModSettings.AllowFactions);
            this.DrawToolTip(rect1, "AllowFactionsTip".Translate());
            Widgets.CheckboxLabeled(rect2, "AllowDomination".Translate(), ref CorruptionModSettings.AllowDomination);
            this.DrawToolTip(rect2, "AllowDominationTip".Translate());
            Widgets.CheckboxLabeled(rect3, "AllowDropships".Translate(), ref CorruptionModSettings.AllowDropships);
            this.DrawToolTip(rect3, "AllowDropshipsTip".Translate());
            Widgets.CheckboxLabeled(rect4, "AllowPsykers".Translate(), ref CorruptionModSettings.AllowPsykers);
            this.DrawToolTip(rect4, "AllowPsykersTip".Translate());

            GUI.EndGroup();

            GUI.EndGroup();
        }

        private void DrawToolTip(Rect rect, string content)
        {
            TipSignal tip = content;
            TooltipHandler.TipRegion(rect, tip);
        }

        public override string SettingsCategory()
        {
            return "Warhammer 40K Corruption";
        }
    }
}
