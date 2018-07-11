using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Corruption
{
    [StaticConstructorOnStartup]
    public class ITab_MSUOperation : ITab
    {
        private static Vector2 billsScrollPosition = Vector2.zero;

        private static float billsScrollHeight = 1000f;

        private const float TitleHeight = 70f;
        
        private const float InfoHeight = 60f;
        
        private Building_MechanicusMedTable medTable
        {
            get
            {
                return this.SelThing as Building_MechanicusMedTable;
            }
        }

        private Pawn patient
        {
            get
            {
                if (this.medTable != null && this.medTable.patient != null)
                {
                    return this.medTable.patient;
                }
                return null;
                     
            }
        }

        protected Pawn currentOperator;


        public ITab_MSUOperation()
        {
            this.size = new Vector2(630f, 430f);
            this.labelKey = "TabMSU";
        }


        private bool ShouldAllowOperations()
        {
            Pawn pawnForHealth = medTable.patient;
            if (pawnForHealth.Dead)
            {
                return false;
            }
            return medTable.def.AllRecipes.Any((RecipeDef x) => x.AvailableNow) && (pawnForHealth.Faction == Faction.OfPlayer || (pawnForHealth.IsPrisonerOfColony || (pawnForHealth.HostFaction == Faction.OfPlayer && !pawnForHealth.health.capacities.CapableOf(PawnCapacityDefOf.Moving))) || ((!pawnForHealth.RaceProps.IsFlesh || pawnForHealth.Faction == null || !pawnForHealth.Faction.HostileTo(Faction.OfPlayer)) && (!pawnForHealth.RaceProps.Humanlike && pawnForHealth.Downed)));
        }

        protected override void FillTab()
        {
            Rect leftRect = new Rect(0f, 35f, 400f, 300f);
            Rect rightRect = new Rect(leftRect.xMax + 5f, 35f, 230f, 300f);
            Rect titleRect = new Rect(0f, 0f, this.size.x, 25f);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            string title = "SMU MK IV Interface";
            Widgets.Label(titleRect, title);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            if (patient != null)
            {
                    //    GUI.BeginGroup(leftRect);
                    Rect rectSO = new Rect(leftRect.x, leftRect.y + 10f, 100f, 20f);
                float cury = 0f;
                DrawMedTableOperationsTab(leftRect, patient, patient, cury, this.medTable);
            }
            else
            {
                Rect rect = new Rect(0f, leftRect.y, leftRect.width, 30f);
                GUI.color = Color.red;
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "MSU_NoPatient".Translate());
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;

                //if (Widgets.ButtonText(rect, "AddPrisoners".Translate()))
                //{
                //    FloatMenu floatMenu = new FloatMenu(this.ForcedPrisonerPatients);
                //    Find.WindowStack.Add(floatMenu);
                //}
            }
        }

        private List<FloatMenuOption> ForcedPrisonerPatients
        {
            get
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                List<Pawn> prisoners = this.medTable.Map.mapPawns.PrisonersOfColonySpawned;
                List<Pawn> wardens = this.medTable.Map.mapPawns.FreeColonistsSpawned.Where(x => x.workSettings.WorkIsActive(WorkTypeDefOf.Warden)).ToList();
                if (wardens.Count > 0 && prisoners.Count > 0)
                {
                    foreach (Pawn prisoner in prisoners)
                    {
                        list.Add(new FloatMenuOption("PrisonerPatient".Translate(), delegate
                        {
                            Pawn warden = wardens.FirstOrDefault();
                            if (warden != null)
                            {
                                Job job = new Job(JobDefOf.HaulToContainer, prisoner, this.medTable);
                                job.count = 1;
                                warden.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            }
                        }));
                    }
                }
                else
                {
                    list.Add(new FloatMenuOption("NoWardensOrPrisoners".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                return list;
            }
        }

        private static float DrawMedTableOperationsTab(Rect rect, Pawn patient, Thing thingForMedBills, float curY, Building_MechanicusMedTable medTable)
        {
            curY += 2f;
            Func<List<FloatMenuOption>> recipeOptionsMakerStandard = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (RecipeDef current in thingForMedBills.def.AllRecipes)
                {
                    if (current.AvailableNow)
                    {
                        IEnumerable<ThingDef> enumerable = current.PotentiallyMissingIngredients(null, medTable.Map);
                        
                        if (enumerable != null && !enumerable.Any((ThingDef x) => x.isBodyPartOrImplant))
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
            };

            Func<List<FloatMenuOption>> recipeOptionsMakerAdvanced = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();


                List<RecipeDef_MSU> advancedRecipes = DefDatabase<RecipeDef_MSU>.AllDefsListForReading;
                //advancedRecipes.AddRange(DefDatabase<RecipeDef>.AllDefsListForReading.FindAll(x => x is Astartes.RecipeDef_AstartesImplant));
                foreach (RecipeDef_MSU current in advancedRecipes)
                {
                    if (current.AvailableNow && CheckAstartesRecipeDef(patient,current) && CheckServitorRequirement(patient, current))
                    {
                        IEnumerable<ThingDef> enumerable = current.PotentiallyMissingIngredients(null, medTable.Map);
                        if (enumerable != null && !enumerable.Any((ThingDef x) => x.isBodyPartOrImplant))
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

                if (list.NullOrEmpty())
                {
                    list.Add(new FloatMenuOption("NoOptionsMSU".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                return list;
            };
            Rect rect2 = new Rect(rect.x + 20f, 40f, rect.width, rect.height - curY - 20f);
            ITab_MSUOperation.DoBillListingSMU(medTable.BillStack, rect2, recipeOptionsMakerStandard, recipeOptionsMakerAdvanced, ref ITab_MSUOperation.billsScrollPosition, ref ITab_MSUOperation.billsScrollHeight);
            return curY;
        }

        private static bool CheckServitorRequirement(Pawn patient, RecipeDef_MSU recipeDef)
        {
            if (recipeDef.RecipeType == RecipeTypeMSU.ServitorReprogram)
            {
                if (patient.TryGetComp<CompServitor>() == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckAstartesRecipeDef(Pawn patient, RecipeDef_MSU recipeDef)
        {
            if (recipeDef.RecipeType == RecipeTypeMSU.AstartesImplant && recipeDef.RequiresHediff != null)
            {
                if (patient.health.hediffSet.hediffs.Any(x => x.def == recipeDef.RequiresHediff))
                    {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private static Bill DoBillListingSMU(BillStack billStack, Rect rect, Func<List<FloatMenuOption>> recipeOptionsMakerStandard, Func<List<FloatMenuOption>> recipeOptionsMakerAdvanced, ref Vector2 scrollPosition, ref float viewHeight)
        {
            Bill result = null;
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Small;
            if (billStack.Count < 15)
            {
                Rect rect2 = new Rect(0f, 0f, 160f, 29f);
                if (Widgets.ButtonText(rect2, "AddStandardSurgery".Translate(), true, false, true))
                {
                    Find.WindowStack.Add(new FloatMenu(recipeOptionsMakerStandard()));
                }
                Rect rect3 = new Rect(170f, 0f, 160f, 29f);
                if (Widgets.ButtonText(rect3, "AddAdvancedSurgery".Translate(), true, false, true))
                {
                    Find.WindowStack.Add(new FloatMenu(recipeOptionsMakerAdvanced()));
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 35f, rect.width, rect.height - 35f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float num = 0f;

            List<Bill> bills = typeof(BillStack).GetField("bills", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(billStack) as List<Bill>;

            for (int i = 0; i < billStack.Count; i++)
            {
                Bill bill = bills[i];
                Rect rect3 = bill.DoInterface(0f, num, viewRect.width, i);
                if (!bill.DeletedOrDereferenced && Mouse.IsOver(rect3))
                {
                    result = bill;
                }
                num += rect3.height + 6f;
            }
            if (Event.current.type == EventType.Layout)
            {
                viewHeight = num + 60f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            return result;
        }

    }
}
