using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    internal static class PawnRendererModded
    {
        public static Func<T, R> GetFieldAccessor<T, R>(string fieldName)
        {
            ParameterExpression param =
                Expression.Parameter(typeof(T), "arg");

            MemberExpression member =
                Expression.Field(param, fieldName);

            LambdaExpression lambda =
                Expression.Lambda(typeof(Func<T, R>), member, param);

            Func<T, R> compiled = (Func<T, R>)lambda.Compile();
            return compiled;
        }

        internal static readonly Func<PawnRenderer, Pawn> _pawnRPI = GetFieldAccessor<PawnRenderer, Pawn>("pawn");
        internal static readonly Func<PawnRenderer, PawnWoundDrawer> _pwdRPI = GetFieldAccessor<PawnRenderer, PawnWoundDrawer>("woundOverlays");

        // RenderPawnInternal detour
        internal static void _RenderPawnInternal(this PawnRenderer _this, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType = RotDrawMode.Fresh, bool portrait = false)
        {
            // Allows accessing private fields from the original class
            Pawn _pawn = _pawnRPI(_this);

            if (!_this.graphics.AllResolved)
            {
                _this.graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;

            // Make sure to render toddlers and babies in bed
            if (renderBody || (_pawn.InBed() && _pawn.ageTracker.CurLifeStageIndex <= 1))
            {
                if (_pawn.RaceProps.Humanlike)
                {
                    if (_pawn.ageTracker.CurLifeStageIndex == 2)
                    {
                        rootLoc.z -= 0.15f;
                    }
                    if (_pawn.ageTracker.CurLifeStageIndex < 2 && _pawn.InBed() && !portrait)
                    {
                        // Undo the offset for babies/toddlers in bed
                        Building_Bed building_bed = _pawn.CurrentBed();
                        Vector3 offset = new Vector3(0, 0, 0.5f).RotatedBy(building_bed.Rotation.AsAngle);
                        rootLoc -= offset;
                    }
                }

                Vector3 loc = rootLoc;

                loc.y += 0.005f;
                if (bodyDrawType == RotDrawMode.Dessicated && !_pawn.RaceProps.Humanlike && _this.graphics.dessicatedGraphic != null && !portrait)
                {
                    _this.graphics.dessicatedGraphic.Draw(loc, bodyFacing, _pawn);
                }
                else
                {
                    if (_pawn.RaceProps.Humanlike)
                    {
                        mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                    }
                    else
                    {
                        mesh = _this.graphics.nakedGraphic.MeshAt(bodyFacing);
                    }
                    // Draw body and apparel
                    List<Material> list = _this.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = _this.graphics.flasher.GetDamagedMat(list[i]);
                        // Scale apparel graphics to fit child body
                        if (_pawn.ageTracker.CurLifeStageIndex == 2 && _pawn.RaceProps.Humanlike)
                        {
                            damagedMat.mainTextureScale = new Vector2(1, 1.3f);
                            damagedMat.mainTextureOffset = new Vector2(0, -0.2f);
                            if (bodyFacing == Rot4.West || bodyFacing == Rot4.East)
                            {
                                damagedMat.mainTextureOffset = new Vector2(-0.015f, -0.2f);
                            }
                        }
                        GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                        loc.y += 0.005f;
                    }
                    if (bodyDrawType == RotDrawMode.Fresh)
                    {
                        Vector3 drawLoc = rootLoc;
                        drawLoc.y += 0.02f;

                        PawnWoundDrawer _woundOverlays = _pwdRPI(_this);
                        _woundOverlays.RenderOverBody(drawLoc, mesh, quat, portrait);
                    }
                }
            }
            Vector3 loc2 = rootLoc;
            Vector3 a = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                a.y += 0.03f;
                loc2.y += 0.025f;
            }
            else
            {
                a.y += 0.025f;
                loc2.y += 0.03f;
            }
            // Does our pawn have a visible head?
            // Only draw head if child or older
            if (_this.graphics.headGraphic != null && _pawn.ageTracker.CurLifeStageIndex >= 1)
            {
                Vector3 b = quat * _this.BaseHeadOffsetAt(headFacing);
                // Hair or helmet location
                Vector3 loc3 = rootLoc + b;
                // Raise it up in the stack
                loc3.y += 0.035f;
                bool flag = false;

                // Is the pawn a child or older?
                if (_pawn.ageTracker.CurLifeStageIndex >= 2)
                {
                    Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                    Material mat = _this.graphics.HeadMatAt(headFacing, bodyDrawType);
                    // Draw the pawn's head
                    GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, mat, portrait);
                    // Find the mesh we want to use for the current direction it's facing
                    Mesh mesh3 = _this.graphics.HairMeshSet.MeshAt(headFacing);
                    // Populate a list of all apparel
                    List<ApparelGraphicRecord> apparelGraphics = _this.graphics.apparelGraphics;
                    for (int j = 0; j < apparelGraphics.Count; j++)
                    {
                        // If the apparel is on the head, let's draw it!
                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                        {
                            if ((!_pawn.story.hairDef.hairTags.Contains("DrawUnderHat") && !_pawn.story.hairDef.hairTags.Contains("Beard")) || _pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.FullHead))
                            {
                                flag = true; // flag=true stops the hair from being drawn
                            }

                            Material material = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                            material = _this.graphics.flasher.GetDamagedMat(material);
                            if (_pawn.ageTracker.CurLifeStageIndex == 2)
                            {
                                material.mainTextureOffset = new Vector2(0, 0.018f);
                                material.mainTexture.wrapMode = TextureWrapMode.Clamp;
                            }
                            GenDraw.DrawMeshNowOrLater(mesh3, loc3 + new Vector3(0, 0.035f, 0), quat, material, portrait);
                        }
                    }
                }
                // Otherwise let's draw the hair instead
                if (!flag && bodyDrawType != RotDrawMode.Dessicated && _pawn.ageTracker.AgeBiologicalYears >= 2)
                {
                    Mesh mesh4 = _this.graphics.HairMeshSet.MeshAt(headFacing);
                    Material mat2 = _this.graphics.HairMatAt(headFacing);

                    // Hopefully stops graphic issues from modifying texture offset/scale
                    mat2.mainTexture.wrapMode = TextureWrapMode.Clamp;

                    // Scale down the child hair to fit the head
                    if (_pawn.ageTracker.CurLifeStageIndex <= 2)
                    {
                        mat2.mainTextureScale = new Vector2(1.13f, 1.13f);
                        mat2.mainTextureOffset = new Vector2(-0.065f, -0.045f);
                    }
                    // Scale down the toddler hair to fit the head
                    if (_pawn.ageTracker.CurLifeStageIndex == 1)
                    {
                        //	mat2.mainTextureScale = new Vector2 (1.25f, 1.25f);
                        mat2.mainTextureOffset = new Vector2(-0.07f, 0.12f);
                    }

                    GenDraw.DrawMeshNowOrLater(mesh4, loc3, quat, mat2, portrait);
                }
            }

            if (renderBody)
            {
                for (int k = 0; k < _this.graphics.apparelGraphics.Count; k++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = _this.graphics.apparelGraphics[k];
                    // Draw the uppermost piece of apparel
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
                    {
                        Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material2 = _this.graphics.flasher.GetDamagedMat(material2);

                        // Draw apparel differently for children
                        if (_pawn.ageTracker.CurLifeStageIndex == 2)
                        {
                            material2.mainTextureScale = new Vector2(1.00f, 1.22f);
                            material2.mainTextureOffset = new Vector2(0, -0.1f);
                        }
                        GenDraw.DrawMeshNowOrLater(mesh, loc2, quat, material2, portrait);
                    }
                }
            }
            if (!portrait)
            {
                //_this.DrawEquipment (rootLoc);
                MethodInfo drawEquip = _this.GetType().GetMethod("DrawEquipment", BindingFlags.NonPublic | BindingFlags.Instance);
                drawEquip.Invoke(_this, new object[] { rootLoc });

                if (_pawn.apparel != null)
                {
                    List<Apparel> wornApparel = _pawn.apparel.WornApparel;
                    for (int l = 0; l < wornApparel.Count; l++)
                    {
                        wornApparel[l].DrawWornExtras();
                    }
                }
                Vector3 bodyLoc = rootLoc;
                bodyLoc.y += 0.045f;

                var _statusOverlays = typeof(PawnRenderer).GetField("statusOverlays", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_this);
                MethodInfo rso = typeof(PawnHeadOverlays).GetMethod("RenderStatusOverlays");
                rso.Invoke(_statusOverlays, new object[] { bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt(headFacing) });
                //_statusOverlays.RenderStatusOverlays (bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt (headFacing));
            }
        }




    }
}
