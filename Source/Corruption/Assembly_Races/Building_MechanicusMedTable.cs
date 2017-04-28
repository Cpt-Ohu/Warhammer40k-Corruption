using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Corruption
{
    public class Building_MechanicusMedTable : Building_Casket, IBillGiver
    {
        
        private CompPowerTrader powerComp;

        private CompBreakdownable breakdownableComp;

        public Building_MechanicusMedTable()
        {
            this.medOpStack = new BillStack(this);
        }

        public override void SpawnSetup(Map map)
        {
            base.SpawnSetup(map);
            this.powerComp = base.GetComp<CompPowerTrader>();
            this.breakdownableComp = base.GetComp<CompBreakdownable>();
        }

        public BillStack medOpStack;

        public Pawn patient
        {
            get
            {
                return this.ContainedThing as Pawn;
            }
            set
            {
                this.innerContainer[0] = value;
            }
        }
        
        public Corpse corpse
        {
            get
            {
                return this.ContainedThing as Corpse;
            }
        }

        private Material toolMat
        {
            get
            {
                return GraphicDatabase.Get<Graphic_Single>("Things/Mechanicus/MechanicusTables/MechanicusTable_Medical_Tools", ShaderDatabase.Cutout, this.Graphic.data.drawSize, Color.white).MatAt(this.Rotation);
            }
        }

        public BillStack BillStack
        {
            get
            {
                return this.medOpStack;
            }
        }

        public IEnumerable<IntVec3> IngredientStackCells
        {
            get
            {
                return GenAdj.CellsOccupiedBy(this);
            }
        }

        public static List<FloatMenuOption> GetRecipeFloatsFor(Building_MechanicusMedTable medTable)
        {
            Pawn patient = medTable.patient;
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (RecipeDef current in patient.def.AllRecipes)
            {
                if (current.AvailableNow)
                {
                    IEnumerable<ThingDef> enumerable = current.PotentiallyMissingIngredients(null, medTable.patient.Map);
                    if (!enumerable.Any((ThingDef x) => x.isBodyPartOrImplant))
                    {
                        if (!enumerable.Any((ThingDef x) => x.IsDrug))
                        {
                            if (current.targetsBodyPart)
                            {
                                foreach (BodyPartRecord current2 in current.Worker.GetPartsToApplyOn(patient, current))
                                {
                                    list.Add(Building_MechanicusMedTable.GenerateSurgeryOptionMedTable(patient, medTable, current, enumerable, current2));
                                }
                            }
                            else
                            {
                                list.Add(Building_MechanicusMedTable.GenerateSurgeryOptionMedTable(patient, medTable, current, enumerable, null));
                            }
                        }
                    }
                }
            }
            return list;
        }        

        public static FloatMenuOption GenerateSurgeryOptionMedTable(Pawn pawn, Building_MechanicusMedTable medTable, RecipeDef recipe, IEnumerable<ThingDef> missingIngredients, BodyPartRecord part = null)
        {
            string text = recipe.Worker.GetLabelWhenUsedOn(pawn, part);
            if (part != null && !recipe.hideBodyPartNames)
            {
                text = text + " (" + part.def.label + ")";
            }
            if (missingIngredients.Any<ThingDef>())
            {
                text += " (";
                bool flag = true;
                foreach (ThingDef current in missingIngredients)
                {
                    if (!flag)
                    {
                        text += ", ";
                    }
                    flag = false;
                    text += "MissingMedicalBillIngredient".Translate(new object[]
                    {
                current.label
                    });
                }
                text += ")";
                return new FloatMenuOption(text, null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            Action action = delegate
            {
                if (medTable == null)
                {
                    return;
                }
                recipe.effectWorking = EffecterDefOf.ConstructMetal;
                Bill_MedicalTable bill_Medical = new Bill_MedicalTable(recipe);
                medTable.medOpStack.AddBill(bill_Medical);
                bill_Medical.Part = part;
                if (recipe.conceptLearned != null)
                {
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
                }
                Map map = medTable.Map;
                if (!map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
                {
                    Bill.CreateNoPawnsWithSkillDialog(recipe);
                }
                if (medTable.patient.Faction != null && !medTable.patient.Faction.HostileTo(Faction.OfPlayer) && recipe.Worker.IsViolationOnPawn(medTable.patient, part, Faction.OfPlayer))
                {
                    Messages.Message("MessageMedicalOperationWillAngerFaction".Translate(new object[]
                    {
                medTable.Faction
                    }), medTable, MessageSound.Negative);
                }
                MethodInfo info = typeof(HealthCardUtility).GetMethod("GetMinRequiredMedicine", BindingFlags.Static | BindingFlags.NonPublic);
                if (info == null) Log.Message("NoInfo");
                Log.Message("tryingToInvoke");
                ThingDef minRequiredMedicine = (ThingDef)info.Invoke(null, new object[] {recipe });
                if (minRequiredMedicine != null && medTable.patient.playerSettings != null && !medTable.patient.playerSettings.medCare.AllowsMedicine(minRequiredMedicine))
                {
                    Messages.Message("MessageTooLowMedCare".Translate(new object[]
                    {
                minRequiredMedicine.label,
                medTable.LabelShort,
                medTable.patient.playerSettings.medCare.GetLabel()
                    }), medTable, MessageSound.Negative);
                }
      //          Log.Message("C3");
            };
            return new FloatMenuOption(text, action, MenuOptionPriority.Default, null, null, 0f, null, null);
        }

        //      private List<ThingAmount> necessaryIngredients(RecipeDef recipe)
        //       {
        //            List<ThingAmount> list = new List<ThingAmount>();
        //          foreach (ThingDef )
        //      }

        public override void Draw()
        {
            base.Draw();
            Vector3 vector = this.DrawPos;
            vector.y = Altitudes.AltitudeFor(AltitudeLayer.Building) + 0.1f;
            float angle = this.Rotation.AsAngle;
            Vector3 s = new Vector3(this.Graphic.data.drawSize.x, 1f, this.Graphic.data.drawSize.y);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.toolMat, 0);
            if (this.patient != null && !this.patient.Dead)
            {
                this.DrawBody(this.patient.Drawer.renderer);
                //        Log.Message("patient: " + this.patient.Rotation.ToString() + "   Bed:  " + this.Rotation.ToString());
            }
            else if (this.corpse != null && !corpse.Spawned)
            {
     //           this.DrawBody(this.corpse.InnerPawn.Drawer.renderer);
            }
        }

        private void DrawBody(PawnRenderer renderer)
        {
            float angle = this.Rotation.AsAngle;
            Material bodymat = renderer.graphics.nakedGraphic.MatFront;
            Material headmat = renderer.graphics.headGraphic.MatFront;
            Material hairmat = this.patient.Drawer.renderer.graphics.hairGraphic.MatFront;
            Vector3 sBody = new Vector3(1.0f, 1f, 1.0f);
            Matrix4x4 matrixBody = default(Matrix4x4);
            Vector3 vector = this.DrawPos;
            vector.y += 0.05f;
            matrixBody.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), sBody);

            Graphics.DrawMesh(MeshPool.humanlikeBodySet.MeshAt(this.Rotation), matrixBody, bodymat, 0);

            Matrix4x4 matrixHead = default(Matrix4x4);
            Vector3 headVec = vector + new Vector3(Mathf.Sin(angle) * 0.2f, 0.03f, Mathf.Cos(angle) * 0.2f);
            matrixHead.SetTRS(headVec, Quaternion.AngleAxis(angle, Vector3.up), new Vector3(1.0f, 1f, 1.0f));
            Graphics.DrawMesh(MeshPool.humanlikeHeadSet.MeshAt(this.Rotation), matrixHead, headmat, 0);
            Graphics.DrawMesh(MeshPool.humanlikeHairSetAverage.MeshAt(this.Rotation), matrixHead, hairmat, 0);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator();
            while (enumerator.MoveNext())
            {
                Gizmo current = enumerator.Current;
                yield return current;
            }
            if (base.Faction == Faction.OfPlayer && this.innerContainer.Count > 0 && this.def.building.isPlayerEjectable)
            {
                Command_Action command_Action = new Command_Action();
                command_Action.action = new Action(this.EjectContents);
                command_Action.defaultLabel = "CommandPodEject".Translate();
                command_Action.defaultDesc = "CommandPodEjectDesc".Translate();
                if (this.innerContainer.Count == 0)
                {
                    command_Action.Disable("CommandPodEjectFailEmpty".Translate());
                }
                command_Action.hotKey = KeyBindingDefOf.Misc1;
                command_Action.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
                yield return command_Action;
            }
            yield break;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(selPawn).GetEnumerator();
            while (enumerator.MoveNext())
            {
                FloatMenuOption current = enumerator.Current;
                yield return current;
            }

            if (this.innerContainer.Count == 0)
            {
                if (!selPawn.CanReserve(this, 1))
                {
                    FloatMenuOption floatMenuOption = new FloatMenuOption("CannotUseReserved".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return floatMenuOption;
                }
                if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    FloatMenuOption floatMenuOption2 = new FloatMenuOption("CannotUseNoPath".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return floatMenuOption2;
                }
                string label = "EnterSurgicalUnit".Translate();
                Action action = delegate
                {
                    Job job = new Job(C_JobDefOf.EnterMecMedTable, this);
                    selPawn.jobs.TryTakeOrderedJob(job);
                };
                yield return new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, null);

                if (this.patient != null && this.BillStack.Count == 0)
                {
                    string labelNoOps = "NoMSUOperations".Translate(new object[]
                        {
                            this.patient.LabelShort
                        });
                    FloatMenuOption floatMenuOption = new FloatMenuOption(labelNoOps, null, MenuOptionPriority.Default, null, null, 0f, null, null);
                    yield return floatMenuOption;
                }

                foreach (Pawn prisoner in selPawn.Map.mapPawns.PrisonersOfColonySpawned)
                {
                    string label2 = "CarryPrisonerSurgicalUnit".Translate(new object[]
                        {
                            prisoner.LabelShort
                        });
                    Action action2 = delegate
                    {
                        Job job = new Job(C_JobDefOf.CarryToMecMedTable, prisoner, this);
                        selPawn.jobs.TryTakeOrderedJob(job);
                    };
                    yield return new FloatMenuOption(label2, action2, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
                List<Pawn> downedPawns = this.Map.mapPawns.AllPawnsSpawned.Where(x => x.Downed).ToList<Pawn>();
                foreach (Pawn current in downedPawns)
                {
                    string label2 = "CarryDownedSurgicalUnit".Translate(new object[]
                        {
                            current.LabelShort
                        });
                    Action action2 = delegate
                    {
                        Job job = new Job(C_JobDefOf.CarryToMecMedTable, current, this);
                        selPawn.jobs.TryTakeOrderedJob(job);
                        selPawn.jobs.curJob.count = 1;
                    };
                    yield return new FloatMenuOption(label2, action2, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
            }

            yield break;
        }

        public override void EjectContents()
        {
            ThingDef filthSlime = ThingDefOf.FilthSlime;
            foreach (Thing current in this.innerContainer)
            {
                Pawn pawn = current as Pawn;
                if (pawn != null)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                    pawn.filth.GainFilth(filthSlime);
            //        pawn.health.AddHediff(HediffDefOf.CryptosleepSickness, null, null);
                }
            }
            if (!base.Destroyed)
            {
                SoundDef.Named("CryptosleepCasketEject").PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
            }
            base.EjectContents();
        }

        public bool CanWorkWithoutPower
        {
            get
            {
                return this.powerComp == null || this.def.building.unpoweredWorkTableWorkSpeedFactor > 0f;
            }
        }

        public bool CurrentlyUsable()
        {
            return (this.CanWorkWithoutPower || (this.powerComp != null && this.powerComp.PowerOn)) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
        }        
    }
}
