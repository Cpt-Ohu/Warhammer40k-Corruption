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

        private Need_Soul soul;

        private bool PsykerPowerAdded = false;

        private bool randomCategoryResolved = false;

        private Graphic Overlay;

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
            if (SProps == null)
                Log.Message("NoSprops");

            if (itemCategory == SoulItemCategories.Corruption)
            {
     //           Log.Message("CorruptionItem");
                this.Overlay = GraphicDatabase.Get<Graphic_Single>("UI/Glow_Corrupt", ShaderDatabase.MetaOverlay, Vector2.one, Color.white);
            }
            else if (itemCategory == SoulItemCategories.Redemption)
            {
  //              Log.Message("RedemptionItem");
                this.Overlay = GraphicDatabase.Get<Graphic_Single>("UI/Glow_Holy", ShaderDatabase.MetaOverlay, Vector2.one, Color.white);
            }
            else
            {
                return;
            }
        }

        public void UpdatePsykerUnlocks(Need_Soul soul)
        {
            List<PsykerPowerDef> list = SProps.UnlockedPsykerPowers;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].PowerLevel <= soul.PsykerPowerLevel)
                {
                    this.parent.TryGetComp<CompPsyker>().psykerPowerManager.AddPsykerPower(list[i]);
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


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (this.parent.def.tickerType == TickerType.Never)
            {
                this.parent.def.tickerType = TickerType.Rare;
            }
            base.PostSpawnSetup(respawningAfterLoad);
        //    Log.Message("GettingOverlay");
            GetOverlayGraphic();
            Find.TickManager.RegisterAllTickabilityFor(this.parent);
        }
        
        public void CheckForOwner()
        {
            CompEquippable tempcomp;
            Apparel tempthing;
            if (this.parent != null && !this.parent.Spawned && this.parent.holdingOwner == null)
            {
                //         Log.Message("Begin Check");
                if (this.parent is Apparel)
                {
                    //             Log.Message("Soul item is Apparel");
                    tempthing = this.parent as Apparel;
                    this.Owner = tempthing.Wearer;
                }
                else if ((tempcomp = this.parent.TryGetComp<CompEquippable>()) != null && tempcomp.PrimaryVerb.CasterPawn != null)
                {
                    //         Log.Message("IsGun");
                    this.Owner = tempcomp.PrimaryVerb.CasterPawn;
                }
                else if (this.parent.holdingOwner != null && this.parent.holdingOwner.Owner is Pawn_CarryTracker)
                {
                    Pawn_CarryTracker tracker = this.parent.holdingOwner.Owner as Pawn_CarryTracker;
                    this.Owner = tracker.pawn;
                }
                if ((this.Owner != null))
                {
                    if ((soul = this.Owner.needs.TryGetNeed<Need_Soul>()) != null)
                    {
                        this.CalculateSoulChanges(soul, SProps);
                    }
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
                                    compPsyker.allpsykerPowers.Add(new PsykerPowerEntry(SProps.UnlockedPsykerPowers[i], true, this.parent.def));
                                }
                            }
                            compPsyker.UpdatePowers();
                        }
                        PsykerPowerAdded = true;
                    }

                }
            }
            if (this.parent.Spawned)
            {
                PsykerPowerAdded = false;
            }
        }

        public void CalculateSoulChanges(Need_Soul nsoul, CompProperties_SoulItem cprops)
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
             //           Log.Message("Corrupted Item");
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
            num = sign * cprops.GainRate * 0.2f / 14000;
 //           Log.Message(num.ToString());
            nsoul.GainNeed(num);
        }       

        public override void PostDrawExtraSelectionOverlays()
        {
            if (Overlay == null) Log.Message("NoOverlay");
            if (Overlay != null)
            {
                Vector3 drawPos = this.parent.DrawPos;
                drawPos.y = Altitudes.AltitudeFor(AltitudeLayer.MoteOverhead);
                Vector3 s = new Vector3(2.0f, 2.0f, 2.0f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawPos, Quaternion.AngleAxis(0, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, this.Overlay.MatSingle, 0);
            }
        }

        public override void CompTickRare()
        {
            //      Log.Message("CompTick");
            if (!this.randomCategoryResolved)
            {
                if (SProps.Category == SoulItemCategories.Random)
                {
                    this.itemCategory = (SoulItemCategories)Rand.RangeInclusive(0, 2);
                }
                this.randomCategoryResolved = true;
            }
            this.CheckForOwner();     
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.PsykerPowerAdded, "PsykerPowerAdded", false, false);
            Scribe_Values.Look<bool>(ref this.randomCategoryResolved, "randomCategoryResolved", false, false);
            Scribe_Values.Look<SoulItemCategories>(ref this.itemCategory, "itemCategory", SoulItemCategories.Neutral, false);

        }
    }
}
