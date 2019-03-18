//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using Verse;

//namespace Corruption.Psykers
//{
//    public class CompPsykerPowers : ThingComp, IVerbOwner, IExposable
//    {

//        public CompPsykerPowers(CompProperties props)
//        {
//            base.Initialize(props);
//            this._powerManager = new PsykerPowerManager(this);
//            this._verbTracker = new VerbTracker(this);
//        }

//        private PsykerPowerManager _powerManager;
//        public PsykerPowerManager PowerManager
//        {
//            get { return _powerManager; }
//        }
        
//        private Pawn _Psyker;
//        public Pawn Psyker
//        {
//            get
//            {
//                if (this._Psyker == null)
//                {
//                    this._Psyker = this.parent as Pawn;
//                }
//                return _Psyker;
//            }
//        }

//        private VerbTracker _verbTracker;
//        public VerbTracker VerbTracker
//        {
//            get
//            {
//                return this._verbTracker;
//            }
//        }

//        private List<VerbProperties> _allVerbProperties;
//        public List<VerbProperties> VerbProperties
//        {
//            get
//            {
//                return _allVerbProperties;
//            }
//        }

//        private List<Tool> _tools;
//        public List<Tool> Tools
//        {
//            get
//            {
//                return _tools;
//            }
//        }

//        private int _ticksToCast;
//        public int MaxTicks { get; set; }

//        public bool ReadyToCast
//        {
//            get
//            {
//                return this._ticksToCast == 0;
//            }
//        }

//        public float RechargeStatus
//        {
//            get
//            {
//                return 1 - (this._ticksToCast / this.MaxTicks);
//            }
//        }

//        public ImplementOwnerTypeDef ImplementOwnerTypeDef => throw new NotImplementedException();

//        public Thing ConstantCaster
//        {
//            get
//            {
//                return this.Psyker;
//            }
//        }

//        public string UniqueVerbOwnerID()
//        {
//            return "CompPsyker_" + this.Psyker.GetUniqueLoadID();
//        }

//        public bool VerbsStillUsableBy(Pawn p)
//        {
//            return true;
//        }

//        public void AddXP(float amountGained)
//        {
//            this.PowerManager.AddXP(amountGained);
//        }

//        private Material CastingDrawMat;
//        public override void PostDraw()
//        {
//            if (Psyker.stances != null)
//            {
//                DrawPsykerTargetReticule();
//            }
//        }
//        public void DrawPsykerTargetReticule()
//        {
//            if (Psyker.stances.curStance.GetType() == typeof(Stance_Warmup) && (this.CurTarget != null && this.curVerb != null))
//            {
//                if (this.CastingDrawMat == null)
//                {
//                    this.CastingDrawMat = GraphicDatabase.Get<Graphic_Single>(curPower.uiIconPath, ShaderDatabase.MoteGlow, Vector2.one, Color.white).MatSingle;
//                }

//                float scale = 2f;
//                if (this.curVerb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
//                {
//                    scale = this.curVerb.verbProps.range * 2;
//                }
//                Vector3 s = new Vector3(scale, 1f, scale);
//                Matrix4x4 matrix = default(Matrix4x4);
//                Vector3 drawPos = new Vector3();

//                drawPos = this.CurTarget.Thing.DrawPos;
//                drawPos.y -= 1f;
//                matrix.SetTRS(drawPos, Quaternion.AngleAxis(curRotation.AsInt, Vector3.up), s);
//                Graphics.DrawMesh(MeshPool.plane10, matrix, CastingDrawMat, 0);
//            }
//            else
//            {
//                this.CastingDrawMat = null;
//            }
//        }



//        public override IEnumerable<Gizmo> CompGetGizmosExtra()
//        {
//            if (Psyker.Drafted)
//            {
//                foreach (Command_Target comm in GetPsykerGizmos())
//                {
//                    yield return comm;
//                }
//            }
//        }

//        private IEnumerable<Command_Target> GetPsykerGizmos()
//        {

//            foreach (PsykerPowerEntry current in this.PowerManager.AllPsykerPowers)
//            {
//                if (PsykerHasEquipment(current))
//                {
//                    Verb_CastWarpPower newverb = (Verb_CastWarpPower)Activator.CreateInstance(current.psykerPowerDef.MainVerb.verbClass);
//                    newverb.caster = this.Psyker;
//                    newverb.verbProps = current.psykerPowerDef.MainVerb;

//                    Command_CastPower command_CastPower = new Command_CastPower(this);
//                    command_CastPower.verb = newverb;
//                    command_CastPower.defaultLabel = current.psykerPowerDef.LabelCap;
//                    command_CastPower.defaultDesc = current.psykerPowerDef.description;
//                    command_CastPower.targetingParams = TargetingParameters.ForAttackAny();
//                    if (newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetSelf || newverb.warpverbprops.PsykerPowerCategory == PsykerPowerTargetCategory.TargetAoE)
//                    {
//                        command_CastPower.targetingParams = TargetingParameters.ForSelf(this.Psyker);
//                    }
//                    command_CastPower.icon = current.psykerPowerDef.uiIcon;
//                    string str;
//                    if (FloatMenuUtility.GetAttackAction(this.Psyker, LocalTargetInfo.Invalid, out str) == null)
//                    {
//                        command_CastPower.Disable(str.CapitalizeFirst() + ".");
//                    }
//                    command_CastPower.action = delegate (Thing target)
//                    {
//                        CompPsyker.TryCastPowerAction(Psyker, target, this, newverb, current.psykerPowerDef)?.Invoke();

//                    };
//                    if (newverb.caster.Faction != Faction.OfPlayer)
//                    {
//                        command_CastPower.Disable("CannotOrderNonControlled".Translate());
//                    }
//                    if (newverb.CasterIsPawn)
//                    {
//                        if (newverb.CasterPawn.story.DisabledWorkTypes.Contains(WorkTypeDefOf.Hunting))
//                        {
//                            command_CastPower.Disable("IsIncapableOfViolence".Translate(new object[]
//                            {
//                            newverb.CasterPawn.Name
//                            }));
//                        }
//                        else if (!newverb.CasterPawn.drafter.Drafted)
//                        {
//                            command_CastPower.Disable("IsNotDrafted".Translate(new object[]
//                            {
//                            newverb.CasterPawn.Name
//                            }));
//                        }
//                        else if (!this.ReadyToCast)
//                        {
//                            command_CastPower.Disable("PsykerPowerRecharging".Translate(new object[]
//                                {
//                                newverb.CasterPawn.Name
//                                }));
//                        }
//                    }
//                    yield return command_CastPower;
//                }
//            }
//            yield break;
//        }

//        public bool PsykerHasEquipment(PsykerPowerEntry entry)
//        {
//            if (entry.EquipmentDependent && entry.DependendOn != null)
//            {
//                if (Psyker.equipment.AllEquipmentListForReading.Any(item => item.def == entry.DependendOn))
//                {
//                    return true;
//                }
//                return false;
//            }
//            return true;
//        }

//        public void ExposeData()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
