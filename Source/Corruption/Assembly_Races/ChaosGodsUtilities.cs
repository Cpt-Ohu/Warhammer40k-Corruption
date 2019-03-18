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
            CompSoul soul;
            if ((soul = CompSoul.GetPawnSoul(pawn)) != null)
            {
                return ChaosGodsUtilities.GetPatronIcon(soul.Patron);
            }
            return new Texture2D(10,10);
        }

        public static Texture2D TryGetPreacherIcon(Pawn pawn)
        {
            CompSoul soul;
            if ((soul = CompSoul.GetPawnSoul(pawn)) != null)
            {
                if (soul.Corrupted)
                {
                    return ChaosGodsUtilities.MoteSermonDark;
                }
                else
                {
                    return ChaosGodsUtilities.MoteSermonHoly;
                }
            }
            return new Texture2D(10, 10);
        }

        public static Texture2D GetPatronIcon(PatronDef patron)
        {
            if (patron == PatronDefOf.Emperor)
            {
                return ChaosGodsUtilities.ButtonEmperor;
            }
            else if (patron == PatronDefOf.ChaosUndivided)
            {
                return ChaosGodsUtilities.ButtonUndivided;
            }
            else if (patron == PatronDefOf.Khorne)
            {
                return ChaosGodsUtilities.ButtonKhorne;
            }
            else if (patron == PatronDefOf.Nurgle)
            {
                return ChaosGodsUtilities.ButtonNurgle;
            }
            else if (patron == PatronDefOf.Tzeentch)
            {
                return ChaosGodsUtilities.ButtonTzeentch;
            }
            else if (patron == PatronDefOf.Slaanesh)
            {
                return ChaosGodsUtilities.ButtonSlaanesh;
            }
            else if (patron == PatronDefOf.GorkMork)
            {
                return ChaosGodsUtilities.OrkButton;
            }
            else if (patron == PatronDefOf.Ynnead)
            {
                return ChaosGodsUtilities.EldarButton;
            }
            else if (patron == PatronDefOf.GreaterGood)
            {
                return ChaosGodsUtilities.ButtonUndivided;
            }

            return ChaosGodsUtilities.ButtonEmperor;
        } 


        public static void DoEffectOn(Pawn target, MentalStateDef mdef)
        {
            if (target.Dead)
            {
                return;
            }
            target.mindState.mentalStateHandler.TryStartMentalState(mdef, null, false);
        }

        public static MentalStateDef SlaaneshEffects(Pawn pawn)
        {
            int num = Rand.RangeInclusive(1, 9);

            if(num >7)
            {
                return C_MentalStateDefOf.LustViolent;
            }
            if (num > 5)
            {
                return MentalStateDefOf.Binging_DrugExtreme;
            }
            if (num > 3)
            {
                return MentalStateDefOf.Binging_DrugMajor;
            }
            return C_MentalStateDefOf.Binging_Food;
        }

        public static MentalStateDef KhorneEffects(Pawn pawn)
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
