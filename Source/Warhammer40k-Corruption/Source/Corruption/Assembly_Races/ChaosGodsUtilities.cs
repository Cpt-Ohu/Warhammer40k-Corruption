using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public static class ChaosGodsUtilities
    {
        private static Texture2D ButtonUndivided = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonUndivided", true);
        private static Texture2D ButtonKhorne = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonKhorne", true);
        private static Texture2D ButtonNurgle = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonNurgle", true);
        private static Texture2D ButtonTzeentch = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonTzeentch", true);
        private static Texture2D ButtonSlaanesh = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonSlaanesh", true);
        private static Texture2D ButtonEmperor = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonEmperor", true);

        private static Texture2D OrkButton = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonOrks", true);
        private static Texture2D EldarButton = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonEldar", true);
        private static Texture2D TauButton = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonTau", true);

        private static Texture2D MoteSermonHoly = ContentFinder<Texture2D>.Get("UI/Motes/MotePrayerEmperor", true);
        private static Texture2D MoteSermonDark = ContentFinder<Texture2D>.Get("UI/Motes/MotePrayerDark", true);

        public static Texture2D TryGetPatronIcon(Pawn pawn)
        {
            Need_Soul soul;
            if ((soul = pawn.needs.TryGetNeed<Need_Soul>()) != null)
            {
                return ChaosGodsUtilities.GetPatronIcon(soul.patronInfo.PatronName);
            }
            return new Texture2D(10,10);
        }

        public static Texture2D TryGetPreacherIcon(Pawn pawn)
        {
            Need_Soul soul;
            if ((soul = pawn.needs.TryGetNeed<Need_Soul>()) != null)
            {
                if (soul.NoPatron)
                {
                    return ChaosGodsUtilities.MoteSermonHoly;
                }
                else
                {
                    return ChaosGodsUtilities.MoteSermonDark;
                }
            }
            return new Texture2D(10, 10);
        }

        public static Texture2D GetPatronIcon(string patron)
        {
            switch(patron)
            {
                case "Undivided":
                    {
                        return ChaosGodsUtilities.ButtonUndivided;
                    }
                case "Khorne":
                    {
                        return ChaosGodsUtilities.ButtonKhorne;
                    }
                case "Nurgle":
                    {
                        return ChaosGodsUtilities.ButtonNurgle;
                    }
                case "Tzeentch":
                    {
                        return ChaosGodsUtilities.ButtonTzeentch;
                    }
                case "Slaanesh":
                    {
                        return ChaosGodsUtilities.ButtonSlaanesh;
                    }
                case "Orks":
                    {
                        return ChaosGodsUtilities.OrkButton;
                    }
                case "Eldar":
                    {
                        return ChaosGodsUtilities.EldarButton;
                    }
                case "Tau":
                    {
                        return ChaosGodsUtilities.TauButton;
                    }
                default:
                    {
                        return ChaosGodsUtilities.ButtonEmperor;
                    }
            }
        } 


        public static void DoEffectOn(Pawn target, MentalStateDef mdef)
        {
            if (target.Dead)
            {
                return;
            }
            target.mindState.mentalStateHandler.TryStartMentalState(mdef, null, false);
        }

        public static MentalStateDef SlaaneshEffects(Pawn pawn, Need_Soul soul)
        {
            int num = Rand.RangeInclusive(1, 9);

            if(num >7)
            {
                return C_MentalStateDefOf.LustViolent;
            }
            if (num > 5)
            {
                return MentalStateDefOf.BingingDrugExtreme;
            }
            if (num > 3)
            {
                return MentalStateDefOf.BingingDrugMajor;
            }
            return C_MentalStateDefOf.BingingFood;
        }

        public static MentalStateDef KhorneEffects(Pawn pawn, Need_Soul soul)
        {
            int num = Rand.RangeInclusive(1, 9);

            if (num > 6)
            {
                return MentalStateDefOf.Berserk;
            }
            return C_MentalStateDefOf.KhorneKillWeak;
        }
    }
}
