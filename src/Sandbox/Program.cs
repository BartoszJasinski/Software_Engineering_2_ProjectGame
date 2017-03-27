using Common.DebugUtils;
using GameMaster.Logic.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            RandomGoalBoardGenerator bg = new RandomGoalBoardGenerator(30, 20, 5, 123);
            BoardPrinter.Print(bg.CreateBoard());
        }
    }
}
