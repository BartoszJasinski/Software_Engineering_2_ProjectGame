using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DebugUtils
{
    public static class ConsoleDebug
    {
        public static void Good(string consoleMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(consoleMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Message(string consoleMessage)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(consoleMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Ordinary(string consoleMessage)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(consoleMessage);    
        }

        public static void Error(string consoleMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(consoleMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }


        public static void Warning(string consoleMessage)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(consoleMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }


    }
}
