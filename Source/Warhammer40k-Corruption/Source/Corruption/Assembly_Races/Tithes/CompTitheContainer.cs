using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace Corruption.Tithes
{
    [StaticConstructorOnStartup]
    public class CompTitheContainer : ThingComp
    {

        public bool activeLoading;

        private static List<CompTitheContainer> tmpContainersInGroup = new List<CompTitheContainer>();

        private static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

        private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter", true);

        private List<TransferableOneWay> leftToTransfer;
        
        public float massUsage
        {
            get
            {
                float totalFreightMass = 0f;
                for (int i =0; i < this.container.GetDirectlyHeldThings().Count;i++)
                {
                    totalFreightMass += this.container.GetDirectlyHeldThings()[i].GetInnerIfMinified().GetStatValue(StatDefOf.Mass);
                }
                return totalFreightMass / tProps.maxContainerCapacity;
            }
        }
        
        public CompProperties_TitheContainer tProps
        {
            get
            {
                return (CompProperties_TitheContainer)this.props;
            }
        }

        public CompTitheContainer()
        {
            this.leftToTransfer = new List<TransferableOneWay>();
        }

        public TitheContainer container
        {
            get
            {
                return (TitheContainer)this.parent;
            }
           
        }

        public Map Map
        {
            get
            {
                return this.parent.MapHeld;
            }
        }

        public bool Spawned
        {
            get
            {
                return this.parent.Spawned;
            }
        }    
    }
}
