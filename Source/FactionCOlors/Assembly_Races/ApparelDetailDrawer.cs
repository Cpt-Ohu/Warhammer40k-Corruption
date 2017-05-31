using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FactionColors
{
    [StaticConstructorOnStartup]
    public class ApparelDetailDrawer : ThingComp
    {
        private bool FirstSpawn = true;

        private ApparelDetail appDetailInt;

        public ApparelDetail AppDetail
        {
            get
            {
                if (FirstSpawn)
                {
                    HasDetail = AppProps.DetailChance >= Rand.Range(0.1f, 0.9f);
                    //          Log.Message("CheckingDetail");
                    if (HasDetail)
                    { 
                        appDetailInt = AppProps.ApparelDetails.RandomElementByWeight((ApparelDetail hd) => hd.Commonality);
                        FirstSpawn = false;
                        return appDetailInt;
                    }
                }
                return appDetailInt;
            }
        }

        private Graphic detailGraphicInt;

        public Graphic DetailGraphic
        {
            get
            {
                if(this.AppDetail!= null && this.apparel.Wearer == null)
                {
                    detailGraphicInt = GraphicDatabase.Get<Graphic_Multi>(AppDetail.DetailGraphicPath, ShaderDatabase.CutoutComplex, drawSize, parent.DrawColor, parent.DrawColorTwo);                    
                }
                else if(this.AppDetail != null && this.apparel.Wearer != null)
                {
                    string path;
                    if (this.apparel.def.apparel.LastLayer == ApparelLayer.Overhead)
                    {
                        path = this.AppDetail.DetailGraphicPath;
                    }
                    else
                    {
                        path = this.AppDetail.DetailGraphicPath + "_" + this.apparel.Wearer.story.bodyType.ToString();
                    }
                    detailGraphicInt = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.CutoutComplex, drawSize, parent.DrawColor, parent.DrawColorTwo);
                }
                return detailGraphicInt;
            }
        }

        public bool HasDetail = false;

        private Vector2 drawSize = new Vector2(2f, 2f);

        private string texPath;


        public ApparelDetailProps AppProps
        {
            get
            {
                return (ApparelDetailProps)this.props;
            }
        }
        
        private Apparel apparel
        {
            get
            {
               return this.parent as Apparel;
            }
        }        

        public override void PostSpawnSetup(bool respawnAfterLoad)
        {
            base.PostSpawnSetup(respawnAfterLoad);
            if (this.AppDetail == null) Log.Message("NoAppdetail");
            if (this.DetailGraphic == null) Log.Message("NoAppGraphic");
            InitiateDetails();
        }
       

        public void InitiateDetails()
        {
            if (FirstSpawn)
            {
                HasDetail = AppProps.DetailChance >= Rand.Range(0.1f, 0.9f);
      //          Log.Message("CheckingDetail");
                if (HasDetail)
                {
                    appDetailInt = AppProps.ApparelDetails.RandomElementByWeight((ApparelDetail hd) => hd.Commonality);
     //               this.DetailGraphic = GraphicDatabase.Get<Graphic_Multi>(AppDetail.DetailGraphicPath, ShaderDatabase.CutoutComplex, drawSize, parent.DrawColor, parent.DrawColorTwo);
     //               Log.Message("HasDetail");
                }
            }
            FirstSpawn = false;
        }
 

        public static bool ReturnApparelDetails(Apparel curr, out ApparelGraphicRecord result)
        {
            ApparelDetailDrawer drawer;
            if((drawer = curr.TryGetComp<ApparelDetailDrawer>()) != null)
            {
          //      Log.Message("Checking Available Details");
                if (drawer.HasDetail)
                {
        //            Log.Message("Found Detail");
                    ApparelGraphicRecord recDetail;
                    if (ApparelDetailDrawer.TryGetApparelDetails(curr, drawer.DetailGraphic, out recDetail))
                    {
          //              Log.Message("Gotten ApparelDetailRecord");
                        result = recDetail;
                        return true;
                    }
                }
            }
            result = new ApparelGraphicRecord();
            return false;
        }

        public static bool TryGetApparelDetails(Apparel curr, Graphic detailgraphic, out ApparelGraphicRecord recDetail)
        {
  //          Log.Message("Trying to get GraphicRecord");
            Apparel temp1 = new Apparel();
            if (curr.def.apparel.LastLayer == ApparelLayer.Overhead)
            {
                temp1.def = FactionColorsDefOf.Overlay_Headgear;
            }
            else
            {
                temp1.def = FactionColorsDefOf.Overlay_Body;
            }
 //           Log.Message("GraphicRecord of DEF: "+ temp1.def.ToString());
            recDetail = new ApparelGraphicRecord(detailgraphic, temp1);
            return true;
        }


        public static void DrawDetails(Pawn pawn, Apparel curr)
        {
            try
            {
                if (pawn.needs != null && pawn.story != null && !pawn.kindDef.factionLeader)
                {
                    ApparelDetailDrawer drawer;
                    if ((drawer = curr.TryGetComp<ApparelDetailDrawer>()) != null)
                    {
                        drawer.PostSpawnSetup(false);
                        if (drawer.HasDetail)
                        {
                            ApparelGraphicRecord recDetail;
                            if (ApparelDetailDrawer.TryGetApparelDetails(curr, drawer.DetailGraphic, out recDetail))
                            {
  //                              Log.Message("Inserting Detail");
                                pawn.Drawer.renderer.graphics.apparelGraphics.Add(recDetail);
                            }
                        }
                    }
                }
            }
            catch
            {
            }

        }

        public static bool GetDetailGraphic(Pawn pawn, Apparel curr, Rot4 bodyFacing, out Material detailGraphic)
        {
            detailGraphic = null;
            try
            {
                if (pawn.needs != null && pawn.story != null)
                {
                    ApparelDetailDrawer drawer;
                    if ((drawer = curr.TryGetComp<ApparelDetailDrawer>()) != null)
                    {
                        drawer.PostSpawnSetup(false);
                        if (drawer.HasDetail)
                        {
                            detailGraphic = drawer.DetailGraphic.MatAt(bodyFacing);
                            return true;
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }

        }


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref HasDetail, "HasDetail", true, false);
            Scribe_Values.Look<bool>(ref FirstSpawn, "FirstSpawn", false, false);
            Scribe_Values.Look<string>(ref this.texPath, "texPath", null, false);
        }     

    }
}

