using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class HediffComp_DemonicPossession : HediffComp
    {
        private Pawn Demon;

        public override void CompPostMake()
        {
            base.CompPostMake();

            this.Demon = DemonUtilities.GenerateDemon();
        }


        private CompSoul soul
        {
            get
            {
                CompSoul soulInt;
                if ((soulInt = CompSoul.GetPawnSoul(this.Pawn)) != null)
                    return soulInt;
                else
                {
                    throw new Exception("Demonic Possession working on pawn without soul!");
                }
            }
        }

        public override  void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (this.Pawn.def.race.Humanlike)
            {
                soul.AffectSoul(-0.00005f);
            }
        }

        public override void Notify_PawnDied()
        {
            string label = "LetterDemonicPossessionResolve".Translate();
            string text2 = "LetterDemonicPossessionResolve_Content".Translate(new object[]
            {
                    this.Pawn.LabelShort,
            });
            Find.LetterStack.ReceiveLetter(label, text2, LetterDefOf.ThreatBig, this.Pawn, null);

            GenSpawn.Spawn(Demon, Pawn.Position, this.Pawn.Map);
            Demon.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);

            if (this.Pawn.Corpse.Spawned)
            {
                this.Pawn.Corpse.Destroy(DestroyMode.Vanish);
            }

        }        
    }
}
