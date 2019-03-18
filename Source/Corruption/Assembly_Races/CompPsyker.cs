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
    public class CompPsyker : CompEquippable
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

        public Pawn psyker
        {
            get
            {
                return this.parent as Pawn;
            }
        }

        public CompSoul soul
        {
            get
            {
                CompSoul s = CompSoul.GetPawnSoul(psyker);
                if (s != null)
                {
                    return s;
                }
                return null;
            }
        }

        public PatronDef Patron
        {
            get
            {
                return this.soul.Patron;
            }
        }

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
            this.psykerPowerManager = new PsykerPowerManager(this);
        }

        public void SetInitialLevel()
        {
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
                        {
                            Log.Error(ex.Message);
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
        }

        public void AddXP(float amountGained)
        {
            this.PsykerPowerManager.AddXP(amountGained);
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption o in base.CompFloatMenuOptions(selPawn))
            {
                yield return o;
            }

            if (!this.soul.KnownToPlayer)
            {
                int successFactorPsyker = (int)(Math.Max(0, (CorruptionStoryTrackerUtilities.DiscoverAlignmentByPsykerModifier + CorruptionStoryTrackerUtilities.PsykerPowerDifference(selPawn, this.psyker) / 7) * 100));

                CompSoul selSoul = CompSoul.GetPawnSoul(selPawn);

                if (selSoul != null && selSoul.PsykerPowerLevel >= PsykerPowerLevel.Iota)
                {
                    yield return new FloatMenuOption("PsychicProbe".Translate(new object[] { successFactorPsyker.ToString() }), delegate
                    {
                        CompPsyker psykerComp = CompPsyker.GetCompPsyker(selPawn);
                        if (psykerComp != null)
                        {
                            PsykerPowerDef powerDef = DefOfs.C_PsykerPowerDefOf.PsykerPower_ProbeMind;
                            Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(powerDef.MainVerb.verbClass);
                            newverb.caster = selPawn;
                            newverb.verbProps = powerDef.MainVerb;
                            CompPsyker.TryCastPowerAction(selPawn, this.psyker, psykerComp, newverb, powerDef)?.Invoke();
                        }

                    });
                }
            }
        }

        public List<PsykerPower> Powers = new List<PsykerPower>();

        public List<PsykerPower> temporaryWeaponPowers = new List<PsykerPower>();

        public List<PsykerPower> temporaryApparelPowers = new List<PsykerPower>();

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

        public override void CompTick()
        {
            base.CompTick();
            if (this.soul != null)
            {
                if (this.Patron != soul.Patron)
                {
                    PortraitsCache.SetDirty(this.psyker);
                }
            }
            this.TicksToCast--;
            if (this.TicksToCast < 0)
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
                compPsyker.CurTarget = null;
                compPsyker.CurTarget = target;
                compPsyker.curVerb = verb;

                compPsyker.curPower = psydef;
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
            switch (cat)
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
                            newverb.CasterPawn.Name
                            }));
                        }
                        else if (!newverb.CasterPawn.drafter.Drafted)
                        {
                            command_CastPower.Disable("IsNotDrafted".Translate(new object[]
                            {
                            newverb.CasterPawn.Name
                            }));
                        }
                        else if (!this.IsActive)
                        {
                            command_CastPower.Disable("PsykerPowerRecharging".Translate(new object[]
                                {
                                newverb.CasterPawn.Name
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

                float scale = 2f;
                if (this.curVerb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
                {
                    scale = this.curVerb.verbProps.range * 2;
                }
                Vector3 s = new Vector3(scale, 1f, scale);
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
            Scribe_Defs.Look<PsykerPowerDef>(ref this.curPower, "curPower");
            Scribe_Values.Look<int>(ref this.TicksToCast, "TicksToCast", 0, false);
            Scribe_Values.Look<int>(ref this.TicksToCastMax, "TicksToCastMax", 1, false);
            Scribe_Values.Look<float>(ref this.TicksToCastPercentage, "TicksToCastPercentage", 1, false);
            Scribe_Values.Look<bool>(ref this.IsActive, "IsActive", false, false);
            Scribe_Values.Look<bool>(ref this.ShotFired, "ShotFired", true, false);
            Scribe_Deep.Look<PsykerPowerManager>(ref this.psykerPowerManager, "psykerPowerManager", new object[]
                {
                  this
                });
        }

        public static CompPsyker GetCompPsyker(Pawn pawn)
        {
            return pawn.TryGetComp<CompPsyker>();
        }
    }
}
