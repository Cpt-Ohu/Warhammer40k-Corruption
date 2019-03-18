using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class CompBuffGiverEQ : ThingComp
    {
        private Pawn cachedOwner;

        public Pawn Owner
        {
            get
            {
                if (this.ParentHolder != null)
                {
                    if (this.cachedOwner == null)
                    {
                        this.CheckForOwner();
                    }
                    return this.cachedOwner;
                }
                else
                {
                    return null;
                }
            }
        }

        public CompProperties_BuffGiver cProps
        {
            get
            {
                return this.props as CompProperties_BuffGiver;
            }                
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
        }


        private void CheckForOwner()
        {
            CompEquippable tempcomp;
            Apparel tempthing;
            if (this.parent != null && !this.parent.Spawned)
            {
                if (this.parent is Apparel)
                {
                    tempthing = this.parent as Apparel;
                    this.cachedOwner = tempthing.Wearer;
                }
                else if ((tempcomp = this.parent.TryGetComp<CompEquippable>()) != null && tempcomp.PrimaryVerb.CasterPawn != null)
                {
                    this.cachedOwner = tempcomp.PrimaryVerb.CasterPawn;
                }
                else if (this.parent.holdingOwner != null && this.parent.holdingOwner.Owner is Pawn_CarryTracker)
                {
                    Pawn_CarryTracker tracker = this.parent.holdingOwner.Owner as Pawn_CarryTracker;
                    this.cachedOwner = tracker.pawn;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }            

            foreach (EquipmentAbilityStruct current in this.cProps.Abilities)
            {
                yield return this.PowerGizmo(current);
            }
        }

        private Gizmo PowerGizmo(EquipmentAbilityStruct data)
        {
            Command_Action command = new Command_Action();
            command.icon = ContentFinder<Texture2D>.Get(data.IconPath);
            command.defaultLabel = data.Label;
            command.defaultDesc = data.Description;
            command.action = new Action(delegate
            {
                if (data.TargetSelf)
                {
                    this.CastOnSelf(data);
                }
                else
                {
                    this.CastOnNearbyPawns(data);
                }
            });
            return command;
        }

        private void CastOnSelf(EquipmentAbilityStruct data)
        {
            this.Owner.health.AddHediff(data.BuffDef);
        }

        private void CastOnNearbyPawns(EquipmentAbilityStruct data)
        {
            float range = GenRadial.NumCellsInRadius(data.Range);
            Room room = this.Owner.GetRoom(RegionType.Set_Passable);
            for (int i = 0; i < range; i++)
            {
                IntVec3 c = this.Owner.Position + GenRadial.RadialPattern[i];
                if (c.InBounds(this.Owner.Map))
                {
                    List<Thing> thingList = c.GetThingList(this.Owner.Map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        Pawn pawn = thingList[j] as Pawn;
                        if (pawn != null && pawn.RaceProps.intelligence >= Intelligence.Humanlike)
                        {
                            Room room2 = pawn.GetRoom(RegionType.Set_Passable);
                            if (room2 == null || room2.CellCount == 1 || (room2 == room && GenSight.LineOfSight(this.Owner.Position, pawn.Position, this.Owner.Map, true, null, 0, 0)))
                            {
                                pawn.health.AddHediff(data.BuffDef);
                            }
                        }
                    }
                }
            }
        }

    }
}
