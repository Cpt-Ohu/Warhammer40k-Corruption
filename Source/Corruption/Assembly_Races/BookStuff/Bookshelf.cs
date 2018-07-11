using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
namespace Corruption.BookStuff
{
    public class Bookshelf : Building
    {
        public Pawn pawn;
        public List<String> BookCategories;
        public List<ThingDef> StoredBooks = new List<ThingDef>();
        public Thing ChoosenBook = null;
        public List<ThingDef> MissingBooksList = new List<ThingDef>();
        public CompBookshelf compBookshelf
        {
            get
            {
                return this.TryGetComp<CompBookshelf>();
            }
        }

        public Graphic BooksGraphic
        {
            get
            {
                if (this.compBookshelf != null && compBookshelf.Props.StoredBookGraphicPath != null)
                {
                    return GraphicDatabase.Get<Graphic_Multi>(compBookshelf.Props.StoredBookGraphicPath, ShaderDatabase.Cutout, Vector2.one, Color.white);
                }
                return null;

            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<ThingDef>(ref StoredBooks, "StoredBooks", LookMode.Def, null);
            Scribe_Collections.Look<ThingDef>(ref MissingBooksList, "MissingBooksList", LookMode.Def, null);
        }

        public override void PostMake()
        {
            base.PostMake();

            foreach (ThingDef bookDef in this.compBookshelf.Props.BooksList)
            {
                this.StoredBooks.Add(bookDef);
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public Thing JobBook(Pawn reader)
        {
            if (StoredBooks.Count <= 0)
            {
                reader.jobs.StopAll();
            }
            else
            {
                ThingDef thingDef = StoredBooks.RandomElement<ThingDef>();
                StoredBooks.Remove(thingDef);
                MissingBooksList.Add(thingDef);
                ChoosenBook = ThingMaker.MakeThing(thingDef);
            }
            return ChoosenBook;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            IEnumerable<FloatMenuOption> result;
            if (!myPawn.CanReserve(this))
            {
                FloatMenuOption item = new FloatMenuOption("CannotUseReserved".Translate(), null);
                result = new List<FloatMenuOption>
                {
                    item
                };
            }
            else
            {
                if (!myPawn.CanReach(this, PathEndMode.Touch, Danger.Some))
                {
                    FloatMenuOption item2 = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    result = new List<FloatMenuOption>
                    {
                        item2
                    };
                }
                else
                {
                    List<Thing> allspawnedbooks = this.Map.listerThings.AllThings.FindAll(x => x.GetType() == typeof(ReadableBooks));
                    for (int i = 0; i < allspawnedbooks.Count; i++)
                    {
                        Thing current = allspawnedbooks[i];

                        Action storebook = delegate
                        {
                            Job newjob = new Job(DefDatabase<JobDef>.GetNamed("AddBookToLibrary"), current, this);
                            myPawn.jobs.jobQueue.EnqueueFirst(newjob);
                            //myPawn.jobs.StopAll();
                            myPawn.Reserve(this, newjob);
                        };
                        list.Add(new FloatMenuOption("AddBookToLibrary".Translate(new object[] { current.LabelCap}), storebook));

                    }
                    if (StoredBooks.Count <= 0)
                    {
                        FloatMenuOption item3 = new FloatMenuOption("NoStoryBooks".Translate(), null);
                        list.Add(item3);
                    }
                    else
                    {
                        Action action = delegate
                        {
                            Job newJob = new Job(DefDatabase<JobDef>.GetNamed("SitAndRead"), this);
                            myPawn.jobs.jobQueue.EnqueueFirst(newJob);
                            //myPawn.jobs.StopAll();
                            pawn = myPawn;
                            myPawn.Reserve(this, newJob);
                        };
                        list.Add(new FloatMenuOption("ReadABook".Translate(), action));        
                    }
                }
            }

            result = list;
            return result;
        }
        
        public override void DeSpawn()
        {
            if (MissingBooksList.Count > 0)
            {
                for (int i = 0; i < MissingBooksList.Count; i++)
                {
                    if (StoredBooks.Count < 3)
                    {
                        StoredBooks.Add(MissingBooksList.ElementAt(i));
                    }
                }
            }
            base.DeSpawn();
        }

        public void AddBookToLibrary(ThingDef_Readables rdef)
        {
                this.StoredBooks.Add(rdef);
        }

        public static Toil PlaceBookInShelf(TargetIndex book, TargetIndex shelf, Pawn pawn)
        {

            ReadableBooks bookint = (ReadableBooks)pawn.jobs.curJob.GetTarget(book).Thing;
            Bookshelf bookshelf = (Bookshelf)pawn.jobs.curJob.GetTarget(shelf).Thing;
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.AddPreInitAction(delegate
            {
                bookint.Destroy(DestroyMode.Vanish);
                bookshelf.AddBookToLibrary(bookint.Tdef);
            });
            return toil;

        }

        public override void Draw()
        {
            if (this.StoredBooks.Count>0 && this.BooksGraphic != null)
            {
                Vector3 drawvec = this.DrawPos;
                drawvec.y += 0.01f;
                Mesh mesh = this.Graphic.MeshAt(this.Rotation);
                Quaternion rotation = this.Rotation.AsQuat;
                Material material = BooksGraphic.MatAt(this.Rotation);
                Graphics.DrawMesh(mesh, drawvec, rotation, material, 1);
            }
            base.Draw();
        }
        
    }
}
