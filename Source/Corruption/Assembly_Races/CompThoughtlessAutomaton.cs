using Corruption.BookStuff;
using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompThoughtlessAutomaton : ThingComp
    {
        public bool IsAutomaton;


        public Pawn pawn
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();           
        }

        public override void CompTick()
        {
            RemoveAutomatonNeed(C_NeedDefOf.Beauty);
            RemoveAutomatonNeed(C_NeedDefOf.Comfort);
            RemoveAutomatonNeed(C_NeedDefOf.Space);
            if (this.pawn.needs.joy != null)
            {
                this.pawn.needs.joy.CurLevel = 0.9f;
            }
        }

        private void RemoveAutomatonNeed(NeedDef nd)
        {
            Need item = pawn.needs.TryGetNeed(nd);
            this.pawn.needs.AllNeeds.Remove(item);
        }
    }
}
