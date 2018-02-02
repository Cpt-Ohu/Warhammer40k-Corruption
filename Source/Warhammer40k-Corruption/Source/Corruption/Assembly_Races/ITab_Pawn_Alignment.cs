using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace Corruption
{
    public class ITab_Pawn_Alignment : ITab
    {
        public String SelPawnPatron;

        public Texture PatronImage;

        public ITab_Pawn_Alignment()
        {
            this.SelPawnPatron = this.SelPawn.needs.TryGetNeed<Need_Soul>().PatronName(SelPawn);
            this.size = new Vector2(440f, 450f);
            String texpath = SelPawnPatron + "_bg";
            this.PatronImage = ContentFinder<Texture2D>.Get(texpath, true);
        }

        public ITab_Pawn_Alignment(Pawn pawn)
        {
            this.SelPawnPatron = pawn.needs.TryGetNeed<Need_Soul>().PatronName(SelPawn);
            this.size = new Vector2(440f, 450f);
            String texpath = "UI/Emperor_bg";
            this.PatronImage = ContentFinder<Texture2D>.Get(texpath, true);
        }

        protected override void FillTab()
        {
            Rect Rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
            this.Draw(Rect);
        }

        public void Draw(Rect mainRect)
        {
            Rect rect = new Rect(mainRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, SelPawnPatron);
            Rect rect2 = new Rect((rect.x / 2) + 150f, rect.y + 20f, 300f, 300f);
            Widgets.DrawTextureFitted(rect2, PatronImage as Texture2D, 1f);
            Rect rect3 = new Rect(rect2);
            rect3.y += 30f;
            Widgets.Label(rect3, "Loyoal to");
        }
    }
}
