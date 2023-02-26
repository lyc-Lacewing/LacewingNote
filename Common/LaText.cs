using Microsoft.Xna.Framework;
using ReLogic.OS;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LacewingNote.Common
{
    public struct LaText : INullable
    {
        public bool IsNull => cache == null;

        private string[] cache;
        private int cIndex;
        private int CIndex { get { return Math.Clamp(cIndex, 0, Math.Max(cache.Length - 1, 0)); } set { cIndex = Math.Clamp(value, 0, Math.Max(cache.Length - 1, 0)); } }
        private int cursor;
        private int Cursor { get { return Math.Clamp(cursor, 0, Text.Length); } set { cursor = Math.Clamp(value, 0, Text.Length); } }

        public string Text { get => cache[CIndex] == null ? string.Empty : cache[CIndex]; }
        public string[] Words { get => Text.Split(' '); }
        public string[] Lines { get => Text.Split('\n'); }
        public int CacheLength { get => cache.Length; }

        #region Construction
        public LaText()
        {
            cache = new string[9];
            CIndex = 0;
            Cursor = 0;
        }
        public LaText(int cacheLength = 9)
        {
            cache = new string[cacheLength];
            CIndex = 0;
            Cursor = 0;
        }
        #endregion

        #region Cache
        /// <summary>
        /// Move all elements in cache by step, exceeding element will be default
        /// </summary>
        /// <param name="step">Can be negative</param>
        private void MoveCache(int step = 1)
        {
            if (cache.Length <= 0 || step == 0)
            {
                return;
            }
            if (Math.Abs(step) >= cache.Length)
            {
                for (int i = 0; i < cache.Length; i++)
                {
                    cache[i] = default;
                }
                return;
            }
            if (step < 0)
            {
                for (int i = 0; i < cache.Length + step; i++)
                {
                    cache[i] = cache[i - step];
                }
                for (int i = -1; i >= step; i--)
                {
                    cache[cache.Length + i] = default;
                }
            }
            else
            {
                for (int i = cache.Length - 1; i > step - 1; i--)
                {
                    cache[i] = cache[i - step];
                }
                for (int i = 0; i < step - 1; i++)
                {
                    cache[i] = default;
                }
            }
        }
        #endregion

        #region Cursor location & movement
        /// <summary>
        /// Index of the char after cursor
        /// </summary>
        /// <returns></returns>
        private int CursorCharIndex()
        {
            return Math.Min(Cursor, Math.Max(Text.Length - 1, 0));
        }
        /// <summary>
        /// Length of num of words for cursor
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private int WordsLength(int num)
        {
            num = Math.Clamp(num, 0, Words.Length);
            int length = 0;
            for (int i = 0; i < num; i++)
            {
                length += Words[i].Length + 1;
            }
            return length;
        }
        /// <summary>
        /// </summary>
        /// <returns>The word's index in Words + 1</returns>
        private int CursorAtWord()
        {
            for (int i = 0; i < Words.Length; i++)
            {
                if (WordsLength(i + 1) >= Cursor)
                {
                    return i + 1;
                }
            }
            return 0;
        }
        /// <summary>
        /// Move cursor to the start of the {num}th word
        /// </summary>
        /// <param name="num"></param>
        private void CursorToPreWord(int num = 1)
        {
            Cursor = WordsLength(num - 1);
        }
        /// <summary>
        /// Move curdor to the end of the {num}th word
        /// </summary>
        /// <param name="num"></param>
        private void CursorToApWord(int num = 1)
        {
            Cursor = WordsLength(num);
            if (!string.IsNullOrEmpty(Text) && Cursor != 0 && Text[CursorCharIndex() - 1] == ' ')
            {
                Cursor -= 1;
            }
        }
        /// <summary>
        /// Length of num lines for cursor
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private int LineLength(int count)
        {
            count = Math.Clamp(count, 0, Lines.Length);
            int length = 0;
            for (int i = 0; i < count; i++)
            {
                length += Lines[i].Length + 1;
            }
            return length;
        }
        /// <summary>
        /// </summary>
        /// <returns>The line's index in Lines + 1</returns>
        private int CursorAtLine()
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                if (LineLength(i + 1) >= Cursor)
                {
                    return i + 1;
                }
            }
            return 0;
        }
        /// <summary>
        /// Move cursor to the start of the {num}th line
        /// </summary>
        /// <param name="num"></param>
        private void CursorToPreLine(int num = 1)
        {
            Cursor = LineLength(num - 1);
        }
        /// <summary>
        /// Move cursor to the end of the {num}th line
        /// </summary>
        /// <param name="num"></param>
        private void CursorToApLine(int num = 1)
        {
            Cursor = LineLength(num);
            if (Cursor != Text.Length && Cursor != 0 && Text[CursorCharIndex() - 1] == '\n')
            {
                Cursor -= 1;
            }
        }

        /// <summary>
        /// Indicate the location of cursor
        /// </summary>
        /// <returns></returns>
        public string TextWithCursor()
        {
            return Text.Insert(Cursor, "|");
        }
        #endregion

        #region Text editing
        /// <summary>
        /// Override text by newText, literal parsed, cached
        /// </summary>
        /// <param name="newText"></param>
        public void Renew(string newText)
        {
            MoveCache(-CIndex + 1);
            cache[0] = ParseLiteral(newText);
            CIndex = 0;
            Cursor = Text.Length;
        }
        /// <summary>
        /// Increase cIndex by step
        /// </summary>
        /// <param name="step"></param>
        public void Undo(int step = 1)
        {
            if (cache.Length <= 0 || step < 1 || CIndex + step > cache.Length - 1)
            {
                return;
            }
            CIndex += step;
        }
        /// <summary>
        /// Decrease cIndex by step
        /// </summary>
        /// <param name="step"></param>
        public void Redo(int step = 1)
        {
            if (cache.Length <= 0 || step < 1 || CIndex - step < 0)
            {
                return;
            }
            CIndex -= step;
            Cursor = Text.Length;
        }
        /// <summary>
        /// Insert text at index, cached
        /// </summary>
        /// <param name="start"></param>
        /// <param name="text"></param>
        public void Add(int start, string text)
        {
            Renew(Text.Insert(start, text));
            Cursor = start + text.Length;
        }
        public void Remove(int start, int count)
        {
            Renew(Text.Remove(start, count));
            Cursor = start;
        }
        /// <summary>
        /// Prepend text at the current char
        /// </summary>
        /// <param name="text"></param>
        public void InsertChar(string text)
        {
            Cursor = CursorCharIndex();
            Add(Cursor, text);
        }
        /// <summary>
        /// Append text at the current char
        /// </summary>
        /// <param name="text"></param>
        public void AppendChar(string text)
        {
            Cursor = CursorCharIndex() + 1;
            Add(Cursor, text);
        }
        /// <summary>
        /// Prepend text at the current word
        /// </summary>
        /// <param name="text"></param>
        public void InsertWord(string text)
        {
            CursorToPreWord(CursorAtWord());
            Add(Cursor, text);
        }
        /// <summary>
        /// Prepend text at the current line
        /// </summary>
        /// <param name="text"></param>
        public void InsertLine(string text)
        {
            CursorToPreLine(CursorAtLine());
            Add(Cursor, text);
        }
        /// <summary>
        /// Append text at the current word
        /// </summary>
        /// <param name="text"></param>
        public void AppendWord(string text)
        {
            CursorToApWord(CursorAtWord());
            Add(Cursor, text);
        }
        /// <summary>
        /// Append text at the current line
        /// </summary>
        /// <param name="text"></param>
        public void AppendLine(string text)
        {
            CursorToApLine(CursorAtLine());
            Add(Cursor, text);
        }
        /// <summary>
        /// Start a new line below the current line with text given
        /// </summary>
        /// <param name="text"></param>
        public void NewLineBelow(string text)
        {
            CursorToApLine();
            Add(Cursor, string.Join("", '\n', text));
        }
        /// <summary>
        /// Start a new line above the current line with text given
        /// </summary>
        /// <param name="text"></param>
        public void NewLineAbove(string text)
        {
            CursorToPreLine();
            Add(Cursor, string.Join("", text, '\n'));
        }
        /// <summary>
        /// Replace the first occurence of find with replace and put cursor at the end of the last replace
        /// </summary>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        public void ReplaceNext(string find, string replace)
        {
            int start = Text.IndexOf(find);
            if (start!= -1)
            {
                Cursor = start;
                Remove(start, find.Length);
                Add(start, replace);
            }
        }
        /// <summary>
        /// Replace all occurence of find with replace and put cursor at the end of the last replace
        /// </summary>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        public void ReplaceAll(string find, string replace)
        {
            while (find != replace && Text.IndexOf(find) != -1)
            {
                ReplaceNext(find, replace);
            }
        }
        /// <summary>
        /// Delete the {num}th char
        /// </summary>
        /// <param name="num"></param>
        public void DeleteChar(int num)
        {
            Cursor = num - 1;
            Remove(CursorCharIndex(), 1);
        }
        /// <summary>
        /// Delete the {num}th word
        /// </summary>
        /// <param name="num"></param>
        public void DeleteWord(int num)
        {
            CursorToPreWord(num);
            int start = CursorCharIndex();
            CursorToApWord(num);
            int end = CursorCharIndex();
            Remove(start, end - start + 1);
        }
        /// <summary>
        /// Delete the {num}th line
        /// </summary>
        /// <param name="num"></param>
        public void DeleteLine(int num)
        {
            CursorToPreLine(num);
            int start = CursorCharIndex();
            CursorToApLine(num);
            int end = CursorCharIndex();
            Remove(start, end - start + 1);
        }
        /// <summary>
        /// Clear Text
        /// </summary>
        public void Clear()
        {
            Remove(0, Text.Length);
        }
        /// <summary>
        /// Clear Text and cache
        /// </summary>
        public void ClearAll()
        {
            Remove(0, Text.Length);
            MoveCache(cache.Length);
        }
        /// <summary>
        /// Copy Text to clipboard
        /// </summary>
        public void Copy()
        {
            Platform.Get<IClipboard>().Value = Text;
        }
        /// <summary>
        /// Copy Text to clipboard then Clear
        /// </summary>
        public void Cut()
        {
            Copy();
            Clear();
        }
        #endregion

        #region Command
        private static class Ops
        {
            public const char Escaper = '\\';
            public const char Trigger = '.';

            public const char ToWord = 't';
            public const char ToLine = 'T';
            public const char MoveBy = 'm';
            public const char MoveSuc = 'M';

            public const char InsertChar = 'h';
            public const char DeleteChar = 'H';
            public const char InsertWord = 'i';
            public const char InsertLine = 'I';
            public const char AppendWord = 'a';
            public const char AppendLine = 'A';
            public const char NewLineBelow = 'o';
            public const char NewLineAbove = 'O';
            public const char Undo = 'z';
            public const char Redo = 'Z';
            public const char ReplaceNext = 'r';
            public const char ReplaceAll = 'R';
            public const char DeleteWord = 'd';
            public const char DeleteLine = 'D';
            public const char Clear = 'c';
            public const char ClearAll = 'C';

            public const char Copy = 'x';
            public const char Cut = 'X';
        }
        /// <summary>
        /// Is the text considered as a command
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsCommand(string text)
        {
            return (text.Length > 0 && text[0] == Ops.Trigger);
        }
        /// <summary>
        /// Parse given args to LaText Args form, continuous literals are merged into one element 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string[] ParseArgs(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                return default;
            }
            string[] result = new string[0];
            string arg = string.Empty, op = string.Empty, literal = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                arg = args[i];
                if (string.IsNullOrEmpty(arg))
                {
                    continue;
                }
                if (arg[0] == Ops.Trigger)
                {
                    result = result.Append(literal).ToArray();
                    literal = string.Empty;
                    result = result.Append(arg).ToArray();
                    continue;
                }
                literal = string.Join(i == 0 ? "" : " ", literal, arg);
                if (i == args.Length - 1)
                {
                    result = result.Append(literal).ToArray();
                }
            }
            return result;
        }
        /// <summary>
        /// Parse a literal LaText Args element to displayed text
        /// </summary>
        /// <param name="literal"></param>
        /// <returns></returns>
        private static string ParseLiteral(string literal)
        {
            string real = literal.Replace("\\.", ".");
            real = real.Replace('_', ' '); //TODO Escape
            return real;
        }
        /// <summary>
        /// Parse op with its numeric params for cursor movement
        /// </summary>
        /// <param name="op"></param>
        /// <param name="moveTo">Cursor should move to</param>
        /// <param name="moveBy">Cursor should move by</param>
        /// <returns></returns>
        public static int[] GetNumParam(string op, out bool moveTo, out bool moveBy)
        {
            int dir = 1;
            int[] nums = new int[0];
            moveTo = false; moveBy = false;
            if (op.Length < 3)
            {
                return new int[] { int.MaxValue };
            }
            string[] paras = op.Substring(2).Split(',');
            for (int i = 0; i < paras.Length; i++)
            {
                string para = paras[i];
                if (para[0] == '+')
                {
                    moveBy = true;
                    para = para.Substring(1);
                }
                else if (para[0] == '-')
                {
                    moveBy = true;
                    dir = -1;
                    para = para.Substring(1);
                }
                if (int.TryParse(para, out int num))
                {
                    nums = nums.Append(Math.Max(num, 0) * dir).ToArray();
                }
                else
                {
                    nums = nums.Append(int.MaxValue).ToArray();
                }

            }
            moveTo = !moveBy && nums.Length > 0;
            return nums;
        }
        /// <summary>
        /// Prepend text with args
        /// </summary>
        /// <param name="args"></param>
        public void DoInsertWord(string[] args)
        {
            string[] largs = args.Take(2).ToArray();
            int[] nums = GetNumParam(largs[0], out bool to, out bool by);
            int words = nums[0];
            if (words == int.MaxValue)
            {
                words = CursorAtWord();
            }
            if (to)
            {
                CursorToPreWord(words);
            }
            else if (by)
            {
                CursorToPreWord(CursorAtWord() + words);
            }
            InsertWord(largs[1]);
        }
        /// <summary>
        /// Prepend text with args
        /// </summary>
        /// <param name="args"></param>
        public void DoInsertLine(string[] args)
        {
            string[] largs = args.Take(2).ToArray(); 
            int[] nums = GetNumParam(largs[0], out bool to, out bool by);
            int lines = nums[0];
            if (lines == int.MaxValue)
            {
                lines = CursorAtLine();
            }
            if (to)
            {
                CursorToPreLine(lines);
            }
            else if (by)
            {
                CursorToPreLine(CursorAtLine() + lines);
            }
            InsertLine(largs[1]);
        }
        /// <summary>
        /// Append text with args
        /// </summary>
        /// <param name="args"></param>
        public void DoAppendWord(string[] args)
        {
            string[] largs = args.Take(2).ToArray();
            int[] nums = GetNumParam(args[0], out bool to, out bool by);
            int words = nums[0];
            if (words == int.MaxValue)
            {
                words = CursorAtWord();
            }
            if (to)
            {
                CursorToPreWord(words);
            }
            else if (by)
            {
                CursorToPreWord(CursorAtWord() + words);
            }
            AppendWord(largs[1]);
        }
        /// <summary>
        /// Append text with args
        /// </summary>
        /// <param name="args"></param>
        public void DoAppendLine(string[] args)
        {
            string[] largs = args.Take(2).ToArray();
            int[] nums = GetNumParam(largs[0], out bool to, out bool by);
            int lines = nums[0];
            if (to)
            {
                CursorToPreLine(lines);
            }
            else if (by)
            {
                CursorToPreLine(CursorAtWord() + lines);
            }
            AppendLine(largs[1]);
        }
        /// <summary>
        /// Run commands in given args
        /// </summary>
        /// <param name="args"></param>
        public void RunCommand(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }
            List<string> largs = ParseArgs(args).ToList();
            while (largs.Count > 0)
            {
                string la = largs[0];
                if (!IsCommand(la))
                {
                    Renew(la);
                    largs.RemoveAt(0);
                    continue;
                }
                string[] cmdPair = new string[2];
                bool withLiteral = largs.Count > 1 && !IsCommand(largs[1]);
                if (withLiteral)
                {
                    cmdPair = largs.Take(2).ToArray();
                }
                char detLa = la[1];
                switch (detLa)
                {
                    default:
                        largs.RemoveAt(0);
                        break;
                    case Ops.InsertWord:
                        if (withLiteral)
                        {
                            DoInsertWord(cmdPair);
                            largs.RemoveRange(0, 2);
                            break;
                        }
                        largs.RemoveAt(0);
                        break;
                    case Ops.InsertLine:
                        if (withLiteral)
                        {
                            DoInsertLine(cmdPair);
                            largs.RemoveRange(0, 2);
                            break;
                        }
                        largs.RemoveAt(0);
                        break;
                    case Ops.AppendWord:
                        if (!withLiteral)
                        {
                            DoAppendWord(cmdPair);
                            largs.RemoveRange(0, 2);
                            break;
                        }
                        largs.RemoveAt(0);
                        break;
                    case Ops.AppendLine:
                        if (withLiteral)
                        {
                            DoAppendLine(cmdPair);
                            largs.RemoveRange(0, 2);
                            break;
                        }
                        largs.RemoveAt(0);
                        break;
                }
            }
        }
        #endregion
    }
}
