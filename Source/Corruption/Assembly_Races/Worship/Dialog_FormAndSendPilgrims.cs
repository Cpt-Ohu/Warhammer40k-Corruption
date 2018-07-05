using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption.Worship
{
    public class Dialog_FormAndSendPilgrims : Dialog_FormCaravan
    {

        public Dialog_FormAndSendPilgrims(Map map) : base(map)
        {

        }
        public override void PostClose()
        {
            base.PostClose();
            foreach (Pawn p in TransferableUtility.GetPawnsFromTransferables(this.transferables))
            {
                Log.Message("Setting Pilgrimage" + p.NameStringShort);
                Need_Soul soul = p.needs.TryGetNeed<Need_Soul>();

                if (soul != null)
                {
                    Log.Message("Setting");
                    soul.IsOnPilgrimage = true;
                }
            }

        }
    }        
    
}
