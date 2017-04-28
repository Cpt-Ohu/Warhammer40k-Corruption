using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace Corruption
{
    public class HediffComp_TemporaryRecruit : HediffComp_Disappears
    {
        public Faction PawnFactionOri;

        public override void CompPostMake()
        {
            base.CompPostMake();
            this.PawnFactionOri = this.Pawn.Faction;
            this.Pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);

            if (this.Pawn.Faction != Faction.OfPlayer)
            {
                InteractionWorker_RecruitAttempt.DoRecruit(this.Pawn.Map.mapPawns.FreeColonists.RandomElement<Pawn>(), this.Pawn, 1f, false);
            }
        }

        public override void CompPostTick()
        {
            base.CompPostTick();
            MoteMaker.MakeStaticMote(this.Pawn.Position, this.Pawn.Map, ThingDefOf.Mote_MicroSparks);
        }

        public override bool CompShouldRemove
        {
            get
            {
                if (base.CompShouldRemove)
                {
                    if (this.PawnFactionOri != Faction.OfPlayer)
                    {
                        this.Pawn.SetFactionDirect(PawnFactionOri);
                        Find.ColonistBar.MarkColonistsDirty();
                        this.Pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.PanicFlee);
                    }
                    return true;
                }
                return false;

            }
        }
    }
}
