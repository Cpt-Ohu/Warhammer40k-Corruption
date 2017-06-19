using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using AlienRace;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Log.Message("Generating Patches");
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.ohu.factionColors.main");

            harmony.Patch(AccessTools.Method(typeof(Verse.PawnGraphicSet), "ResolveApparelGraphics", null), new HarmonyMethod(typeof(HarmonyPatches), "ResolveApparelGraphicsOriginal"), null);
            harmony.Patch(AccessTools.Method(typeof(Verse.PawnRenderer), "DrawEquipmentAiming"), new HarmonyMethod(typeof(HarmonyPatches), "DrawEquipmentAimingModded"), null);
            harmony.Patch(AccessTools.Method(typeof(RimWorld.FactionGenerator), "GenerateFactionsIntoWorld"), null, new HarmonyMethod(typeof(HarmonyPatches), "GenerateFactionsIntoWorldPostFix"));
            harmony.Patch(AccessTools.Method(typeof(Verse.Root_Entry), "Update"), new HarmonyMethod(typeof(HarmonyPatches), "UpdatePrefix"), null);
        }

        public static bool UpdatePrefix()
        {
            GraphicDatabase.Clear();
            //GraphicDatabase.DebugLogAllGraphics();
            return true;
        }

        public static void GenerateFactionsIntoWorldPostFix()
        {
            Log.Message("Generating PlayerFaction Story Tracker");
            PlayerFactionStoryTracker corrTracker = (PlayerFactionStoryTracker)WorldObjectMaker.MakeWorldObject(FactionColorsDefOf.PlayerFactionStoryTracker);
            int tile = 0;
            while (!(Find.WorldObjects.AnyWorldObjectAt(tile) || Find.WorldGrid[tile].biome == BiomeDefOf.Ocean))
            {
                tile = Rand.Range(0, Find.WorldGrid.TilesCount);
            }
            corrTracker.Tile = tile;
            Find.WorldObjects.Add(corrTracker);
        }

        public static bool ResolveApparelGraphicsOriginal(PawnGraphicSet __instance)
        {
            __instance.ClearCache();
            __instance.apparelGraphics.Clear();
            List<Apparel> OriginalItems = new List<Apparel>();
            foreach (Apparel current in __instance.pawn.apparel.WornApparelInDrawOrder)
            {
                ApparelGraphicRecord item;
                if (current.GetComp<CompFactionColor>() != null)
                {                    
                    if ((ApparelGraphicGetterFC.TryGetGraphicApparelModded(current, __instance.pawn.story.bodyType, out item)))
                    {
                        if (current.GetComp<ApparelDetailDrawer>() != null && !current.Spawned)
                        {
                            OriginalItems.Add(current);
                        }

                        __instance.apparelGraphics.Add(item);
                    }
                }
                else if (ApparelGraphicRecordGetter.TryGetGraphicApparel(current, __instance.pawn.story.bodyType, out item))
                {
                    __instance.apparelGraphics.Add(item);
                }
            }
            //    Corruption.AfflictionDrawerUtility.DrawChaosOverlays(this.pawn);
            //foreach (Apparel app in OriginalItems)
            //{
            //    ApparelDetailDrawer.DrawDetails(__instance.pawn, app);
            //}
            return false;
        }

        private static ThingDef_AlienRace AlienDefFor(Thing eq, out Pawn pawn)
        {
            ThingWithComps actualThing = eq as ThingWithComps;
            if (actualThing != null)
            {
                CompEquippable tempcomp;
                if ((tempcomp = actualThing.TryGetComp<CompEquippable>()) != null && tempcomp.PrimaryVerb.CasterPawn != null)
                {
                    //         Log.Message("IsGun");
                    pawn = tempcomp.PrimaryVerb.CasterPawn;
                    if (pawn != null)
                    {
                        return pawn.def as ThingDef_AlienRace;
                    }
                }
            }
            pawn = null;
            return null;
        }

        public static bool DrawEquipmentAimingModded(Thing eq, Vector3 drawLoc, float aimAngle)
        {
            Pawn pawn;
            ThingDef_AlienRace alienRaceDef = HarmonyPatches.AlienDefFor(eq, out pawn);
            if (alienRaceDef != null)
            {
                switch(pawn.Rotation.AsInt)
                {
                    case 1:
                        {
                            drawLoc.x += (alienRaceDef.alienRace.generalSettings.alienPartGenerator.CustomDrawSize.x - 1);
                            break;
                        }
                    case 2:
                        {
                            drawLoc.z -= (alienRaceDef.alienRace.generalSettings.alienPartGenerator.CustomDrawSize.x - 1);
                            break;
                        }
                    case 3:
                        {
                            drawLoc.x -= (alienRaceDef.alienRace.generalSettings.alienPartGenerator.CustomDrawSize.x - 1);
                            break;
                        }
                }
            }
            
            float num = aimAngle - 90f;
            Mesh mesh;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
            Material matSingle;
            if (graphic_StackCount != null)
            {
                matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            }
            else
            {
                matSingle = eq.Graphic.MatSingle;
            }

            if (eq.GetType() == typeof(FactionItem))
            {
                FactionItemDef facdef = eq.def as FactionItemDef;
                float scalePawn = alienRaceDef != null ? alienRaceDef.alienRace.generalSettings.alienPartGenerator.CustomDrawSize.x : 1f;
                Vector3 scale = facdef.ItemMeshSize * scalePawn;
                Material Mat = eq.Graphic.MatAt(eq.Rotation);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), scale * 1.2f);
                Graphics.DrawMesh(mesh, matrix, matSingle, 0);

                //                Matrix4x4 matrix = default(Matrix4x4);
                //                matrix.SetTRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), facdef.ItemMeshSize);
                //                Graphics.DrawMesh(mesh, matrix, matSingle, 0);
                //               Graphics.DrawMesh()
                
            }

            else
            {                
                Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);
            }

            return false;
        }
    }
}
