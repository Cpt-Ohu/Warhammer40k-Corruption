using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption.Worship
{
    public class JobDriver_Prayer : JobDriver_RelaxAlone
    {
        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil lastToil = new Toil();
            IEnumerator<Toil> enumerator = base.MakeNewToils().GetEnumerator();
            while (enumerator.MoveNext())
            {
                lastToil = enumerator.Current;
                yield return enumerator.Current;
            }

            lastToil.AddPreInitAction(new Action(delegate
            {
                this.ThrowMote(this.GetActor());
            }));

            lastToil.tickAction = new Action(delegate 
            {
                if (Find.TickManager.TicksGame % 120 == 0)
                this.ThrowMote(this.GetActor());

                });

            lastToil.AddFinishAction(new Action(delegate
            {
                Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(this.GetActor());
                if (soul != null)
                {
                    float num = 0.005f;
                    if (soul.NotCorrupted)
                    {
                        num *= -1f * Rand.Range(0.5f, 1);
                    }
                    soul.GainNeed(num);
                }
            }));
            //Toil LastToil = new Toil();
            //IEnumerator<Toil> enumerator = base.MakeNewToils().GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    Toil current = enumerator.Current;
            //    LastToil = current;

            //    yield return current;
            //}

            //LastToil.preTickActions.Add(new Action(delegate
            //{
            //    this.ThrowMote(this.pawn);
            //}));

            ////LastToil.tickAction += delegate
            ////{
            ////    if (this.soul != null)
            ////    {
            ////        float num = 0.0005f / 60;
            ////        if (soul.NoPatron)
            ////        {
            ////            num *= -1f * Rand.Range(0.01f, 0.1f);
            ////        }
            ////        soul.GainNeed(num);
            ////    }
            ////};
            //this.AddFinishAction(new Action(delegate
            //{
            //    Need_Soul soul = CorruptionStoryTrackerUtilities.GetPawnSoul(this.GetActor());
            //    if (soul != null)
            //    {
            //        float num = 0.0005f / 60;
            //        if (soul.NoPatron)
            //        {
            //            num *= -1f * Rand.Range(0.01f, 0.1f);
            //        }
            //        soul.GainNeed(num);
            //    }
            //}));

            yield break;
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }

        protected void ThrowMote(Pawn pawn)
        {
            //   Log.Message("M1");
            MoteBubble moteBubble2 = (MoteBubble)ThingMaker.MakeThing(ThingDefOf.Mote_Speech, null);
            moteBubble2.SetupMoteBubble(ChaosGodsUtilities.TryGetPreacherIcon(pawn), pawn);
            moteBubble2.Attach(pawn);
            GenSpawn.Spawn(moteBubble2, pawn.Position, pawn.Map);
        }
    }
}
