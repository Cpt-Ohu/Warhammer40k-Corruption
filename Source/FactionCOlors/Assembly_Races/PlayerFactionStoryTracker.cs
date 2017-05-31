using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;

namespace FactionColors
{
    public class PlayerFactionStoryTracker : WorldObject
    {
        public PlayerFactionStoryTracker()
        {
            this.PlayerColorOne = Color.red;
            this.PlayerColorTwo = Color.black;
        }

        public Color PlayerColorOne;

        public Color PlayerColorTwo;

        public string BannerGraphicPath = "UI/Flags/Plain";

        public Dictionary<Faction, string> FactionGraphicPaths = new Dictionary<Faction, string>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Color>(ref this.PlayerColorOne, "PlayerColorOne", Color.red, false);
            Scribe_Values.Look<Color>(ref this.PlayerColorTwo, "PlayerColorTwo", Color.red, false);
            Scribe_Values.Look<string>(ref this.BannerGraphicPath, "BannerGraphicPath", "UI/Flags/Plain", false);
        }

        public override void Draw()
        {
        }

        public List<string> BannerOptions
        {
            get
            {
                IEnumerable<Texture2D> array = ContentFinder<Texture2D>.GetAllInFolder("UI/Flags");
                List<string> list = new List<string>();
                foreach (Texture2D current in array)
                {
                    string name = current.name;
                    if (!name.Contains("_m"))
                    {
                        list.Add(name);
                    }
                }

                return list;
            }
        }
    }
}
