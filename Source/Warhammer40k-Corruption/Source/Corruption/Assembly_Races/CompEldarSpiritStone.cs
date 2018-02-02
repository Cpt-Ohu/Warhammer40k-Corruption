using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Corruption.DefOfs;

namespace Corruption
{
    public class CompEldarSpiritStone : ThingComp
    {
        private Pawn Eldar
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        private IntVec3 curpos = new IntVec3();

        private bool IsSpawned;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.Eldar == null)
            {
                Log.Error("Tried to put Spirit Stone Comp on non-Pawn");
            }
        }

        public override void PostDeSpawn(Map map)
        {
            if (!IsSpawned && this.Eldar.Dead)
            {
                Thing spiritstone = ThingMaker.MakeThing(C_ThingDefOfs.SpiritStone);
                GenSpawn.Spawn(spiritstone, curpos, map);
                IsSpawned = true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.parent.Spawned)
            {
                this.curpos = Eldar.Position;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.IsSpawned, "IsSpawned", true, false);
        }
    }
}
