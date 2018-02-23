using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Corruption.Worship
{
    public class MainTabWindow_Worship : MainTabWindow
    {
        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        public override void DoWindowContents(Rect fillRect)
        {
            base.DoWindowContents(fillRect);
            Rect position = new Rect(0f, 0f, fillRect.width, fillRect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);
            Rect rect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, rect, true);
            float num = 0f;
            foreach (Faction current in Find.FactionManager.AllFactionsInViewOrder)
            {
                if (!current.IsPlayer)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    Widgets.DrawLineHorizontal(0f, num, rect.width);
                    GUI.color = Color.white;
                    num += DrawColonyRow(num, rect);
                    //num += FactionUIUtility.DrawFactionRow(current, num, rect);
                }
            }
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = num;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private static float DrawColonyRow(float rowY, Rect fillrect)
        {


            return rowY;
        }
    }
}
