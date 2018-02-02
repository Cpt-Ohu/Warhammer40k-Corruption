using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Corruption.IoM
{
    public static class Toils_InterpersonalToilsIoM
    {
        public static Toil ChatToPawn(Pawn pawn, Pawn talkee, IoMChatType chatType = IoMChatType.SimpleChat)
        {
            return new Toil
            {
                initAction = delegate
                {
                    if (!pawn.interactions.TryInteractWith(talkee, InteractionDefOf.BuildRapport))
                    {
                        pawn.jobs.curDriver.ReadyForNextToil();
                    }
                    else
                    {
                        pawn.records.Increment(RecordDefOf.PrisonersChatted);
                    }
                },
                socialMode = RandomSocialMode.Off,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = 240,
                finishActions = new List<Action>()
                    {
                        delegate
                        {
                            PerformPostChatActions(pawn, talkee, chatType);
                        },
                }                
            };
        }

        public static void PerformPostChatActions(Pawn talker, Pawn talkee, IoMChatType chatType)
        {
            if (chatType != IoMChatType.ConvertTau)
            {
                Need_Soul talkerSoul = CorruptionStoryTrackerUtilities.GetPawnSoul(talker);
                Need_Soul talkeeSoul = CorruptionStoryTrackerUtilities.GetPawnSoul(talkee);

                if (talkerSoul != null && talkeeSoul != null)
                {
                    if (chatType != IoMChatType.InquisitorInvestigation)
                    {
                        if (talkerSoul.NoPatron && talkeeSoul.NoPatron)
                        {
                            if (Rand.Range(4, 6) + GetChatIntrigueFactor(talker, talkee) > 0)
                            {
                                talkeeSoul.GainNeed(-0.0005f);
                            }
                            else
                            {
                                talkee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed, talker);
                            }
                        }
                        else if (talkerSoul.NoPatron && !talkeeSoul.NoPatron)
                        {
                            StartReligiousSocialFight(talker, talkee);
                        }
                        else if (!talkerSoul.NoPatron && !talkeeSoul.NoPatron)
                        {
                            talkeeSoul.GainNeed(0.005f);
                        }
                        else if (!talkerSoul.NoPatron && talkeeSoul.NoPatron)
                        {
                            if (Rand.Range(-2, 0) + GetChatIntrigueFactor(talker, talkee) > 0)
                            {
                                talkeeSoul.GainNeed(-0.0005f);
                            }
                            else
                            {
                                StartReligiousSocialFight(talker, talkee);
                            }
                        }
                    }
                    else
                    {
                        //switch (talkeeSoul.CurCategory)
                        //{
                        //    case SoulAffliction.Lost:
                        //        {

                        //            return;
                        //        }
                        //    case SoulAffliction.Corrupted:
                        //        {

                        //            return;
                        //        }
                        //    case SoulAffliction.Tainted:
                        //        {

                        //            return;
                        //        }
                        //}
                        Lord lord = talker.GetLord();
                        LordJob_IntrusiveWanderer lordJob = lord.LordJob as LordJob_IntrusiveWanderer;
                        lordJob.InquisitorFoundHeretic = true;
                    }
                }
            }
            else
            {
            }
        }

        private static void StartReligiousSocialFight(Pawn talker, Pawn talkee)
        {
            Messages.Message("MessageReligiousSocialFight".Translate(new object[]
            {
                talker.LabelShort,
                talkee.LabelShort,
            }), talker, MessageTypeDefOf.NegativeEvent);


            talker.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.SocialFighting, null, false, false, talkee);
            MentalStateHandler handlerTalkee = talkee.mindState.mentalStateHandler;
            Pawn otherPawn2 = talker;
            handlerTalkee.TryStartMentalState(MentalStateDefOf.SocialFighting, null, false, false, otherPawn2);
        }

        private static int GetChatIntrigueFactor(Pawn pawn, Pawn talkee)
        {
            return pawn.skills.GetSkill(SkillDefOf.Social).Level - talkee.skills.GetSkill(SkillDefOf.Social).Level;
        }

        public static Toil GotoPawn(Pawn pawn, Pawn talkee, PrisonerInteractionModeDef mode)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                pawn.pather.StartPath(talkee, PathEndMode.Touch);
            };
            toil.AddFailCondition(() => talkee.Destroyed);
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }
    }
}
