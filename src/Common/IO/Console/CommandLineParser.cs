using CommandLine;

namespace Common.IO.Console
{
    public static class CommandLineParser
    {
        public static T ParseArgs<T>(string[] args, ICommandLineOptions commandLineOptions) where T: ICommandLineOptions
        {
            Parser parser = new Parser();

            if (!parser.ParseArguments(args, commandLineOptions))
                throw new ParserException("Command Line parser error");

            return (T)commandLineOptions;

        }
    }
}
