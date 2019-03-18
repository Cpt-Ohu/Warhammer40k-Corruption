using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class Dialog_NegotiationIoM : Dialog_NodeTree
    {
        private const float TitleHeight = 70f;

        private const float InfoHeight = 60f;

        protected Pawn negotiator;

        protected ICommunicable commTarget;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(720f, 600f);
            }
        }

        public Dialog_NegotiationIoM(Pawn negotiator, ICommunicable commTarget, DiaNode startNode, bool radioMode) : base(startNode, radioMode, false)
		{
            this.negotiator = negotiator;
            this.commTarget = commTarget;
            CFind.MissionManager.FinishMission("UseCCC");
        }

        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            Rect rect = new Rect(0f, 0f, inRect.width / 2f, 70f);
            Rect rect2 = new Rect(0f, rect.yMax, rect.width, 60f);
            Rect rect3 = new Rect(inRect.width / 2f, 0f, inRect.width / 2f, 70f);
            Rect rect4 = new Rect(inRect.width / 2f, rect.yMax, rect.width, 60f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, this.negotiator.LabelCap);
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect3, this.commTarget.GetCallLabel());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            Widgets.Label(rect2, "SocialSkillIs".Translate(new object[]
            {
                this.negotiator.skills.GetSkill(SkillDefOf.Social).Level
            }));
            Rect rect5 = new Rect(inRect.width / 2f - 64f, 0f, 128f, 128f);
            GUI.DrawTexture(rect5, CorruptionStoryTrackerUtilities.Aquila);

            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(rect4, this.commTarget.GetInfoText());
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            GUI.EndGroup();
            float num = 147f;
            Rect rect6 = new Rect(0f, num, inRect.width, inRect.height - num);
            base.DrawNode(rect6);
        }
    }
}
