using FactionColors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace Corruption
{
    public class ShipColorable : OHUShips.ShipBase
    {


        #region FactionColorStuff


        public Color Col1 = Color.white;
        public Color Col2 = Color.magenta;

        public override Color DrawColor
        {
            get
            {
                return Col1;
            }
            set
            {
                this.SetColor(value, true);
            }
        }

        public override Color DrawColorTwo
        {
            get
            {
                return Col2;
            }
        }



        private void InitiateColors()
        {
            if (FirstSpawned)
            {

                if (this.Faction == null) this.factionInt = Faction.OfPlayer;
                CompFactionColor compF = this.GetComp<CompFactionColor>();
                if (compF != null)
                {

                    if (this.Faction != null)
                    {
                        FactionDefUniform udef = this.Faction.def as FactionDefUniform;
                        if (udef != null)
                        {
                            Col1 = udef.FactionColor1;
                            Col2 = udef.FactionColor2;
                        }
                        if (this.Faction == Faction.OfPlayer)
                        {
                            Col1 = FactionColors.FactionColorUtilities.currentFactionColorTracker.PlayerColorOne;
                            Col2 = FactionColors.FactionColorUtilities.currentFactionColorTracker.PlayerColorTwo;
                        }
                    }
                    else
                    {
                        CompColorable comp = this.GetComp<CompColorable>();
                        if (comp != null && comp.Active)
                        {
                            Col1 = comp.Color;
                            Col2 = comp.Color;
                        }


                    }
                    if ((compF != null && compF.CProps.UseCamouflageColor))
                    {
                        Col1 = CamouflageColorsUtility.CamouflageColors[0];
                        Col2 = CamouflageColorsUtility.CamouflageColors[1];
                    }
                }
                FirstSpawned = false;
            }
        }

        public override void RecolorShip()
        {
            base.RecolorShip();
            this.InitiateColors();
        }
        #endregion

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
