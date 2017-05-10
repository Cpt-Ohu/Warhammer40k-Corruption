using Corruption.Ships;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class ScenPart_StartWithShip : ScenPart
    {
        public List<Ships.ShipBase> StartingShips = new List<Ships.ShipBase>();

        public ThingDef ShipDef;

        private List<Thing> startingCargo = new List<Thing>();

        public void AddToStartingCargo(Thing newCargo)
        {
            this.startingCargo.Add(newCargo);
        }

        public void AddToStartingCargo(IEnumerable<Thing> newCargo)
        {
            this.startingCargo.AddRange(newCargo);
        }
        public override IEnumerable<Thing> PlayerStartingThings()
        {
            return this.startingCargo;
        }

        public override void GenerateIntoMap(Map map)
        {
            ShipBase newShip = (ShipBase)ThingMaker.MakeThing(this.ShipDef);
            newShip.SetFaction(Faction.OfPlayer);
            newShip.ShouldSpawnFueled = true;
            this.StartingShips.Add(newShip);
            DropShipUtility.LoadNewCargoIntoRandomShips(this.PlayerStartingThings().ToList(), this.StartingShips);
            DropShipUtility.DropShipGroups(map.Center, map, this.StartingShips, TravelingShipArrivalAction.EnterMapFriendly);
        }

        public override void DoEditInterface(Listing_ScenEdit listing)
        {
            base.DoEditInterface(listing);
        }


    }
}
