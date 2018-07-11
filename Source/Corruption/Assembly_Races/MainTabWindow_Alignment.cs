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

        public PatronDef SelPawnPatron;

        public string SelPawnSoulState;

        public string psykerPowerLevel;

        public CulturalToleranceCategory culturalTolerance;

        private static readonly Texture2D questionMark = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark", true);

        private static readonly Texture2D AddPowerTex = ContentFinder<Texture2D>.Get("UI/Icons/Trainables/Rescue", true);
        

        private SoulAffliction[] afflictionTypes = (SoulAffliction[]) Enum.GetValues(typeof(SoulAffliction));

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(768, 512f);
            }
        }

        private string culturalToleranceToolTip(CulturalToleranceCategory cat)
        {
            switch (cat)
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
            if (SelPawn != null)
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
                            return "";
                        }
                }
            }
            return "";
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

        protected Pawn SelPawn
        {
            get
            {
                if (this.SelThing != null)
                {
                    return this.SelThing as Pawn;
                }
                this.Close();
                return null;
            }
        }

        private CompSoul soul
        {
            get
            {
                if (this.SelPawn != null)
                    {
                    return CompSoul.GetPawnSoul(this.SelPawn);
                }
                this.Close();
                return null;
            }
        }

        public float CorruptionLevel
        {
            get
            {
                return 1f - this.soul.CurLevel;
            }
        }

        private CompPsyker compPsyker
        {
            get
            {
                return this.SelPawn.TryGetComp<CompPsyker>();
            }
        }

        private PsykerPowerManager currentPowerManager
        {
            get
            {
                return this.compPsyker.PsykerPowerManager;
            }
        }
        
        private Texture2D pykerPowerTex
        {
            get
            {
                return PsykerUtility.GetPsykerPowerLevelTexture(soul.PsykerPowerLevel);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            if (SelPawn != null && SelPawn.def.race.Humanlike)
            {
                this.SelPawnPatron = soul.Patron;
                this.SelPawnSoulState = soul.CurCategory.ToString();
                if (!soul.AllSoulTraits.NullOrEmpty())
                {
                    STraits = soul.AllSoulTraits;
                }
                this.psykerPowerLevel = soul.PsykerPowerLevel.ToString();
                this.culturalTolerance = soul.CulturalTolerance;
            }
            else
            {
                this.Close();
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (this.SelThing == null)
            {
                this.Close();
            }
            if (this.SelPawn == null || !this.SelPawn.RaceProps.Humanlike)
            {
                this.Close();
            }
            if (Find.Selector.SingleSelectedThing as Pawn != this.SelPawn)
            {
                this.PreOpen();
            }
            this.SetInitialSizeAndPosition();

            if (soul.KnownToPlayer)
            {

                Rect soulmeterRect = inRect.ContractedBy(16f);
                soulmeterRect.width -= 32f;
                soulmeterRect.height = 96f;
                GUI.BeginGroup(soulmeterRect);
                DrawSoulMeter(soulmeterRect);

                Rect descriptionRect = new Rect(0f, 52f, soulmeterRect.width, 32f);
                Widgets.Label(descriptionRect, this.AfflictionCategoryToolTip(soul.CurCategory));

                GUI.EndGroup();
                //Widgets.DrawLineHorizontal(0f, soulmeterRect.yMax + 4f, soulmeterRect.width);

                Rect WorshipRect = new Rect(soulmeterRect);
                WorshipRect.y = soulmeterRect.yMax + 8f;
                WorshipRect.width = 256f;
                WorshipRect.height = 256f;
                Rect worshipBorderRect = WorshipRect.ExpandedBy(16f);
                GUI.DrawTexture(worshipBorderRect, CorruptionStoryTrackerUtilities.BorderWorship);
                Rect topBarRect = new Rect(worshipBorderRect.x - 16f, worshipBorderRect.y, 320f, 48f);
                GUI.DrawTexture(topBarRect, soul.Patron.worshipBarTexture);
                //GUI.DrawTexture(WorshipRect, BaseContent.BlackTex);
                GUI.BeginGroup(WorshipRect);

                Rect worshipTitleRect = new Rect(2f, 2f, 32f, 32f);

                //Rect patronIconRect = new Rect(2f, 38f, 32f, 32f);
                //GUI.DrawTexture(patronIconRect, soul.Patron.SmallTexture);
                Rect patronLabelRect = new Rect(0f, 38f, 256f, 32f);
                GUI.color = soul.Patron.MainColor;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(patronLabelRect, soul.Patron.label);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;

                //Widgets.DrawLineHorizontal(2f, patronLabelRect.yMax + 2f, WorshipRect.width);
                Rect TraitRect = new Rect(patronLabelRect);
                TraitRect.x += 4f;
                TraitRect.y += 40f;
                TraitRect.height = 23f;

                float curTraitRectHeight = TraitRect.y;
                if (Mouse.IsOver(TraitRect))
                {
                    Widgets.DrawHighlight(TraitRect);
                }
                Widgets.Label(TraitRect, this.culturalTolerance.ToString());
                TipSignal tip = new TipSignal(() => culturalToleranceToolTip(this.culturalTolerance), (int)curTraitRectHeight * 37);
                TooltipHandler.TipRegion(TraitRect, tip);
                curTraitRectHeight += TraitRect.height + 2f;

                for (int i = 0; i < STraits.Count(); i++)
                {
                    SoulTrait trait = STraits[i];
                    Rect rect12 = new Rect(TraitRect.x, curTraitRectHeight, TraitRect.width, 23f);
                    if (Mouse.IsOver(rect12))
                    {
                        Widgets.DrawHighlight(rect12);
                    }
                    Widgets.Label(rect12, trait.SoulCurrentData.label);
                    curTraitRectHeight += rect12.height + 2f;
                    SoulTrait trLocal = trait;
                    TipSignal tip2 = new TipSignal(() => trLocal.TipString(SelPawn), (int)curTraitRectHeight * 37);

                    TooltipHandler.TipRegion(rect12, tip2);
                }
                GUI.EndGroup();


                Rect PsykerRect = new Rect(soulmeterRect);
                PsykerRect.x = 312f;
                PsykerRect.y = WorshipRect.y;
                PsykerRect.height = WorshipRect.height;
                PsykerRect.width = 400f; //inRect.xMax - PsykerRect.x - 8f;

                Rect psyBorderRect = new Rect(PsykerRect.ExpandedBy(16f));
                GUI.DrawTexture(psyBorderRect, CorruptionStoryTrackerUtilities.BorderPsyker);
                Rect psyPowerLevelRect = new Rect(psyBorderRect.x + (psyBorderRect.width - 64f) / 2, psyBorderRect.y - 16f, 64f, 64);
                GUI.DrawTexture(psyPowerLevelRect, pykerPowerTex);
                TipSignal tipPowerLevel = new TipSignal(() => soul.PsykerPowerLevel.ToString(), 37);
                TooltipHandler.TipRegion(psyPowerLevelRect, tipPowerLevel);
                GUI.BeginGroup(PsykerRect);
                Rect psykerlabelRect = new Rect(PsykerRect);

                float curPowerLocY = 44f;
                float curPowerLocX = 2f;
                if (soul.PsykerPowerLevel >= PsykerPowerLevel.Iota)
                {
                    foreach (KeyValuePair<PsykerPowerLevel, int> slot in PsykerPowerManager.PowerLevelSlots)
                    {
                        if (soul.PsykerPowerLevel >= slot.Key)
                        {
                            Rect powerColumnRect = new Rect(curPowerLocX - 4, curPowerLocY - 4, 40f, 40f);
                            GUI.DrawTexture(powerColumnRect, PsykerUtility.GetPsykerPowerLevelTexture(slot.Key));
                            curPowerLocY += 36f;
                            bool drawnAddPower = false;
                            for (int i = 0; i < slot.Value; i++)
                            {
                                Rect curPowerRect = new Rect(curPowerLocX, curPowerLocY, 32f, 32f);
                                Widgets.DrawBox(curPowerRect, 1);
                                if (currentPowerManager.GetPsykerPowerList(slot.Key).ElementAtOrDefault(i) != null)
                                {
                                    PsykerPowerEntry entry = currentPowerManager.GetPsykerPowerList(slot.Key)[i];
                                    GUI.DrawTexture(curPowerRect, entry.psykerPowerDef.uiIcon);
                                    TipSignal tip2 = new TipSignal(() => entry.psykerPowerDef.label + Environment.NewLine + entry.psykerPowerDef.description, (int)curPowerLocY * 37);
                                    TooltipHandler.TipRegion(curPowerRect, tip2);
                                }
                                else if (currentPowerManager.PsykerXP >= PsykerUtility.PsykerXPCost[slot.Key] && this.SelPawn.IsColonist && !drawnAddPower)
                                {
                                    GUI.DrawTexture(curPowerRect, AddPowerTex);
                                    if (Widgets.ButtonInvisible(curPowerRect, true))
                                    {
                                        Dialog_LearnPsykerPower dialog = new Dialog_LearnPsykerPower(slot.Key, this.currentPowerManager);
                                        Find.WindowStack.Add(dialog);
                                    }
                                    drawnAddPower = true;
                                }
                                curPowerLocY += 36f;
                            }

                            curPowerLocX += 64f;
                            curPowerLocY = 44f;
                        }

                    }
                    Rect XPRect = new Rect(curPowerLocX, curPowerLocY, 256f, 32f);
                    Widgets.Label(XPRect, currentPowerManager.PsykerXP.ToString() + " XP");
                }

            }
            else
            {
                Rect labelRect = new Rect(32f, 0f, inRect.width - 32f, 64f);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.TextArea(labelRect, "AlignmentUnknownDesc".Translate(), true);
                Text.Anchor = TextAnchor.UpperLeft;
                Rect centerRect = new Rect((inRect.width / 2) - 64, (inRect.height / 2) - 64, 128, 128);
                GUI.DrawTexture(centerRect, questionMark);
            }
            GUI.EndGroup();
            if (Widgets.CloseButtonFor(inRect.AtZero()))
            {
                this.Close();
            }
            if (this.SelThing == null || this.SelPawn == null)
            {
                this.Close();
            }
        }

        private void DrawSoulMeter(Rect meterRect)
        {
            Rect bgRect = new Rect(0f, 8f, 512f, 32f);

            GUI.DrawTexture(bgRect, CorruptionStoryTrackerUtilities.BackgroundTile);
            Rect progressRect = new Rect(bgRect);
            progressRect.height = 15f;
            progressRect.y += 9f;
            Widgets.FillableBar(progressRect, CorruptionLevel, CorruptionStoryTrackerUtilities.SoulmeterProgressTex, CorruptionStoryTrackerUtilities.TransparentBackground, false);
            DrawSoulmeterNodes();
        }

        private void DrawSoulmeterNodes()
        {
            DrawNode(SoulAffliction.Pure, 0f);
            DrawNode(SoulAffliction.Intrigued, 86f);
            DrawNode(SoulAffliction.Warptouched, 203f);
            DrawNode(SoulAffliction.Tainted, 331);
            DrawNode(SoulAffliction.Corrupted, 382f);
            DrawNode(SoulAffliction.Lost, 512f);
        }

        private void DrawNode(SoulAffliction affliction, float curX)
        {
            Rect nodeRect = new Rect(curX, 0f, 48f, 48f);
            if (soul.CurCategory >= affliction)
            {
                GUI.DrawTexture(nodeRect.ContractedBy(4f), CorruptionStoryTrackerUtilities.SoulmeterProgressTex);
            }
            else
            {
                GUI.DrawTexture(nodeRect, CorruptionStoryTrackerUtilities.SoulNodeBG);
            }
            GUI.DrawTexture(nodeRect, GetAfflictionTexture(affliction));
            TipSignal tip4 = new TipSignal(() => affliction.ToString().Translate(), (int)curX * 37);
            TooltipHandler.TipRegion(nodeRect, tip4);
        }

        private Texture2D GetAfflictionTexture(SoulAffliction type)
        {
            switch (type)
            {
                case SoulAffliction.Pure:
                    {
                        return CorruptionStoryTrackerUtilities.SoulNodePure;
                    }
                case SoulAffliction.Intrigued:
                    {
                        return CorruptionStoryTrackerUtilities.SoulNodeIntrigued;
                    }
                case SoulAffliction.Warptouched:
                    {
                        return CorruptionStoryTrackerUtilities.SoulNodeWarptouched;
                    }
                case SoulAffliction.Tainted:
                    {
                        return CorruptionStoryTrackerUtilities.SoulNodeTainted;
                    }
                case SoulAffliction.Corrupted:
                    {
                        return CorruptionStoryTrackerUtilities.SoulNodeCorrupted;
                    }
                case SoulAffliction.Lost:
                    {
                        return CorruptionStoryTrackerUtilities.SoulNodeLost;
                    }
                default:
                    {
                        return CorruptionStoryTrackerUtilities.SoulNodeWarptouched;
                    }
            }
                    
        }
    }
}
