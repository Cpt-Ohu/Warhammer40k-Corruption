using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_Alignment : MainTabWindow
    {

        public string SelPawnPatron = "Emperor";

        public string SelPawnSoulState;

        public string psykerPowerLevel;

        public CulturalToleranceCategory culturalTolerance;

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(800f, 600f);
            }
        }

        private string culturalToleranceToolTip(CulturalToleranceCategory cat)
        {
            switch(cat)
            {
                case (CulturalToleranceCategory.Neutral):
                    {
                        return "CulturalToleranceNeutralDesc".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (CulturalToleranceCategory.Xenophile):
                    {
                        return "CulturalToleranceXenophilelDesc".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (CulturalToleranceCategory.Xenophobe):
                    {
                        return "CulturalToleranceXenophobeDesc".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private string PsykerPowerLevelToolTip(PsykerPowerLevel level)
        {
            switch (level)
            {
                case (PsykerPowerLevel.Omega):
                    {
                        return "PsykerLevelDescOmega".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (PsykerPowerLevel.Omicron):
                    {
                        return "PsykerLevelDescOmicron".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (PsykerPowerLevel.Sigma):
                    {
                        return "PsykerLevelDescSigma".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (PsykerPowerLevel.Rho):
                    {
                        return "PsykerLevelDescRho".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (PsykerPowerLevel.Iota):
                    {
                        return "PsykerLevelDescIota".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (PsykerPowerLevel.Zeta):
                    {
                        return "PsykerLevelDescZeta".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (PsykerPowerLevel.Epsilon):
                    {
                        return "PsykerLevelDescEpsilon".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (PsykerPowerLevel.Delta):
                    {
                        return "PsykerLevelDescDelta".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private string AfflictionCategoryToolTip(SoulAffliction level)
        {
            switch (level)
            {
                case (SoulAffliction.Pure):
                    {
                        return "SoulAfflictionDescPure".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (SoulAffliction.Intrigued):
                    {
                        return "SoulAfflictionDescIntrigued".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (SoulAffliction.Warptouched):
                    {
                        return "SoulAfflictionDescWarptouched".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (SoulAffliction.Tainted):
                    {
                        return "SoulAfflictionDescTainted".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (SoulAffliction.Corrupted):
                    {
                        return "SoulAfflictionDescCorrupted".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                case (SoulAffliction.Lost):
                    {
                        return "SoulAfflictionDescLost".Translate(new object[]
                        {
                            SelPawn.NameStringShort
                        });
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private List<SoulTrait> STraits;


        private Color PatronColor;

        protected Thing SelThing
        {
            get
            {
                return Find.Selector.SingleSelectedThing;
            }
        }

        protected Pawn SelPawn;

        private Need_Soul soul;

        public override void PreOpen()
        {
            base.PreOpen();
            this.SelPawn = this.SelThing as Pawn;
            if (SelPawn != null)
            {
                soul = SelPawn.needs.TryGetNeed<Need_Soul>();
                this.SelPawnPatron = soul.patronInfo.PatronName;
                this.SelPawnSoulState = soul.CurCategory.ToString();
                PatronColor = PatronInfo.PatronColor(SelPawnPatron);
                if (soul.SoulTraits.NullOrEmpty()) Log.Message("NoSoulTraits");
                if (!soul.SoulTraits.NullOrEmpty())
                {
                    Log.Message(soul.SoulTraits.Count.ToString());
                    STraits = soul.SoulTraits;
                }
                this.psykerPowerLevel = soul.PsykerPowerLevel.ToString();
                this.culturalTolerance = soul.CulturalTolerance;
            }
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            if (this.SelThing == null)
            {
                this.Close();
            }
            if (Find.Selector.SingleSelectedThing as Pawn != this.SelPawn)
            {
                this.PreOpen();
            }
            this.SetInitialSizeAndPosition();
            Rect rect2 = inRect.ContractedBy(10f);
            rect2.height = 30f;
            rect2.width = 300;
            rect2.x = inRect.x + (inRect.width / 2)-150;
            rect2.y += 10f;
      //      GUI.BeginGroup(rect2);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            var tmp1 = rect2.y;
            Widgets.Label(rect2, "Patron :");
  //          GUI.EndGroup();
            Rect rect3 = rect2;
            rect3.y += 30f;
   //         GUI.BeginGroup(rect3);
            Rect rect4 = rect3;
            rect4.height = 300f;
            String texpath = "UI/" + SelPawnPatron + "_bg";
            Texture2D tex = ContentFinder<Texture2D>.Get(texpath, true);
            GUI.DrawTexture(rect4, tex as Texture);
//            GUI.EndGroup();
            Rect rect5 = rect3;
            rect5.y += 320f;
            rect5.height = 30f;
            //        GUI.BeginGroup(rect5);
            GUI.color = PatronColor;
            var tmp2 = rect5.y+5;
            Widgets.Label(rect5, SelPawnPatron.Translate());
            Widgets.ListSeparator(ref tmp2, inRect.width, "");
            GUI.color = Color.white;

            Rect rect7 = rect5;
            rect7.x = 0f;
            rect7.width = inRect.width / 3;
            rect7.y += 30;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect7), "ReligiousTraits".Translate());
            Text.Font = GameFont.Small;
            float num = rect7.y + 30f;
            Rect rect13 = new Rect(rect7.x, num, rect7.width - 50, 23f);
            for (int i = 0; i < STraits.Count(); i++)
            {
                SoulTrait trait = STraits[i];
                Rect rect12 = new Rect(rect7.x, num, rect7.width - 50, 23f);
                if (Mouse.IsOver(rect12))
                {
                    Widgets.DrawHighlight(rect12);
                }
                Widgets.Label(rect12, trait.SoulCurrentData.label);
                num += rect12.height + 15f;
                SoulTrait trLocal = trait;
                TipSignal tip2 = new TipSignal(() => trLocal.TipString(SelPawn), (int)num * 37);
                
                TooltipHandler.TipRegion(rect12, tip2);
                rect13 = rect12;
            }            

            rect13.y += 53;
            if (Mouse.IsOver(rect13))
            {
                Widgets.DrawHighlight(rect13);
            }
            Widgets.Label(rect13, this.culturalTolerance.ToString());
            num += rect13.height + 15;
            TipSignal tip = new TipSignal(() => culturalToleranceToolTip(this.culturalTolerance), (int)num * 37);
            TooltipHandler.TipRegion(rect13, tip);


            Rect rect8 = rect7;
            rect8.x = inRect.x + (inRect.width / 2) - 150;
            rect8.width -= 20;
            String desc = SelPawnPatron + "_Description";
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect8), "Description".Translate());
            Rect rect8a = rect8;
            rect8a.y += 30f;
            rect8a.height = 200f;
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect8a), desc.Translate());

            Rect rect9 = rect7;
            rect9.x = rect8.xMax + 20;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(rect9), "SoulStatus".Translate());
            Rect rect9a = rect9;
            rect9a.y += 30f;
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(rect9a), SelPawnSoulState.Translate());
            TipSignal tip3 = new TipSignal(() => this.AfflictionCategoryToolTip(soul.CurCategory), (int)num * 37);            
            TooltipHandler.TipRegion(rect9a, tip3);
            Rect rect9b = rect9a;
            rect9b.y += rect9a.height + 15f;
            Widgets.Label(new Rect(rect9b), this.psykerPowerLevel);
            TipSignal tip4 = new TipSignal(() => this.PsykerPowerLevelToolTip(soul.PsykerPowerLevel), (int)num * 37);
            TooltipHandler.TipRegion(rect9b, tip4);
            Text.Anchor = TextAnchor.UpperLeft;
            if (Widgets.CloseButtonFor(inRect.AtZero()))
            {
                this.Close();
            }
            if (this.SelPawn == null)
            {
                this.Close();
            }
        }
    }
}
