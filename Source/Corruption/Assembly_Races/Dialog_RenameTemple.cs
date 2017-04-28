using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Corruption
{
    public class Dialog_RenameTemple : Window
    {
        BuildingAltar Altar;        

        private string curName;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(500f, 200f);
            }
        }

        public Dialog_RenameTemple(BuildingAltar altar)
        {
            this.Altar = altar;
            this.forcePause = true;
            this.closeOnEscapeKey = false;
            this.absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Small;
            bool flag = false;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                flag = true;
                Event.current.Use();
            }
            Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), "NameTempleMessage".Translate());
            this.curName = Widgets.TextField(new Rect(0f, rect.height - 35f, rect.width / 2f - 20f, 35f), this.curName);
            if (Widgets.ButtonText(new Rect(rect.width / 2f + 20f, rect.height - 35f, rect.width / 2f - 20f, 35f), "OK".Translate(), true, false, true) || flag)
            {
                if (this.IsValidRoomName(this.curName))
                {
                    this.Altar.RoomName = this.curName;
                    Find.WindowStack.TryRemove(this, true);
                }
                else
                {
                    Messages.Message("ColonyNameIsInvalid".Translate(), MessageSound.RejectInput);
                }
                Event.current.Use();
            }
        }

        private bool IsValidRoomName(string s)
        {
            return s.Length != 0 && GenText.IsValidFilename(s);
        }
    }
}
