using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class StarMapObject : IExposable
    {
        public string objectName;

        public StarMapObjectType objectType;

        public Rect objectRect
        {
            get
            {
                return new Rect(this.x, this.y, this.width, this.height);
            }
        }

        public float x;

        public float y;

        public float width;

        public float height;

        public int diameter;

        public List<ThingDef> possibleResources = new List<ThingDef>();

        public string texPath = "UI/SectorMap/Planet_Medium";

        public Texture2D objectTex
        {
            get
            {
                return ContentFinder<Texture2D>.Get(texPath, true);
            }
        }
        public StarMapObject()
        {
            Vector2 center = new Vector2(360f, 300f);
            this.objectName = "Object";
            this.x = center.x + Rand.Range(160f, 170f) * Mathf.Cos(45);
            this.y = center.y - Rand.Range(160f, 170f) * Mathf.Sin(45);
            this.width = 60f;
            this.height = 60f;
            this.diameter = Rand.Range(50000, 100000);
        }

        public StarMapObject(int prevAngle, out int curAngle, Vector2 center, List<string> existingObjects, StarMapObjectType newObjectType = StarMapObjectType.PlanetMedium)
        {
            this.objectType = newObjectType;
            List<string> objectNames = existingObjects;

            float angle = Rand.RangeInclusive(prevAngle + 30, 330);
            curAngle = (int)angle;
            switch (this.objectType)
            {
                case (StarMapObjectType.PlanetMedium):
                    {
                        this.objectName = NameGenerator.GenerateName(RulePackDefOf.NamerWorld, objectNames, false);
                        this.x = center.x + Rand.Range(160f, 170f) * Mathf.Cos(angle);
                        this.y = center.y - Rand.Range(160f, 170f) * Mathf.Sin(angle);
                        this.width = 60f;
                        this.height = 60f;
                        this.diameter = Rand.Range(50000, 100000);
                        return;
                    }
                case (StarMapObjectType.PlanetSmall):
                    {
                        this.texPath = "UI/SectorMap/Planet_Small";
                        this.objectName = NameGenerator.GenerateName(RulePackDefOf.NamerWorld, objectNames, false);
                        this.x = center.x + Rand.Range(110f, 170f) * Mathf.Cos(angle);
                        this.y = center.y - Rand.Range(110f, 170f) * Mathf.Sin(angle);
                        this.width = 36f;
                        this.height = 36f;
                        this.diameter = Rand.Range(3000, 20000);
                        return;
                    }
                case (StarMapObjectType.Moon):
                    {
                        this.texPath = "UI/SectorMap/Moon";
                        this.objectName = NameGenerator.GenerateName(RulePackDefOf.NamerWorld, objectNames, false);
                        this.x = center.x + Rand.Range(60f, 70f) * Mathf.Cos(angle);
                        this.y = center.y - Rand.Range(60f, 70f) * Mathf.Sin(angle);
                        this.width = 12f;
                        this.height = 12f;
                        this.diameter = Rand.Range(300, 1000);
                        return;
                    }
            }


        }

        public void ExposeData()
        {
            Scribe_Values.LookValue<string>(ref this.objectName, "objectName", "None", false);
            Scribe_Values.LookValue<string>(ref this.texPath, "texPath", "UI/SectorMap/Planet_Medium", false);
            Scribe_Values.LookValue<StarMapObjectType>(ref this.objectType, "objectType", StarMapObjectType.PlanetSmall, false);
            Scribe_Values.LookValue<int>(ref this.diameter, "diameter", 20000, false);

            Scribe_Values.LookValue<float>(ref this.x, "x", 360f, false);
            Scribe_Values.LookValue<float>(ref this.y, "y", 300f, false);
            Scribe_Values.LookValue<float>(ref this.width, "width", 60f, false);
            Scribe_Values.LookValue<float>(ref this.height, "height", 60f, false);
            // Scribe_Deep.LookDeep<Rect>(ref this.objectRect, "objectRect", new Rect());
        }
    }
}
