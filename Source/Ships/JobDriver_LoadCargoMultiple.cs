using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace OHUShips
{
    public class JobDriver_LoadCargoMultiple : JobDriver_HaulToContainer
    {
        private ShipBase ship
        {
            get
            {
               return (ShipBase)TargetB;
            }
        }

        private TransferableOneWay transferable
        {
            get
            {
                return TransferableUtility.TransferableMatchingDesperate(this.TargetA.Thing, ship.compShip.leftToLoad);
            }
        }


        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            Toil toil = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            toil.AddFinishAction(delegate { Log.Message("Finished A"); });
            toil.tickAction += delegate
            {
                if (this.ShipFull(ship))
                {
                    Log.Message("ShipFUll");
                    this.EndJobWith(JobCondition.Incompletable);
                }
            };
            yield return toil;
            yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            Toil toilPickup = Toils_Haul.StartCarryThing(TargetIndex.A, false, true);//.FailOn(() => this.ShipFull(ship));
            toilPickup.AddFinishAction(delegate { Log.Message("Finished B"); });
            yield return toilPickup;
            //yield return Toils_Haul.JumpIfAlsoCollectingNextTargetInQueue(toil, TargetIndex.A);
            Toil toil2 = Toils_Haul.CarryHauledThingToContainer();
            toil2.AddFinishAction(delegate { Log.Message("Finished C"); });
            //Toil toil2 = Toils_Goto.GotoCell(TargetB.Cell, PathEndMode.ClosestTouch);
            toil2.tickAction += delegate
            {
                if (this.ShipFull(ship, false))
                {
                    this.EndJobWith(JobCondition.Incompletable);
                }
            };
            yield return toil2;
            Toil toil3 = Toils_Haul.DepositHauledThingInContainer(TargetIndex.C, TargetIndex.None);
            yield return toil3;
            yield break;
        }
        
        public override bool TryMakePreToilReservations()
        {
            //this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.A), this.job, 1, -1, null);
            //this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 10, 1, null);
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null);// && this.pawn.Reserve(this.job.GetTarget(TargetIndex.B), this.job, 20, 1, null);
        }


        private Action RestoreRemainingThings(Thing t, int amount)
        {
            return delegate
            {
                pawn.jobs.TryTakeOrderedJob(HaulAIUtility.HaulToStorageJob(pawn, t));
            };
        }

        private bool ShipFull(ShipBase ship, bool firstCheck = true)
        {
            //Log.Message("Checking");
            CompShip compShip = ship.compShip;
                if (transferable != null)
                {
                    if (firstCheck && this.job.count > transferable.CountToTransfer)
                    {
                        return true;
                    }
                    if (!firstCheck && this.TargetA.Thing.stackCount > transferable.CountToTransfer)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            
            
        }

    }
}
