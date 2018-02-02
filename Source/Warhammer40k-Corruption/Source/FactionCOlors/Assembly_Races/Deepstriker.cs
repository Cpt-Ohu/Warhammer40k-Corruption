using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace FactionColors
{
    public class Deepstriker : Deepstriker_Base
    {
        public int age;

        public DropPodInfo info;

        private bool FirstDrop = true;

        public Deepstriker_ThingDef tdef
        {
            get
            {
                return this.def as Deepstriker_ThingDef;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.LookValue<int>(ref this.age, "age", 0, false);
            Scribe_Values.LookValue<bool>(ref this.FirstDrop, "FirstDrop", false, false);
            Scribe_Deep.LookDeep<DropPodInfo>(ref this.info, "info", new object[0]);
        }

        public override void Tick()
        {
            this.age++;
            if (this.age > this.info.openDelay && this.FirstDrop)
            {
                this.Unload();
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (mode == DestroyMode.Kill)
            {
                for (int j = 0; j < 10; j++)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.ChunkSlagSteel, null);
                    GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near, null);
                }
                List<Thing> plunder = this.info.containedThings;
                for (int i = 0; i < plunder.Count; i++)
                {
                    int x = Rand.RangeInclusive(0, 1);
                    if (x == Rand.RangeInclusive(0, 1))
                    {
                        GenPlace.TryPlaceThing(plunder[i], base.Position, ThingPlaceMode.Near, null);
                    }
                }
            }

            for (int i = this.info.containedThings.Count - 1; i >= 0; i--)
            {
                this.info.containedThings[i].Destroy(DestroyMode.Vanish);
            }

            base.Destroy(mode);
        }

        private void FillUp(Thing newContent)
        {
            this.info.containedThings.Add(newContent);

        }

        private void TakeOff()
        {
            MoteMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), 2f);
            Deepstriker_Leaving deepStrikerOUT = (Deepstriker_Leaving)ThingMaker.MakeThing(tdef.LeavingDef, null);
            GenSpawn.Spawn(deepStrikerOUT, base.Position, base.Rotation);
        }

        private void Unload()
        {
            for (int i = 0; i < this.info.containedThings.Count; i++)
            {
                Thing thing = this.info.containedThings[i];
                Thing thing2;
                GenPlace.TryPlaceThing(thing, base.Position, ThingPlaceMode.Near, out thing2, delegate (Thing placedThing, int count)
                {});
            }
            this.info.containedThings.Clear();
            this.FirstDrop = false;
            SoundDef.Named("DropPodOpen").PlayOneShot(base.Position);
        }
    }
}
