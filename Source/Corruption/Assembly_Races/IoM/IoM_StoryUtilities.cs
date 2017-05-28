using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace Corruption.IoM
{
    public static class IoM_StoryUtilities
    {
        public static LordToil GetWandererChatToil(IoMChatType chatType)
        {
            switch (chatType)
            {
                case IoMChatType.SimpleChat:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndChat);
                    }
                case IoMChatType.ConvertEmperor:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndChat);
                    }
                case IoMChatType.ConvertChaos:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndChat);
                    }
                case IoMChatType.ConvertTau:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndChat);
                    }
                case IoMChatType.InquisitorInvestigation:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndInvestigate);
                    }
            }
            return null;
        }

        public static bool InquisitorShouldStartDirectAttack(Map map, List<Pawn> inquisitors)
        {
            float colonyFighterStrength = 0f;
            float inquisitionFightingStrength = 0f;

            for (int i = 0; i < map.mapPawns.FreeColonistsSpawnedCount; i++)
            {
                colonyFighterStrength += 50f;
            }

            List<Building> turrets = map.listerBuildings.allBuildingsColonist.FindAll(x => x is Building_Turret);
            for (int i=0; i < turrets.Count; i++)
            {
                colonyFighterStrength += 50f;
            }

            for (int i = 0; i < inquisitors.Count; i++)
            {
                inquisitionFightingStrength += inquisitors[0].kindDef.combatPower;
            }

            if (inquisitionFightingStrength >= colonyFighterStrength)
            {
                return true;
            }
            return false;
        }

        public static bool PawnInPrivateQuarters(Pawn pawn)
        {
            Room privateRoom = pawn.ownership.OwnedRoom;
            if (privateRoom == null || (privateRoom != null && !privateRoom.Cells.Contains(pawn.Position)))
            {
                return false;
            }
            return true;
        }
        
    }
}
