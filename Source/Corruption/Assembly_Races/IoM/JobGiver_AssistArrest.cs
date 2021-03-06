﻿using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption.IoM
{
    public class JobGiver_AssistArrest : ThinkNode_JobGiver
    {
        protected virtual int FollowJobExpireInterval
        {
            get
            {
                return 200;
            }
        }

        private Pawn Governor
        {
            get
            {
                Pawn pawn = CFind.StoryTracker.PlanetaryGovernor;
                if (pawn != null)
                {
                    return pawn;
                }
                else
                {
                    return null;
                }
            }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (this.Governor != null && pawn.Map.reservationManager.IsReservedByAnyoneOf(this.Governor, pawn.Faction) && pawn.Faction != this.Governor.Faction)
            {
                Pawn followee = pawn.Map.reservationManager.FirstRespectedReserver(this.Governor, pawn);
                if (followee == null)
                {
                    Log.Error(base.GetType() + "has null followee.");
                    return null;
                }
                if (followee.CurJob.def == C_JobDefOf.ArrestGovernor)
                {
                    float radius = 5f;
                    if ((followee.pather.Moving && followee.pather.Destination.Cell.DistanceToSquared(pawn.Position) > radius * radius) || followee.GetRoom() != pawn.GetRoom() || followee.Position.DistanceToSquared(pawn.Position) > radius * radius)
                    {
                        IntVec3 root;
                        if (followee.pather.Moving && followee.pather.curPath != null)
                        {
                            root = followee.pather.curPath.FinalWalkableNonDoorCell(followee.Map);
                        }
                        else
                        {
                            root = followee.Position;
                        }
                        IntVec3 c = CellFinder.RandomClosewalkCellNear(root, followee.Map, Mathf.RoundToInt(radius * 0.7f));
                        Job job = new Job(JobDefOf.Goto, c);
                        job.expiryInterval = this.FollowJobExpireInterval;
                        job.checkOverrideOnExpire = true;
                        if (pawn.mindState.duty != null && pawn.mindState.duty.locomotion != LocomotionUrgency.None)
                        {
                            job.locomotionUrgency = LocomotionUrgency.Jog;
                        }
                        return job;
                    }
                }
                return null;
            }
            return null;
        }
    }
}
