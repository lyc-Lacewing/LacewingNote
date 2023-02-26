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
                //int i = LaText.ParseNumOp(args[0], out _, out _).Length;
                //string s = string.Join(" ", LaText.ParseArgs(args));
                //caller.Player.GetModPlayer<NotePlayer>().InfoNote.Renew(s);
                //caller.Player.GetModPlayer<NotePlayer>().InfoNote.InsertWord(args[0]);
                //caller.Player.GetModPlayer<NotePlayer>().InfoNote.DoAppendWord(args);
            }
            Main.NewText(caller.Player.GetModPlayer<NotePlayer>().InfoNote.TextWithCursor());
        }
    }
}
