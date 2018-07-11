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
        protected virtual IoMChatType ChatType
        {
            get
            {
                return IoMChatType.SimpleChat;
            }
        }
        
        protected override bool TryResolveParmsGeneral(IncidentParms parms)
        {
            parms.faction = CorruptionStoryTrackerUtilities.CurrentStoryTracker.IoM_NPC;
            if (parms.faction == null) Log.Message("NUll wanderer faction");
            return true;
        }

        protected virtual TraderKindDef TraderKind
        {
            get
            {
                return DefOfs.C_TraderKindDefs.Visitor_IoM_Wanderer;
            }
        }

        protected virtual PawnKindDef PawnKind
        {
            get
            {
                return DefOfs.C_PawnKindDefOf.IoM_WanderingTrader;
            }
        }

        protected virtual float SocialSkillBoost
        {
            get
            {
                return 10000f;
            }
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!this.TryResolveParmsGeneral(parms))
            {
                return false;
            }
            Pawn centerPawn = null;

            if (IoM_StoryUtilities.GenerateIntrusiveWanderer(map, PawnKind, parms.faction, this.ChatType, "IoM_WandererArrives", out centerPawn))
            {
                centerPawn.skills.Learn(SkillDefOf.Social, SocialSkillBoost, true);
                this.TryConvertOnePawnToSmallTrader(centerPawn, parms.faction, map);
                return true;
            }            

            return false;
        }

        private bool TryConvertOnePawnToSmallTrader(Pawn pawn, Faction faction, Map map )
        {
            Lord lord = pawn.GetLord();
            pawn.mindState.wantsToTradeWithColony = true;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
            TraderKindDef traderKindDef = TraderKind;
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
