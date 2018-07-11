using OHUShips;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class DropshipCargoFreighter : ShipBase
    {

        public DropshipCargoFreighter() : base()
        {

        }

        public DropshipCargoFreighter(bool isIncoming = false, bool shouldSpawnRefueled = false) : base(isIncoming, shouldSpawnRefueled)
        {

        }

        public override void DeSpawn()
        {
            base.DeSpawn();
            this.Destroy();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.ShipUnload(false, true, true);
                var target = new GlobalTargetInfo(0);
                this.TryLaunch(target, RimWorld.PawnsArriveMode.EdgeDrop, TravelingShipArrivalAction.Despawn);
            }

        }
    }
}
