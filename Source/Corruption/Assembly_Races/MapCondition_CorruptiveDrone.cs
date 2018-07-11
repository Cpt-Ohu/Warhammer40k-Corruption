using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class GameCondition_CorruptiveDrone : GameCondition
    {
        private SkyColorSet CorruptiveDroneColors;

        public GameCondition_CorruptiveDrone()
        {
            ColorInt colorInt = new ColorInt(240, 141, 74);
            ColorInt colorInt2 = new ColorInt(254, 245, 176);
            ColorInt colorInt3 = new ColorInt(170, 95, 60);
            this.CorruptiveDroneColors = new SkyColorSet(colorInt.ToColor, colorInt2.ToColor, colorInt3.ToColor, 1.0f);
        }

        public override void GameConditionTick()
        {
            if (Find.TickManager.TicksGame % 3451 == 0)
            {
                List<Pawn> allPawnsSpawned = base.Map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < allPawnsSpawned.Count; i++)
                {
                    Pawn pawn = allPawnsSpawned[i];
                    CompSoul soul = CompSoul.GetPawnSoul(pawn);
                    
                    if (soul != null && !pawn.Position.Roofed(base.Map) && pawn.def.race.IsFlesh)
                    {
                        float num = 0.011758334f;
                        num *= pawn.GetStatValue(StatDefOf.PsychicSensitivity, true);
                        if (num != 0f)
                        {
                            float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 74374237));
                            num *= num2;
                            soul.AffectSoul(num);
                        }
                    }
                }
            }
        }


        public override SkyTarget? SkyTarget()
        {
            return new SkyTarget?(new SkyTarget(0f, this.CorruptiveDroneColors, 1f, 0f)
            {
                glow = 0.85f
            });
        }

    }
}
