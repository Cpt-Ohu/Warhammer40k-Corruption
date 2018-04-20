using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class MechanicumTrader : TradeShip
    {
        public new void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            base.GiveSoldThingToTrader(toGive, countToGive, playerNegotiator);
        }
    }
}
