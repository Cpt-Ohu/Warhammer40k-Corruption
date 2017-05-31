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
        public Toil LastToil;

        Need_Soul soul
        {
            get
            {
                Pawn pawn = this.GetActor();
                if (pawn != null && pawn.needs != null)
                {
                    return pawn.needs.TryGetNeed<Need_Soul>();
                }
                return null;
            }
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            IEnumerator<Toil> enumerator = base.MakeNewToils().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Toil current = enumerator.Current;
                LastToil = current;

                yield return current;
            }

            LastToil.preTickActions.Add(new Action(delegate
            {
                this.ThrowMote(this.pawn);
            }));

            LastToil.tickAction += delegate
            {
                if (this.soul != null)
                {
                    float num = 0.0005f / 60;
                    if (soul.NoPatron)
                    {
                        num *= -1f * Rand.Range(0.01f, 0.1f);
                    }
                    soul.GainNeed(num);
                }
            };

            yield break;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Toil>(ref this.LastToil, "LastToil");
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
