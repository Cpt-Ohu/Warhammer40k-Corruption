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
        
        protected override bool TryResolveParmsGeneral(IncidentParms parms)
        {
            parms.faction = CorruptionStoryTrackerUtilities.currentStoryTracker.IoM;            
            return true;
        }

        public IncidentWorker_WanderingTrader()
        {
            this.ChatType = IoMChatType.SimpleChat;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!this.TryResolveParmsGeneral(parms))
            {
                return false;
            }
            Pawn centerPawn = null;

            if (IoM_StoryUtilities.GenerateIntrusiveWanderer(map, DefOfs.C_PawnKindDefOf.IoM_WanderingTrader, parms.faction, this.ChatType, "IoM_WandererArrives", out centerPawn))
            {
                this.TryConvertOnePawnToSmallTrader(centerPawn, parms.faction, map);
                return true;
            }

            Pawn pawn = PawnGenerator.GeneratePawn(DefOfs.C_PawnKindDefOf.IoM_WanderingTrader, parms.faction);
            if (pawn == null)
            {
                return false;
            }

            return false;
        }

        private bool TryConvertOnePawnToSmallTrader(Pawn pawn, Faction faction, Map map )
        {
            Lord lord = pawn.GetLord();
            pawn.mindState.wantsToTradeWithColony = true;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
            TraderKindDef traderKindDef = DefOfs.C_TraderKindDefs.Visitor_IoM_Wanderer;
            pawn.trader.traderKind = traderKindDef;
            pawn.inventory.DestroyAll(DestroyMode.Vanish);

            ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
            parms.traderDef = traderKindDef;
            parms.tile = map.Tile;
            parms.traderFaction = faction;

            foreach (Thing current in ItemCollectionGeneratorDefOf.TraderStock.Worker.Generate(parms)) //in ItemCollectionGenerator_TraderStock.thingsBeingGeneratedNow(traderKindDef, map))
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
