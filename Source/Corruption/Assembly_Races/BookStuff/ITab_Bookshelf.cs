using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;

namespace Corruption.BookStuff
{
    public class ITab_Bookshelf : ITab
    {
        private const float TopPadding = 20f;

        private Texture2D dropbutton;

        private const float ThingIconSize = 28f;

        private const float ThingRowHeight = 28f;

        private const float ThingLeftX = 36f;

        private const float StandardLineHeight = 22f;

        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        private static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private static List<Thing> workingInvList = new List<Thing>();

        private bool CanControl
        {
            get
            {
                return this.Shelf.Faction == Faction.OfPlayer;
            }
        }

        private Bookshelf Shelf
        {
            get
            {
                if (base.SelThing != null)
                {
                    return (Bookshelf)base.SelThing;
                }

                throw new InvalidOperationException("Bookshelf Tab on null thing");
            }
        }

        public ITab_Bookshelf()
        {
            this.size = new Vector2(440f, 300);
            this.labelKey = "TabBookshelf";
            this.tutorTag = "Bookshelf";
        }


        protected override void FillTab()
        {
            this.dropbutton = ContentFinder<Texture2D>.Get("UI/Buttons/DropBook", true);
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, 20f, this.size.x, this.size.y - 20f);
            Rect rect2 = rect.ContractedBy(10f);
            Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, position.width, position.height);
            Rect viewRect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect);
            float num = 50f;
            if (this.Shelf.StoredBooks != null)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(viewRect, "StoredBooks".Translate());                
                Text.Font = GameFont.Small;
                for (int i = 0; i < this.Shelf.StoredBooks.Count; i++)
                {
                    this.DrawThingRow(ref num, viewRect.width, this.Shelf.StoredBooks[i]);
                }
            }
            if (Event.current.type == EventType.Layout)
            {
                this.scrollViewHeight = num + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawThingRow(ref float y, float width, ThingDef thingDef)
        {
            Rect rect = new Rect(0f, y, width, 28f);
            Widgets.InfoCardButton(rect.width - 24f, y, thingDef);
            rect.width -= 24f;
            if (this.CanControl)
            {
                Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                TooltipHandler.TipRegion(rect2, "DropThing".Translate());
                if (Widgets.ButtonImage(rect2, dropbutton))
                {
                    SoundDefOf.TickHigh.PlayOneShotOnCamera();
                    this.InterfaceDrop(thingDef, this.Shelf);
                }
                rect.width -= 24f;
            }
            if (Mouse.IsOver(rect))
            {
                GUI.color = ITab_Bookshelf.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
                Rect rect12 = rect;
                ThingDef_Readables readablesDef = (ThingDef_Readables)thingDef;
                string learnableStuff = "Nothing";
                if (readablesDef.SkillToLearn != null)
                {
                    learnableStuff = readablesDef.SkillToLearn.label;
                }
                TooltipHandler.TipRegion(rect12, "GetBookToolTip".Translate(new object[]
                {
                    readablesDef.description,
                    learnableStuff,
                }));
            }
            if (thingDef.DrawMatSingle != null && thingDef.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thingDef);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Bookshelf.ThingLabelColor;
            Rect rect3 = new Rect(36f, y, width - 36f, 28f);
            string text = thingDef.LabelCap;

            Widgets.Label(rect3, text);
            y += 28f;
        }

        private void InterfaceDrop(ThingDef def, Bookshelf shelf)
        {
            Thing DroppedBook;
            shelf.StoredBooks.Remove(def);
            DroppedBook = ThingMaker.MakeThing(def);
            GenPlace.TryPlaceThing(DroppedBook, shelf.Position, this.Shelf.Map, ThingPlaceMode.Near);

        }

    }
}
