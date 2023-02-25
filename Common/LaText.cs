using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LacewingNote.Common
{
    public struct LaText
    {
        private string[] cache;
        private int cIndex { get { return Math.Clamp(cIndex, 0, cache.Length - 1); } set { value = Math.Clamp(value, 0, cache.Length - 1); } }
        private int cursor { get { return Math.Clamp(cursor, 0, Text.Length); } set { value = Math.Clamp(value, 0, Text.Length); } }
        private List<Point> selection;

        public string Text { get => cache[cIndex]; }
        public string[] Words { get => Text.Split(' '); }
        public string[] Lines { get => Text.Split('\n'); }
        public int CacheLength { get => cache.Length; }

        #region Construction
        public LaText(string[] text = default, int cacheLength = 9)
        {
            this.cache = new string[cacheLength];
            this.cIndex = 0;
            this.cursor = 0;
            this.selection = new List<Point>();
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
        private int CursorCharIndex()
        {
            return Math.Max(cursor - 1, 0);
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
                if (WordsLength(i) >= cursor)
                {
                    return i;
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
            cursor = WordsLength(num - 1);
        }
        /// <summary>
        /// Move curdor to the end of the {num}th word
        /// </summary>
        /// <param name="num"></param>
        private void CursorToApWord(int num = 1)
        {
            cursor = WordsLength(num);
        }
        /// <summary>
        /// Length of num lines for cursor
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private int LineLength(int num)
        {
            num = Math.Clamp(num, 0, Lines.Length);
            int length = 0;
            for (int i = 0; i < num; i++)
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
                if (LineLength(i) >= cursor)
                {
                    return i;
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
            cursor = LineLength(num - 1);
        }
        /// <summary>
        /// Move cursor to the end of the {num}th line
        /// </summary>
        /// <param name="num"></param>
        private void CursorToApLine(int num = 1)
        {
            cursor = LineLength(num);
        }
        #endregion

        #region Selection
        private void SelectChar(int start, int end)
        {
            Point range = new Point();
            cursor = start;
            range.X = CursorCharIndex();
            cursor = end;
            range.Y = CursorCharIndex();
            selection.Add(range);
        }
        private void SelectWord(int start, int end)
        {
            Point range = new Point();
            CursorToPreWord(start);
            range.X = cursor;
            CursorToApWord(end);
            range.Y = CursorCharIndex();
            selection.Add(range);
        }
        private void SelectLine(int start, int end)
        {
            Point range = new Point();
            CursorToPreLine(start);
            range.X = cursor;
            CursorToApLine(end);
            range.Y = CursorCharIndex();
            selection.Add(range);
        }
        #endregion

        #region Text editing
        /// <summary>
        /// Override text by newText, cached
        /// </summary>
        /// <param name="newText"></param>
        public void Renew(string newText)
        {
            MoveCache(-cIndex + 1);
            cache[0] = newText;
            cIndex = 0;
            cursor = Text.Length;
            selection = new List<Point>();
        }
        /// <summary>
        /// Increase cIndex by step
        /// </summary>
        /// <param name="step"></param>
        public void Undo(int step = 1)
        {
            if (cache.Length <= 0 || step < 1 || cIndex + step > cache.Length - 1)
            {
                return;
            }
            cIndex += step;
        }
        /// <summary>
        /// Decrease cIndex by step
        /// </summary>
        /// <param name="step"></param>
        public void Redo(int step = 1)
        {
            if (cache.Length <= 0 || step < 1 || cIndex - step < 0)
            {
                return;
            }
            cIndex -= step;
            cursor = Text.Length;
        }
        /// <summary>
        /// Insert text at index, cached
        /// </summary>
        /// <param name="index"></param>
        /// <param name="text"></param>
        public void Add(int index, string text)
        {
            Renew(Text.Insert(index, text));
            cursor += text.Length;
        }
        /// <summary>
        /// Prepend text at the current word
        /// </summary>
        /// <param name="text"></param>
        public void Insert(string text)
        {
            CursorToPreWord(CursorAtWord());
            Add(cursor, text);
        }
        /// <summary>
        /// Prepend text at the current line
        /// </summary>
        /// <param name="text"></param>
        public void InsertLine(string text)
        {
            CursorToPreLine(CursorAtLine());
            Add(cursor, text);
        }

        #endregion

        #region Command
        private static class Ops
        {
            public const char Escaper = '\\';
            public const char Trigger = '.';

            public const string SelectWord = "s";
            public const string SelectLine = "S";
            public const string Insert = "i";
            public const string InsertLine = "I";
            public const string Append = "a";
            public const string AppendLine = "A";
            public const string NewLineBelow = "o";
            public const string NewLineAbove = "O";
            public const string InsertAtWord = "e";
            public const string AppendAtWord = "E";
            public const string InsertAtLine = "l";
            public const string AppendAtLine = "L";
            public const string Undo = "z";
            public const string Redo = "Z";
            public const string ReplaceNext = "r";
            public const string ReplaceAll = "R";
            public const string Clear = "c";
            public const string ClearAll = "C";
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
        private static string[] ParseArgs(string[] args)
        {
            if (args.Length <= 0)
            {
                return default;
            }
            string[] result = new string[0];
            bool[] literal = new bool[args.Length];
            string literals = string.Empty, arg = string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                arg = args[i];
                if (arg[0] != Ops.Trigger)
                {
                    literal[i] = true;
                }
            }
            for (int i = 0; i < args.Length; i++)
            {
                arg = args[i];
                if (literal[i])
                {
                    literals = string.Join(i == 0 ? "" : " ", literals, arg);
                }
                if (!literal[i] || i == literal.Length - 1)
                {
                    result.Append(literals);
                    literals = string.Empty;
                    result.Append(arg);
                }
            }
            return result;
        }
        #endregion
    }
}
