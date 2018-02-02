using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
namespace Corruption.BookStuff
{
    public class JobDriver_ReadBookLong : JobDriver
    {
        private const TargetIndex BookshelfInd = TargetIndex.A;
        private const TargetIndex BookInd = TargetIndex.B;
        private List<string> Story = new List<string>();
        private Need_Soul soul;
        private ReadableBooks tempbook;
        private Bookshelf tempshelf;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (pawn.needs.TryGetNeed<Need_Soul>() != null)
            {
                this.soul = pawn.needs.TryGetNeed<Need_Soul>();
            }
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            yield return TakeBookFromBookshelf(TargetIndex.A, pawn, TargetIndex.B);
            yield return CarryBookToSeat(pawn);
            yield return PlaceItemToRead(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            yield return ReadingBook(pawn, 1500, TargetIndex.A, TargetIndex.B);
            //         yield return ReadEnd(pawn, TargetIndex.B, TargetIndex.A);Toil toil2 = null;
            //yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            //yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            //yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A);

            this.AddFinishAction(delegate
            {
                Job newjob = new Job(DefDatabase<JobDef>.GetNamed("AddBookToLibrary"), TargetB, TargetA);
                pawn.jobs.jobQueue.EnqueueLast(newjob);
            });
         //   yield return Bookshelf.PlaceBookInShelf(tempbook, this.TargetA.Thing as Bookshelf);
            yield break;
        }
        public Toil ReadEnd(Pawn reader, TargetIndex bookInd, TargetIndex BookShelfInd)
        {
            return new Toil
            {
                initAction = delegate
                {
                    Bookshelf bookshelf = (Bookshelf)reader.jobs.curJob.GetTarget(BookShelfInd).Thing;
                    ReadableBooks readableBooks = (ReadableBooks)reader.jobs.curJob.GetTarget(bookInd).Thing;
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
        public Toil ReadingBook(Pawn reader, int durationMultiplier, TargetIndex Ind, TargetIndex bookInd)
        {
            Toil toil = new Toil();
            List<string> text = new List<string>();
            Bookshelf thing = null;
            ReadableBooks thingBook = null;
            int s = 0;
            int i = 0;
            int prog = 0;
            int oldprog = 0;
            float showTextFactor = 0;
            bool tickOnce = false;
            toil.tickAction = delegate
            {
                if (!tickOnce)
                {
                    thing = (Bookshelf)reader.jobs.curJob.GetTarget(Ind).Thing;
                    thingBook = (ReadableBooks)reader.jobs.curJob.GetTarget(bookInd).Thing;
                    this.tempbook = thingBook;
                    if (thingBook.PrepareText().Count > 0)
                    {
                        text = thingBook.PrepareText();
                    }
                    showTextFactor = thingBook.Tdef.TextSpeedShowFactor;
                    if (soul != null)
                    {
                        foreach (KeyValuePair<ThingDef, int> current in soul.readablesManager.ReadableProgressEntry)
                        {
                            if(current.Key.defName == thingBook.def.defName)
                            {
                                prog = current.Value;
                                oldprog = prog;
                                s = current.Value / 150;
                                break;
                            }
                        }
                    }
                    thingBook.TexChange = true;
                    tickOnce = true;
                }
                if (thingBook.Tdef.SkillToLearn != null)
                {
                    this.pawn.skills.Learn(thingBook.Tdef.SkillToLearn, thingBook.Tdef.SkillGainFactor * 1f);
                }
                prog += 1;
                if (text.Count > 0)
                {
                    if (i > showTextFactor * 150)
                    {
                        if (text.Count > s)
                        {
                            MoteMaker.ThrowText(reader.TrueCenter() + new Vector3(0f, 0f, 0.7f), thingBook.Map, text.ElementAt(s), Color.green);
                            reader.needs.joy.CurLevel += 0.03f;
                            s++;
                            thingBook.ReadCorruptionTick(this.pawn, thingBook);
                            i = 0;
                        }
                    }
                    i++;
                }
                
                reader.rotationTracker.FaceCell(thingBook.Position);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDespawnedOrNull(TargetIndex.B);
            toil.defaultDuration = durationMultiplier;
            toil.AddFinishAction(delegate
            {
                if (soul != null)
                {
                    Dictionary<ThingDef, int> entries = soul.readablesManager.ReadableProgressEntry;
                    if (entries.ContainsKey(thingBook.def))
                    {
                        entries[thingBook.def] = prog;
                    }
                    else
                    {
                        entries.Add(thingBook.def, prog);
                    }
                }
                thingBook.PostReadEffectSelection(prog, oldprog);
                thingBook.TexChange = false;
                this.tempshelf = thing;
                this.tempbook = thingBook;
            });

            return toil;
        }
        public Toil CarryBookToSeat(Pawn pawn)
        {
            Toil carryBook = new Toil();
            IntVec3 position;
            carryBook.initAction = delegate
            {
                Predicate<Thing> validator = delegate(Thing t)
                {
                    bool result;
                    if (t.def.building == null || !t.def.building.isSittable)
                    {
                        result = false;
                    }
                    else
                    {
                        if (t.IsForbidden(pawn))
                        {
                            result = false;
                        }
                        else
                        {
                            if (!carryBook.actor.CanReserve(t))
                            {
                                result = false;
                            }
                            else
                            {
                                if (!t.IsSociallyProper(carryBook.actor))
                                {
                                    result = false;
                                }
                                else
                                {
                                    if (t.IsBurning())
                                    {
                                        result = false;
                                    }
                                    else
                                    {
                                        bool flag = false;
                                        for (int i = 0; i < 4; i++)
                                        {
                                            Building edifice = (t.Position + GenAdj.CardinalDirections[i]).GetEdifice(this.pawn.Map);
                                            if (edifice != null && (edifice.def.surfaceType == SurfaceType.Eat || edifice.def.surfaceType == SurfaceType.Item))
                                            {
                                                flag = true;
                                                break;
                                            }
                                        }
                                        result = flag;
                                    }
                                }
                            }
                        }
                    }
                    return result;
                };
                Thing thing = GenClosest.ClosestThingReachable(carryBook.actor.Position, this.pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(carryBook.actor), 25f, validator, null, 1);
                if (thing != null)
                {
                    position = thing.Position;
                    this.pawn.Map.reservationManager.Reserve(carryBook.actor, this.job, thing);
                }
                else
                {
                    position = RCellFinder.SpotToChewStandingNear(carryBook.actor, carryBook.actor.CurJob.targetA.Thing);
                }
                this.pawn.Map.pawnDestinationReservationManager.Reserve(carryBook.actor, this.job, position);
                carryBook.actor.pather.StartPath(position, PathEndMode.OnCell);
            };
            carryBook.defaultCompleteMode = ToilCompleteMode.PatherArrival;

            return carryBook;
        }
        public Toil PlaceItemToRead(TargetIndex bookInd)
        {
            Toil placeItem = new Toil();
            placeItem.initAction = delegate
            {
                Pawn actor = placeItem.actor;
                Thing carriedThing = actor.carryTracker.CarriedThing;
                ThingDef_Readables Readables_Def = (ThingDef_Readables)carriedThing.def;
                if (!Readables_Def.IsABook)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                }
                else
                {
                    if (carriedThing.Destroyed)
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    }
                    else
                    {
                        IntVec3 position;
                        if (!TryFindAdjacentReadPlaceSpot(actor.Position, carriedThing.def, out position))
                        {
                            Log.Error(string.Concat(new object[]
							{
								actor,
								" could not read: found no place spot near ",
								actor.Position,
								". Correcting."
							}));
                            position = actor.Position;
                        }
                        if (!position.InBounds(this.pawn.Map))
                        {
                            Log.Error(string.Concat(new object[]
							{
								actor,
								" tried to place book out of bounds at ",
								position,
								". Correcting."
							}));
                            position = actor.Position;
                        }
                        if (!actor.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out carriedThing))
                        {
                            Log.Error(string.Concat(new object[]
							{
								actor,
								" could not read: book vanished when placed at ",
								position,
								"."
							}));
                            actor.jobs.EndCurrentJob(JobCondition.Errored);
                        }
                        else
                        {
                            actor.jobs.curJob.SetTarget(bookInd, carriedThing);
                            IntVec3 intVec = position - actor.Position;
                            if (carriedThing.def.rotatable && intVec != IntVec3.Zero)
                            {
                                carriedThing.Rotation = Rot4.FromIntVec3(intVec);
                            }
                        }
                    }
                }


            };
            placeItem.defaultCompleteMode = ToilCompleteMode.Instant;

            return placeItem;
        }
        public Toil TakeBookFromBookshelf(TargetIndex ind, Pawn reader, TargetIndex bookInd)
        {
            Toil takeBook = new Toil();
            takeBook.initAction = delegate
            {
                bool flag = true;
                Pawn actor = takeBook.actor;
                Bookshelf bookshelf = (Bookshelf)actor.jobs.curJob.GetTarget(ind).Thing;
                Thing thing = bookshelf.JobBook(reader);
                if (thing == null)
                {
                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                }
                else
                {
                    ReadableBooks readableBooks = thing as ReadableBooks;
                    readableBooks.currentReader = reader;
                    actor.carryTracker.TryStartCarry(readableBooks);
                    actor.jobs.curJob.targetB = actor.carryTracker.CarriedThing;
                }
                if (flag)
                {
                    if (this.pawn.Map.reservationManager.FirstRespectedReserver(bookshelf, pawn) == reader)
                    {
                        this.pawn.Map.reservationManager.Release(bookshelf, reader, this.job);
                    }
                }
            };
            takeBook.defaultCompleteMode = ToilCompleteMode.Delay;
            takeBook.defaultDuration = 20;
            return takeBook;
        }
        public bool TryFindAdjacentReadPlaceSpot(IntVec3 root, ThingDef bookDef, out IntVec3 placeSpot)
        {
            placeSpot = IntVec3.Invalid;
            bool result;
            for (int i = 0; i < 4; i++)
            {
                IntVec3 intVec = root + GenAdj.CardinalDirections[i];
                bool arg_75_0;
                if (intVec.HasEatSurface(this.pawn.Map))
                {
                    arg_75_0 = (
                        from t in this.pawn.Map.thingGrid.ThingsAt(intVec)
                        where t.def == bookDef
                        select t).Any<Thing>();
                }
                else
                {
                    arg_75_0 = true;
                }
                if (!arg_75_0)
                {
                    placeSpot = intVec;
                    result = true;
                    return result;
                }
            }
            if (!placeSpot.IsValid)
            {
                List<IntVec3> list = GenAdj.CardinalDirections.InRandomOrder().Concat(GenAdj.DiagonalDirections.InRandomOrder()).ToList<IntVec3>();
                list.Add(IntVec3.Zero);
                for (int j = 0; j < list.Count; j++)
                {
                    IntVec3 intVec2 = root + list[j];
                    bool arg_125_0;
                    if (intVec2.Walkable(this.pawn.Map))
                    {
                        arg_125_0 = (
                        from t in this.pawn.Map.thingGrid.ThingsAt(intVec2)
                        where t.def == bookDef
                            select t).Any<Thing>();
                    }
                    else
                    {
                        arg_125_0 = true;
                    }
                    if (!arg_125_0)
                    {
                        placeSpot = intVec2;
                        result = true;
                        return result;
                    }
                }
            }
            result = false;
            return result;
        }

        public override bool TryMakePreToilReservations()
        {
            return true;
        }
    }
}
