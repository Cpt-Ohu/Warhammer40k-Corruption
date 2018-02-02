using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompTeleporter : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
        }
    }
}
