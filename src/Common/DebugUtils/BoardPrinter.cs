using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Schema;
using Common.SchemaWrapper;

namespace Common.DebugUtils
{
    public class BoardPrinter
    {

        public static void Print(AddressableBoard board)
        {
            if (board.Width > Console.LargestWindowWidth)
            {
                Console.WriteLine("Board too wide. Won't be displayed properly");
            }

            uint boardHeight = board.TasksHeight + 2 * board.GoalsHeight;
            for (int y = 0; y < boardHeight; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    PrintField(board.Fields[x,y]);
                }
                Console.WriteLine();
            }
        }

        private static void PrintField(Field field)
        {
            Console.WriteLine("handling generic");
        }

        private static void PrintField(GoalField field)
        {
            Console.WriteLine("handling goal");
        }

        private static void PrintField(TaskField field)
        {
            
        }
    }
}
