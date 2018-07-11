using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.IoM
{
    public class DialogTrade_Imperial : Dialog_Trade
    {
        private ImperialTraderOfficial imperialTrader;


        public DialogTrade_Imperial(Pawn playerNegotiator, ITrader trader) : base(playerNegotiator, trader)
        {
            this.imperialTrader = (ImperialTraderOfficial)trader;
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            this.imperialTrader.SpawnCargoFreigher();            
        }
    }
}
