using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace Corruption
{
    public class HediffComp_NeuroController : HediffComp, IThingContainerOwner
    {
        private const int maxWeaponSystems = 4;

        public ThingContainer innerContainer;

        public bool WeaponSystemsActive = false;
                
        public HediffComp_NeuroController()
        {
            this.innerContainer = new ThingContainer(this, false, LookMode.Deep);
        }

        public override void CompPostTick()
        {
            base.CompPostTick();
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

        public ThingContainer GetInnerContainer()
        {
            return this.innerContainer;
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
        
    }
}
