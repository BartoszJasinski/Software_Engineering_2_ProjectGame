using CommandLine;

namespace Common.IO.Console
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
