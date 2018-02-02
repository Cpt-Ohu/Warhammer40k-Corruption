using Corruption;
using FactionColors;
using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using AlienRace;


namespace _40KPawnRenderer
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        /// <summary>
        /// Drawing Chaos overlays, hediffs and pauldrons, adjusted for Alien Race
        /// </summary>
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.ohu.pawnRenderer.main");
            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) }), null, new HarmonyMethod(typeof(HarmonyPatches), nameof(RenderPawnInternal)), null);
        }

        internal static List<HediffComp_DrawImplant> implantDrawers(Pawn pawn)
        {
            List<HediffComp_DrawImplant> list = new List<HediffComp_DrawImplant>();
            for (int l = 0; l < pawn.health.hediffSet.hediffs.Count; l++)
            {
                HediffComp_DrawImplant drawer;
                if ((drawer = pawn.health.hediffSet.hediffs[l].TryGetComp<HediffComp_DrawImplant>()) != null)
                {
                    list.Add(drawer);
                }
            }
            return list;
        }

        private static void RenderPawnInternal(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType = RotDrawMode.Fresh, bool portrait = false)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.def.race.Humanlike)
            {
                return;
            }

            Corruption.CompPsyker compPsyker = pawn.TryGetComp<CompPsyker>();
            List<HediffComp_DrawImplant> implantDrawers = HarmonyPatches.implantDrawers(pawn);
            string patronName = "Emperor";
            bool drawChaos = false;
            if (compPsyker != null)
            {
                patronName = compPsyker.patronName;
                //        Log.Message("GettingComp: " + patronName);
                if (patronName == "Khorne" || patronName == "Nurgle" || patronName == "Tzeentch" || patronName == "Undivided")
                {
                    //            Log.Message("drawChaos");
                    drawChaos = true;
                }
            }

            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;
            if (renderBody)
            {
                Vector3 loc = rootLoc;
                loc.y += 0.005f;
                if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && __instance.graphics.dessicatedGraphic != null && !portrait)
                {
                    __instance.graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn);
                }
                else
                {
                    ThingDef_AlienRace alienDef = pawn.def as ThingDef_AlienRace;
                    if (alienDef != null)
                    {
                        Mesh mesh2;
                        if (bodyDrawType == RotDrawMode.Rotting)
                        {
                            if (__instance.graphics.dessicatedGraphic.ShouldDrawRotated)
                            {
                                mesh2 = MeshPool.GridPlane(portrait ? alienDef.alienRace.generalSettings.alienPartGenerator.CustomPortraitDrawSize : alienDef.alienRace.generalSettings.alienPartGenerator.CustomDrawSize);
                            }
                            else
                            {
                                Vector2 size = portrait ? alienDef.alienRace.generalSettings.alienPartGenerator.CustomPortraitDrawSize : alienDef.alienRace.generalSettings.alienPartGenerator.CustomDrawSize;
                                if (bodyFacing.IsHorizontal)
                                {
                                    size = size.Rotated();
                                }
                                if (bodyFacing == Rot4.West && (__instance.graphics.dessicatedGraphic.data == null || __instance.graphics.dessicatedGraphic.data.allowFlip))
                                {
                                    mesh = MeshPool.GridPlaneFlip(size);
                                }
                                mesh = MeshPool.GridPlane(size);
                            }
                        }
                        else
                        {
                            mesh = (portrait ? alienDef.alienRace.generalSettings.alienPartGenerator.bodyPortraitSet.MeshAt(bodyFacing) : alienDef.alienRace.generalSettings.alienPartGenerator.bodySet.MeshAt(bodyFacing));
                        }
                    }
                    else
                    {

                        if (pawn.RaceProps.Humanlike)
                        {
                            mesh = MeshPool.humanlikeBodySet.MeshAt(bodyFacing);
                        }
                        else
                        {
                            mesh = __instance.graphics.nakedGraphic.MeshAt(bodyFacing);
                        }
                    }
                    List<Material> list = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
         //               Material damagedMat = __instance.graphics.flasher.GetDamagedMat(list[i]);
         //               GenDraw.DrawMeshNowOrLater(mesh, loc, quat, damagedMat, portrait);
                        loc.y += 0.005f;
                        if (i == 0)
                        {
                            if (drawChaos)
                            {
                                Material markMat = (Corruption.AfflictionDrawerUtility.GetBodyOverlay(pawn.story.bodyType, patronName).MatAt(bodyFacing));
                                if (pawn.ageTracker.CurLifeStageIndex == 2 && pawn.RaceProps.Humanlike)
                                {
                                    markMat.mainTextureScale = new Vector2(1, 1.3f);
                                    markMat.mainTextureOffset = new Vector2(0, -0.2f);
                                    if (bodyFacing == Rot4.West || bodyFacing == Rot4.East)
                                    {
                                        markMat.mainTextureOffset = new Vector2(-0.015f, -0.2f);
                                    }
                                }
                                GenDraw.DrawMeshNowOrLater(mesh, loc, quat, markMat, portrait);
                                loc.y += 0.005f;
                            }
                        }

                        if (bodyDrawType == RotDrawMode.Fresh)
                        {
                            Vector3 drawLoc = rootLoc;
                            drawLoc.y += 0.02f;
        //                    Traverse.Create(__instance).Field("woundOverlays").GetValue<PawnWoundDrawer>().RenderOverBody(drawLoc, mesh, quat, portrait);
                        }
                    }
                }
                Vector3 vector = rootLoc;
                Vector3 a = rootLoc;
                if (bodyFacing != Rot4.North)
                {
                    a.y += 0.03f;
                    vector.y += 0.0249999985f;
                }
                else
                {
                    a.y += 0.0249999985f;
                    vector.y += 0.03f;
                }
                if (__instance.graphics.headGraphic != null)
                {
                    Vector3 b = quat * __instance.BaseHeadOffsetAt(headFacing);
                    Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
                    Material mat = __instance.graphics.HeadMatAt(headFacing, bodyDrawType);
        //            GenDraw.DrawMeshNowOrLater(mesh2, a + b, quat, mat, portrait);
                    if (drawChaos)
                    {
                        Material headMarkMat = (AfflictionDrawerUtility.GetHeadGraphic(pawn, patronName).MatAt(bodyFacing));
                     //   vector.y += 0.005f;
                        GenDraw.DrawMeshNowOrLater(mesh2, a+b+new Vector3(0f, 0.004f, 0f), quat, headMarkMat, portrait);
                    }

                    Vector3 loc2 = rootLoc + b;
                    loc2.y += 0.035f;
                    bool flag = false;
                    Mesh mesh3 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                    List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;
                    for (int j = 0; j < apparelGraphics.Count; j++)
                    {
                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                        {
                            flag = true;
                            Material material = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                            material = __instance.graphics.flasher.GetDamagedMat(material);
             //               GenDraw.DrawMeshNowOrLater(mesh3, loc2, quat, material, portrait);
                        }

                        if (!flag && bodyDrawType != RotDrawMode.Dessicated)
                        {
                            Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
                            Material mat2 = __instance.graphics.HairMatAt(headFacing);
         //                   GenDraw.DrawMeshNowOrLater(mesh4, loc2, quat, mat2, portrait);
                        }
                    }
                    if (renderBody)
                    {
                        for (int k = 0; k < __instance.graphics.apparelGraphics.Count; k++)
                        {
                            Material pauldronMat;
                            if (CompPauldronDrawer.ShouldDrawPauldron(pawn, __instance.graphics.apparelGraphics[k].sourceApparel, bodyFacing, out pauldronMat))
                            {
                                if (pawn.ageTracker.CurLifeStageIndex == 2)
                                {
                                    pauldronMat.mainTextureScale = new Vector2(1.00f, 1.22f);
                                    pauldronMat.mainTextureOffset = new Vector2(0, -0.1f);
                                }
                                vector.y += 0.035f;
                                GenDraw.DrawMeshNowOrLater(mesh, vector, quat, pauldronMat, portrait);
                            }
                            ApparelGraphicRecord apparelGraphicRecord = __instance.graphics.apparelGraphics[k];
                            if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                            {
                     //           Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                     //           material2 = __instance.graphics.flasher.GetDamagedMat(material2);
                     //           GenDraw.DrawMeshNowOrLater(mesh, vector, quat, material2, portrait);

                            }
                            if (!pawn.Dead)
                            {
                                for (int l = 0; l < pawn.health.hediffSet.hediffs.Count; l++)
                                {
                                    HediffComp_DrawImplant drawer;
                                    if ((drawer = pawn.health.hediffSet.hediffs[l].TryGetComp<HediffComp_DrawImplant>()) != null)
                                    {
                                        if (drawer.implantDrawProps.implantDrawerType != ImplantDrawerType.Head)
                                        {
                                            vector.y += 0.005f;
                                            if (bodyFacing == Rot4.South && drawer.implantDrawProps.implantDrawerType == ImplantDrawerType.Backpack)
                                            {
                                                vector.y -= 0.3f;
                                            }
                                            Material implantMat = drawer.ImplantMaterial(pawn, bodyFacing);
                                            GenDraw.DrawMeshNowOrLater(mesh, vector, quat, implantMat, portrait);
                                            vector.y += 0.005f;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }            
        }
    }
}

