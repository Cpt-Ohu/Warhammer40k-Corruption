using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.Sound;
using UnityEngine;
using Corruption.DefOfs;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public static class DemonUtilities
    {

        public static void FinishedSummoningRitual(Pawn summoner)
        {
            int rand = Rand.RangeInclusive(0, 2);
            switch (rand)
            {
                case 0:
                    {
                        IntVec3 spawnloc = summoner.Position;
                        SoundInfo info = SoundInfo.InMap(new TargetInfo(spawnloc, summoner.Map), MaintenanceType.None);
                        SoundDefOf.Thunder_OnMap.PlayOneShot(info);
                        Thing rift = ThingMaker.MakeThing(C_ThingDefOfs.WarpRift);
                        summoner.Destroy(DestroyMode.Vanish);
                        GenSpawn.Spawn(rift, spawnloc, summoner.Map);
                        break;
                    }
                case 1:
                    {

                        break;
                    }

            }
        }

        public static Pawn GenerateDemon(PawnKindDef demonKind = null)
        {
            PawnKindDef pdef = DemonDefOfs.Demon_Undivided;
            if (demonKind != null)
            {
                pdef = demonKind;
            }
            else
            {
                int num = Rand.RangeInclusive(0, 4);
                switch (num)
                {
                    case 0:
                        {
                            pdef = DemonDefOfs.Demon_Bloodletter;
                            break;
                        }
                    case 1:
                        {
                            pdef = DemonDefOfs.Demon_Plaguebearer;
                            break;
                        }
                    case 2:
                        {
                            pdef = DemonDefOfs.Demon_Daemonette;
                            break;
                        }
                    case 3:
                        {
                            pdef = DemonDefOfs.Demon_Horror;
                            break;
                        }
                    case 4:
                        {
                            pdef = DemonDefOfs.Demon_Undivided;
                            break;
                        }
                }
            }

            PawnGenerationRequest request = new PawnGenerationRequest(pdef, Find.FactionManager.FirstFactionOfDef(C_FactionDefOf.ChaosCult_NPC));
            Pawn pawn = null;
            try
            {
                pawn = PawnGenerator.GeneratePawn(request);
            }
            catch (Exception arg)
            {
                Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
                pawn = PawnGenerator.GeneratePawn(request);
            }
            pawn.relations.everSeenByPlayer = true;
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            return pawn;
        }

    }
}
