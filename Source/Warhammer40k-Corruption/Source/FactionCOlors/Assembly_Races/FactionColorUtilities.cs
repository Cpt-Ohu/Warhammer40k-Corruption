using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    public static class FactionColorUtilities
    {
        public static Texture2D buttonBanner = ContentFinder<Texture2D>.Get("UI/Buttons/Banner");

        public static Texture2D getColoredBannerTex()
        {
            Texture2D tempMain = (Texture2D)currentPlayerBanner.MatSingle.mainTexture;
            Texture2D tempTint = currentPlayerBanner.MatSingle.GetMaskTexture();

            for (int x=0; x < tempMain.width; x++)
            {
                for (int y=0; y < tempMain.height; y++)
                {
                    if (tempTint.GetPixel(x,y) == Color.red)
                    {
                        tempMain.SetPixel(x, y, tempMain.GetPixel(x, y) * currentPlayerBanner.color);
                    }
                    if (tempTint.GetPixel(x, y) == Color.green)
                    {
                        tempMain.SetPixel(x, y, tempMain.GetPixel(x, y) * currentPlayerBanner.ColorTwo);
                    }
                }
            }
            return tempMain;
        }

        public static Graphic currentPlayerBanner
        {
            get
            {                
                return GraphicDatabase.Get<Graphic_Single>(currentPlayerStoryTracker.BannerGraphicPath, ShaderDatabase.CutoutComplex, Vector2.one, currentPlayerStoryTracker.PlayerColorOne, currentPlayerStoryTracker.PlayerColorTwo);
            }           

        }

        public static PlayerFactionStoryTracker currentPlayerStoryTracker
        {
            get
            {
                return Find.WorldObjects.AllWorldObjects.FirstOrDefault(x => x.def == FactionColorsDefOf.PlayerFactionStoryTracker) as PlayerFactionStoryTracker;
            }
        }

        public static void DrawFactionColorTab(Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(rect.x, rect.y + 20f, 250f, 55f);
            Text.Font = GameFont.Medium;
            Widgets.Label(rect2, "PlayerHeraldry".Translate(new object[]
            {
                Faction.OfPlayer.Name
            }));
            Text.Font = GameFont.Small;

            Rect rect4 = new Rect(rect.width - 205f, rect2.yMax + 5f, 200f, 200f);
            Rect rect5 = new Rect(rect2.x, rect2.height + 5f, 150f, 30f);
            Rect pos = new Rect(rect5.x + 150f, rect5.y, 15f, 15f);
            Widgets.Label(rect5, "PlayerColorOne".Translate());
            Widgets.DrawRectFast(pos, FactionColorUtilities.currentPlayerStoryTracker.PlayerColorOne);
            if (Widgets.ButtonInvisible(pos, true))
            {
                ShowColorDialog(FactionColorUtilities.currentPlayerStoryTracker.PlayerColorOne, false);
            }

            Rect rect6 = rect5;
            rect6.y += rect5.height + 5f;
            Rect pos2 = new Rect(rect6.x + 150f, rect6.y, 15f, 15f);
            Widgets.Label(rect6, "PlayerColorTwo".Translate());
            Widgets.DrawRectFast(pos2, FactionColorUtilities.currentPlayerStoryTracker.PlayerColorTwo);
            if (Widgets.ButtonInvisible(pos2, true))
            {
                ShowColorDialog(FactionColorUtilities.currentPlayerStoryTracker.PlayerColorTwo, true);
            }
            //        Texture2D bannerTint = ContentFinder<Texture2D>.Get(FactionColorUtilities.currentPlayerStoryTracker.BannerGraphicPath);
            Texture2D bannerTint = currentPlayerBanner.MatSingle.GetMaskTexture();
            Rect rect7 = rect6;
            rect7.y += rect6.height + 5f;
            Rect pos3 = new Rect(rect7.x + 150f, rect7.y, 15f, 15f);
            Widgets.Label(rect7, "BannerType".Translate());
            if (Widgets.ButtonImage(pos3, bannerTint))
            {
                FactionColorUtilities.ShowBannerDialog();
            }

            Text.Font = GameFont.Small;
            RenderTexture image = new RenderTexture(64, 64, 24);
            Rect heraldryRect = new Rect(rect.width - 100f, rect.y + 50f, 64f, 64f);
            //         Texture2D bannerTex = FactionColorUtilities.getColoredBannerTex();
  //          GUI.color = currentPlayerBanner.color;
            GUI.DrawTexture(heraldryRect, bannerTint);
            GUI.color = Color.white;

            Vector3 loc = new Vector3(UI.screenWidth / 2f, 50f, UI.screenHeight / 2f);
            loc.y += 50f;
            Mesh mesh = currentPlayerBanner.MeshAt(Rot4.North);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(loc, Quaternion.AngleAxis(0f, Vector3.up), new Vector3(1f,1f,1f));
            Graphics.DrawMesh(mesh, matrix, currentPlayerBanner.MatSingle, 0);
            GUI.EndGroup();
        }

        private static void ShowBannerDialog()
        {
            List<string> options = FactionColorUtilities.currentPlayerStoryTracker.BannerOptions;
            Dialog_ChooseBanner dialog_Options = new Dialog_ChooseBanner(options);
            Find.WindowStack.Add(dialog_Options);
        }

        private static void ShowColorDialog(Color color, bool IsSecondaryColor)
        {
            Dialog_ChooseColor dialog_chooseColor = new Dialog_ChooseColor(color, IsSecondaryColor);
            Find.WindowStack.Add(dialog_chooseColor);
        }

        private static Texture2D GetBannerTint(string path)
        {
            return FactionColorUtilities.currentPlayerBanner.MatSingle.GetMaskTexture();
        }
    }
}