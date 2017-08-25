using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompServitor : CompRefuelable
    {
        public Pawn pawn
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Log.Message("2A");
            this.Props.fuelCapacity = 200;
            this.Props.fuelConsumptionRate = 1f;
            this.Props.fuelFilter = new ThingFilter();
            Log.Message("2B");

            FieldInfo info = typeof(ThingFilter).GetField("thingDefs", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
            {
                ThingFilter filter = new ThingFilter();
                List<ThingDef> defs;
                try
                {
                    Log.Message("Trying");
                    defs = info.GetValue(filter) as List<ThingDef>;
                    defs.Add(DefOfs.C_ThingDefOfs.IoM_ServitorFuel);
                }
                catch (TargetInvocationException ex)
                {
                    Log.Message("Failing");
                    Log.Message(ex.InnerException.ToString());
                }
                catch (Exception ex)
                {
                    Log.Message(ex.InnerException.ToString());
                }
            }

            //  List<ThingDef> allowedFood =  Traverse.Create(this.Props.fuelFilter).Field("thingDefs").GetValue<List<ThingDef>>();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!this.HasFuel && !this.pawn.Downed)
            {
                Hediff def = HediffMaker.MakeHediff(DefOfs.C_HediffDefOf.ServitorOutOfFuel, this.pawn);
                this.pawn.health.AddHediff(def);
            }
            if (this.pawn.Downed && this.HasFuel)
            {
                this.pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(DefOfs.C_HediffDefOf.ServitorOutOfFuel));
            }
        }
        

    }
}
