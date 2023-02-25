using LacewingNote.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LacewingNote.Content.Commands
{
    public class InfoNoteCommand : ModCommand
    {
        public override string Command => "ino";

        public override CommandType Type => CommandType.Chat;

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            caller.Player.GetModPlayer<NotePlayer>().InfoNote.InsertWord(input.Remove(0, 6));
        }
    }
}
