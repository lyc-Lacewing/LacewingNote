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
            if (args.Length > 0)
            {
                caller.Player.GetModPlayer<NotePlayer>().InfoNote.RunCommand(args);
                caller.Player.GetModPlayer<NotePlayer>().InfoNote.ReplaceAll(args[0], args[1]);
            }
        }
    }
}
