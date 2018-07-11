using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Corruption
{
    public class MentalState_KhorneKillWeak : MentalState_KillSinglePawn
    {
        protected override Pawn ChooseVictim()
        {
            List<Pawn> prisoners = this.pawn.Map.mapPawns.PrisonersOfColonySpawned;
            return prisoners.RandomElement();
        }

        public override bool ForceHostileTo(Thing t)
        {
            Pawn prisoner = t as Pawn;

            if (prisoner != null && prisoner.IsPrisoner)
            {
                return true;
            }
            return false;
        }

    }
}
