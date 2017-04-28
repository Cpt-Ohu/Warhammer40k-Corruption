using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Corruption
{
    public class IncidentWorker_CorruptiveDrone : IncidentWorker_PsychicEmanation
    {
        public override bool TryExecute(IncidentParms parms)
        {
            this.DoConditionAndLetter(Find.VisibleMap, Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f), Gender.None);
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
            return true;
        }

        protected override void DoConditionAndLetter(Map map, int duration, Gender gender)
        {
            MapCondition_CorruptiveDrone MapCondition_corruptiveDrone = (MapCondition_CorruptiveDrone)MapConditionMaker.MakeCondition(C_MapConditionDefOf.CorruptiveDrone, duration, 0);
            map.mapConditionManager.RegisterCondition(MapCondition_corruptiveDrone);
            string text = "LetterIncidentCorruptiveDrone".Translate();
            Find.LetterStack.ReceiveLetter("LetterLabelCorruptiveDrone".Translate(), text, LetterType.BadNonUrgent, null);
        }
    }
}
