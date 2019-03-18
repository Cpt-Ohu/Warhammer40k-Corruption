using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Corruption
{
    public class HediffComp_NeuroController : HediffComp, IThingHolder
    {
        private const int maxWeaponSystems = 4;

        public ThingOwner innerContainer;

        public bool WeaponSystemsActive = false;
                
        public HediffComp_NeuroController()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public override  void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            for (int i = 0; i < innerContainer.Count; i++)
            {
                ThingWithComps thingWithComps = this.innerContainer[i] as ThingWithComps;
                if (thingWithComps != null)
                {
                    thingWithComps.GetComp<CompEquippable>().verbTracker.VerbsTick();
                }
            }
        }

        public bool Spawned
        {
            get
            {
                return this.Pawn.Spawned;
            }
        }

        public IThingHolder ParentHolder
        {
            get
            {
                return this.Pawn;
            }
        }

        public IntVec3 GetPosition()
        {
            return this.Pawn.PositionHeld;
        }

        public Map GetMap()
        {
            return this.Pawn.MapHeld;
        }

        public void InstallWeapon(Thing weapon)
        {
            if (this.innerContainer.Count > HediffComp_NeuroController.maxWeaponSystems)
            {
                this.innerContainer.TryAdd(weapon);
            }
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return this.innerContainer;
        }
    }
}
