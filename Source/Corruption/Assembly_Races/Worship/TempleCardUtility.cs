using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public class TempleCardUtility
    {
        public static Vector2 TempleCardSize = new Vector2(570f, 250f);

        public static void DrawTempleCard(Rect rect, BuildingAltar altar)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(rect.x, rect.y + 20f, 250f, 55f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, altar.RoomName);
            Text.Font = GameFont.Small;

            Rect rect3 = rect2;
            rect3.y = rect2.yMax + 30f;
            rect3.width = 200f;
            rect3.height = 25f;
            Widgets.Label(rect3, "Preacher".Translate() + ": ");
            rect3.xMin = rect3.center.x + 10f;
            string label2 = PreacherLabel(altar);
            if (Widgets.ButtonText(rect3, label2, true, false, true))
                {
                    TempleCardUtility.OpenPreacherSelectMenu(altar);
                }
            Rect rect4 = rect3;
            rect4.y += 35f;
            rect4.width = 200f;
            if (Widgets.ButtonText(rect4, "RenameTemple".Translate(), true, false, true))
            {
                Find.WindowStack.Add(new Dialog_RenameTemple(altar));
            }
            Rect rectDebug1 = rect4;
            rectDebug1.y += 25f;
            if (DebugSettings.godMode) 
            {
                if (Widgets.ButtonText(rectDebug1, "ForceSermonDebug".Translate(), true, false, true))
                {
                    SermonUtility.ForceSermon(altar, Worship.WorshipActType.MorningPrayer);
                }
                Rect rectDebug2 = rectDebug1;
                rectDebug2.y += 25f;
                if (Widgets.ButtonText(rectDebug2, "ForceListenersDebug".Translate(), true, false, true))
                {
                    TempleCardUtility.ForceListenersTest(altar);
                }
            }

            Rect rect5 = rect4;
            rect5.x = rect4.xMax + 5f;
            rect5.width = 200f;
            rect5.y -= 20f;
            Widgets.CheckboxLabeled(rect5, "MorningSermons".Translate(), ref altar.OptionMorning, false);
            Rect rect6 = rect5;
            rect6.y += 20f;
            Widgets.CheckboxLabeled(rect6, "EveningSermons".Translate(), ref altar.OptionEvening, false);


            GUI.EndGroup();
        }

        private static string PreacherLabel(BuildingAltar altar)
        {
            if (altar.preacher == null)
            {
                return "None";
            }
            else
            {
                return altar.preacher.NameStringShort;
            }
        }

        private static void ForceListenersTest(BuildingAltar altar)
        {

            IntVec3 result;
            Building chair;
            foreach (Pawn p in altar.Map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike))
            {
                if (! SermonUtility.IsPreacher(p))
                {
                    if (!WatchBuildingUtility.TryFindBestWatchCell(altar, p, true, out result, out chair))
                    {

                        if (!WatchBuildingUtility.TryFindBestWatchCell(altar as Thing, p, false, out result, out chair))
                        {
                            return;
                        }
                    }
                    if (chair != null)
                    {
                        Job J = new Job(C_JobDefOf.AttendSermon, altar.preacher, altar, chair);
                        p.jobs.jobQueue.EnqueueLast(J);
                        p.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                    else
                    {
                        Job J = new Job(C_JobDefOf.AttendSermon, altar.preacher, altar, result);
                        p.jobs.jobQueue.EnqueueLast(J);
                        p.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
            }
        }

        public static void OpenPreacherSelectMenu(BuildingAltar altar)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            list.Add(new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate
            {
                altar.preacher = null;
            }, MenuOptionPriority.Default, null, null, 0f, null));

            foreach (Pawn current in altar.Map.mapPawns.FreeColonistsSpawned)
            {
                if (!SermonUtility.IsPreacher(current))
                {
                    Need_Soul nsoul = current.needs.TryGetNeed<Need_Soul>();
                    if (nsoul == null) nsoul = new Need_Soul(current);
                    SoulTrait strait = nsoul.DevotionTrait;
                    string text1 = current.NameStringShort + " (" + strait.SoulCurrentData.label + ")";

                    Action action;
                    Pawn localCol = current;
                    action = delegate
                    {
                        altar.preacher = localCol;
                    };
                    list.Add(new FloatMenuOption(text1, action, MenuOptionPriority.Default, null, null, 0f, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }
}
