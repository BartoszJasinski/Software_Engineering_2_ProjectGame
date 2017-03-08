using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace MockPlayer.IO
{
    public class CommandLineParser
    {
        public static CommandLineOptions ParseArgs(string[] args)
        {
            CommandLineOptions options = new CommandLineOptions();
            CommandLine.Parser parser = new Parser();

            if (!parser.ParseArguments(args, options))
                throw new ParserException("Command Line parser error");

            return options;

        }
    }
}
