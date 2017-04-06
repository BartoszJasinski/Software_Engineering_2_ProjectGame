using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if(field is GoalField)
            {
                var f = field as GoalField;
                switch(f.Team)
                {
                    case Schema.TeamColour.red:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                    case Schema.TeamColour.blue:
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        break;
                }
                switch(f.Type)
                {
                    case Schema.GoalFieldType.goal:
                        {
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.Write(" ");
                            break;
                        }
                    case Schema.GoalFieldType.nongoal:
                        {
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            Console.Write(" ");
                            break;
                        }
                    case Schema.GoalFieldType.unknown:
                        {
                            Console.BackgroundColor = ConsoleColor.Gray;
                            Console.Write(" ");
                            break;
                        }
                }
                
            }
            else if(field is TaskField)
            {
                var f = field as TaskField;
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                
                if(f.PieceId.HasValue)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write("P");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" ");
                }
                
            }

            Console.ResetColor();
        }
    }
}
