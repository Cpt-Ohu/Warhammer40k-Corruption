using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Verse;
using RimWorld;
using FactionColors;
using Corruption;
using AlienRace;
using UnityEngine;
using System.Reflection;
using Harmony;

namespace _40KPawnRenderer
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.ohu._40kRenderer.main");

            harmony.Patch(AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool) }), new HarmonyMethod(typeof(HarmonyPatches), nameof(_RenderPawnInternal)), null);

        }

        internal static void ResolveAgeGraphics(PawnGraphicSet graphics)
        {
            // Beards
            String beard = "";
            if (graphics.pawn.story.hairDef.hairTags.Contains("Beard"))
            {
                if (graphics.pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.UpperHead))
                {
                    beard = "_BeardOnly";
                }
                if (graphics.pawn.ageTracker.CurLifeStageIndex <= 3)
                {
                    graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(DefDatabase<HairDef>.GetNamed("Mop").texPath, ShaderDatabase.Cutout, Vector2.one, graphics.pawn.story.hairColor);
                }
                else
                    graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(graphics.pawn.story.hairDef.texPath + beard, ShaderDatabase.Cutout, Vector2.one, graphics.pawn.story.hairColor);
            }
            else
                graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(graphics.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, graphics.pawn.story.hairColor);

            // Reroute the graphics for children
            // For babies and toddlers
            if (graphics.pawn.ageTracker.CurLifeStageIndex <= 1)
            {
                string toddler_hair = "Boyish";
                if (graphics.pawn.gender == Gender.Female)
                {
                    toddler_hair = "Girlish";
                }
                graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Children/Hairs/Child_" + toddler_hair, ShaderDatabase.Cutout, Vector2.one, graphics.pawn.story.hairColor);
                graphics.headGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/null", ShaderDatabase.Cutout, Vector2.one, Color.white);
            }
            // The pawn is a baby
            if (graphics.pawn.ageTracker.CurLifeStageIndex == 0)
            {
                graphics.nakedGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Humanlike/Children/Bodies/Newborn", ShaderDatabase.CutoutSkin, Vector2.one, graphics.pawn.story.SkinColor);
            }
            // The pawn is a toddler
            if (graphics.pawn.ageTracker.CurLifeStageIndex == 1)
            {
                string upright = "";
                if (graphics.pawn.ageTracker.AgeBiologicalYears >= 2)
                {
                    upright = "Upright";
                }
                graphics.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Children/Bodies/Toddler" + upright, ShaderDatabase.CutoutSkin, Vector2.one, graphics.pawn.story.SkinColor);
            }
            // The pawn is a child
            else if (graphics.pawn.ageTracker.CurLifeStageIndex == 2)
            {
  //              graphics.nakedGraphic = ChildGraphics.GetChildBodyGraphics(graphics, ShaderDatabase.CutoutSkin, graphics.pawn.story.SkinColor);
  //              graphics.headGraphic = ChildGraphics.GetChildHeadGraphics(ShaderDatabase.CutoutSkin, graphics.pawn.story.SkinColor);
            }
            // Otherwise, just use the normal methods
            else if (graphics.pawn.ageTracker.CurLifeStageIndex >= 3)
            {
                graphics.nakedGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(graphics.pawn.story.bodyType, ShaderDatabase.CutoutSkin, graphics.pawn.story.SkinColor);
                graphics.headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(graphics.pawn.story.HeadGraphicPath, graphics.pawn.story.SkinColor);
            }

            graphics.rottingGraphic = GraphicGetter_NakedHumanlike.GetNakedBodyGraphic(graphics.pawn.story.bodyType, ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor);
            graphics.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/HumanoidDessicated", ShaderDatabase.Cutout);
            graphics.desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(graphics.pawn.story.HeadGraphicPath, PawnGraphicSet.RottingColor);
            graphics.skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
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

        internal static void _ResolveAllGraphics(this PawnGraphicSet _this)
        {
            _this.ClearCache();
            if (_this.pawn.RaceProps.Humanlike)
            {
                ResolveAgeGraphics(_this);
                _this.ResolveApparelGraphics();
            }
            else
            {
                PawnKindLifeStage curKindLifeStage = _this.pawn.ageTracker.CurKindLifeStage;
                if (_this.pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
                {
                    _this.nakedGraphic = curKindLifeStage.bodyGraphicData.Graphic;
                }
                else
                {
                    _this.nakedGraphic = curKindLifeStage.femaleGraphicData.Graphic;
                }
                _this.rottingGraphic = _this.nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, PawnGraphicSet.RottingColor, PawnGraphicSet.RottingColor);
                if (curKindLifeStage.dessicatedBodyGraphicData != null)
                {
                    _this.dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(_this.pawn);
                }
            }
        }

        // My own methods
        internal static Graphic GetChildHeadGraphics(Shader shader, Color skinColor)
        {
            string str = "Male_Child";
            string path = "Things/Pawn/Humanlike/Children/Heads/" + str;
            return GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor);
        }
        internal static Graphic GetChildBodyGraphics(this PawnGraphicSet _this, Shader shader, Color skinColor)
        {
            string str = "Naked_Boy";
            if (_this.pawn.gender == Gender.Female)
            {
                str = "Naked_Girl";
            }
            string path = "Things/Pawn/Humanlike/Children/Bodies/" + str;
            return GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor);
        }

        // ResolveApparelGraphics Detour
        internal static void _ResolveApparelGraphics(this PawnGraphicSet _this)
        {
            // Updates the beard
            if (_this.pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.UpperHead))
            {
                ResolveAgeGraphics(_this);
            }

            _this.ClearCache();
            _this.apparelGraphics.Clear();

            BodyType btype = BodyType.Male;

            if (_this.pawn.ageTracker.CurLifeStageIndex == 2)
            {
                btype = BodyType.Thin;
            }

            if (_this.pawn.apparel.WornApparelCount > 0)
            {
                foreach (Apparel current in _this.pawn.apparel.WornApparelInDrawOrder)
                {
                    ApparelGraphicRecord item;
                    if (current.AllComps.Any(i => i.GetType() == typeof(CompFactionColor)))
                    {
                        if (ApparelGraphicGetterFC.TryGetGraphicApparelModded(current, btype, out item))
                        {
                            _this.apparelGraphics.Add(item);
                        }
                    }
                    else if (ApparelGraphicRecordGetter.TryGetGraphicApparel(current, _this.pawn.story.bodyType, out item))
                    {
                        _this.apparelGraphics.Add(item);
                    }
                }
            }
        }


        private static FieldInfo pawnInfo = typeof(PawnRenderer).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo woundInfo = typeof(PawnRenderer).GetField("woundOverlays", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo statusInfo = typeof(PawnRenderer).GetField("statusOverlays", BindingFlags.Instance | BindingFlags.NonPublic);

        private static MethodInfo equipInfo = typeof(PawnRenderer).GetMethod("DrawEquipment", BindingFlags.Instance | BindingFlags.NonPublic);

        public static Dictionary<Vector2, GraphicMeshSet[]> meshPools = new Dictionary<Vector2, GraphicMeshSet[]>();

        public static void __RenderPawnInternalPostFix(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType = 0, bool portrait = false)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            Vector3 vector = rootLoc;
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


 
        }


        public static bool _RenderPawnInternal(PawnRenderer __instance, Vector3 rootLoc, Quaternion quat, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType = 0, bool portrait = false)
        {
            Log.Message("Calling");
            PawnRenderer renderer = __instance;
            if (!renderer.graphics.AllResolved)
            {
                renderer.graphics.ResolveAllGraphics();
            }
            Mesh mesh = null;
            Pawn pawn = (Pawn)HarmonyPatches.pawnInfo.GetValue(renderer);
            if (pawn is AlienPawn && (pawn as AlienPawn).bodySet == null)
            {
                (pawn as AlienPawn).UpdateSets();
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

                if (renderBody || (pawn.InBed() && pawn.ageTracker.CurLifeStageIndex <= 1))
            {
                
                if (pawn.RaceProps.Humanlike)
                {
                    if (pawn.ageTracker.CurLifeStageIndex == 2)
                    {
                        rootLoc.z -= 0.15f;
                    }
                    if (pawn.ageTracker.CurLifeStageIndex < 2 && pawn.InBed() && !portrait)
                    {
                        // Undo the offset for babies/toddlers in bed
                        Building_Bed building_bed = pawn.CurrentBed();
                        Vector3 offset = new Vector3(0, 0, 0.5f).RotatedBy(building_bed.Rotation.AsAngle);
                        rootLoc -= offset;
                    }
                }                
                Vector3 vector = rootLoc;
                vector.y += 0.005f;
                
                if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && renderer.graphics.dessicatedGraphic != null && !portrait)
                {
                    renderer.graphics.dessicatedGraphic.Draw(vector, bodyFacing, pawn);
                }
                else
                {
                    bool humanlike = pawn.RaceProps.Humanlike;
                    if (humanlike)
                    {
                        mesh = ((pawn is AlienPawn) ? (pawn as AlienPawn).bodySet : MeshPool.humanlikeBodySet).MeshAt(bodyFacing);
                    }
                    else
                    {
                        mesh = renderer.graphics.nakedGraphic.MeshAt(bodyFacing);
                    }
                    
                    List<Material> list = renderer.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Material damagedMat = renderer.graphics.flasher.GetDamagedMat(list[i]);
                        if (pawn.ageTracker.CurLifeStageIndex == 2 && pawn.RaceProps.Humanlike)
                        {
                            damagedMat.mainTextureScale = new Vector2(1, 1.3f);
                            damagedMat.mainTextureOffset = new Vector2(0, -0.2f);
                            if (bodyFacing == Rot4.West || bodyFacing == Rot4.East)
                            {
                                damagedMat.mainTextureOffset = new Vector2(-0.015f, -0.2f);
                            }
                        }

                        GenDraw.DrawMeshNowOrLater(mesh, vector, quat, damagedMat, portrait);
                        vector.y += 0.003f;
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
                                GenDraw.DrawMeshNowOrLater(mesh, vector, quat, markMat, portrait);
                                vector.y += 0.003f;
                            }
                        }
                    }
                    
                    if (bodyDrawType == 0)
                    {
                        Vector3 vector2 = rootLoc;
                        vector2.y += 0.02f;
                        ((PawnWoundDrawer)HarmonyPatches.woundInfo.GetValue(renderer)).RenderOverBody(vector2, mesh, quat, portrait);
                    }
                }
            }
            Vector3 vector3 = rootLoc;
            Vector3 vector4 = rootLoc;
            if (bodyFacing != Rot4.North)
            {
                vector4.y += 0.03f;
                vector3.y += 0.0249999985f;
            }
            else
            {
                vector4.y += 0.0249999985f;
                vector3.y += 0.03f;
            }
            
            if (renderer.graphics.headGraphic != null && pawn.ageTracker.CurLifeStageIndex >= 1)
            {
                Vector3 vector5 = quat * renderer.BaseHeadOffsetAt(headFacing);
                Mesh mesh2 = ((pawn is AlienPawn) ? (pawn as AlienPawn).headSet : MeshPool.humanlikeHeadSet).MeshAt(headFacing);
                Material material = renderer.graphics.HeadMatAt(headFacing, bodyDrawType);
                GenDraw.DrawMeshNowOrLater(mesh2, vector4 + vector5, quat, material, portrait);

                if (drawChaos)
                {
                    Material headMarkMat = (AfflictionDrawerUtility.GetHeadGraphic(pawn, patronName).MatAt(bodyFacing));
                    vector5.y += 0.005f;
                    GenDraw.DrawMeshNowOrLater(mesh2, vector4 + vector5, quat, headMarkMat, portrait);
                }

                Vector3 vector6 = rootLoc + vector5;
                vector6.y += 0.035f;
                bool flag7 = false;
                if (pawn.ageTracker.CurLifeStageIndex >= 2)
                {
                        Mesh mesh3 = HarmonyPatches.HairMeshSet(pawn).MeshAt(headFacing);
                        List<ApparelGraphicRecord> apparelGraphics = renderer.graphics.apparelGraphics;
                    for (int j = 0; j < apparelGraphics.Count; j++)
                    {
                        if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                        {
                            if ((!pawn.story.hairDef.hairTags.Contains("DrawUnderHat") && !pawn.story.hairDef.hairTags.Contains("Beard")) || pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.FullHead))
                            {
                                flag7 = true; // flag=true stops the hair from being drawn
                            }
                            Material material2 = apparelGraphics[j].graphic.MatAt(bodyFacing, null);
                            material2 = renderer.graphics.flasher.GetDamagedMat(material2);
                            if (pawn.ageTracker.CurLifeStageIndex == 2)
                            {
                                material2.mainTextureOffset = new Vector2(0, 0.018f);
                                material2.mainTexture.wrapMode = TextureWrapMode.Clamp;
                            }
                            GenDraw.DrawMeshNowOrLater(mesh3, vector6, quat, material2, portrait);
                            Material detailMat;
                            if (ApparelDetailDrawer.GetDetailGraphic(pawn, apparelGraphics[j].sourceApparel, bodyFacing, out detailMat))
                            {
                                Vector3 vectorDet = vector6;
                                vectorDet.y += 0.005f;
                                GenDraw.DrawMeshNowOrLater(mesh3, vectorDet, quat, detailMat, portrait);
                            }

                        }
                    }
                }
                
                if (!flag7 && bodyDrawType != RotDrawMode.Dessicated)
                {
                    Mesh mesh4 = HarmonyPatches.HairMeshSet(pawn).MeshAt(headFacing);
                    Material material3 = renderer.graphics.HairMatAt(headFacing);
                        // Hopefully stops graphic issues from modifying texture offset/scale
                        material3.mainTexture.wrapMode = TextureWrapMode.Clamp;

                        // Scale down the child hair to fit the head
                        if (pawn.ageTracker.CurLifeStageIndex <= 2)
                        {
                            material3.mainTextureScale = new Vector2(1.13f, 1.13f);
                            material3.mainTextureOffset = new Vector2(-0.065f, -0.045f);
                        }
                        // Scale down the toddler hair to fit the head
                        if (pawn.ageTracker.CurLifeStageIndex == 1)
                        {
                            //	mat2.mainTextureScale = new Vector2 (1.25f, 1.25f);
                            material3.mainTextureOffset = new Vector2(-0.07f, 0.12f);
                        }
                        GenDraw.DrawMeshNowOrLater(mesh4, vector6, quat, material3, portrait);
                }
            }
            
            if (renderBody)
            {
                for (int k = 0; k < renderer.graphics.apparelGraphics.Count; k++)
                {
                    ApparelGraphicRecord apparelGraphicRecord = renderer.graphics.apparelGraphics[k];
                    if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayer.Shell)
                    {
                        Material material4 = apparelGraphicRecord.graphic.MatAt(bodyFacing, null);
                        material4 = renderer.graphics.flasher.GetDamagedMat(material4);
                        if (pawn.ageTracker.CurLifeStageIndex == 2)
                        {
                            material4.mainTextureScale = new Vector2(1.00f, 1.22f);
                            material4.mainTextureOffset = new Vector2(0, -0.1f);
                        }

                        GenDraw.DrawMeshNowOrLater(mesh, vector3, quat, material4, portrait);
                    }
                    Material pauldronMat;
                    if (CompPauldronDrawer.ShouldDrawPauldron(pawn, renderer.graphics.apparelGraphics[k].sourceApparel, bodyFacing, out pauldronMat))
                    {
                        if (pawn.ageTracker.CurLifeStageIndex == 2)
                        {
                            pauldronMat.mainTextureScale = new Vector2(1.00f, 1.22f);
                            pauldronMat.mainTextureOffset = new Vector2(0, -0.1f);
                        }
                        vector3.y += 0.005f;
                        GenDraw.DrawMeshNowOrLater(mesh, vector3, quat, pauldronMat, portrait);
                    }
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
                                vector3.y += 0.005f;
                                if (bodyFacing == Rot4.South && drawer.implantDrawProps.implantDrawerType == ImplantDrawerType.Backpack)
                                {
                                    vector3.y -= 0.3f;
                                }
                                Material implantMat = drawer.ImplantMaterial(pawn, bodyFacing);
                                GenDraw.DrawMeshNowOrLater(mesh, vector3, quat, implantMat, portrait);
                                vector3.y += 0.005f;
                            }
                        }
                    }
                }
            }
            
            if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count() > 0)
            {
                Graphics.DrawMesh(mesh, vector3, quat, renderer.graphics.packGraphic.MatAt(pawn.Rotation, null), 0);
            }
            if (!portrait)
            {
                HarmonyPatches.equipInfo.Invoke(renderer, new object[]
                {
                    rootLoc
                });
                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int l = 0; l < wornApparel.Count; l++)
                    {
                        wornApparel[l].DrawWornExtras();
                    }
                }
                Vector3 vector7 = rootLoc;
                vector7.y += 0.0449999981f;
                ((PawnHeadOverlays)HarmonyPatches.statusInfo.GetValue(renderer)).RenderStatusOverlays(vector7, quat, ((pawn is AlienPawn) ? (pawn as AlienPawn).headSet : MeshPool.humanlikeHeadSet).MeshAt(headFacing));
            }
            return false;
        }

        private static GraphicMeshSet HairMeshSet(Pawn pawn)
        {
            CrownType crownType = pawn.story.crownType;
            bool flag = crownType == CrownType.Average;
            GraphicMeshSet result;
            if (flag)
            {
                result = ((pawn is AlienPawn) ? (pawn as AlienPawn).hairSetAverage : MeshPool.humanlikeHairSetAverage);
            }
            else
            {
                if (crownType == CrownType.Narrow)
                {
                    result = ((pawn is AlienPawn) ? (pawn as AlienPawn).hairSetNarrow : MeshPool.humanlikeHairSetNarrow);
                }
                else
                {
                    Log.Error("Unknown crown type: " + crownType);
                    result = ((pawn is AlienPawn) ? (pawn as AlienPawn).hairSetAverage : MeshPool.humanlikeHairSetAverage);
                }
            }
            return result;
        }


        private static void DrawCorruptionBodyOverlay(Pawn pawn, Rot4 bodyFacing, List<Material> listIn, out List<Material> listOut)
        {
            List<Material> list = listIn;
            Need_Soul soul = pawn.needs.TryGetNeed<Need_Soul>();
            if (soul != null)
            {
                string patronName = soul.patronInfo.PatronName;
                if (!soul.NoPatron && patronName != "Slaanesh")
                {
                    list.Add(AfflictionDrawerUtility.GetHeadGraphic(pawn, patronName).MatAt(bodyFacing));
                    list.Add(AfflictionDrawerUtility.GetBodyOverlay(pawn.story.bodyType, patronName).MatAt(bodyFacing));
                }
            }
            listOut = list;
        }


    }
}
