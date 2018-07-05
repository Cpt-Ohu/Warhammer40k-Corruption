using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Corruption.Worship.Wonders
{
   public class WonderWorker
    {
        public WonderDef Def;
        
        public virtual bool TryExecuteWonder(int worshipPoints) { return false; }

    }
}
