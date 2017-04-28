using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel;
using Verse;
using RimWorld;
using UnityEngine;

namespace AlienRace
{
   public static class AlienRaceUtilities
    {        

        public static TConvert ConvertTo<TConvert>(this object entity) where TConvert : new()
        {
            var convertProperties = TypeDescriptor.GetProperties(typeof(TConvert)).Cast<PropertyDescriptor>();
            var entityProperties = TypeDescriptor.GetProperties(entity).Cast<PropertyDescriptor>();

            var convert = new TConvert();


                foreach (var entityProperty in entityProperties)
                {
                    var property = entityProperty;
                    var convertProperty = convertProperties.FirstOrDefault(prop => prop.Name == property.Name);
                    if (convertProperty != null)
                    {
                    try
                        {
                        if (CanChangeType(convertProperty, typeof(Verse.Name)))
                        convertProperty.SetValue(convert, Convert.ChangeType(entityProperty.GetValue(entity), convertProperty.PropertyType));
                        }

                    catch (Exception)
                    {
                        Log.Error("Could not convert Pawn to AlienPawn.");
                        throw;
                    }

                    }
                }            

            return convert;
        }

        public static bool CanChangeType(object value, Type conversionType)
        {
            if (conversionType == null)
            {
                return false;
            }

            if (value == null)
            {
                return false;
            }

            IConvertible convertible = value as IConvertible;

            if (convertible == null)
            {
                return false;
            }

            return true;
        }

        public static void DoRecruitAlien(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
        {
            string text = recruitee.LabelIndefinite();
            if (recruitee.guest != null)
            {
                recruitee.guest.SetGuestStatus(null, false);
            }
            bool flag = recruitee.Name != null;
            if (recruitee.Faction != recruiter.Faction)
            {

                if (recruitee.kindDef.race.ToString().Contains("Alien"))
                {
                    Log.Message("RecruitingAlienPawn");
                    var x = recruitee.kindDef;
                    AlienPawn temprecruitee = recruitee as AlienPawn;
                    temprecruitee.SetFaction(recruiter.Faction, recruiter);
                    temprecruitee.kindDef = x;
                    Log.Message("Pawn Converted to Kind:  " + recruitee.kindDef.race.ToString());
                }
                else
                {
                    recruitee.SetFaction(recruiter.Faction, recruiter);
                }
            }
            if (recruitee.RaceProps.Humanlike)
            {
                if (useAudiovisualEffects)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelMessageRecruitSuccess".Translate(), "MessageRecruitSuccess".Translate(new object[]
                    {
                recruiter,
                recruitee,
                recruitChance.ToStringPercent()
                    }), LetterType.Good, recruitee, null);
                }
                TaleRecorder.RecordTale(TaleDefOf.Recruited, new object[]
                {
            recruiter,
            recruitee
                });
                recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
                recruitee.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RecruitedMe, recruiter);
            }
            else
            {
                if (useAudiovisualEffects)
                {
                    if (!flag)
                    {
                        Messages.Message("MessageTameAndNameSuccess".Translate(new object[]
                        {
                    recruiter.LabelShort,
                    text,
                    recruitChance.ToStringPercent(),
                    recruitee.Name.ToStringFull
                        }).AdjustedFor(recruitee), recruitee, MessageSound.Benefit);
                    }
                    else
                    {
                        Messages.Message("MessageTameSuccess".Translate(new object[]
                        {
                    recruiter.LabelShort,
                    text,
                    recruitChance.ToStringPercent()
                        }), recruitee, MessageSound.Benefit);
                    }
                    MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, "TextMote_TameSuccess".Translate(new object[]
                    {
                recruitChance.ToStringPercent()
                    }), 8f);
                }
                recruiter.records.Increment(RecordDefOf.AnimalsTamed);
                RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.05f);
                float num = Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness);
                if (Rand.Value < num)
                {
                    TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, new object[]
                    {
                recruiter,
                recruitee
                    });
                }
            }
            if (recruitee.caller != null)
            {
                recruitee.caller.DoCall();
            }
        }

    }
}
