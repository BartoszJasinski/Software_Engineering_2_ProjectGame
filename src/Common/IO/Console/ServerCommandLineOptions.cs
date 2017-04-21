using CommandLine;

namespace Common.IO.Console
{
    public class ServerCommandLineOptions: ICommandLineOptions
    {
        [Option('p', "port", HelpText = "server port number")]
        public int Port { get; set; }

        [Option('c', "conf", Required = false, HelpText = "configuration file")]
        public string Conf { get; set; }

    }
}
