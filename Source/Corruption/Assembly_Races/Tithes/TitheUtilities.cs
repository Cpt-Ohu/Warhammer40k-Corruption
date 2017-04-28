using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.Tithes
{
    [StaticConstructorOnStartup]
    public static class TitheUtilities
    {
        public static readonly Texture2D TitheBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));
        public static readonly Texture2D TitheBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));


        public static float DrawTitheRow(float rowY, TitheEntryGlobal entry)
        {
            Rect rect2 = new Rect(10f, rowY, 220f, 26f);

            Widgets.Label(rect2, entry.titheDef.label);
            Rect rect3 = new Rect(rect2);
            rect3.y = rect2.yMax + 10f;
            rect3.height = 30f;
            rect3.width = 200f;
            Widgets.FillableBar(rect3, entry.tithePercent, TitheUtilities.TitheBarFillTex, TitheUtilities.TitheBarBGTex, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect3, entry.collectedTitheAmount.ToString("F0") + " / " + entry.requestedTitheAmount.ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
            float num = rowY + 60f;
            return num;
        }


        public static void CalculateColonyTithes(CorruptionStoryTracker tracker)
        {
            Log.Message("Calculating Tithes");
            float wealthInt = 0f;
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome)
                {
                    wealthInt += maps[i].wealthWatcher.WealthTotal;
                }
            }
            float calc = 0f;

            float totalAmount = TitheUtilities.TaxCalculation(wealthInt);
            float baseAmount = totalAmount / DefDatabase<TitheDef>.AllDefsListForReading.Count;
            foreach (TitheDef current in DefDatabase<TitheDef>.AllDefsListForReading)
            {
                if (!tracker.currentTithes.Any(x => x.titheDef == current))
                {
                    if (calc <= totalAmount)
                    {

                        calc += baseAmount;
                        float num = baseAmount * current.wealthFactor;
                        TitheEntryGlobal entry = new TitheEntryGlobal(current, num);
                        tracker.currentTithes.Add(entry);
                    }
                }
            }
            tracker.DaysToTitheCollection = 60;
            tracker.TitheCollectionActive = false;
        }

        public static float TaxCalculation(float wealthInt)
        {
            return 100000 * (1 - (5000 / wealthInt));
        }

        public static float SpaceRemainingForThing(CompTitheContainer titheContainer, Thing t)
        {
            return Mathf.Clamp((titheContainer.tProps.maxContainerCapacity * (1 - titheContainer.massUsage)) / t.GetStatValue(StatDefOf.Mass), 0f, titheContainer.tProps.maxContainerCapacity);
        }

        public static int RemainingTitheToCollect(TitheEntryGlobal entry, Thing t)
        {
            float num = 0f;
            num = (entry.requestedTitheAmount - entry.collectedTitheAmount) / t.def.BaseMarketValue;
            return (int)num;
        }

        public static void UpdateAllContaners()
        {
            List<Building> list = TitheUtilities.allTitheContainers;
            for (int j = 0; j < list.Count; j++)
            {
                TitheContainer current = (TitheContainer)list[j];
                current.UpdateEntries();
            }

        }


        public static List<Building> allTitheContainers
        {
            get
            {
                List<Building> list = new List<Building>();
                List<Map> list2 = Find.Maps;
                for (int i = 0; i < list2.Count; i++)
                {
                    if (list2[i].IsPlayerHome)
                    {
                        list.AddRange(list2[i].listerBuildings.allBuildingsColonist.FindAll(x => x is TitheContainer));
                    }
                }
                return list;
            }
        }

        public static void PerformRefusedTitheEvaluation()
        {
            CorruptionStoryTracker tracker = CorruptionStoryTrackerUtilities.currentStoryTracker;
            tracker.ResetIoMAcknowledgement();
            int num = tracker.PlanetaryGovernor.skills.GetSkill(SkillDefOf.Social).levelInt + Rand.Range(10, 15);
            if (num > 27)
            {
                return;
            }
            else if (num > 20)
            {
                CorruptionStoryTrackerUtilities.InitiateGovernorArrestEvent(tracker.PlanetaryGovernor.Map);
            }            
            else
            {

            }
        }

    }
}
