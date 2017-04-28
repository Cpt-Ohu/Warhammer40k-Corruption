﻿using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.BookStuff
{
    public class ReadableBooks : ThingWithComps
    {
        public Pawn currentReader = null;
        public ThingDef_Readables Tdef;
        private List<string> BookText = new List<string>();
        public bool TexChange = false;
        private Graphic OpenBook;
        private List<string> DefaultText = new List<string>
        {
            "It was a dark and stormy night.",
            "Suddenly, a shot rang out!",
            "A door slammed.",
            "The maid screamed.",
            "Suddenly, a pirate ship appeared on the horizon!",
            "While millions of people were starving, the king lived in luxury.",
            "Meanwhile, on a small farm in Kansas, a boy was growing up.",
            "A light snow was falling....",
            "and the little girl with the tattered shawl had not sold a violet all day.",
            "At that very moment...",
            "a young intern at City Hospital was making an important discovery.",
            "The mysterious patient in Room 213 had finally awakened",
            "She moaned softly",
            "Could it be that she was the sister of the boy in Kansas...",
            "who loved the girl with the tattered shawl",
            "who was the daughter of the maid who had escaped from the pirates?",
            "The intern frowned."
        };
        public override Graphic Graphic
        {
            get
            {
                ReadFormXML();
                Graphic result;
                if (!TexChange && OpenBook != null)
                {
                    result = OpenBook;
                }
                else
                {
                    result = base.Graphic;
                }
                return result;
            }
        }
        public List<string> PrepareText()
        {
            ThingDef_Readables Readables_Def = (ThingDef_Readables)def;
            BookText = Readables_Def.BookText;
            List<string> result;
            if (BookText.Count > 0)
            {
                result = TextChooping(BookText);
            }
            else
            {
                result = TextChooping(DefaultText);
            }
            return result;
        }


        public void FeedBackPulse()
        {
            foreach (Thing current in this.Map.listerThings.AllThings)
            {
                if (current.GetType() == typeof(Bookshelf))
                {
                    Bookshelf bookshelf = current as Bookshelf;
                    if (bookshelf.MissingBooksList.Contains(def))
                    {
                        if (bookshelf.StoredBooks.Count < 3)
                        {
                            bookshelf.MissingBooksList.Remove(def);
                            bookshelf.StoredBooks.Add(def);
                            if (Spawned)
                            {
                                Destroy();
                            }
                            break;
                        }
                    }
                }
            }
            if (Spawned)
            {
                Destroy();
            }
        }
        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            ReadFormXML();
            if (OpenBook == null)
            {
                OpenBook = GraphicDatabase.Get<Graphic_Single>("Items/Books/Cover_BookGeneric");
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.LookList<string>(ref BookText, "BookText", LookMode.Undefined, null);
            Scribe_References.LookReference<Pawn>(ref currentReader, "currentReader");
        }
        private List<string> TextChooping(List<string> textlist)
        {
            List<string> list = new List<string>();
            int num = Rand.RangeInclusive(0, textlist.Count - 7);
            int num2 = 0;
            for (int i = num; i < textlist.Count; i++)
            {
                list.Add(textlist[i]);
                num2++;
                if (num2 >= 7)
                {
                    break;
                }
            }
            return list;
        }
        private void ReadFormXML()
        {
            List<string> list = new List<string>();
            ThingDef_Readables Readables_Def = (ThingDef_Readables)def;
            if (Readables_Def.BookText.Count > 0)
            {
                list = Readables_Def.BookText;
            }
            if (!Readables_Def.CloseTexture.NullOrEmpty())
            {
                OpenBook = GraphicDatabase.Get<Graphic_Single>(Readables_Def.CloseTexture, ShaderDatabase.CutoutComplex, Vector2.one, DrawColor, DrawColorTwo);
            }
            this.Tdef = (ThingDef_Readables)this.def;
        }

        public void PostReadEffectSelection(int prog, int oldprog)
        {
            if (this.Tdef.ReadableEffectEntries != null)
            {

                float progperc = (float)prog / Tdef.TicksToRead;
                foreach (ReadableEffektEntry entry in this.Tdef.ReadableEffectEntries)
                {
                    float threshold = entry.ReadThreshold;
                    float oldprogperc = (float)oldprog / Tdef.TicksToRead / threshold;
                    bool readbefore = threshold <= oldprogperc && entry.AffectOnlyOnce;
   //                 Log.Message("progess: " + prog.ToString() + "  /   " + Tdef.TicksToRead.ToString() + "  =   " + progperc.ToString());
   //                 Log.Message("Process is " + progperc.ToString() + " and ReadBefore is :" + readbefore.ToString());
                    if (threshold <= progperc && !readbefore)
                    {
                        Log.Message("Trying Effect");
                        this.TryPostReadEffect(entry);
                    }
                }
            }
        }

        public void TryPostReadEffect(ReadableEffektEntry entry)
        {
            switch (entry.readableEffectCategory)
            {
                case (ReadableEffectCategory.LearnPsykerPower):
                    {
                        CompPsyker compPsyker;
                        if ((compPsyker = this.currentReader.GetComp<CompPsyker>()) != null)
                        {
                            compPsyker.psykerPowerManager.AddPsykerPower(entry.PsykerPowerUnlocked);
                            return;
                        }
                        return;
                    }
                case (ReadableEffectCategory.GetMentalBreak):
                    {
                        if (currentReader.needs != null)
                        {
                            currentReader.mindState.mentalStateHandler.TryStartMentalState(entry.MentalBreak);
                        }
                        return;
                    }
                case (ReadableEffectCategory.GetHediff):
                    {
                        if (currentReader.health != null)
                        {
                            currentReader.health.AddHediff(entry.HediffGained);
                        }
                        return;
                    }
            }
        }
        public void ReadCorruptionTick(Pawn pawn, ReadableBooks book)
        {
            Need_Soul soul = pawn.needs.TryGetNeed<Need_Soul>();
            if (soul != null)
            {
                float num;
                int sign = 0;
                switch (book.Tdef.soulItemCategory)
                {
                    case (SoulItemCategories.Neutral):
                        {
                            sign = 0;
                            break;
                        }
                    case (SoulItemCategories.Corruption):
                        {
                            sign = -1;
                            break;
                        }
                    case (SoulItemCategories.Redemption):
                        {
                            sign = 1;
                            break;
                        }
                    default:
                        {
                            Log.Error("No Soul Item Category Found");
                            break;
                        }
                }
                num = sign * this.Tdef.SoulGainRate * 0.2f / 1200;
                soul.GainNeed(num);
            }
        }
    }
}
