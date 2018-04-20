using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Corruption.Worship.Wonders;

namespace Corruption.Worship
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_Worship : MainTabWindow
    {
        private Vector2 scrollPositionInfo = Vector2.zero;
        private Vector2 scrollPositionWonder = Vector2.zero;

        private PatronDef SelectedGod;


        private static readonly Texture2D HighlightTex = ContentFinder<Texture2D>.Get("UI/HeroArt/Storytellers/Highlight", true);
        
        private PatronDef PlayerGod
        {
            get
            {
                return CorruptionStoryTrackerUtilities.currentStoryTracker.PlayerGod;
            }
        }

        private Color PatronColor
        {
            get
            {
                return SelectedGod.MainColor;
            }
        }

        private Texture2D WorshipProgressTex
        {
            get
            {
                return SolidColorMaterials.NewSolidColorTexture(SelectedGod.MainColor);
            }
        }
        
        public override void PreOpen()
        {
            base.PreOpen();
            this.SelectedGod = this.PlayerGod;
        }

        public override void DoWindowContents(Rect fillRect)
        {            
            Rect inRect = fillRect;
            this.SetInitialSizeAndPosition();
            Rect SelectionRect = inRect;
            SelectionRect.height = 68f;

            DrawSelectionRows(SelectionRect);

            float bottomLineY = SelectionRect.yMax;
            Widgets.ListSeparator(ref bottomLineY, inRect.width, "");

            Rect PatronOverviewRect = new Rect(0f, bottomLineY + 16f, 256f, this.windowRect.yMax - bottomLineY);
            GUI.BeginGroup(PatronOverviewRect);

            Text.Anchor = TextAnchor.MiddleLeft;
            Rect patronTitleRect = new Rect(0f, 0f, 256f, 64f);
            Text.Font = GameFont.Medium;
            Widgets.Label(patronTitleRect, "Patron".Translate());
            GUI.color = SelectedGod.MainColor;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(patronTitleRect, SelectedGod.label);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Rect patronImageRect = new Rect(patronTitleRect);
            patronImageRect.y = patronTitleRect.yMax + 4f;
            patronImageRect.height = 256f;
            GUI.DrawTexture(patronImageRect, SelectedGod.Texture);
            GUI.EndGroup();

            Rect PatronInfoRect = new Rect(PatronOverviewRect);
            PatronInfoRect.x = PatronOverviewRect.xMax + 8f;
            PatronInfoRect.y = patronImageRect.y + PatronOverviewRect.y;
            PatronInfoRect.width = this.windowRect.width - PatronInfoRect.xMax - 8f;

            GUI.BeginGroup(PatronInfoRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect descriptionTitleRect = new Rect(0f, 0f, PatronInfoRect.width, 32f);
            descriptionTitleRect.height = 32f;
            Widgets.Label(descriptionTitleRect, "Description".Translate());
            String desc = SelectedGod.label + "_Description";
            Rect descriptionRect = new Rect(descriptionTitleRect);
            descriptionRect.y = descriptionTitleRect.yMax + 4f;
            descriptionRect.height = 64f;
            Text.Font = GameFont.Small;
            Widgets.LabelScrollable(descriptionRect, SelectedGod.description, ref scrollPositionInfo, true);

            Text.Anchor = TextAnchor.UpperLeft;

            Rect infoScrollRect = new Rect(descriptionRect);
            infoScrollRect.y = descriptionRect.yMax + 8f;
            infoScrollRect.height = 256f;
            Rect viewRect = new Rect(descriptionRect);
            viewRect.height = Text.LineHeight * this.SelectedGod.PsykerPowers.Count;
            viewRect.width -= 32f;

            Widgets.BeginScrollView(infoScrollRect, ref this.scrollPositionInfo, viewRect);

            Listing_Standard Listing_PermaInfo = new Listing_Standard();
            Listing_PermaInfo.Begin(viewRect);
            int index = 0;
            Text.Font = GameFont.Small;

            SoulTraitDegreeData sData = this.SelectedGod.PatronTraits[0].SDegreeDatas[0];


            Rect traitRect = Listing_PermaInfo.GetRect(Text.LineHeight);
            if (Mouse.IsOver(traitRect))
            {
                Widgets.DrawHighlight(traitRect);
                TooltipHandler.TipRegion(traitRect, sData.description);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(traitRect, "PatronTrait".Translate());
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(traitRect, sData.label);

            
            Text.Anchor = TextAnchor.MiddleLeft;
            foreach (PsykerPowerDef def in this.SelectedGod.PsykerPowers)
            {
                Rect fullRect = Listing_PermaInfo.GetRect(Text.LineHeight);
                if (Mouse.IsOver(fullRect))
                {
                    Widgets.DrawHighlight(fullRect);
                    TooltipHandler.TipRegion(fullRect, def.description);
                }
                Rect iconRect = new Rect(fullRect.x, fullRect.y, Text.LineHeight, Text.LineHeight);
                GUI.DrawTexture(iconRect, def.uiIcon);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(fullRect, def.label);
                Text.Anchor = TextAnchor.MiddleLeft;
                index++;
            }
            Listing_PermaInfo.End();
            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.EndGroup();

            //Rect WonderRect = new Rect(infoScrollRect);
            //WonderRect.x = infoScrollRect.xMax + 4f;
            //Listing_Standard Listing_Wonders = new Listing_Standard();
            //Widgets.BeginScrollView(WonderRect, ref this.scrollPositionWonder, viewRect);
            //Listing_Wonders.Begin(WonderRect);

            //foreach (WonderDef def in this.SelectedGod.Wonders)
            //{

            //    Rect fullRect2 = Listing_PermaInfo.GetRect(Text.LineHeight);
            //    Listing_Wonders.ButtonImage(def.WonderIcon, )
            //}

            //Listing_Wonders.End();
            //Widgets.EndScrollView();

            Rect soulmeterRect = fillRect;
            soulmeterRect.height = 96f;
            soulmeterRect.y = fillRect.yMax - 96f;

            //DrawWonderEndPoints(soulmeterRect);

            GUI.BeginGroup(soulmeterRect);
            DrawWonderMeter(soulmeterRect);
            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
            if (Widgets.CloseButtonFor(inRect.AtZero()))
            {
                this.Close();
            }
        }


        private int WorshipProgress
        {
            get
            {
                return CorruptionStoryTrackerUtilities.currentStoryTracker.PlayerWorshipProgressLookup[SelectedGod];
            }
        }

        private float WorshipProgressPercentage
        {
            get
            {
                return Math.Min((float)WorshipProgress / 10000f, 1f);
            }
        }

        private void DrawWonderMeter(Rect meterRect)
        {
            Rect bgRect = meterRect.ContractedBy(16f);
            bgRect.height = 32f;
            bgRect.y = 16f;
            bgRect.x += 64f;
            bgRect.width -= 128f;

            GUI.DrawTexture(bgRect, CorruptionStoryTrackerUtilities.BackgroundTile);
            Rect progressRect = new Rect(bgRect);
            progressRect.height = 15f;
            progressRect.y += 9f;
            Widgets.FillableBar(progressRect, WorshipProgressPercentage, WorshipProgressTex, CorruptionStoryTrackerUtilities.TransparentBackground, false);

            DrawWonderEndPoints(progressRect);
            DrawWonderNodes(progressRect);
        }

        private void DrawWonderEndPoints(Rect progressRect)
        {
            Rect NodeRect = new Rect(progressRect.x - 56f, 0f, 64f, 64f);
            GUI.DrawTexture(NodeRect, CorruptionStoryTrackerUtilities.SoulNode);
            NodeRect.x = progressRect.xMax - 8f;
            GUI.DrawTexture(NodeRect, CorruptionStoryTrackerUtilities.SoulNode);
        }

        private void DrawWonderNodes(Rect progressRect)
        {
            foreach (WonderDef wonderDef in this.SelectedGod.Wonders)
            {
                DrawWonderNode(wonderDef, progressRect);
            }
        }

        private void DrawWonderNode(WonderDef wonderDef, Rect progressRect)
        {
            float adjustedWonderCost = wonderDef.worshipCost / 10000f;
            float curX = progressRect.x + (adjustedWonderCost * progressRect.width);
            if (adjustedWonderCost == 0f)
            {
                curX = progressRect.x - 48f;
            }
            else if (adjustedWonderCost >= 1f)
            {
                curX = progressRect.xMax;
            }
            Rect nodeRect = new Rect(curX, 8f, 48f, 48f);
            
            if (Widgets.ButtonImage(nodeRect, wonderDef.WonderIcon))
            {
                if (wonderDef.worshipCost <= WorshipProgress)
                {
                    int pointsToConsume = wonderDef.worshipCost;

                    if (wonderDef.PointsScalable)
                    {
                        Func<int, string> textGetter;
                        textGetter = ((int x) => "SetWorshipPoints".Translate());
                        Dialog_Slider window = new Dialog_Slider(textGetter, wonderDef.worshipCost, WorshipProgress, delegate (int value)
                       {
                           wonderDef.Worker.TryExecuteWonder(value);
                           CorruptionStoryTrackerUtilities.ConsumeFavour(value, SelectedGod);
                       }, wonderDef.worshipCost);
                        Find.WindowStack.Add(window);
                    }
                    else
                    {
                        wonderDef.Worker.TryExecuteWonder(wonderDef.worshipCost);
                        CorruptionStoryTrackerUtilities.ConsumeFavour(wonderDef.worshipCost, SelectedGod);                        
                    }
                    this.Close(false);

                }
                else
                {
                    string text = "WonderInsufficientWorshipProgress".Translate();
                    if (!text.NullOrEmpty())
                    {
                        Messages.Message(text, MessageTypeDefOf.CautionInput);
                    }
                }

            }
            TipSignal tip4 = new TipSignal(() => wonderDef.description, (int)curX * 37);
            TooltipHandler.TipRegion(nodeRect, tip4);
        }

        private void DrawSelectionRows(Rect fillrect)
        {
            float rowX = 8f;
            foreach (PatronDef current in CorruptionStoryTrackerUtilities.currentStoryTracker.AvailableGods)
            {
                Rect drawRect = new Rect(rowX, fillrect.y, 64f, 64f);
                if (Widgets.ButtonImage(drawRect, current.SmallTexture))
                {
                    this.SelectedGod = current;
                }

                if (current == SelectedGod)
                {
                    GUI.DrawTexture(drawRect, HighlightTex);
                }
                rowX += 68f;
            }
        }
    }
}
