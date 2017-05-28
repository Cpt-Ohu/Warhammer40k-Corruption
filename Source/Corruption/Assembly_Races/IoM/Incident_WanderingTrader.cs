using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace Corruption.IoM
{    
    public class IncidentWorker_WanderingTrader : IncidentWorker_NeutralGroup
    {
        protected IoMChatType ChatType;

        protected override bool TryResolveParms(IncidentParms parms)
        {
            parms.faction = CorruptionStoryTrackerUtilities.currentStoryTracker.IoM;            
            return true;
        }

        public IncidentWorker_WanderingTrader()
        {
            this.ChatType = IoMChatType.SimpleChat;
        }

        public override bool TryExecute(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!this.TryResolveParms(parms))
            {
                return false;
            }

            Pawn pawn = PawnGenerator.GeneratePawn(DefOfs.C_PawnKindDefOf.IoM_WanderingTrader, parms.faction);
            if (pawn == null)
            {
                return false;
            }
            IntVec3 loc;
            RCellFinder.TryFindRandomPawnEntryCell(out loc, map);
            GenSpawn.Spawn(pawn, loc, map);
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out chillSpot);
            LordJob_IntrusiveWanderer lordJob = new LordJob_IntrusiveWanderer(chillSpot, pawn, ChatType);
            Lord lord = LordMaker.MakeNewLord(parms.faction, lordJob, map);
            lord.AddPawn(pawn);
            this.TryConvertOnePawnToSmallTrader(pawn, parms.faction, map);
            string label = "LetterLabelSingleVisitorArrives".Translate();
            string text3 = "IoM_WandererArrives".Translate(new object[]
            {
                pawn.Name                
            });
            text3 = text3.AdjustedFor(pawn);
            Find.LetterStack.ReceiveLetter(label, text3, LetterType.Good, pawn, null);
            return true;
        }

        private bool TryConvertOnePawnToSmallTrader(Pawn pawn, Faction faction, Map map)
        {
            Lord lord = pawn.GetLord();
            pawn.mindState.wantsToTradeWithColony = true;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
            TraderKindDef traderKindDef = DefOfs.C_TraderKindDefs.Visitor_IoM_Wanderer;
            pawn.trader.traderKind = traderKindDef;
            pawn.inventory.DestroyAll(DestroyMode.Vanish);
            foreach (Thing current in TraderStockGenerator.GenerateTraderThings(traderKindDef, map))
            {
                Pawn pawn2 = current as Pawn;
                if (pawn2 != null)
                {
                    if (pawn2.Faction != pawn.Faction)
                    {
                        pawn2.SetFaction(pawn.Faction, null);
                    }
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 5);
                    GenSpawn.Spawn(pawn2, loc, map);
                    lord.AddPawn(pawn2);
                }
                else if (!pawn.inventory.innerContainer.TryAdd(current, true))
                {
                    current.Destroy(DestroyMode.Vanish);
                }
            }
            PawnInventoryGenerator.GiveRandomFood(pawn);
            return true;
        }
    }
}
