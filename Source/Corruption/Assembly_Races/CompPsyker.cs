using Corruption.DefOfs;
using Corruption.Worship;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace Corruption
{
    public class CompPsyker : CompUseEffect
    {
        private PsykerPowerManager psykerPowerManager;

        public PsykerPowerManager PsykerPowerManager
        {
            get
            {
                return this.psykerPowerManager;
            }
        }

        public bool ShotFired = true;        

        public int ticksToImpact = 500;

        private bool initialized = false;

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
        
        public PatronDef Patron;

        public LocalTargetInfo CurTarget;

        public PsykerPowerDef curPower;

        public Verb_CastWarpPower curVerb;

        public Rot4 curRotation;

        public float TicksToCastPercentage = 1;

        public int TicksToCastMax = 100;

        public int TicksToCast = 0;
         
        public bool IsActive;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        private void Initialize()
        {
            if (this.psykerPowerManager == null)
            {
                this.psykerPowerManager = new PsykerPowerManager(this);
                if (CorruptionModSettings.AllowPsykers)
                {
                    if (this.soul.PawnAfflictionProps.CommmonPsykerPowers != null)
                    {
                        for (int i = 0; i < this.soul.PawnAfflictionProps.CommmonPsykerPowers.Count; i++)
                        {

                            try
                            {
                                this.PsykerPowerManager.AddPsykerPower(this.soul.PawnAfflictionProps.CommmonPsykerPowers[i]);
                            }
                            catch (Exception ex)
                            { }
                        }
                    }
                }

            }

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

            this.initialized = true;
        }
        
        public void AddXP(float amountGained)
        {
            this.PsykerPowerManager.AddXP(amountGained);
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!this.soul.KnownToPlayer)
            {
                int successFactorSocial = (int)(Math.Max(0, (CorruptionStoryTrackerUtilities.DiscoverAlignmentByConfessionModifier + CorruptionStoryTrackerUtilities.SocialSkillDifference(selPawn, this.psyker) / 20) * 100));
                int successFactorPsyker = (int)(Math.Max(0, (CorruptionStoryTrackerUtilities.DiscoverAlignmentByPsykerModifier + CorruptionStoryTrackerUtilities.PsykerPowerDifference(selPawn, this.psyker) / 7 )* 100));

                BuildingAltar altar = null;
                if (this.soul.PsykerPowerLevel >= PsykerPowerLevel.Iota)
                {
                    yield return new FloatMenuOption("PsychicProbe".Translate(new object[] { successFactorPsyker.ToString() }), delegate
                    {
                        List<Pawn> list = new List<Pawn>() { selPawn, this.psyker };
                        Lord lord = LordMaker.MakeNewLord(altar.Faction, new LordJob_Sermon(altar, WorshipActType.Confession), altar.Map, list);

                    });
                }

                if (CorruptionStoryTrackerUtilities.IsPreacher(selPawn, out altar))
                {
                    yield return new FloatMenuOption("StartConfession".Translate(new object[] { successFactorSocial.ToString() }), delegate
                    {
                        List<Pawn> list = new List<Pawn>() {  selPawn, this.psyker};
                        Lord lord = LordMaker.MakeNewLord(altar.Faction, new LordJob_Sermon(altar, WorshipActType.Confession), altar.Map, list);

                    });
                }

                yield return new FloatMenuOption("InvestigateAlignment".Translate(new object[] { successFactorSocial.ToString() }), delegate
                {
                    Job job = new Job(C_JobDefOf.FollowAndInvestigate, this.psyker);
                    selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);

                });
            }

            yield return null;
        }

        public List<PsykerPower> Powers = new List<PsykerPower>();
        
        public List<PsykerPower> temporaryWeaponPowers = new List<PsykerPower>();

        public List<PsykerPower> temporaryApparelPowers = new List<PsykerPower>();

        public List<PsykerPower> allPowers = new List<PsykerPower>();
                
        public List<PsykerPowerEntry> AllPsykerPowers
        {
            get
            {
                List<PsykerPowerEntry> result = new List<PsykerPowerEntry>();
                result.AddRange(this.PsykerPowerManager.IotaPowers);
                result.AddRange(this.PsykerPowerManager.ZetaPowers);
                result.AddRange(this.PsykerPowerManager.EpsilonPowers);
                result.AddRange(this.PsykerPowerManager.DeltaPowers);
                result.AddRange(this.PsykerPowerManager.EquipmentPowers);
                return result;
            }
        }

        public Dictionary<PsykerPower, Verb_CastWarpPower> psykerPowers = new Dictionary<PsykerPower, Verb_CastWarpPower>();

        public List<Verb_CastWarpPower> PowerVerbs = new List<Verb_CastWarpPower>();

        public void UpdatePowers()
        {

            PowerVerbs.Clear();
            List<PsykerPower> psylist = new List<PsykerPower>();
            
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
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!this.initialized && this.psyker != null)
            {
                Initialize();
            }
            if (this.soul != null)
            {
                if (this.Patron != soul.Patron)
                {
                    this.Patron = soul.Patron;
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
            foreach (PsykerPowerEntry current in this.AllPsykerPowers)
            {
                if (PsykerHasEquipment(current))
                {
                    Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(current.psykerPowerDef.MainVerb.verbClass);
                    newverb.caster = this.psyker;
                    newverb.verbProps = current.psykerPowerDef.MainVerb;

                    Command_CastPower command_CastPower = new Command_CastPower(this);
                    command_CastPower.verb = newverb;
                    command_CastPower.defaultLabel = current.psykerPowerDef.LabelCap;
                    command_CastPower.defaultDesc = current.psykerPowerDef.description;
                    command_CastPower.targetingParams = TargetingParameters.ForAttackAny();
                    if (newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetSelf || newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
                    {
                        command_CastPower.targetingParams = TargetingParameters.ForSelf(this.psyker);
                    }
                    command_CastPower.icon = current.psykerPowerDef.uiIcon;
                    string str;
                    if (FloatMenuUtility.GetAttackAction(this.psyker, LocalTargetInfo.Invalid, out str) == null)
                    {
                        command_CastPower.Disable(str.CapitalizeFirst() + ".");
                    }
                    command_CastPower.action = delegate (Thing target)
                    {
                        CompPsyker.TryCastPowerAction(psyker, target, this, newverb, current.psykerPowerDef)?.Invoke();

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
            Scribe_Defs.Look<PatronDef>(ref this.Patron, "Patron");
            Scribe_Defs.Look<PsykerPowerDef>(ref this.curPower, "curPower");
            Scribe_Values.Look<int>(ref this.TicksToCast, "TicksToCast", 0, false);
            Scribe_Values.Look<int>(ref this.TicksToCastMax, "TicksToCastMax", 1, false);
            Scribe_Values.Look<float>(ref this.TicksToCastPercentage, "TicksToCastPercentage", 1, false);
            Scribe_Values.Look<bool>(ref this.IsActive, "IsActive", false, false);
            Scribe_Values.Look<bool>(ref this.ShotFired, "ShotFired", true, false);
            Scribe_Values.Look<bool>(ref this.initialized, "initialized", true, false);
            Scribe_Deep.Look<PsykerPowerManager>(ref this.psykerPowerManager, "psykerPowerManager", new object[]
                {                    
                  this
                });
        }
    }
}
