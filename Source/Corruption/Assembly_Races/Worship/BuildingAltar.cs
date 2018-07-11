using Corruption.DefOfs;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Corruption
{
    public class BuildingAltar : Building
    {
        public bool OptionMorning = false;

        public bool OptionEvening = false;

        private bool HeldSermon;

        public bool activeSermon;

        public string RoomName;

        public bool CalledInFlock = false;

        public Pawn preacher = null;             

        public bool DoMorningSermon
        {
            get
            {
                return (OptionMorning && (GenLocalDate.HourFloat(this.Map) < 6 && GenLocalDate.HourFloat(this.Map) > 10));
            }
        }

        public bool DoEveningSermon
        {
            get
            {
                return (OptionEvening && (GenLocalDate.HourFloat(this.Map) < 18 && GenLocalDate.HourFloat(this.Map) > 22));
            }
        }

        public BuildingAltarDef bdef
        {
            get
            {
                return this.def as BuildingAltarDef;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                this.preacher = Map.mapPawns.FreeColonistsSpawned.RandomElement<Pawn>();
            }
            RoomName = "Temple";
            TickManager f = Find.TickManager;

            f.RegisterAllTickabilityFor(this);
           
        }
        
        public override void Tick()
        {
            base.Tick();
            if (this.OptionMorning)
            {
                if (Rand.RangeInclusive(6, 10) == GenLocalDate.HourFloat(this.Map))
                {
                    if (!HeldSermon)
                    {
                //        Log.Message("starting morning sermon");
                        SermonUtility.ForceSermon(this, Worship.WorshipActType.MorningPrayer);
                        this.HeldSermon = true;
                    }
                }
            }

            if (this.OptionEvening)
            {
                if (Rand.RangeInclusive(18, 22) == GenLocalDate.HourFloat(this.Map))
                {
                    if (!HeldSermon)
                    {
                        SermonUtility.ForceSermon(this, Worship.WorshipActType.EveningPrayer);
                        this.HeldSermon = true;
                    }
                }
            }

            if (GenLocalDate.HourFloat(this.Map) == 1 || GenLocalDate.HourFloat(this.Map) == 12)
            {
                this.HeldSermon = false;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look<Pawn>(ref this.preacher, "preacher", false);
            Scribe_Values.Look<string>(ref this.RoomName, "RoomName", "Temple", false);
            Scribe_Values.Look<bool>(ref this.OptionEvening, "OptionEvening", false, false);
            Scribe_Values.Look<bool>(ref this.OptionMorning, "OptionMorning", false, false);
            Scribe_Values.Look<bool>(ref this.HeldSermon, "HeldSermon", true, false);
            Scribe_Values.Look<bool>(ref this.CalledInFlock, "CalledInFlock", false, false);

            Scribe_Values.Look<bool>(ref this.HeldSermon, "HeldSermon", true, false);

        }        
    }
}
