using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class Apparel_HardShield : Apparel
    {
        [StaticConstructorOnStartup]
        internal class Gizmo_HardShield : Gizmo
        {
            public Apparel_HardShield shield;

            private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.5f, 0.2f, 0.5f, 0.24f));

            private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

            private Rect iconTexCoords = new Rect(0f, 0f, 1f, 1f);

            public Gizmo_HardShield(Apparel_HardShield shield)
            {
                this.shield = shield;
            }

            public override float GetWidth(float maxWidth)
            {
                return 140f;
            }

            public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
            {
                Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
                Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
                {
                    Rect rect = overRect.AtZero().ContractedBy(6f);
                    Rect rectshield = rect;
                    rectshield.height -= overRect.height / 3f;
                    rectshield.yMin -= overRect.height / 3f;
                    Texture2D badTex = this.shield.def.uiIcon;
                    if (badTex == null)
                    {
                        badTex = BaseContent.BadTex;
                    }
                    Widgets.DrawTextureFitted(new Rect(rect), badTex, 0.85f, Vector2.one, this.iconTexCoords);
                    Rect rect2 = rect;
                    rect2.height = overRect.height / 2f;
                    Text.Font = GameFont.Tiny;
                    Widgets.Label(rect2, this.shield.LabelCap);
                    Rect rect3 = rect;
                    rect3.yMin = overRect.height / 2f * 3f;
                    float fillPercent = this.shield.HitPoints / this.shield.MaxHitPoints;
                    Widgets.FillableBar(rect3, fillPercent, Gizmo_HardShield.FullShieldBarTex, Gizmo_HardShield.EmptyShieldBarTex, false);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect3, (this.shield.HitPoints).ToString("F0") + " / " + (this.shield.MaxHitPoints * 100f).ToString("F0"));
                    Text.Anchor = TextAnchor.UpperLeft;
                }, true, false, 1f);
                return new GizmoResult(GizmoState.Clear);
            }
        }

        private CompHardShield cachedComp;

        private Vector3 actDrawPos;

        private FactionItemDef factionItemDef
        {
            get
            {
                return this.def as FactionItemDef;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                return base.Graphic;
            }
        }


        public CompHardShield shieldComp
        {
            get
            {
                if (this.cachedComp == null)
                {
                    this.cachedComp = this.TryGetComp<CompHardShield>();
                }
                return this.cachedComp;
            }
        }


        public override bool CheckPreAbsorbDamage(DamageInfo dinfo)
        {
            float defSkill = this.GetPawnMeleeSkill();
            float sizeFactor = this.shieldComp.CProps.ShieldSizeFactor;
            float sturdiness = this.shieldComp.CProps.ShieldSturdiness;
            int attackerSkill = GetAttackerSkill(dinfo);

            float power = attackerSkill / (defSkill * sizeFactor * sturdiness);
            if (Mathf.Pow(0.5f, power) > Rand.Value)
            {
                this.HitPoints -= (int)(dinfo.Amount / sturdiness);
                return true;

            }

            return false;
        }

        public override void DrawWornExtras()
        {
            if (this.ShouldDraw())
            {
                Vector3 drawLoc = this.DrawPos;
                Vector3 scale = this.factionItemDef != null ? this.factionItemDef.ItemMeshSize : Vector3.one;
                Material Mat = this.Graphic.MatAt(this.Wearer.Rotation);
                Matrix4x4 matrix = default(Matrix4x4);
                Mesh mesh = MeshPool.plane10;
                if (this.Wearer.Rotation == Rot4.North)
                {
                    drawLoc += new Vector3(-0.2f, -1f, 0f);
                }
                else if (this.Wearer.Rotation == Rot4.East)
                {
                    drawLoc += new Vector3(0.2f, -1f, 0f);
                    mesh = MeshPool.plane10Flip;
                }
                else if (this.Wearer.Rotation == Rot4.South)
                {
                    drawLoc += new Vector3(0.2f, 0.1f, 0f);
                }
                matrix.SetTRS(drawLoc, Quaternion.AngleAxis(0, Vector3.up), scale * 1.2f);
                Graphics.DrawMesh(mesh, matrix, Mat, 0);
            }
        }

        private bool ShouldDraw()
        {
            return (this.Wearer.carryTracker == null || this.Wearer.carryTracker.CarriedThing == null) && (this.Wearer.Drafted || (this.Wearer.CurJob != null && this.Wearer.CurJob.def.alwaysShowWeapon) || (this.Wearer.mindState.duty != null && this.Wearer.mindState.duty.def.alwaysShowWeapon));
        }


        private int GetPawnMeleeSkill()
        {
            if (this.Wearer != null)
            {
                return this.Wearer.skills.GetSkill(SkillDefOf.Melee).Level;
            }
            return 1;
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            Gizmo_HardShield gizmo = new Gizmo_HardShield(this);
            yield return gizmo;

        }

        private int GetAttackerSkill(DamageInfo dinfo)
        {
            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker != null)
            {
                if (dinfo.Weapon.IsRangedWeapon)
                {
                    return attacker.skills.GetSkill(SkillDefOf.Shooting).Level;
                }
                else
                {
                    return this.Wearer.skills.GetSkill(SkillDefOf.Melee).Level;
                }
            }
            return 10;
        }
    }
}
