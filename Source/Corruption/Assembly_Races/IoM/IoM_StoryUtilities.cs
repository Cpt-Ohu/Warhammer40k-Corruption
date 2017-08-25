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
        public static LordToil_WanderAndChat GetWandererChatToil(IoMChatType chatType)
        {
            switch (chatType)
            {
                case IoMChatType.SimpleChat:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndChat);
                    }
                case IoMChatType.ConvertEmperor:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndPraise);
                    }
                case IoMChatType.ConvertChaos:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndCorrupt);
                    }
                case IoMChatType.ConvertTau:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndConvertTau);
                    }
                case IoMChatType.VisitingHealer:
                    {
                        return new LordToil_WanderAndChat(DefOfs.C_DutyDefOfs.FollowAndHeal);
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
        
        public static bool GenerateIntrusiveWanderer(Map map, PawnKindDef healerDef, Faction faction, IoMChatType chattype, string LetterContent, out Pawn wanderer)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(healerDef, faction);
            wanderer = pawn;
            if (wanderer == null)
            {
                return false;
            }
            IntVec3 loc;
            RCellFinder.TryFindRandomPawnEntryCell(out loc, map, 0.8f);
            GenSpawn.Spawn(wanderer, loc, map);
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out chillSpot);
            LordJob_IntrusiveWanderer lordJob = new LordJob_IntrusiveWanderer(chillSpot, wanderer, chattype);
            Lord lord = LordMaker.MakeNewLord(faction, lordJob, map);
            lord.AddPawn(wanderer);
            string label = "LetterLabelSingleVisitorArrives".Translate();
            string text3 = LetterContent.Translate(new object[]
            {
                wanderer.Name
            });
            text3 = text3.AdjustedFor(wanderer);
            Find.LetterStack.ReceiveLetter(label, text3, LetterDefOf.Good, wanderer, null);
            return true;
        }        
    }
}
