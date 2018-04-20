using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class JobDriver_WanderAndChat : JobDriver
    {
        private IoMChatType chatType = IoMChatType.SimpleChat;

        protected virtual IoMChatType ChatType
        {
            get
            {
                return this.chatType;
            }
            set
            {
                this.chatType = value;
            }
        }

        protected Pawn Talkee
        {
            get
            {
                return (Pawn)base.job.targetA.Thing;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnNotAwake(TargetIndex.A);
            this.FailOn(() => IoM_StoryUtilities.PawnInPrivateQuarters(Talkee));

            for (int i = 0; i < Rand.Range(2, 5); i++)
            {
                yield return Toils_InterpersonalToilsIoM.GotoPawn(this.pawn, this.Talkee, PrisonerInteractionModeDefOf.Chat);
                yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
                yield return Toils_InterpersonalToilsIoM.ChatToPawn(this.pawn, this.Talkee, ChatType);
            }

            yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
            if (base.job.def == JobDefOf.PrisonerAttemptRecruit)
            {
                yield return Toils_Interpersonal.TryRecruit(TargetIndex.A);
            }
            this.AddFinishAction(delegate { Toils_InterpersonalToilsIoM.PerformPostChatActions(pawn, Talkee, ChatType); });
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IoMChatType>(ref this.chatType, "chatType");
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
            //return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
        }
    }

    public class JobDriver_FollowAndPraiseEmperor : JobDriver_WanderAndChat
    {
        public JobDriver_FollowAndPraiseEmperor()
        {
            base.ChatType = IoMChatType.ConvertEmperor;
        }
    }
    public class JobDriver_FollowAndCorrupt : JobDriver_WanderAndChat
    {
        public JobDriver_FollowAndCorrupt()
        {
            base.ChatType = IoMChatType.ConvertChaos;
        }
    }
    public class JobDriver_FollowAndConvertTau : JobDriver_WanderAndChat
    {
        public JobDriver_FollowAndConvertTau()
        {
            base.ChatType = IoMChatType.ConvertTau;
        }
    }
    public class JobDriver_FollowAndInvestigate : JobDriver_WanderAndChat
    {
        public JobDriver_FollowAndInvestigate()
        {
            base.ChatType = IoMChatType.InquisitorInvestigation;
        }
    }

}
