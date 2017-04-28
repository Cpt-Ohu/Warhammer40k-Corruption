using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{ 
    public class Dialog_ChooseColor : Window
    {
        private float[] RGB = new float[3];

        private Color oldColor;

        private string title = "PrimaryColor";

        private bool isSecondaryColor;

        public Dialog_ChooseColor(Color color, bool IsSecondaryColor = false)
        {
            this.oldColor = new Color(color.r, color.g, color.b);
            this.isSecondaryColor = IsSecondaryColor;
            if (IsSecondaryColor)
            {
                this.title = "SecondaryColor";
            }
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(250f, 150f);
            }
        }
        
        public void DrawFactionColorSliders(Rect rect, string title)
        {
            Color color = this.oldColor;
            Rect rectTitle = new Rect(rect.x, rect.y, rect.width, 20f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rectTitle, title);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.red;
            Rect rectSlider = rect;
            rectSlider.y += 20f;
            this.RGB[0] = GUI.HorizontalSlider(new Rect(rectSlider.x + 40f, rectSlider.y - 1f, 136f, 16f), RGB[0], 0f, 1f);
            GUI.color = Color.green;
            this.RGB[1] = GUI.HorizontalSlider(new Rect(rectSlider.x + 40f, rectSlider.y + 19f, 136f, 16f), RGB[1], 0f, 1f);
            GUI.color = Color.blue;
            this.RGB[2] = GUI.HorizontalSlider(new Rect(rectSlider.x + 40f, rectSlider.y + 39f, 136f, 16f), RGB[2], 0f, 1f);
            GUI.color = Color.white;
            Color newColor = new Color(RGB[0], RGB[1], RGB[2]);
            Widgets.DrawRectFast(new Rect(rect.x, rect.y, 32f, 32f), newColor);
            Rect rect2 = new Rect(rect.width / 2 - 50f, rectSlider.y + 59f, 100f, 25f);

            if (Widgets.ButtonText(rect2, "Confirm".Translate(), true, true))
            {
                if (this.isSecondaryColor)
                {
                    FactionColorUtilities.currentPlayerStoryTracker.PlayerColorTwo = newColor;
                }
                else
                {
                    FactionColorUtilities.currentPlayerStoryTracker.PlayerColorOne = newColor;
                }                
                this.Close(true);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            this.RGB[0] = this.oldColor.r;
            this.RGB[1] = this.oldColor.g;
            this.RGB[2] = this.oldColor.b;
        }

        public override void DoWindowContents(Rect inRect)
        {
            this.forcePause = true;
            Rect rect = inRect;
            GUI.BeginGroup(inRect);
            Rect rect2 = rect.ContractedBy(10f);
            rect2.width -= 20f;
            DrawFactionColorSliders(rect2, title.Translate());
            Rect rect3 = new Rect(inRect.x, inRect.y, 15f, 15f);
            if (Widgets.CloseButtonFor(inRect.AtZero()))
            {
                this.Close(true);
            }
            GUI.EndGroup();
        }
    }

    internal class Dialog_ChooseBanner : Window
    {
        private List<string> options;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(300f, 500f);
            }
        }

        public Dialog_ChooseBanner(List<string> options)
        {
            this.options = options;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = inRect;
            GUI.BeginGroup(inRect);
            Rect rect2 = rect.ContractedBy(10f);
            rect2.height = 20f;

            for (int i = 0; i < this.options.Count; i++)
            {
                if (Widgets.RadioButtonLabeled(rect2, options[i], FactionColorUtilities.currentPlayerStoryTracker.BannerGraphicPath == "UI/Flags/"+options[i]))
                {
                    FactionColorUtilities.currentPlayerStoryTracker.BannerGraphicPath = "UI/Flags/" + options[i];
                }
                rect2.y += 28f;
            }
            Rect rect3 = new Rect(rect2.x + rect2.width / 2 - 50f, rect2.y, 100f, 25f);
            if (Widgets.ButtonText(rect3, "Confirm".Translate(), true, true))
            {
                this.Close(true);
            }
            if (Widgets.CloseButtonFor(inRect.AtZero()))
            {
                this.Close(true);
            }
            GUI.EndGroup();
        }
    }
}

