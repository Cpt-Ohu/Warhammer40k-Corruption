using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Corruption.Domination
{
    public class Dialog_ChooseBattleSides : Window
    {
        private BattleZone battleZone;
        private string curFactionName
        {
            get
            {
                if (this.battleZone.PlayerChosen == null)
                {
                    return "None".Translate();
                }
                else if (this.battleZone.PlayerChosen == CorruptionStoryTrackerUtilities.currentStoryTracker.DominationTracker.PlayerAlliance)
                {
                    return "BattleAttackBoth".Translate();
                }
                else
                {
                    return this.battleZone.PlayerChosen.AllianceName;
                }
            }
        }

        public Dialog_ChooseBattleSides(BattleZone battleZone)
        {
            this.forcePause = true;
            this.closeOnEscapeKey = true;
            this.absorbInputAroundWindow = true;
            this.battleZone = battleZone;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 300f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect rect = new Rect(inRect);
            rect.height = 50f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, "ChooseBattleSideTitle".Translate());

            Text.Font = GameFont.Small;
            Rect selectorRect = new Rect(100f, 150f, 300f, 56f);
            if (Widgets.ButtonText(selectorRect, curFactionName, true, false, true))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption("SpectateBattle".Translate(), delegate { battleZone.PlayerChosen = null; }, MenuOptionPriority.Default, null, null, 0, null));
                list.Add(new FloatMenuOption("BattleAttackBoth".Translate(), delegate { battleZone.PlayerChosen = CorruptionStoryTrackerUtilities.currentStoryTracker.DominationTracker.PlayerAlliance; }, MenuOptionPriority.Default, null, null, 0, null));
                foreach (PoliticalAlliance current in battleZone.WarringAlliances)
                {
                    list.Add(new FloatMenuOption(current.AllianceName.CapitalizeFirst(), delegate { battleZone.PlayerChosen = current; }, MenuOptionPriority.Low, null, null, 0, null));

                }
                Find.WindowStack.Add(new FloatMenu(list, null, false));
            }
            //Rect prevButtonRect = new Rect(selectorRect.x - Textures.TextureButtonPrevious.width - 3, selectorRect.y + 4, Textures.TextureButtonPrevious.width, Textures.TextureButtonPrevious.height);
            //Rect nextButtonRect = new Rect(selectorRect.x + selectorRect.width + 1, selectorRect.y + 4, Textures.TextureButtonPrevious.width, Textures.TextureButtonPrevious.height);
            //if (prevButtonRect.Contains(Event.current.mousePosition))
            //{
            //    GUI.color = ButtonHighlightColor;
            //}
            //else
            //{
            //    GUI.color = ButtonColor;
            //}
            //GUI.DrawTexture(prevButtonRect, Textures.TextureButtonPrevious);
            //if (Widgets.ButtonInvisible(prevButtonRect, false))
            //{
            //    SoundDefOf.TickTiny.PlayOneShotOnCamera();
            //    battleZone.PlayerChosen = this.battleZone.WarringFactions[(battleZone.WarringFactions.IndexOf(currentChosenFaction) + 1) == battleZone.WarringFactions.Count ? 0 : (battleZone.WarringFactions.IndexOf(currentChosenFaction) + 1)];
            //}

            //if (nextButtonRect.Contains(Event.current.mousePosition))
            //{
            //    GUI.color = ButtonHighlightColor;
            //}
            //else
            //{
            //    GUI.color = ButtonColor;
            //}
            //GUI.DrawTexture(nextButtonRect, Textures.TextureButtonNext);
            //if (Widgets.ButtonInvisible(nextButtonRect, false))
            //{
            //    SoundDefOf.TickTiny.PlayOneShotOnCamera();
            //    battleZone.PlayerChosen = this.battleZone.WarringFactions[(battleZone.WarringFactions.IndexOf(currentChosenFaction) - 1) == 0 ? battleZone.WarringFactions.Count : (battleZone.WarringFactions.IndexOf(currentChosenFaction) - 1)];
            //}
            
            Rect rectA = new Rect(0f, selectorRect.yMax + 10f, 100f, 30f);
            Rect rectB = new Rect(inRect.xMax - 100f, selectorRect.yMax + 10f, 100f, 30f);

            if (Widgets.ButtonText(rectA, "JoinBattle".Translate(), true, false, true))
            {
                this.battleZone.GenerateMap();
                this.Close();
            }

            GUI.color = Color.white;
            if (Widgets.ButtonText(rectB, "Close".Translate()))
            {
                this.Close();
            }
        }
        
    }
}
