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
                CompSoul soul = CompSoul.GetPawnSoul(this.GetActor());
                if (soul != null)
                {
                    float num = 0.005f;
                    if (soul.Corrupted)
                    {
                        num *= -1f * Rand.Range(0.5f, 1);
                    }
                    soul.AffectSoul(num);
                }
            }));

            yield break;
        }
        public override void ExposeData()
        {
            base.ExposeData();
        }

        protected void ThrowMote(Pawn pawn)
        {
            MoteBubble moteBubble2 = (MoteBubble)ThingMaker.MakeThing(ThingDefOf.Mote_Speech, null);
            moteBubble2.SetupMoteBubble(ChaosGodsUtilities.TryGetPreacherIcon(pawn), pawn);
            moteBubble2.Attach(pawn);
            GenSpawn.Spawn(moteBubble2, pawn.Position, pawn.Map);
        }
    }
}
