using Corruption.DefOfs;
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

        public override void PostDraw()
        {
            //base.PostDraw();
            if (!this.HasFuel && this.Props.drawOutOfFuelOverlay)
            {
                this.parent.Map.overlayDrawer.DrawOverlay(this.parent, OverlayTypes.NeedsPower);
            }
        }

        public override string CompInspectStringExtra()
        {
            string text = string.Concat(new string[]
            {
                "Fuel".Translate(),
                ": ",
                this.Fuel.ToStringDecimalIfSmall(),
                " / ",
                this.Props.fuelCapacity.ToStringDecimalIfSmall()
            });
            if (!this.Props.consumeFuelOnlyWhenUsed && this.HasFuel)
            {
                int numTicks = (int)(this.Fuel / this.Props.fuelConsumptionRate * 60000f);
                text = text + " (" + numTicks.ToStringTicksToPeriod(true, false, true) + ")";
            }

            return text;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.Props.fuelCapacity = 3;
            this.Props.fuelConsumptionRate = 0.1f;
            this.Props.fuelFilter = new ThingFilter();

            this.Props.fuelFilter.SetAllow(DefOfs.C_ThingDefOfs.IoM_ServitorFuel, true);
            this.Props.targetFuelLevelConfigurable = true;
            this.Props.showFuelGizmo = true;
            this.Props.initialConfigurableTargetFuelLevel = 2;
            
            //FieldInfo info = typeof(ThingFilter).GetField("thingDefs", BindingFlags.NonPublic | BindingFlags.Instance);
            //if (info != null)
            //{
            //    ThingFilter filter = new ThingFilter();
            //    List<ThingDef> defs;
            //    try
            //    {
            //        Log.Message("Trying");
            //        defs = Traverse.Create(filter).Field("thingDefs").GetValue() as List<ThingDef>;


            //      //  defs = info.GetValue(filter) as List<ThingDef>;
            //        if (defs == null) Log.Message("Nothing");
            //        defs.Add(DefOfs.C_ThingDefOfs.IoM_ServitorFuel);
            //        this.Props.fuelFilter = filter;
            //    }
            //    catch (TargetInvocationException ex)
            //    {
            //        Log.Message("Failing");
            //        Log.Message(ex.InnerException.ToString());
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Message(ex.InnerException.ToString());
            //    }
            //}

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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            return base.CompGetGizmosExtra();
        }
        


        public void RemoveAutomatonNeed(NeedDef nd)
        {
            try
            {
                if (this.pawn != null)
                {
                    if (this.pawn.needs != null)
                    {
                        Need item = this.pawn.needs.TryGetNeed(nd);
                        if (item != null)
                        {
                            this.pawn.needs.AllNeeds.Remove(item);
                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}
