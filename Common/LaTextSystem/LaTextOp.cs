using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LacewingNote.Common.LaTextSystem
{
    internal abstract class LaTextOp
    {
        public abstract string Trigger => string.Empty;
        public virtual string NeedsLiteral => false;
        public virtual void Run(LaText text, string[] args)
        {

        }

        /// <summary>
        /// Parse op with its numeric params for cursor movement
        /// </summary>
        /// <param name="op"></param>
        /// <param name="moveTo">Cursor should move to</param>
        /// <param name="moveBy">Cursor should move by</param>
        /// <returns></returns>
        protected static int[] GetNumParam(string op, out bool moveTo, out bool moveBy)
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
    }
}
