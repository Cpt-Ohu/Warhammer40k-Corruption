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
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.DoConditionAndLetter(Find.CurrentMap,60, Gender.None, Mathf.RoundToInt(this.def.durationDays.RandomInRange * 60000f));
            SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera();
            return true;
        }

        protected override void DoConditionAndLetter(Map map, int duration, Gender gender, float points)
        {
            GameCondition_CorruptiveDrone MapCondition_corruptiveDrone = (GameCondition_CorruptiveDrone)GameConditionMaker.MakeCondition(C_GameConditionDefOf.CorruptiveDrone, duration, 0);
            map.gameConditionManager.RegisterCondition(MapCondition_corruptiveDrone);
            string text = "LetterIncidentCorruptiveDrone".Translate();
            Find.LetterStack.ReceiveLetter("LetterLabelCorruptiveDrone".Translate(), text, LetterDefOf.NegativeEvent, null);
        }

    }
}
