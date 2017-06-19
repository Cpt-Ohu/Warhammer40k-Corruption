using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace FactionColors
{
    public class Graphic_RandomApparelMulti : Graphic
    {
        protected virtual GraphicData_FC dataFC
        {
            get
            {
                return this.data as GraphicData_FC;
            }
        }

        private Material[] mats = new Material[3];
        
        public static readonly string[] MultiMaskSuffix = new string[6] {"_frontm", "_backm", "_sidem", "_front", "_back", "_sidem" };

        private bool isMask(string path)
        {
            for (int i=0; i < Graphic_RandomApparelMulti.MultiMaskSuffix.Count(); i++)
            {
                if (path.EndsWith(Graphic_RandomApparelMulti.MultiMaskSuffix[i]))
                {
                    return true;
                }
            }
            return false;
        }

        protected Graphic[] subGraphics;

        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            if (req.path.NullOrEmpty())
            {
                throw new ArgumentNullException("folderPath");
            }
            if (req.shader == null)
            {
                throw new ArgumentNullException("shader");
            }
            this.path = req.path;
            this.color = req.color;
            this.drawSize = req.drawSize;
            List<Texture2D> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
                                    where !x.name.EndsWith(Graphic_Single.MaskSuffix) && !this.isMask(x.name)
                                    select x).ToList<Texture2D>();
            if (list.NullOrEmpty<Texture2D>())
            {
                Log.Error("Collection cannot init: No textures found at path " + req.path);
                this.subGraphics = new Graphic[]
                {
                    BaseContent.BadGraphic
                };
                return;
            }
            this.subGraphics = new Graphic[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                string path = req.path + "/" + list[i].name;
                this.subGraphics[i] = GraphicDatabase.Get<Graphic_Single>(path, req.shader, this.drawSize, this.color);
            }
            Texture2D[] array = new Texture2D[3];
            string graphicPath = list.RandomElement().name;
            array[1] = ContentFinder<Texture2D>.Get(graphicPath + "_side", false);
            if (array[1] == null)
            {
                array[1] = array[0];
            }
            array[2] = ContentFinder<Texture2D>.Get(graphicPath + "_front", false);
            if (array[2] == null)
            {
                array[2] = array[0];
            }
            Texture2D[] array2 = new Texture2D[3];
            if (req.shader.SupportsMaskTex())
            {
                array2[0] = ContentFinder<Texture2D>.Get(graphicPath + "_backm", false);
                if (array2[0] != null)
                {
                    array2[1] = ContentFinder<Texture2D>.Get(graphicPath + "_sidem", false);
                    if (array2[1] == null)
                    {
                        array2[1] = array2[0];
                    }
                    array2[2] = ContentFinder<Texture2D>.Get(graphicPath + "_frontm", false);
                    if (array2[2] == null)
                    {
                        array2[2] = array2[0];
                    }
                }
            }
            for (int i = 0; i < 3; i++)
            {
                MaterialRequest req2 = default(MaterialRequest);
                req2.mainTex = array[i];
                req2.shader = req.shader;
                req2.color = this.color;
                req2.colorTwo = this.colorTwo;
                req2.maskTex = array2[i];
                this.mats[i] = MaterialPool.MatFrom(req2);
            }
            this.path = graphicPath;
        }
                
        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_RandomApparelMulti>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data);
        }
    }
}
