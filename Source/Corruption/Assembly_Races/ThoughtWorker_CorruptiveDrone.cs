using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ThoughtWorker_CorruptiveDrone : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {

            Need_Soul soul = p.needs.TryGetNeed<Need_Soul>();
            if (soul == null)
            {
                soul = new Need_Soul(p);
            }

            float factor = 1f;
            soul.GainNeed(factor * 0.00001f);

            PsychicDroneLevel psychicDroneLevel = PsychicDroneLevel.None;
            GameCondition_PsychicEmanation activeCondition = p.Map.gameConditionManager.GetActiveCondition<GameCondition_PsychicEmanation>();
            if (activeCondition != null && activeCondition.def.droneLevel > psychicDroneLevel)
            {
                psychicDroneLevel = activeCondition.def.droneLevel;
            }
            else return ThoughtState.Inactive;
            switch (psychicDroneLevel)
            {
                case PsychicDroneLevel.None:
                    factor = 0f;
                    return false;
                case PsychicDroneLevel.BadLow:
                    factor = 1f;
                    break;
                case PsychicDroneLevel.BadMedium:
                    factor = 1.3f;
                    break;
                case PsychicDroneLevel.BadHigh:
                    factor = 1.5f;
                    break;
                case PsychicDroneLevel.BadExtreme:
                    factor = 2f;
                    break;
                default:
                    return ThoughtState.Inactive;
            }


            if (soul.NoPatron)
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (soul.PsykerPowerLevel == PsykerPowerLevel.Omega)
            {
                return ThoughtState.Inactive;
            }
            return ThoughtState.ActiveAtStage(1);



        }
    }
}
