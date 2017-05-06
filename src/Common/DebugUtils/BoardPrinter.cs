using System;
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

        public static void PrintAlternative(AddressableBoard board)
        {
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var field = board.Fields[x, y];
                    if (field == null)
                    {
                        Console.Write("?");
                        return;
                    }
                    if (field is GoalField)
                    {
                        var f = field as GoalField;
                        switch (f.Team)
                        {
                            case Schema.TeamColour.red:
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                break;
                            case Schema.TeamColour.blue:
                                Console.ForegroundColor = ConsoleColor.DarkBlue;
                                break;
                        }
                        switch (f.Type)
                        {
                            case Schema.GoalFieldType.goal:
                                {
                                    Console.BackgroundColor = ConsoleColor.Yellow;
                                    Console.Write("g");
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
                                    Console.Write("?");
                                    break;
                                }
                        }

                    }
                    else if (field is TaskField)
                    {
                        var f = field as TaskField;
                        Console.BackgroundColor = ConsoleColor.DarkGreen;

                        if (f.PieceId.HasValue)
                        {
                            Console.BackgroundColor = ConsoleColor.Magenta;
                            //Console.Write("P");
                        }
                        if (f.PlayerId.HasValue)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write(f.PlayerId.Value);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write(" ");
                        }

                    }

                    Console.ResetColor();
                }
                Console.WriteLine();
            }

        }

        public static void Print(Field[,] fields)
        {
            for (int i = 0; i < fields.GetLength(1); i++)
            {
                for (int j = 0; j < fields.GetLength(0); j++)
                {
                    PrintField(fields[j, i]);
                }
                Console.WriteLine();
            }
        }

        private static void PrintField(Field field)
        {
            if(field == null)
            {
                Console.Write("?");
                return;
            }
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
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(f.DistanceToPiece);
                }
                
            }

            Console.ResetColor();
        }
    }
}
