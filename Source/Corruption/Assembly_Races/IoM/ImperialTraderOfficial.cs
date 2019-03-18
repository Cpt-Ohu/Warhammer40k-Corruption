using OHUShips;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class ImperialTraderOfficial : PassingShip, ITrader, IThingHolder
    {
        public TraderKindDef def;

        private ThingOwner things;

        private List<Pawn> soldPrisoners = new List<Pawn>();

        private int randomPriceFactorSeed = -1;

        private static List<string> tmpExtantNames = new List<string>();

        private List<Thing> thingsForDrop = new List<Thing>();

        public override string FullTitle
        {
            get
            {
                return this.name + " (" + this.def.label + ")";
            }
        }

        public int Silver
        {
            get
            {
                return this.CountHeldOf(ThingDefOf.Silver, null);
            }
        }

        public IThingHolder ParentHolder
        {
            get
            {
                return base.Map;
            }
        }

        public TraderKindDef TraderKind
        {
            get
            {
                return this.def;
            }
        }

        public int RandomPriceFactorSeed
        {
            get
            {
                return this.randomPriceFactorSeed;
            }
        }

        public string TraderName
        {
            get
            {
                return this.name;
            }
        }

        public bool CanTradeNow
        {
            get
            {
                return !base.Departed;
            }
        }

        public float TradePriceImprovementOffsetForPlayer
        {
            get
            {
                return 0f;
            }
        }

        public Faction Faction
        {
            get
            {
                return this.factionInt;
            }
        }

        public IEnumerable<Thing> Goods
        {
            get
            {
                for (int i = 0; i < this.things.Count; i++)
                {
                    Pawn p = this.things[i] as Pawn;
                    if (p == null || !this.soldPrisoners.Contains(p))
                    {
                        yield return this.things[i];
                    }
                }
            }
        }

        private Faction factionInt;

        public ImperialTraderOfficial()
        {
            this.factionInt = Faction.OfPlayer;
        }

        public ImperialTraderOfficial(TraderKindDef def, Faction faction)
        {
            this.factionInt = faction;
            this.def = def;
            this.things = new ThingOwner<Thing>(this);
            List<Map> maps = Find.Maps;
            this.name = this.GetName();
            this.randomPriceFactorSeed = Rand.RangeInclusive(1, 10000000);
            this.loadID = Find.UniqueIDsManager.GetNextPassingShipID();
        }

        private string GetName()
        {
                return this.Faction.Name;            
        }

        [DebuggerHidden]
        public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
        {
            foreach (Thing t in TradeUtility.AllLaunchableThingsForTrade(base.Map))
            {
                yield return t;
            }
            foreach (Pawn p in TradeUtility.AllSellableColonyPawns(base.Map))
            {
                yield return p;
            }
        }

        public void GenerateThings()
        {
            ThingSetMakerParams parms = default(ThingSetMakerParams);
            parms.traderDef = this.def;
            parms.tile = new int?(base.Map.Tile);
            this.things.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms), true, false);
        }

        public override void PassingShipTick()
        {
            base.PassingShipTick();
            for (int i = this.things.Count - 1; i >= 0; i--)
            {
                Pawn pawn = this.things[i] as Pawn;
                if (pawn != null)
                {
                    pawn.Tick();
                    if (pawn.Dead)
                    {
                        this.things.Remove(pawn);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<TraderKindDef>(ref this.def, "def");
            Scribe_Deep.Look<ThingOwner>(ref this.things, "things", new object[]
            {
                this
            });
            Scribe_Collections.Look<Pawn>(ref this.soldPrisoners, "soldPrisoners", LookMode.Reference, new object[0]);
            Scribe_Values.Look<int>(ref this.randomPriceFactorSeed, "randomPriceFactorSeed", 0, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.soldPrisoners.RemoveAll((Pawn x) => x == null);
            }
        }

        public override void TryOpenComms(Pawn negotiator)
        {
            if (!this.CanTradeNow)
            {
                return;
            }
            Find.WindowStack.Add(new Dialog_Trade(negotiator, this));
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.Critical);
            string empty = string.Empty;
            string empty2 = string.Empty;
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(this.Goods.OfType<Pawn>(), ref empty, ref empty2, "LetterRelatedPawnsTradeShip".Translate(), false, true);
            if (!empty2.NullOrEmpty())
            {
                Find.LetterStack.ReceiveLetter(empty, empty2, LetterDefOf.PositiveEvent, null);
            }
            TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.TradeGoodsMustBeNearBeacon);
        }

        public override void Depart()
        {
            base.Depart();
            this.things.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
            this.soldPrisoners.Clear();
        }

        public override string GetCallLabel()
        {
            return this.name + " (" + this.def.label + ")";
        }

        public int CountHeldOf(ThingDef thingDef, ThingDef stuffDef = null)
        {
            Thing thing = this.HeldThingMatching(thingDef, stuffDef);
            if (thing != null)
            {
                return thing.stackCount;
            }
            return 0;
        }

        public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this);
            Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this, thing);
            if (thing2 != null)
            {
                if (!thing2.TryAbsorbStack(thing, false))
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
            else
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null && pawn.RaceProps.Humanlike)
                {
                    this.soldPrisoners.Add(pawn);
                }
                this.things.TryAdd(thing, false);
            }
            float goodwillFactor = 100.5f - 1 / (Math.Min(0, this.Faction.RelationWith(Faction.OfPlayer).goodwill)) ;
            int goodwillBought =Math.Max(100, (int)( goodwillFactor * (toGive.MarketValue * countToGive)));
            CorruptionStoryTrackerUtilities.AffectGoodwillWithSpacerFaction(Faction.OfPlayer, this.Faction, goodwillBought);
        }

        public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this);
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                this.soldPrisoners.Remove(pawn);
            }
            if (thing.def == ThingDefOf.Silver)
            {
                int goodwillBoost =(int)(thing.stackCount / 1000f);
                Log.Message(goodwillBoost.ToString());
                this.Faction.TryAffectGoodwillWith(Faction.OfPlayer, goodwillBoost);
                thing.Destroy();
            }
            else
            {
                this.thingsForDrop.Add(thing);
            }
        }

        public void SpawnCargoFreigher()
        {
            if (this.thingsForDrop.Count > 0)
            {
                DropshipCargoFreighter freighter = new DropshipCargoFreighter(true, true);
                foreach (Thing thing in this.thingsForDrop)
                {
                    freighter.GetDirectlyHeldThings().TryAdd(thing);
                }
                List<ShipBase> ships = new List<ShipBase>();
                ships.Add(freighter);
                DropShipUtility.DropShipGroups(DropCellFinder.TradeDropSpot(base.Map), base.Map, ships, ShipArrivalAction.EnterMapFriendly);
            }
            this.Depart();
        }

        private Thing HeldThingMatching(ThingDef thingDef, ThingDef stuffDef)
        {
            for (int i = 0; i < this.things.Count; i++)
            {
                if (this.things[i].def == thingDef && this.things[i].Stuff == stuffDef)
                {
                    return this.things[i];
                }
            }
            return null;
        }

        public void ChangeCountHeldOf(ThingDef thingDef, ThingDef stuffDef, int count)
        {
            Thing thing = this.HeldThingMatching(thingDef, stuffDef);
            if (thing == null)
            {
                Log.Error("Changing count of thing trader doesn't have: " + thingDef);
            }
            thing.stackCount += count;
        }

        public override string ToString()
        {
            return this.FullTitle;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.things;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }
    }
}
