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
    public class Window_CCMBase : Window
    {
        private const float TitleHeight = 70f;

        private CorruptionStoryTracker storyTracker;

        private const float InfoHeight = 60f;

        protected Pawn negotiator;
        
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(800f, 600f);
            }
        }

        public override void PreOpen()
        {
            TickManager tickManager = Find.TickManager;
            if (!tickManager.Paused)
            {
                tickManager.TogglePaused();
            }
            base.PreOpen();
        }

        public Window_CCMBase(CorruptionStoryTracker tracker, Pawn negotiator)
        {
            this.storyTracker = tracker;
            this.negotiator = negotiator;
            this.forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect baseRect = inRect;
            Rect leftRect = new Rect(0f, 15f, 120f, baseRect.height - 25f);
            Rect centerRect = new Rect(baseRect.width / 2f - 220f, 120f, 440f, baseRect.height - 120f);
            Rect rightRect = new Rect(baseRect.width - 120f, 15f, 120f, baseRect.height - 25f);
            GUI.BeginGroup(inRect);
            Rect titleRect = new Rect(baseRect.x, baseRect.y, baseRect.width, 30f);            
     //       GUI.BeginGroup(titleRect);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string title = "Subsector " + storyTracker.SubsectorName;
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
    //        GUI.EndGroup();

            GUI.BeginGroup(leftRect);
            float num = leftRect.y;
            CorruptionStoryTrackerUtilities.ListSeparatorBig(ref num, leftRect.width, "ImperialFrequencies".Translate());
            num += 50f;
            for (int i = 0; i < storyTracker.ImperialFactions.Count; i++)
            {
                num += DrawFactionRowCommsIoM(storyTracker.ImperialFactions[i], num, leftRect, negotiator);
            }
      //      Widgets.DrawRectFast(leftRect, Color.white);
            GUI.EndGroup();

            this.DrawStarGrid(centerRect);
            //        GUI.BeginGroup(centerRect);
            Rect worldRect = new Rect((centerRect.x + centerRect.width / 2f) - 30f, (centerRect.y + centerRect.height / 2f) - 30f, 60f, 60f);

            if (Widgets.ButtonImage(worldRect, CorruptionStoryTrackerUtilities.PlanetMedium))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                IEnumerable<ICommunicable> enumerable = negotiator.Map.passingShipManager.passingShips.Cast<ICommunicable>().Concat(Find.FactionManager.AllFactionsInViewOrder.Cast<ICommunicable>());
                foreach (ICommunicable commTarget in enumerable)
                {
                    ICommunicable localCommTarget = commTarget;
                    string text = "CallOnRadio".Translate(new object[]
                    {
                    localCommTarget.GetCallLabel()
                    });
                    Faction faction = localCommTarget as Faction;
                    if (faction != null)
                    {
                        if (faction.IsPlayer)
                        {
                            continue;
                        }
                    }
                    Action action = delegate
                    {
                        localCommTarget.TryOpenComms(negotiator);
                    };
                    list.Add(new FloatMenuOption(text, action, MenuOptionPriority.InitiateSocial, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list, null, false));
            }
            TooltipHandler.TipRegion(worldRect, "CurrentWorldInfoRect".Translate(new object[]
                {
                            Find.World.info.name                            
                }));

            Rect worldLabelrect = worldRect;
            worldLabelrect.y += 65f;
            worldLabelrect.width = 80f;
            GUI.color = Color.red;
            Widgets.Label(worldLabelrect, Find.World.info.name);
            GUI.color = Color.white;

            foreach (StarMapObject current in storyTracker.SubSectorObjects)
            {
      //          Log.Message(current.objectName + current.objectRect.ToString());
      //          Widgets.DrawRectFast(current.objectRect, Color.red);
                
                if (Widgets.ButtonImage(current.objectRect, current.objectTex))
                {
                }

                TooltipHandler.TipRegion(current.objectRect, "StarmapObjectInfo".Translate(new object[]
                    {
                            current.objectName,
                            current.diameter,
                            "Unknown"
                    }));

            }

            //            GUI.EndGroup();

            GUI.BeginGroup(rightRect);

      //      Widgets.DrawRectFast(rightRect, Color.red);
            float num2 = rightRect.y;
            CorruptionStoryTrackerUtilities.ListSeparatorBig(ref num2, rightRect.width, "UnknownFrequencies".Translate());
            num2 += 50f;
            for (int i = 0; i < storyTracker.XenoFactions.Count; i++)
            {
                num2 += DrawFactionRowCommsXeno(storyTracker.XenoFactions[i], num2, rightRect, negotiator);

            }

            GUI.EndGroup();
            GUI.EndGroup();
            if (Widgets.CloseButtonFor(inRect.AtZero()))
            {
                this.Close();
            }
        }

        private void DrawStarGrid(Rect rect)
        {
            Widgets.DrawRectFast(rect, new Color(0.09f, 0.07f, 0.05f), null);
            float x = rect.x;
            float y = rect.y;
            for (int i = 0; i < 9; i++)
            {
                DrawLineHorizontalMap(x, y, rect.width);
                y += 55f;
            }
            y = rect.y;
            for (int i = 0; i < 6; i++)
            {
                DrawLineVerticalMap(x, y, rect.width);
                x += 88f;
            }
        }

        private static void DrawLineHorizontalMap(float x, float y, float length)
        {
            Rect position = new Rect(x, y, length, 1f);
            GUI.DrawTexture(position, BaseContent.GreyTex);
        }
        private static void DrawLineVerticalMap(float x, float y, float length)
        {
            Rect position = new Rect(x, y, 1f, length);
            GUI.DrawTexture(position, BaseContent.GreyTex);
        }


        private float DrawFactionRowCommsIoM(Faction faction, float rowY, Rect fillRect, Pawn negotiator)
        {
            Rect rect = new Rect(5f, rowY, fillRect.width - 10f, 50f);
            Rect rect2 = rect;
            if (Widgets.ButtonText(rect, faction.def.LabelCap, true, true, true))
            {
                if (faction.leader == null) Log.Error("NoLeader for: " + faction.Name);
                this.Close();
                CorruptionStoryTrackerUtilities.TryOpenIoMComms(negotiator, faction);

            }
            float num = Text.CalcHeight(faction.def.LabelCap, fillRect.width);
            float num2 = Mathf.Max(80f, num);
            
            return num2;
        }

        private float DrawFactionRowCommsXeno(Faction faction, float rowY, Rect fillRect, Pawn negotiator)
        {
            Rect rect = new Rect(5f, rowY, fillRect.width - 10f, 50f);
            Rect rect2 = rect;
            if (Widgets.ButtonText(rect, "", true, true, true))
            {
                if (faction.leader == null) Log.Error("NoLeader for: " + faction.Name);
                this.Close();
                CorruptionStoryTrackerUtilities.TryOpenIoMComms(negotiator, faction);

            }
            GUI.DrawTexture(new Rect(rect.x, rect.y + 15f, rect.width, 20f), CorruptionStoryTrackerUtilities.XenoFactionCrypticLabel(faction));
            float num = Text.CalcHeight(faction.def.LabelCap, fillRect.width);
            float num2 = Mathf.Max(80f, num);

            return num2;
        }
    }
}
