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

        public List<FactionColorEntry> FactionColorList = new List<FactionColorEntry>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<Color>(ref this.PlayerColorOne, "PlayerColorOne", Color.red, false);
            Scribe_Values.Look<Color>(ref this.PlayerColorTwo, "PlayerColorTwo", Color.red, false);
            Scribe_Values.Look<string>(ref this.BannerGraphicPath, "BannerGraphicPath", "UI/Flags/Plain", false);
            Scribe_Collections.Look<FactionColorEntry>(ref this.FactionColorList, "FactionColorList", LookMode.Deep);
        }

        public override void Draw()
        {
        }

        public override void PostAdd()
        {
            base.PostAdd();
            foreach (Faction current in Find.World.factionManager.AllFactions)
            {
                AddColorEntry(current);
            }
        }

        public void AddColorEntry(Faction faction)
        {
            FactionDefUniform udef = faction.def as FactionDefUniform;
            if (udef != null && !this.FactionColorList.Any(x => x.Faction == faction))
            {
                this.FactionColorList.Add(new FactionColorEntry(faction, udef.FactionColor1, udef.FactionColor2));
            }
        }

        public bool GetColorEntry(Faction faction, out FactionColorEntry entry)
        {
            if (this.FactionColorList.Any(x => x.Faction == faction))
            {
                entry = this.FactionColorList.FirstOrDefault(x => x.Faction == faction);
                return true;
            }
            else
            {
                entry = new FactionColorEntry();
                return false;
            }
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
