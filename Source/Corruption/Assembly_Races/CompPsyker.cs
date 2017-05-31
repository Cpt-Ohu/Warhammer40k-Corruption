using Corruption.DefOfs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Corruption
{
    public class CompPsyker : CompUseEffect
    {
        public PsykerPowerManager psykerPowerManager;

        public bool ShotFired = true;        

        public int ticksToImpact = 500;
               

        public Pawn psyker
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        public Need_Soul soul
        {
            get
            {
                Need_Soul s = psyker.needs.TryGetNeed<Need_Soul>();
                if (s != null)
                {
                    return s;
                }
                return null;
            }
        }

        public string patronName = "Emperor";

        public LocalTargetInfo CurTarget;

        public PsykerPowerDef curPower;

        public Verb_CastWarpPower curVerb;

        public Rot4 curRotation;

        public float TicksToCastPercentage = 1;

        public int TicksToCastMax = 100;

        public int TicksToCast = 0;

        public bool IsActive;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.psykerPowerManager = new PsykerPowerManager(this);
            ChaosFollowerPawnKindDef pdef = this.psyker.kindDef as ChaosFollowerPawnKindDef;
            if (pdef != null && pdef.RenamePawns)
            {
                    string rawName = NameGenerator.GenerateName(pdef.OverridingNameRulePack, delegate (string x)
                    {
                        NameTriple nameTriple4 = NameTriple.FromString(x);
                        nameTriple4.ResolveMissingPieces(null);
                        return !nameTriple4.UsedThisGame;
                    }, false);
                    NameTriple nameTriple = NameTriple.FromString(rawName);
                    nameTriple.CapitalizeNick();
                    nameTriple.ResolveMissingPieces(null);
                    psyker.Name = nameTriple;                
            }

        }

        public List<PsykerPower> Powers = new List<PsykerPower>();
        
        public List<PsykerPower> learnedPowers = new List<PsykerPower>();

        public List<PsykerPower> temporaryWeaponPowers = new List<PsykerPower>();

        public List<PsykerPower> temporaryApparelPowers = new List<PsykerPower>();

        public List<PsykerPower> allPowers = new List<PsykerPower>();

        public List<PsykerPowerEntry> allpsykerPowers = new List<PsykerPowerEntry>();

        public Dictionary<PsykerPower, Verb_CastWarpPower> psykerPowers = new Dictionary<PsykerPower, Verb_CastWarpPower>();

        public List<Verb_CastWarpPower> PowerVerbs = new List<Verb_CastWarpPower>();

        public void UpdatePowers()
        {

            PowerVerbs.Clear();
            List<PsykerPower> psylist = new List<PsykerPower>();

            psylist.AddRange(this.learnedPowers);

            psylist.AddRange(this.temporaryWeaponPowers);
            psylist.AddRange(this.temporaryApparelPowers);

            this.allPowers = psylist;

            for (int i = 0; i < allPowers.Count; i++)
            {
                Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(psylist[i].powerdef.MainVerb.verbClass);
                if (!PowerVerbs.Any(item => item.verbProps == newverb.verbProps))
                {
                    newverb.caster = this.psyker;
                    newverb.verbProps = psylist[i].powerdef.MainVerb;
                    PowerVerbs.Add(newverb);
                }
            }

            this.psykerPowers.Clear();

            foreach (PsykerPower pow in psylist)
            {
                Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(pow.powerdef.MainVerb.verbClass);
                if (!PowerVerbs.Any(item => item.verbProps == newverb.verbProps))
                {
                    newverb.caster = this.psyker;
                    newverb.verbProps = pow.powerdef.MainVerb;
                    this.psykerPowers.Add(pow, newverb);
                }
            }
     //       Log.Message(this.psykerPowers.Count.ToString());
        }

        public override void CompTick()
        {
            base.CompTick();
            if (this.soul != null)
            {
                if (this.patronName != soul.patronInfo.PatronName)
                {
                    this.patronName = soul.patronInfo.PatronName;
                    PortraitsCache.SetDirty(this.psyker);
                }
            }
                        
            this.TicksToCast--;
            if (this.TicksToCast <0)
            {
                this.IsActive = true;
                this.ShotFired = true;
                this.TicksToCast = 0;
            }


            this.TicksToCastPercentage = (1 - (this.TicksToCast / this.TicksToCastMax));
        }
        
        public static Action TryCastPowerAction(Pawn pawn, LocalTargetInfo target, CompPsyker compPsyker, Verb_CastWarpPower verb, PsykerPowerDef psydef)
        {

                Action act = new Action(delegate
                {
                    //            compPsyker.TicksToCast = verb.warpverbprops.TicksToRecharge;
                    //            compPsyker.TicksToCastMax = verb.warpverbprops.TicksToRecharge;
                    compPsyker.CurTarget = null;
                    compPsyker.CurTarget = target;
                    compPsyker.curVerb = verb;


                    compPsyker.curPower = psydef;

               //     Log.Message("Casting Stuff");
               //     Log.Message(compPsyker.curPower.defName);
                    compPsyker.curRotation = target.Thing.Rotation;
                    Job job = CompPsyker.PsykerJob(verb.warpverbprops.PsykerPowerCategory, target);
                    job.playerForced = true;
                    job.verbToUse = verb;
                    Pawn pawn2 = target.Thing as Pawn;
                    if (pawn2 != null)
                    {
                        job.killIncappedTarget = pawn2.Downed;
                    }
                    pawn.jobs.TryTakeOrderedJob(job);
                });
                return act;            
        }

        public static Job PsykerJob(PsykerPowerTargetCategory cat, LocalTargetInfo target)
        {
            switch(cat)
            {
                case PsykerPowerTargetCategory.TargetSelf:
                    {
                        return new Job(C_JobDefOf.CastPsykerPowerSelf, target);
                    }
                case PsykerPowerTargetCategory.TargetAoE:
                    {
                        return new Job(C_JobDefOf.CastPsykerPowerSelf, target);
                    }
                case PsykerPowerTargetCategory.TargetThing:
                    {
                        return new Job(C_JobDefOf.CastPsykerPowerVerb, target);
                    }
                default:
                    {
                        return new Job(C_JobDefOf.CastPsykerPowerVerb, target);
                    }
            }            
        }

        public IEnumerable<Command_CastPower> GetPsykerVerbsNew()
        {
            foreach (KeyValuePair<PsykerPower, Verb_CastWarpPower> entry in this.psykerPowers)
            {
                Verb_CastWarpPower newverb = entry.Value;
                newverb.caster = this.psyker;
                newverb.verbProps = entry.Value.verbProps;

                Command_CastPower command_CastPower = new Command_CastPower(this);
                command_CastPower.verb = newverb;
                command_CastPower.defaultLabel = entry.Key.def.LabelCap;
                command_CastPower.defaultDesc = entry.Key.def.description;
                command_CastPower.targetingParams = TargetingParameters.ForAttackAny();
                if (newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetSelf || newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
                {
                    command_CastPower.targetingParams = TargetingParameters.ForSelf(this.psyker);
                }
                command_CastPower.icon = entry.Key.def.uiIcon;
                string str;
                if (FloatMenuUtility.GetAttackAction(this.psyker, LocalTargetInfo.Invalid, out str) == null)
                {
                    command_CastPower.Disable(str.CapitalizeFirst() + ".");
                }
                command_CastPower.action = delegate (Thing target)
                {                    
                        Action attackAction = CompPsyker.TryCastPowerAction(psyker, target, this, newverb, entry.Key.def as PsykerPowerDef);
                        if (attackAction != null)
                        {
                            attackAction();
                        }
                    
                };
                if (newverb.caster.Faction != Faction.OfPlayer)
                {
                    command_CastPower.Disable("CannotOrderNonControlled".Translate());
                }
                if (newverb.CasterIsPawn)
                {
                    if (newverb.CasterPawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hunting))
                    {
                        command_CastPower.Disable("IsIncapableOfViolence".Translate(new object[]
                        {
                            newverb.CasterPawn.NameStringShort
                        }));
                    }
                    else if (!newverb.CasterPawn.drafter.Drafted)
                    {
                        command_CastPower.Disable("IsNotDrafted".Translate(new object[]
                        {
                            newverb.CasterPawn.NameStringShort
                        }));
                    }
                    else if (!this.IsActive)
                    {
                        command_CastPower.Disable("PsykerPowerRecharging".Translate(new object[]
                            {
                                newverb.CasterPawn.NameStringShort
                            }));
                    }
                }
                yield return command_CastPower;
            }
            yield break;
        }

        public bool PsykerHasEquipment(PsykerPowerEntry entry)
        {
            if (entry.EquipmentDependent && entry.DependendOn != null)
            {
                if (psyker.equipment.AllEquipmentListForReading.Any(item => item.def == entry.DependendOn))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public IEnumerable<Command_CastPower> GetPsykerVerbsNewV3()
        {

         //   Log.Message("INIT");
            foreach (PsykerPowerEntry current in this.allpsykerPowers)
            {
          //      Log.Message("A");
                if (PsykerHasEquipment(current))
                {
                    Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(current.psykerPowerDef.MainVerb.verbClass);
                    newverb.caster = this.psyker;
                    newverb.verbProps = current.psykerPowerDef.MainVerb;
              //      Log.Message("B");

                    Command_CastPower command_CastPower = new Command_CastPower(this);
                    command_CastPower.verb = newverb;
                    command_CastPower.defaultLabel = current.psykerPowerDef.LabelCap;
                    command_CastPower.defaultDesc = current.psykerPowerDef.description;
           //         Log.Message("C");
                    command_CastPower.targetingParams = TargetingParameters.ForAttackAny();
                    if (newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetSelf || newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
                    {
                        command_CastPower.targetingParams = TargetingParameters.ForSelf(this.psyker);
                    }
                   // Log.Message("C2");
                    command_CastPower.icon = current.psykerPowerDef.uiIcon;
           //         Log.Message("D1");
                    string str;
                    if (FloatMenuUtility.GetAttackAction(this.psyker, LocalTargetInfo.Invalid, out str) == null)
                    {
                        command_CastPower.Disable(str.CapitalizeFirst() + ".");
                    }
           //         Log.Message("D");
                    command_CastPower.action = delegate (Thing target)
                    {
                        CompPsyker.TryCastPowerAction(psyker, target, this, newverb, current.psykerPowerDef as PsykerPowerDef)?.Invoke();

                    };
                    if (newverb.caster.Faction != Faction.OfPlayer)
                    {
                        command_CastPower.Disable("CannotOrderNonControlled".Translate());
                    }
                    if (newverb.CasterIsPawn)
                    {
                        if (newverb.CasterPawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hunting))
                        {
                            command_CastPower.Disable("IsIncapableOfViolence".Translate(new object[]
                            {
                            newverb.CasterPawn.NameStringShort
                            }));
                        }
                        else if (!newverb.CasterPawn.drafter.Drafted)
                        {
                            command_CastPower.Disable("IsNotDrafted".Translate(new object[]
                            {
                            newverb.CasterPawn.NameStringShort
                            }));
                        }
                        else if (!this.IsActive)
                        {
                            command_CastPower.Disable("PsykerPowerRecharging".Translate(new object[]
                                {
                                newverb.CasterPawn.NameStringShort
                                }));
                        }
                    }
                    yield return command_CastPower;
                }
            }
            yield break;
        }


        public IEnumerable<Command_CastPower> GetPsykerVerbsNewV2()
        {
            //     Log.Message("Found temp powers: " + this.temporaryPowers.Count.ToString() + " while finding Verbs: " + temporaryPowers.Count.ToString());
            //     Log.Message(this.PowerVerbs.Count.ToString());
            List<Verb_CastWarpPower> temp = new List<Verb_CastWarpPower>();
            temp.AddRange(this.PowerVerbs);
            for (int i = 0; i < allPowers.Count; i++)
            {
                int j = i;
                Verb_CastWarpPower newverb = temp[j];
                newverb.caster = this.psyker;
                newverb.verbProps = temp[j].verbProps;

                Command_CastPower command_CastPower = new Command_CastPower(this);
                command_CastPower.verb = newverb;
                command_CastPower.defaultLabel = allPowers[j].def.LabelCap;
                command_CastPower.defaultDesc = allPowers[j].def.description;
                command_CastPower.targetingParams = TargetingParameters.ForAttackAny();
                if (newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetSelf || newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
                {
                    command_CastPower.targetingParams = TargetingParameters.ForSelf(this.psyker);
                }
                command_CastPower.icon = allPowers[j].def.uiIcon;
                string str;
                if (FloatMenuUtility.GetAttackAction(this.psyker, LocalTargetInfo.Invalid, out str) == null)
                {
                    command_CastPower.Disable(str.CapitalizeFirst() + ".");
                }
                command_CastPower.action = delegate (Thing target)
                {                    
                        Action attackAction = CompPsyker.TryCastPowerAction(psyker, target, this, newverb, allPowers[j].def as PsykerPowerDef);
                        if (attackAction != null)
                        {
                            attackAction();
                        }
                                 
                };
                if (newverb.caster.Faction != Faction.OfPlayer)
                {
                    command_CastPower.Disable("CannotOrderNonControlled".Translate());
                }
                if (newverb.CasterIsPawn)
                {
                    if (newverb.CasterPawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hunting))
                    {
                        command_CastPower.Disable("IsIncapableOfViolence".Translate(new object[]
                        {
                            newverb.CasterPawn.NameStringShort
                        }));
                    }
                    else if (!newverb.CasterPawn.drafter.Drafted)
                    {
                        command_CastPower.Disable("IsNotDrafted".Translate(new object[]
                        {
                            newverb.CasterPawn.NameStringShort
                        }));
                    }
                    else if (!this.IsActive)
                    {
                        command_CastPower.Disable("PsykerPowerRecharging".Translate(new object[]
                            {
                                newverb.CasterPawn.NameStringShort
                            }));
                    }
                }
                yield return command_CastPower;
            }
            yield break;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {        
            if (psyker.Drafted)
            {
                foreach (Command_Target comm in GetPsykerVerbsNewV3().ToList())
                {
                    yield return comm;
                }
            }                            
        }

        private Material CastingDrawMat;
        

        public void DrawPsykerTargetReticule()
        {
            if (psyker.stances.curStance.GetType() == typeof(Stance_Warmup) && (this.CurTarget != null && this.curVerb != null))
            {
                    if (this.CastingDrawMat == null)
                    {
                        this.CastingDrawMat = GraphicDatabase.Get<Graphic_Single>(curPower.uiIconPath, ShaderDatabase.MoteGlow, Vector2.one, Color.white).MatSingle;
                    }

                    //    curRotation.Rotate(RotationDirection.Clockwise);  
                    float scale = 2f;
                    if (this.curVerb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
                    {
                        scale = this.curVerb.verbProps.range * 2;
                    }
                    //     Log.Message(this.CurTarget.Thing.Label);
                    Vector3 s = new Vector3(scale, 1f, scale);
                    //      Log.Message(curPower.label);
                    Matrix4x4 matrix = default(Matrix4x4);
                    Vector3 drawPos = new Vector3();

                    drawPos = this.CurTarget.Thing.DrawPos;
                    drawPos.y -= 1f;
                    matrix.SetTRS(drawPos, Quaternion.AngleAxis(curRotation.AsInt, Vector3.up), s);
                    Graphics.DrawMesh(MeshPool.plane10, matrix, CastingDrawMat, 0);
                }            
            else
            {
                this.CastingDrawMat = null;
            }
        }

        public override void PostDraw()
        {
            if (psyker.stances != null)
            {
                DrawPsykerTargetReticule();
            }           
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            //            Scribe_Collections.Look<PsykerPower, Verb_CastWarpPower>(ref this.psykerPowers, "psykerPowers", LookMode.Deep, LookMode.Reference);

            //          Scribe_Collections.Look<PsykerPower>(ref this.allPowers, "allPowers", LookMode.Deep, new object[]
            //          {
            //              this.psyker,
            //              new object()
            //          });
            //          Scribe_Collections.Look<PsykerPower>(ref this.temporaryApparelPowers, "temporaryApparelPowers", LookMode.Reference, new object[]
            //          {
            //              this.psyker,
            //              new object()
            //          });
            //       Scribe_Collections.Look<PsykerPower>(ref this.temporaryWeaponPowers, "temporaryWeaponPowers", LookMode.Reference, new object[0]);
            Scribe_Collections.Look(ref this.allpsykerPowers, "allpsykerPowers", LookMode.Deep, new object[0]);


            Scribe_Values.Look<string>(ref this.patronName, "patronName", "Emperor", false);
            Scribe_Values.Look<int>(ref this.TicksToCast, "TicksToCast", 0, false);
            Scribe_Values.Look<int>(ref this.TicksToCastMax, "TicksToCastMax", 1, false);
            Scribe_Values.Look<float>(ref this.TicksToCastPercentage, "TicksToCastPercentage", 1, false);
            Scribe_Values.Look<bool>(ref this.IsActive, "IsActive", false, false);
            Scribe_Values.Look<bool>(ref this.ShotFired, "ShotFired", true, false);
  //          Scribe_Deep.Look<Verb_CastWarpPower>(ref this.curVerb, "curVerb", null);
  //          Scribe_TargetInfo.LookTargetInfo(ref this.CurTarget, "CurTarget", null);

            Scribe_Deep.Look<PsykerPowerManager>(ref this.psykerPowerManager, "psykerPowerManager", new object[]
                {
                  this
                });
            



        }
    }
}
