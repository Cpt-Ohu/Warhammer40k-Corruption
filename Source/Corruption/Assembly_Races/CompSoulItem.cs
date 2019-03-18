using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class CompSoulItem : ThingComp
    {
        private float sign;

        public Pawn Owner = new Pawn();

        public PatronDef DedicatedGod = PatronDefOf.Emperor;

        private CompSoul soul;

        private bool PsykerPowerAdded = false;

        private bool randomCategoryResolved = false;

        private string OverlayPath = "";

        protected Graphic OverlayGraphic
        {
            get
            {
                if (this.OverlayPath != "")
                {
                    return GraphicDatabase.Get<Graphic_Single>(OverlayPath, ShaderDatabase.MetaOverlay, Vector2.one, Color.white);
                }
                return null;
            }
        }

        private SoulItemCategories itemCategory = SoulItemCategories.Neutral;

        public CompProperties_SoulItem SProps
        {
            get
            {
                return (CompProperties_SoulItem)this.props;
            }
        }

     
        public List<PsykerPower> psykerItemPowers = new List<PsykerPower>();

        public void GetOverlayGraphic()
        {

                if (itemCategory == SoulItemCategories.Corruption)
                {
                    this.OverlayPath = "UI/Glow_Corrupt";

                    //this.Overlay = GraphicDatabase.Get<Graphic_Single>("UI/Glow_Corrupt", ShaderDatabase.MetaOverlay, Vector2.one, Color.white);
                }
                else if (itemCategory == SoulItemCategories.Redemption)
                {
                    //Log.Message("RedemptionItem");
                    this.OverlayPath = "UI/Glow_Holy";
                    //this.Overlay = GraphicDatabase.Get<Graphic_Single>("UI/Glow_Holy", ShaderDatabase.MetaOverlay, Vector2.one, Color.white);
                }
                else
                {
                    //Log.Message("Returning");
                    return;
                }
        }

        public void UpdatePsykerUnlocks(CompSoul soul)
        {
            List<PsykerPowerDef> list = SProps.UnlockedPsykerPowers;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].PowerLevel <= soul.PsykerPowerLevel)
                {
                    this.parent.TryGetComp<CompPsyker>().PsykerPowerManager.AddPsykerPower(list[i]);
                }
            }            
        }

        public void UpdatePsykerVerbs()
        {
            if (Owner != null)
            {
                ThingWithComps equipment = this.parent as ThingWithComps;
                List<VerbProperties_WarpPower> list = SProps.UnlockedPsykerVerbs;
                for (int i = 0; i < SProps.UnlockedPsykerVerbs.Count; i++)
                {
                    if (list.Any(x => x.ReplacesStandardAttack))
                    {
                        List<VerbProperties> verbs = typeof(ThingDef).GetField("verbs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(equipment) as List<VerbProperties>;
                        verbs.Clear();
                        if (list[i].ReplacesStandardAttack)
                        {
                            if (verbs != null)
                            {
                                verbs.Add(list[i]);
                            }
                        }
                    }
                }
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            this.itemCategory = SProps.Category;
            //Log.Message("CompTickRare");
            if (!this.randomCategoryResolved)
            {
                if (SProps.Category == SoulItemCategories.Random)
                {
                    this.itemCategory = (SoulItemCategories)Rand.RangeInclusive(0, 2);
                }
                this.randomCategoryResolved = true;
            }
            if (this.SProps?.DedicatedGod != null)
            {
                this.DedicatedGod = this.SProps.DedicatedGod;
            }
            else
            {
                this.DedicatedGod = (this.itemCategory == SoulItemCategories.Corruption ? PatronDefOf.ChaosUndivided : PatronDefOf.Emperor);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            GetOverlayGraphic();
            if (this.parent.def.tickerType == TickerType.Never)
            {
                this.parent.def.tickerType = TickerType.Rare;
            }
            base.PostSpawnSetup(respawningAfterLoad);
         //   Log.Message("GettingOverlay");
            Find.TickManager.RegisterAllTickabilityFor(this.parent);
        }
        
        public void CheckForOwner()
        {
            if (this.parent != null && !this.parent.Spawned)
            {
                //Log.Message("Begin Check");


                if (this.parent.holdingOwner != null)
                {
                    if (this.parent.holdingOwner != null)
                    {
                        if (this.parent.holdingOwner.Owner is Pawn_EquipmentTracker)
                        {
                            Pawn_EquipmentTracker tracker = this.parent.holdingOwner.Owner as Pawn_EquipmentTracker;
                            if (tracker.ParentHolder != null)
                            {
                                this.Owner = tracker.ParentHolder as Pawn;
                            }
                        }
                        else if (this.parent.holdingOwner.Owner is Pawn_ApparelTracker)
                        {
                            Pawn_ApparelTracker tracker = this.parent.holdingOwner.Owner as Pawn_ApparelTracker;
                            if (tracker.pawn != null)
                            {
                                this.Owner = tracker.pawn;
                            }
                        }
                        if (this.parent.holdingOwner.Owner is Pawn_InventoryTracker)
                        {
                            Pawn_InventoryTracker tracker = this.parent.holdingOwner.Owner as Pawn_InventoryTracker;
                            if (tracker.pawn != null)
                            {
                                this.Owner = tracker.pawn;
                            }
                        }
                        else if (this.parent.holdingOwner.Owner is Pawn_CarryTracker)
                        {
                            Pawn_CarryTracker tracker = this.parent.holdingOwner.Owner as Pawn_CarryTracker;
                            if (tracker.pawn != null)
                            {
                                this.Owner = tracker.pawn;
                            }
                        }
                    }
                }

                if (this.Owner != null)
                {
                    if ((soul = CompSoul.GetPawnSoul(Owner)) != null)
                    {
                        this.CalculateSoulChanges(soul, SProps);

                        if (CorruptionModSettings.AllowPsykers)
                        {
                            if (!PsykerPowerAdded)
                            {
                                CompPsyker compPsyker;
                                if ((compPsyker = Owner.TryGetComp<CompPsyker>()) != null)
                                {
                                    for (int i = 0; i < SProps.UnlockedPsykerPowers.Count; i++)
                                    {
                                        if (soul.PsykerPowerLevel >= SProps.UnlockedPsykerPowers[i].PowerLevel)
                                        {
                                            //    Log.Message("Adding Power to: " + compPsyker.psyker + " : " + SProps.UnlockedPsykerPowers[i].defName);
                                            compPsyker.PsykerPowerManager.AddPsykerPower(SProps.UnlockedPsykerPowers[i], true, this.parent.def);
                                        }
                                    }
                                }
                                PsykerPowerAdded = true;
                            }
                        }
                    }

                }
            }
            if (this.parent.Spawned)
            {
                PsykerPowerAdded = false;
            }
        }

        public void CalculateSoulChanges(CompSoul soul, CompProperties_SoulItem cprops)
        {
            float num;
            switch (itemCategory)
            {
                case (SoulItemCategories.Neutral):
                    {
            //            Log.Message("NeutralItem");
                        sign = 0;
                        break;
                    }
                case (SoulItemCategories.Corruption):
                    {
                        //Log.Message("Corrupted Item");
                        sign = -1;
                        break;
                    }
                case (SoulItemCategories.Redemption):
                    {
      //                  Log.Message("Redemptive Item");
                        sign = 0.5f;
                        break;
                    }
                default:
                    {
                        Log.Error("No Soul Item Category Found");
                        break;
                    }
            }
            num = sign * cprops.GainRate * 0.2f / 7200;
            soul.AffectSoul(num);
        }       

        public override void PostDrawExtraSelectionOverlays()
        {
            if (OverlayGraphic != null)
            {
                Vector3 drawPos = this.parent.DrawPos;
                drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                Vector3 s = new Vector3(2.0f, 2.0f, 2.0f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(0, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, this.OverlayGraphic.MatSingle, 0);
            }
        }

        public override void CompTickRare()
        {
            this.CheckForOwner();     
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.PsykerPowerAdded, "PsykerPowerAdded", false, false);
            Scribe_Values.Look<bool>(ref this.randomCategoryResolved, "randomCategoryResolved", false, false);
            Scribe_Values.Look<string>(ref this.OverlayPath, "OverlayPath", "", false);
            Scribe_Defs.Look<PatronDef>(ref this.DedicatedGod, "DedicatedGod");
            Scribe_Values.Look<SoulItemCategories>(ref this.itemCategory, "itemCategory", SoulItemCategories.Neutral, false);

        }
    }
}
