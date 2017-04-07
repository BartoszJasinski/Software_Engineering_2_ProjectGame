using Common.DebugUtils;
using GameMaster.Logic.Board;


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
