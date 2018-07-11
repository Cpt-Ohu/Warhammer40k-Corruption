using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Corruption
{
    public class CompTeleporter : ThingComp
    {
        public Apparel apparel

        {
            get
            {
                return this.parent as Apparel;
            }
        }

        public Pawn pawn
        {
            get
            {
                if (this.apparel != null)
                {
                        return this.apparel.Wearer;                 
                }

                return null;
            }
        }

        private TargetingParameters GetTargetingParameters()
        {
            return new TargetingParameters()
            {
                canTargetPawns = false,
                canTargetBuildings = false,
                canTargetItems = false,
                canTargetLocations = true
            };
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }

            Command_Action command = new Command_Action();
            command.action = delegate {
                Find.Targeter.BeginTargeting(GetTargetingParameters(), delegate (LocalTargetInfo t)
                {
                    IntVec3 cell = t.Cell;
                    this.Teleport(cell);
                });
            };
            

        }

        private void Teleport(IntVec3 location)
        {

            this.pawn.Position = location;
        }
    }
}
