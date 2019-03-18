using Corruption.IoM;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption.ProductionSites
{
    [StaticConstructorOnStartup]
    public class WITab_ProductionSite_Production : WITab
    {
        public static Texture2D ConstructTex { get; private set; }

        private static Color ExpansionColor = new Color(0.53f, 0.81f, 1f);

        private const float TopPadding = 20f;

        private static Color GroupboxColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

        private static Color BorderColor = new ColorInt(135, 135, 135).ToColor;
        
        private const float ThingIconSize = 28f;

        private const string stockFormat = "{0,10}{1,100}";

        private List<Thing> cachedItems = new List<Thing>();

        private WorkForceWidget WorkerWidget;

        private Dictionary<WorkForce, Pawn> ForemanLookup = new Dictionary<WorkForce, Pawn>();

        private List<UpgradeBuildProgress> builtUpgradesCached;

        private ResourceProductionComp selectedProduction;

        public ResourceProductionComp SelectedProduction
        {
            get { return selectedProduction; }
            set
            {
                selectedProduction = value;
                this.WorkerWidget.SetProduction(value);
                this.builtUpgradesCached = value.Upgrades;
            }
        }

        private Def defToShow;
        public Def DefToShow
        {
            get
            {
                return this.defToShow;
            }

            set
            {
                this.defToShow = value;
            }
        }

        private ProductionSiteLevelDef SelectedLevel
        {
            get
            {
                return this.SelectedProduction.SiteLevel;
            }
        }

        private ProductionSiteLevelDef NextLevel
        {
            get
            {
                return (this.SelectedProduction.Props.Levels.FirstOrDefault(x => x.Level > this.SelectedLevel?.Level));
            }
        }

        private ProductionSiteUpgradeDef SelectedUpgrade
        {
            get
            {
                return DefToShow as ProductionSiteUpgradeDef;
            }
        }

        private const float ThingRowHeight = 28f;

        private const float ThingLeftX = 36f;

        private const float StandardLineHeight = 22f;

        private Vector2 scrollPositionProduction = Vector2.zero;

        private Vector2 scrollPositionUpgrade = Vector2.zero;

        private List<ResourceProductionComp> cachedAltProduction;

        private static readonly Color ThingLabelColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        private static List<Thing> workingInvList = new List<Thing>();

        public ProductionSite ProductionSite
        {
            get
            {
                return (ProductionSite)this.SelObject;
            }
        }

        private bool CanControl
        {
            get
            {
                return this.ProductionSite.Faction == Faction.OfPlayer;
            }
        }

        public WITab_ProductionSite_Production()
        {
            this.size = new Vector2(1200f, 700f);
            this.labelKey = "TabProductionSite";
        }

        private void CacheProduction()
        {
            this.cachedAltProduction = this.ProductionSite.AllProduction.Where(x => x != this.ProductionSite.MainProduction).ToList();
        }

        private void CacheForemen()
        {
            foreach (var production in this.ProductionSite.AllProduction)
            {
                foreach (var workforce in production.WorkForce)
                {
                    Pawn foreman = PawnGenerator.GeneratePawn(workforce.PawnKind, this.ProductionSite.Faction);
                    this.ForemanLookup.Add(workforce, foreman);
                }
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.CacheProduction();
            this.WorkerWidget = new WorkForceWidget(this.ProductionSite);
            this.SelectedProduction = this.ProductionSite.MainProduction;
            this.DefToShow = this.ProductionSite.MainProduction.SiteLevel;
            this.builtUpgradesCached = this.ProductionSite.MainProduction.Upgrades;
        }

        private void DrawPoductionList(Rect inRect)
        {
            Rect mainProductionRect = new Rect(0, 0, 116f, 124f);
            DrawProductionSelector(mainProductionRect, this.ProductionSite.MainProduction);
            Widgets.DrawLineHorizontal(0f, mainProductionRect.yMax + 2f, mainProductionRect.width);
            Rect outRect = new Rect(0f, mainProductionRect.yMax + 6f, Storyteller.PortraitSizeTiny.x + 16f, inRect.height);
            Rect viewRect = new Rect(0f, mainProductionRect.yMax + 6f, Storyteller.PortraitSizeTiny.x, (float)(this.ProductionSite.AllProduction.Count() - 1) * (Storyteller.PortraitSizeTiny.y + 10f));
            Widgets.BeginScrollView(outRect, ref scrollPositionProduction, viewRect, true);
            float curY = outRect.yMin;
            for (int i = 1; i < this.cachedAltProduction.Count; i++)
            {
                ResourceProductionComp production = this.cachedAltProduction[i];
                Rect productionRect = new Rect(outRect.x, curY, ResourceProductionComp.IconSizeTiny.x, ResourceProductionComp.IconSizeTiny.y);
                DrawProductionSelector(productionRect, production);
                curY += ResourceProductionComp.IconSizeTiny.y + 4f;
            }

            Widgets.EndScrollView();

        }

        private void DrawProductionSelector(Rect inRect, ResourceProductionComp productionComp)
        {
            Rect iconRect = inRect.ContractedBy(14f);
            GUI.DrawTexture(iconRect, productionComp.SiteLevel.uiIcon);
            if (Widgets.ButtonInvisible(inRect))
            {
                this.SelectedProduction = productionComp;
                this.DefToShow = productionComp.SiteLevel;
            }
            if (this.SelectedProduction == productionComp)
            {
                GUI.DrawTexture(inRect, CorruptionStoryTrackerUtilities.SelectorHighlightTex);
            }
        }

        private void DrawProductionDetails(Rect inRect)
        {
            this.DrawMap(inRect);
            //    Rect innerRect = inRect.ContractedBy(2f);
            //    GUI.BeginGroup(innerRect);

            //    float curY = this.DrawHeader(innerRect.width);

            //    Rect stockRect = new Rect(0f, curY, 156f, innerRect.yMax - curY);
            //    Widgets.DrawBox(stockRect, 2);
            //    Rect stockRectInner = stockRect.ContractedBy(2f);
            //    Text.Font = GameFont.Medium;
            //    Widgets.Label(stockRectInner, "ProductionOutput".Translate());
            //    Text.Font = GameFont.Small;
            //    float num = stockRectInner.y + 18f;
            //    foreach (var stock in this.SelectedProduction.ProductionOptions.OrderBy(x => x.yieldFactor))
            //    {
            //        this.DrawProductionRow(ref num, stockRectInner.width, stock);
            //    }

            //    Rect upgradeRect = new Rect(stockRect.xMax + 8f, stockRect.y, innerRect.width - stockRect.xMax, (stockRect.height / 2f) - 2f);
            //    Widgets.DrawBox(upgradeRect, 2);
            //    Rect innerUpgradeRect = upgradeRect.ContractedBy(2f);
            //    this.DrawUpgrades(upgradeRect);
            //    Rect workForceRect = new Rect(upgradeRect);
            //    workForceRect.y = upgradeRect.yMax + 2f;
            //    Widgets.DrawBox(workForceRect);
            //    Rect workForceRectInner = new Rect(workForceRect);
            //    this.DrawWorkForce(workForceRectInner);

            //    GUI.EndGroup();

        }

        private void DrawMap(Rect inRect)
        {
            GUI.BeginGroup(inRect);
            Rect siteGUIRect = new Rect(0f, 0f, 765f, 525f);
            Widgets.DrawMenuSection(siteGUIRect);
            Rect mapRect = new Rect(124f, 124f, 640f, 400f);
            GUI.DrawTexture(mapRect, this.SelectedLevel.SiteTexture);
            //Rect centralRect = new Rect((inRect.width - SelectedLevel.uiIconSize.x) / 2f, (inRect.height - SelectedLevel.uiIconSize.y) / 2f, SelectedLevel.uiIconSize.x, SelectedLevel.uiIconSize.y);
            //GUI.DrawTexture(centralRect, this.SelectedProduction.SiteLevel.uiIcon);
            if (Widgets.ButtonInvisible(mapRect))
            {
                this.DefToShow = this.SelectedLevel;
            }

            DrawUpgradeIcons();

            GUI.color = WITab_ProductionSite_Production.BorderColor;
            Widgets.DrawLineHorizontal(124f, 124f, 640f);
            Widgets.DrawLineVertical(124f, 124f, 400f);
            GUI.color = Color.white;

            Rect infoRect = new Rect(0f, mapRect.yMax + 2f, inRect.width, 150f);
            Rect innerInfoRect = infoRect.ContractedBy(4f);
            GUI.BeginGroup(innerInfoRect);
            if (this.SelectedUpgrade != null)
            {
                this.DrawUpgradeInfo(innerInfoRect);
            }
            else
            {
                this.DrawLeveLinfo(innerInfoRect);
            }

            if (this.ProductionSite.IsPlayerControlled && (this.NextLevel != null || (!this.builtUpgradesCached.Any(x => x.Def == this.SelectedUpgrade))))
            {
                Rect buttonRect = new Rect(inRect.width / 2 - 64f, innerInfoRect.height - 36f, 128f, 32f);
                string buttonText = this.SelectedUpgrade != null ? "BuildUpgradeCommand".Translate() : "ExpandProduction".Translate();
                if (Widgets.ButtonText(buttonRect, buttonText))
                {
                    if (this.SelectedUpgrade != null)
                    {
                        Find.WindowStack.Add(new Dialog_BuildUpgrade(this.ProductionSite, this.SelectedProduction, this.SelectedUpgrade));
                    }
                    else
                    {
                        Find.WindowStack.Add(new Dialog_ExpandSiteLevel(this.ProductionSite, this.SelectedProduction, this.NextLevel, this.SelectedLevel));
                    }
                }
            }
            GUI.EndGroup();
            GUI.EndGroup();
        }

        private void DrawUpgradeIcons()
        {
            foreach (var upgrade in this.SelectedProduction.AvailableUpgrades)
            {
                Rect upgradeRect = new Rect(upgrade.DrawPos, upgrade.DrawSize);
                if (this.builtUpgradesCached.Any(x => x.Def == upgrade))
                {
                    GUI.DrawTexture(upgradeRect, upgrade.Icon);
                }
                else if (this.DefToShow == upgrade || upgrade.DrawPos.x == 0f || upgrade.DrawPos.y == 400f)
                {
                    GenUI.DrawTextureWithMaterial(upgradeRect, upgrade.Icon, TexUI.GrayscaleGUI, default(Rect));
                }
                if (this.ProductionSite.IsPlayerControlled)
                {
                    if (Widgets.ButtonInvisible(upgradeRect))
                    {
                        this.DefToShow = upgrade;
                    }
                    Vector2 selectorPos = new Vector2(upgradeRect.x + (upgradeRect.width - 24f) / 2f, upgradeRect.y + (upgradeRect.height - 24f) / 2f);

                    if (Widgets.RadioButton(selectorPos, upgrade == this.DefToShow))
                    {
                        this.DefToShow = upgrade;
                    }
                }
            }
        }

        private void DrawLeveLinfo(Rect inRect)
        {
            float curY = this.DrawInfoHeader(inRect.width);
            Rect productionRect = new Rect(0f, curY, inRect.width, inRect.height - curY);
            this.DrawProductionEffects(productionRect);
        }

        private void DrawUpgradeInfo(Rect inRect)
        {
            GUI.color = this.builtUpgradesCached.Any(x => x.Def == this.SelectedUpgrade) ? Color.white : WITab_ProductionSite_Production.ExpansionColor;
            float curY = this.DrawInfoHeader(inRect.width);
            Rect effectRect = new Rect(0f, curY, inRect.width, Text.LineHeight * 2.2f);
            this.DrawUpgradeEffects(effectRect);
            GUI.color = Color.white;
        }

        private float DrawInfoHeader(float width)
        {
            Rect titleRect = new Rect(0f, 0f, width, Text.LineHeight);
            Widgets.Label(titleRect, this.DefToShow.label);
            Text.Font = GameFont.Tiny;
            Rect descRect = new Rect(0f, titleRect.yMax + 2f, titleRect.width, Text.LineHeight * 3f);
            Widgets.TextArea(descRect, this.DefToShow.description, true);
            Text.Font = GameFont.Small;
            return descRect.yMax + 2f;
        }

        private void DrawProductionEffects(Rect effectRect)
        {
            Rect baseProductionRect = new Rect(effectRect.x, effectRect.y, effectRect.width, 2 * Text.LineHeight);
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(baseProductionRect, "BaseProduction".Translate(this.SelectedLevel.BaseProduction));
            Text.Anchor = TextAnchor.LowerRight;
            Widgets.Label(baseProductionRect, "BaseProductionCycle".Translate(this.SelectedLevel.DaysProductionCycle));
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawUpgradeEffects(Rect effectRect)
        {
            Text.Anchor = TextAnchor.UpperLeft;
            if (SelectedUpgrade.SpeedBoost != 1f)
            {
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(effectRect, "ProductionSpeed".Translate());

                Text.Anchor = TextAnchor.LowerLeft;
                Widgets.Label(effectRect, string.Concat((SelectedUpgrade.SpeedBoost - 1f) * 100, "%"));
            }

            if (SelectedUpgrade.YieldBoost != 1f)
            {
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(effectRect, "YieldBoost".Translate());
                Text.Anchor = TextAnchor.LowerCenter;
                Widgets.Label(effectRect, string.Concat((SelectedUpgrade.YieldBoost - 1f) * 100, "%"));
            }

            if (SelectedUpgrade.extraProduct != null)
            {
                Text.Anchor = TextAnchor.UpperRight;
                Widgets.Label(effectRect, "ExtraProduct".Translate());
                Text.Anchor = TextAnchor.LowerRight;
                Widgets.Label(effectRect, SelectedUpgrade.extraProduct.thingDef?.label);
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }


        private float DrawHeader(float width)
        {

            Rect titleRect = new Rect(0, 0, width, 32f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            titleRect.height = Text.LineHeight;
            Widgets.Label(titleRect, this.SelectedProduction.SiteLevel.label);
            Rect descriptionRect = new Rect(titleRect);
            descriptionRect.y = titleRect.yMax + 4f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            descriptionRect.height = 2 * Text.LineHeight;
            Widgets.Label(descriptionRect, this.SelectedProduction.SiteLevel.description);
            return descriptionRect.yMax + 4f;
        }


        protected override void FillTab()
        {
            Rect listRect = new Rect(0, 0, 128f, this.size.y);
            this.DrawPoductionList(listRect);
            Widgets.DrawLineVertical(listRect.xMax + 2f, 0f, listRect.height);
            Rect detailRect = new Rect(listRect.xMax + 8f, 2f, 765f, 675f);
            this.DrawProductionDetails(detailRect);
            Rect workForceRect = new Rect(detailRect.xMax + 4f, detailRect.y, 180f, detailRect.height);
            this.DrawWorkForce(workForceRect);
            Widgets.CloseButtonFor(Rect.zero);
        }

        private void DrawWorkForce(Rect inRect)
        {
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            this.WorkerWidget.DoOnGUI(inRect);
        }

        private IEnumerable<ResourceEntry> Stock
        {
            get
            {
                return this.ProductionSite.Stock;
            }
        }
        
        private void DrawThingRow(ref float y, float width, ResourceEntry entry)
        {
            Rect rect = new Rect(0f, y, width, 28f);
            //Widgets.InfoCardButton(rect.width - 24f, y, entry);
            rect.width -= 24f;
            if (Mouse.IsOver(rect))
            {
                GUI.color = WITab_ProductionSite_Production.HighlightColor;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
            if (entry.Def.DrawMatSingle != null && entry.Def.DrawMatSingle.mainTexture != null)
            {
                Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), entry.Def);
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = WITab_ProductionSite_Production.ThingLabelColor;
            Rect rect3 = new Rect(36f, y, width - 36f, 28f);
            string text = string.Format(stockFormat, entry.Count, entry.Def.label);

            Widgets.Label(rect3, text);
            y += 32f;
        }



        protected override void CloseTab()
        {
            base.CloseTab();
        }

    }
}

