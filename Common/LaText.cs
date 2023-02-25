using Microsoft.Xna.Framework;
using ReLogic.OS;
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
        private int cIndex;
        private int CIndex { get { return Math.Clamp(cIndex, 0, Math.Max(cache.Length - 1, 0)); } set { cIndex = Math.Clamp(value, 0, Math.Max(cache.Length - 1, 0)); } }
        private int cursor;
        private int Cursor { get { return Math.Clamp(cursor, 0, Text.Length); } set { cursor = Math.Clamp(value, 0, Text.Length); } }

        public string Text { get => cache[CIndex]; }
        public string[] Words { get => Text.Split(' '); }
        public string[] Lines { get => Text.Split('\n'); }
        public int CacheLength { get => cache.Length; }

        #region Construction
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
            return Math.Min(Cursor, Text.Length - 1);
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
                if (WordsLength(i) >= Cursor)
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
            Cursor = WordsLength(num - 1);
        }
        /// <summary>
        /// Move curdor to the end of the {num}th word
        /// </summary>
        /// <param name="num"></param>
        private void CursorToApWord(int num = 1)
        {
            Cursor = WordsLength(num);
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
                if (LineLength(i) >= Cursor)
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
            Cursor = LineLength(num - 1);
        }
        /// <summary>
        /// Move cursor to the end of the {num}th line
        /// </summary>
        /// <param name="num"></param>
        private void CursorToApLine(int num = 1)
        {
            Cursor = LineLength(num);
        }
        #endregion

        #region Text editing
        /// <summary>
        /// Override text by newText, cached
        /// </summary>
        /// <param name="newText"></param>
        public void Renew(string newText)
        {
            MoveCache(-CIndex + 1);
            cache[0] = newText;
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
                Text.Remove(start, find.Length);
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

            public const string ToWord = "t";
            public const string ToLine = "T";

            public const string InsertWord = "i";
            public const string InsertLine = "I";
            public const string AppendWord = "a";
            public const string AppendLine = "A";
            public const string NewLineBelow = "o";
            public const string NewLineAbove = "O";
            public const string Undo = "z";
            public const string Redo = "Z";
            public const string ReplaceNext = "r";
            public const string ReplaceAll = "R";
            public const string DeleteWord = "d";
            public const string DeleteLine = "D";
            public const string Clear = "c";
            public const string ClearAll = "C";

            public const string Copy = "cc";
            public const string Cut = "cx";
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
        private void ReadCommand(string[] args)
        {

        }
        #endregion
    }
}
