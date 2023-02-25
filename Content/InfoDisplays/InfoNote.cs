using LacewingNote.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace LacewingNote.Content.InfoDisplays
{
    internal class InfoNote : InfoDisplay
    {
        public override bool Active()
        {
                return !string.IsNullOrEmpty(Main.LocalPlayer.GetModPlayer<NotePlayer>().InfoNote);
        }
        public override string DisplayValue()
        {
            return Main.LocalPlayer.GetModPlayer<NotePlayer>().InfoNote;
        }
    }
}
