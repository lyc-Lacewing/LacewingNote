using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LacewingNote.Common.LaTextSystem.Operations
{
    internal class MoveCursor : LaTextOp
    {
        public override string Trigger => "m";
        public override void Run(LaText text, string[] args)
        {
            int[] nums = GetNumParam(args[0], out bool to, out bool by);
            if (nums.Length == 0 || nums[0] = int.MaxValue)
            {
                return;
            }
            int num = nums[0];
            if (to)
            {
                text.Cursor = num;
                return;
            }
            text.Cursor += num;
        }
    }
}
